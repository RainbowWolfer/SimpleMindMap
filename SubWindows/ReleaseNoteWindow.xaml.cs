using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMap.SubWindows {
	public partial class ReleaseNoteWindow: Window {
		public readonly Dictionary<string, string[]> notes;
		public ReleaseNoteWindow(Window owner) {
			InitializeComponent();
			this.Owner = owner;
			notes = new() {
				{ "2022-07-02    v1.0.2.0", new string[]{
					"New presets management",
					"Some buges fixed & changes",
				}},
				{ "2022-03-31    v1.0.1.0", new string[]{
					"Welcome page rework",
					"Some bugs fixed",
				}},
				{ "2022-03-29    v1.0.0.0", new string[]{
					"Initial release",
				}},
			};

			SideListView.Items.Clear();
			foreach(KeyValuePair<string, string[]> item in notes) {
				SideListView.Items.Add(new TextBlock() {
					Text = item.Key,
					Tag = item.Key,
				});
			}
			SideListView.Focus();
			SideListView.SelectedIndex = 0;
			this.KeyDown += (s, e) => {
				if(e.Key == Key.Escape) {
					this.Close();
				}
			};
		}

		private (string version, string date) GetVersionDate(string key) {
			var split = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if(split.Length != 2) {
				return ("Error", "Error");
			}
			return (split[0], split[1]);
		}

		private void SideListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems.Count != 0 && e.AddedItems[0] is TextBlock text && text.Tag is string key) {
				if(notes.TryGetValue(key, out string[]? values) && values != null) {
					(string version, string date) = GetVersionDate(key);
					VersionText.Text = version;
					DateText.Text = date;
					NotesPanel.Children.Clear();
					foreach(var item in values) {
						NotesPanel.Children.Add(new TextBlock() {
							Text = $"- {item}",
						});
					}
				}
			}
		}
	}
}
