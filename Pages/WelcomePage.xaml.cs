using MindMap.SubWindows;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Pages {
	public partial class WelcomePage: Page {
		private readonly MainWindow _mainWindow;
		public WelcomePage(MainWindow mainWindow) {
			InitializeComponent();
			this._mainWindow = mainWindow;
		}

		private void NewFileButton_Click(object sender, RoutedEventArgs e) {
			_mainWindow.NavigateToMindMap();
		}

		private void OpenFileButton_Click(object sender, RoutedEventArgs e) {
			_mainWindow.OpenFile();
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e) {
			new SettingsWindow(_mainWindow).ShowDialog();
		}
	}
}
