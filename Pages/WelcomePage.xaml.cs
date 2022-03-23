﻿using MindMap.SubWindows;
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
