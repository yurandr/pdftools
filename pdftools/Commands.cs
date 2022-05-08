using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace pdftools.commands
{
	public static class CustomCommands
	{
		public static readonly RoutedUICommand AddFiles = new RoutedUICommand ( "Добавить файлы", "AddFiles", typeof(CustomCommands));
		public static readonly RoutedUICommand MergeFiles = new RoutedUICommand("Объединить файлы...", "MergeFiles", typeof(CustomCommands));
		public static readonly RoutedUICommand RemoveFiles = new RoutedUICommand("Удалить файлы", "RemoveFiles", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.Delete) });
	}
}
