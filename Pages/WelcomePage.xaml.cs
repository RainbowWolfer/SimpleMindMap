using MindMap.Entities.Locals;
using MindMap.SubWindows;
using MindMap.UserControls.WelcomePageControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Pages {
	public partial class WelcomePage: Page {
		private readonly MainWindow _mainWindow;
		public WelcomePage(MainWindow mainWindow) {
			InitializeComponent();
			this._mainWindow = mainWindow;
			LoadRecentFile();
		}

		private void LoadRecentFile() {
			if(AppSettings.Current == null) {
				return;
			}

			RecentFileStackPanel.Children.Clear();
			NoDataGrid.Visibility = Visibility.Collapsed;
			if(AppSettings.Current.RecentOpenFilesList.Count == 0) {
				NoDataGrid.Visibility = Visibility.Visible;
				return;
			}

			Dictionary<RecentType, StackPanel> panels = new();
			var margin = new Thickness(20, 0, 0, 0);
			panels.Add(RecentType.Today, new StackPanel() { Margin = margin });
			panels.Add(RecentType.Yesterday, new StackPanel() { Margin = margin });
			panels.Add(RecentType.ThisMonth, new StackPanel() { Margin = margin });
			panels.Add(RecentType.LastMonth, new StackPanel() { Margin = margin });
			panels.Add(RecentType.Earlier, new StackPanel() { Margin = margin });

			//foreach((string name, string path, DateTime time) item in AppSettings.Current.GetDefaultTestValues()) {
			foreach((string name, string path, DateTime time) item in AppSettings.Current.RecentOpenFilesList) {
				RecentType recentType = CalculateDateTimeType(item.time);
				if(!panels.ContainsKey(recentType)) {
					throw new Exception($"Recent Type ({recentType}) Not Found");
				}
				var child = new RecentFileButton() {
					FileName = item.name,
					Path = item.path,
					Date = item.time.ToString("yyyy-MM-dd HH:mm"),
					OnDelete = async target => {
						StackPanel? parent = null;
						Expander? expander = null;
						foreach(Expander item in RecentFileStackPanel.Children) {
							foreach(RecentFileButton button in ((StackPanel)item.Content).Children) {
								if(button == target) {
									parent = (StackPanel)item.Content;
									expander = item;
								}
							}
						}
						if(parent != null) {
							parent.Children.Remove(target);
							if(parent.Children.Count == 0 && expander != null) {
								RecentFileStackPanel.Children.Remove(expander);
							}
							if(RecentFileStackPanel.Children.Count == 0) {
								NoDataGrid.Visibility = Visibility.Visible;
							}
						}

						AppSettings.Current.RecentOpenFilesList.RemoveAll(f => {
							return f.name == target.FileName && f.path == target.Path;
						});
						await Local.SaveAppSettings();
					},
				};
				child.OnClick = target => {
					_mainWindow.OpenFile(item.path, async () => {
						panels[recentType].Children.Remove(child);
						AppSettings.Current.RecentOpenFilesList.Remove(item);
						await Local.SaveAppSettings();
					});
				};
				panels[recentType].Children.Add(child);
			}
			foreach(KeyValuePair<RecentType, StackPanel> item in panels) {
				if(item.Value.Children.Count == 0) {
					continue;
				}
				Expander expander = new() {
					Header = GetRecentTypeString(item.Key),
					Content = item.Value,
					IsExpanded = true,
					FontSize = 18,
				};
				RecentFileStackPanel.Children.Add(expander);
			}
		}

		private RecentType CalculateDateTimeType(DateTime dateTime) {
			DateTime now = DateTime.Now;
			TimeSpan span = now.Subtract(dateTime);
			if(span.TotalDays < 1) {
				return RecentType.Today;
			} else if(span.TotalDays < 2) {
				return RecentType.Yesterday;
			} else if(span.TotalDays < 30) {
				return RecentType.ThisMonth;
			} else if(span.TotalDays < 60) {
				return RecentType.LastMonth;
			} else {
				return RecentType.Earlier;
			}
		}

		private string GetRecentTypeString(RecentType recentType) {
			switch(recentType) {
				case RecentType.Today:
					return "Today";
				case RecentType.Yesterday:
					return "Yesterday";
				case RecentType.ThisMonth:
					return "This Month";
				case RecentType.LastMonth:
					return "Last Month";
				case RecentType.Earlier:
					return "Earlier";
				default:
					return "None";
			}
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

	public enum RecentType {
		Today, Yesterday, ThisMonth, LastMonth, Earlier
	}
}
