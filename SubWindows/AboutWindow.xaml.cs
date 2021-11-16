using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
	}
}
