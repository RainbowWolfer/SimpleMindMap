﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private void MyButton_Click(object sender, RoutedEventArgs e) {
			Process.Start(new ProcessStartInfo() {
				FileName = "https://rainbowwolfer.github.io/",
				UseShellExecute = true
			});
		}
	}
}
