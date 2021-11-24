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
		private Frame? _mainFrame;
		private MindMapPage? _mindMapPage;
		public MainWindow() {
			InitializeComponent();
			_mainFrame = MainFrame;
			_mainFrame.NavigationService.RemoveBackEntry();
			MainFrame.Navigate(new WelcomePage(this));
		}

		private void AboutThisMenuItem_Click(object sender, RoutedEventArgs e) {
			new AboutWindow(this).ShowDialog();
		}

		public void NavigateToMindMap() {
			_mindMapPage = new MindMapPage();
			_mainFrame?.Navigate(_mindMapPage);
		}

		public async void OpenFile() {
			if(_mindMapPage == null) {
				NavigateToMindMap();
			}
			string json = await File.ReadAllTextAsync("WriteText.txt");
			Debug.WriteLine(json);
			Local.EverythingInfo? result = Local.Load(json);
			if(result != null) {
				_mindMapPage?.Load(result);
			}
		}

		private void NewFileMenuItem_Click(object sender, RoutedEventArgs e) {
			NavigateToMindMap();
		}

		private void SaveMenuItem_Click(object sender, RoutedEventArgs e) {
			if(_mindMapPage != null) {
				Local.Save(_mindMapPage.elements.Select(x => x.Value).ToList());
			}
		}

		private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e) {
			OpenFile();
		}
	}
}
