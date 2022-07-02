using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMap.SubWindows {
	public partial class AboutWindow: Window {
		//private static string Version => $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";
		public AboutWindow(Window onwer) {
			InitializeComponent();
			this.Owner = onwer;
			this.KeyDown += (s, e) => {
				if(e.Key == Key.Escape) {
					this.Close();
				}
			};
			//VersionText.Text = Version;
		}

		private void MyButton_Click(object sender, RoutedEventArgs e) {
			Process.Start(new ProcessStartInfo() {
				FileName = "https://rainbowwolfer.github.io/",
				UseShellExecute = true
			});
		}

		private void CopyItem_Click(object sender, RoutedEventArgs e) {
			Thread thread = new Thread(() => {
				Clipboard.SetText("RainbowWolfer@outlook.com");
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}
	}
}
