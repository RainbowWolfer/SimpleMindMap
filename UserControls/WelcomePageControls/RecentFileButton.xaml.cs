using MindMap.Entities.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MindMap.UserControls.WelcomePageControls {
	public partial class RecentFileButton: UserControl {
		private string path = "";
		private string date = "";
		private string fileName = "";

		public string FileName {
			get => fileName;
			set {
				fileName = value;
				FileNameText.Text = fileName;
			}
		}
		public string Date {
			get => date;
			set {
				date = value;
				DateText.Text = date;
			}
		}
		public string Path {
			get => path;
			set {
				path = value;
				PathText.Text = path;
			}
		}

		public Action<RecentFileButton>? OnClick { get; set; }
		public Action<RecentFileButton>? OnDelete { get; set; }

		public RecentFileButton() {
			InitializeComponent();
			ContextMenu = new ContextMenu();
			MenuItem item_delete = new() {
				Header = "Delete",
				Icon = new FontIcon("\uE107").Generate(),
			};
			MenuItem item_copy = new() {
				Header = "Copy Path",
				Icon = new FontIcon("\uE8C8").Generate(),
			};
			item_delete.Click += (s, e) => {
				OnDelete?.Invoke(this);
			};
			item_copy.Click += (s, e) => {
				Clipboard.SetText(Path);
			};
			ContextMenu.Items.Add(item_copy);
			ContextMenu.Items.Add(item_delete);
		}

		private void MainButton_Click(object sender, RoutedEventArgs e) {
			OnClick?.Invoke(this);
		}
	}
}
