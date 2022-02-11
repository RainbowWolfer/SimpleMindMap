using MindMap.Entities.Elements;
using MindMap.Entities.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Entities {
	public static class FlyoutMenu {
		public static void CreateBase(FrameworkElement root, RoutedEventHandler handler) {
			root.ContextMenu = new ContextMenu();
			MenuItem item = new() {
				Header = "Delete",
				Icon = new FontIcon("\uE74D", 14).Generate(),
			};
			item.Click += handler;
			root.ContextMenu.Items.Add(item);
			root.ContextMenu.PlacementTarget = root;
		}

		public static void Append(this ContextMenu menu) {

		}
	}
}
