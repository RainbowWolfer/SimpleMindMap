using MindMap.Entities.Locals;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Pages {
	public partial class InitializingPage: Page {
		private readonly MainWindow mainWindow;
		public InitializingPage(MainWindow mainWindow) {
			InitializeComponent();
			this.mainWindow = mainWindow;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) {
			await Local.ReadAppSettings();
			mainWindow.NavigateToWelcomePage();
			mainWindow.EnableMenu();
		}
	}
}
