using MindMap.Pages;
using MindMap.SubWindows;
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

namespace MindMap {
	public partial class MainWindow: Window {
		private static Frame? _mainFrame;
		public MainWindow() {
			InitializeComponent();
			_mainFrame = MainFrame;
			MainFrame.Navigate(new WelcomePage());
		}

		private void AboutThisMenuItem_Click(object sender, RoutedEventArgs e) {
			new AboutWindow(this).ShowDialog();
		}

		public static void NavigateToMindMap() {
			_mainFrame?.Navigate(new MindMapPage());
		}

		private void NewFileMenuItem_Click(object sender, RoutedEventArgs e) {
			NavigateToMindMap();
		}
	}
}
