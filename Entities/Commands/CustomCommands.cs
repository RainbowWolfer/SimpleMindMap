using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MindMap.Entities.Commands {
	public static class CustomCommands {
		public static RoutedCommand Exit = new RoutedCommand();
		public static RoutedCommand Open = new RoutedCommand();
		public static RoutedCommand New = new RoutedCommand();
		public static RoutedCommand Save = new RoutedCommand();
		public static RoutedCommand SaveAs = new RoutedCommand();
	}
}
