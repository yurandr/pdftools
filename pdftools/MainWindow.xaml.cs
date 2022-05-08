using Microsoft.Win32;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pdftools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        public ObservableCollection<FileData> Files { get; } = new ObservableCollection<FileData>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void Merge(IEnumerable<string> filepaths, string outputFilePath)
        {
            using (var outputDocument = new PdfDocument())
            {
                foreach (var filepath in filepaths)
                {
                    var inputDocument = PdfReader.Open(filepath, PdfDocumentOpenMode.Import);
                    for (var iPage = 0; iPage < inputDocument.PageCount; ++iPage)
                    {
                        outputDocument.AddPage(inputDocument.Pages[iPage]);
                    }
                }
                outputDocument.Save(outputFilePath);
            }
        }
        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog() { Filter="*.pdf|*.pdf", Multiselect=true, CheckFileExists=true };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach(var selectedPath in openFileDialog.FileNames)
                {
                    this.Files.Add(new FileData(selectedPath));
                }
            }
        }
        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            if (Files.Count > 0)
            {
                var firstFile = Files.First();
                var outputFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(firstFile.FilePath) ?? System.IO.Directory.GetCurrentDirectory(), "merged.pdf");

                var saveFileDialog = new SaveFileDialog() { Filter = "*.pdf|*.pdf", FileName = outputFilePath};
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        Merge(Files.Select(x => x.FilePath), saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var toRemove = (sender as FrameworkElement)?.DataContext as FileData;
            if (null != toRemove)
                Files.Remove(toRemove);
        }
        private void ListViewItemDoubleClick(object sender, RoutedEventArgs e)
        {
            var fileData = (sender as ListViewItem)?.DataContext as FileData;
            if (null != fileData)
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(fileData.FilePath)
                {
                    UseShellExecute = true
                };
                p.Start();
            }
        }

        #region DnD
        private List<FileData> draggedFiles = new List<FileData>();
        private void ListViewItemPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var listboxItem = sender as ListBoxItem;
            if (e.LeftButton == MouseButtonState.Pressed && null != listboxItem && !draggedFiles.Any())
            {
                foreach(var fileData in (lvFiles.SelectedItems.OfType<FileData>()))
                {
                    draggedFiles.Add(fileData);
                }
                if (draggedFiles.Any())
                {
                    var data = new DataObject();
                    data.SetData(new FileData(string.Empty)); // just a mark, actual files will be in private collection
                    DragDrop.DoDragDrop(listboxItem, data, DragDropEffects.Move | DragDropEffects.Copy);
                }
                e.Handled = true;
            }
        }
        private void ListViewDrop(object sender, DragEventArgs e)
        {
            if (draggedFiles.Any())
            {
                var targetFileData = (sender as ListBoxItem)?.DataContext as FileData;
                if (targetFileData == null)
                {   // no target data file -> add draggedFiles to the end of collection
                    foreach (var fileData in draggedFiles)
                    {
                        Files.Remove(fileData); // maybe we were dragged something present in collection -> remove it from source position
                        Files.Add(fileData); // and append to the end of collection
                    }
                } else
                {
                    var indexOfTarget = Files.IndexOf(targetFileData);
                    if (indexOfTarget != -1)
                    {
                        draggedFiles.Remove(targetFileData); // maybe target is inside dragged files collection -> remove it from there, we will not touch it
                        foreach (var dataFile in draggedFiles)
                            Files.Remove(dataFile); // remove everything from source collection

                        indexOfTarget = Files.IndexOf(targetFileData); // renew position after items deletion
                        foreach (var dataFile in draggedFiles)
                            Files.Insert(indexOfTarget, dataFile); // remove everything from source collection
                    }
                }
                draggedFiles.Clear();
                e.Handled = true;
            }
        }
        private void ListViewDragOver(object sender, DragEventArgs e)
        {
            if (draggedFiles.Any())
            {
                e.Effects = DragDropEffects.Move | DragDropEffects.Copy;
                e.Handled = true;
            }
        }
        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            string[]? dragged = e.Data.GetDataPresent(DataFormats.FileDrop) ? e.Data.GetData(DataFormats.FileDrop, true) as string[] : null;
            if (dragged != null)
            {
                draggedFiles.AddRange(dragged.Select(fn => new FileData(fn)));
                e.Effects = DragDropEffects.Move | DragDropEffects.Copy;
                e.Handled = true;
            }
        }
        #endregion
    }
    public class FileData
    {
        private static System.Globalization.NumberFormatInfo numberFormatInfo = new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = 0, NumberGroupSeparator = " " };
        public string FilePath { get; }
        public FileData(string filePath)
        {
            FilePath = filePath;
        }
        public string FileSize => $"{new System.IO.FileInfo(FilePath).Length.ToString("N", numberFormatInfo)}";
    }
}
