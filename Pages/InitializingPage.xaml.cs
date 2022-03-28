using MindMap.Entities.Locals;
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
