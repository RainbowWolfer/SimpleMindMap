using MindMap.Entities;
using MindMap.Entities.Elements;
using MindMap.Entities.Locals;
using MindMap.Pages;
using MindMap.Pages.Interfaces;
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
		public static MainWindow? Instance { get; private set; }
		private MindMapPage? _mindMapPage;

		public ComboKeyManager KeyManager;

		public MainWindow() {
			Instance = this;
			InitializeComponent();

			SaveMenuItem.IsEnabled = false;
			SaveAsMenuItem.IsEnabled = false;
			RedoMenuItem.IsEnabled = false;
			UndoMenuItem.IsEnabled = false;

			MainFrame.Navigated += (s, e) => {
				MainFrame.NavigationService.RemoveBackEntry();
			};
			KeyManager = new ComboKeyManager(this);

			MainFrame.Navigate(new WelcomePage(this));

			Icon = new BitmapImage(new Uri("pack://application:,,,/Images/AppIcon_Color.png"));
			SetTitle("Mind Map");
		}

		private void AboutThisMenuItem_Click(object sender, RoutedEventArgs e) {
			new AboutWindow(this).ShowDialog();
		}

		public void NavigateToMindMap() {
			ClosePreviousPage();
			_mindMapPage = new MindMapPage();
			MainFrame.Navigate(_mindMapPage);
			_mindMapPage.Focus();

			SaveAsMenuItem.IsEnabled = true;
			SaveMenuItem.IsEnabled = true;
		}

		public void ClosePreviousPage() {
			if(MainFrame.Content is IPage page) {
				page.OnClose();
			}
		}

		public async void OpenFile() {
			LocalInfo? result = await Local.Load();
			if(result != null && result.MapInfo != null) {
				NavigateToMindMap();
				_mindMapPage?.Load(result.MapInfo, result.FileInfo);
			}
		}

		public void SetRedoMenuItemActive(bool active) {
			RedoMenuItem.IsEnabled = active;
		}

		public void SetUndoMenuItemActive(bool active) {
			UndoMenuItem.IsEnabled = active;
		}

		public static void SetTitle(string title) {
			if(Instance == null) {
				return;
			}
			Instance.Title = " " + title;
		}

		private void NewFileMenuItem_Click(object sender, RoutedEventArgs e) {
			if(_mindMapPage?.HasChanged ?? false) {
				MessageBoxResult result = MessageBox.Show(this, "You have unsaved file. Are you sure to start a new canvas?", "New File", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
				if(result != MessageBoxResult.OK) {
					return;
				}
			}
			NavigateToMindMap();
		}

		private void SaveMenuItem_Click(object sender, RoutedEventArgs e) {
			_mindMapPage?.Save();
		}

		private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e) {
			OpenFile();
		}

		private void RedoMenuItem_Click(object sender, RoutedEventArgs e) {
			_mindMapPage?.Redo();
		}

		private void UndoMenuItem_Click(object sender, RoutedEventArgs e) {
			_mindMapPage?.Undo();
		}

		private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e) {
			_mindMapPage?.Save(true);
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
			if(_mindMapPage?.HasChanged ?? false) {
				MessageBoxResult result = MessageBox.Show(this, "You have unsaved file. Are you sure to quit?", "Exit", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
				if(result != MessageBoxResult.OK) {
					return;
				}
			}
			Application.Current.Shutdown();
		}

		private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) {

		}
	}
}
