using MindMap.Entities;
using MindMap.Entities.Elements;
using MindMap.Entities.Locals;
using MindMap.Pages;
using MindMap.SubWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace MindMap {
	public partial class MainWindow: Window {
		public static MainWindow? Instance { get; private set; }
		private MindMapPage? _mindMapPage;

		public ComboKeyManager KeyManager;
		public MainWindow() {
			Instance = this;
			InitializeComponent();
			MainFrame.Navigated += (s, e) => MainFrame.NavigationService.RemoveBackEntry();
			KeyManager = new ComboKeyManager(this);

			//NavigateToMindMap();
			MainFrame.Navigate(new WelcomePage(this));
		}

		private void AboutThisMenuItem_Click(object sender, RoutedEventArgs e) {
			new AboutWindow(this).ShowDialog();
		}

		public void NavigateToMindMap() {
			_mindMapPage = new MindMapPage();
			MainFrame.Navigate(_mindMapPage);
			_mindMapPage.Focus();
		}

		public async void OpenFile() {
			Local.LocalInfo? result = await Local.Load();
			if(result != null && result.MapInfo != null) {
				NavigateToMindMap();
				_mindMapPage?.Load(result.MapInfo, result.FileInfo);
			}
		}

		private void NewFileMenuItem_Click(object sender, RoutedEventArgs e) {
			NavigateToMindMap();
		}

		private void SaveMenuItem_Click(object sender, RoutedEventArgs e) {
			if(_mindMapPage != null) {
				_mindMapPage.Save();
			}
		}

		private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e) {
			OpenFile();
		}
	}
}
