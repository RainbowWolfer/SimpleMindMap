using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace MindMap.SubWindows {
	public partial class AboutWindow: Window {
		private static string Version => $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";
		public AboutWindow(Window onwer) {
			InitializeComponent();
			this.Owner = onwer;
			this.KeyDown += (s, e) => {
				if(e.Key == Key.Escape) {
					this.Close();
				}
			};
			VersionText.Text = Version;
		}

		private void MyButton_Click(object sender, RoutedEventArgs e) {
			Process.Start(new ProcessStartInfo() {
				FileName = "https://rainbowwolfer.github.io/",
				UseShellExecute = true
			});
		}
	}
}
