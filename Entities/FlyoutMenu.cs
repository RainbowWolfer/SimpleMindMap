using MindMap.Entities.Icons;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Entities {
	public static class FlyoutMenu {
		public static void CreateBase(FrameworkElement root, RoutedEventHandler handler) {
			root.ContextMenu = new ContextMenu();
			MenuItem item_delete = new() {
				Header = "Delete",
				Icon = new FontIcon("\uE74D", 14).Generate(),
			};
			item_delete.Click += handler;
			root.ContextMenu.Items.Add(item_delete);
			root.ContextMenu.PlacementTarget = root;
		}

		public static void Append(this ContextMenu menu) {

		}
	}
}
