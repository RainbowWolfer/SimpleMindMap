using MindMap.Entities.Locals;
using MindMap.Entities.Presets;
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
using System.Windows.Shapes;

namespace MindMap.SubWindows.Dialogs {
	public partial class ElementPresetsGroupSelectorView: Window {
		private ElementPresetsGroup? currentSelected = null;
		private string newNameText = "";

		public string NewNameText {
			get => newNameText;
			private set {
				newNameText = value;
				ConfirmButton.IsEnabled = CurrentSelected != null && NameValid;
			}
		}

		public ElementPresetsGroup? CurrentSelected {
			get => currentSelected;
			private set {
				currentSelected = value;
				ConfirmButton.IsEnabled = CurrentSelected != null && NameValid;
				if(value != null) {
					ErrorText.Text = "";
					ErrorText.Visibility = Visibility.Collapsed;
					GroupSelectedText.Text = value.Name;
				} else {
					GroupSelectedText.Text = "No Group Selected";
				}
			}
		}

		public bool NameValid { get; private set; } = false;

		private readonly Dictionary<string, IEnumerable<string>> existNames = new();

		public ElementPresetsGroupSelectorView(Window owner, string typeName) {
			InitializeComponent();
			this.Owner = owner;

			ConfirmButton.IsEnabled = false;

			if(AppSettings.Current == null) {
				return;
			}
			int count = 0;
			GroupsListView.Items.Clear();
			foreach(ElementPresetsGroup item in AppSettings.Current.ElementPresetsGroups.Where(e => !e.Unchangable)) {
				GroupsListView.Items.Add(new TextBlock() {
					Tag = item,
					Text = $"{item.Name} - ({item.Presets.Count})",
				});
				existNames.Add(item.Name, item.Presets.Select(p => p.Name));
				count += item.Presets.Count;
			}
			NewPresetName.Text = $"{typeName} Custom {count + 1}";
			NewNameText = NewPresetName.Text;
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			if(CurrentSelected == null || string.IsNullOrWhiteSpace(NewNameText)) {
				return;
			}
			DialogResult = true;
		}

		private void BackButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
		}

		private void NewPresetName_TextChanged(object sender, TextChangedEventArgs e) {
			CalculateNameValid();
		}

		private void CalculateNameValid() {
			string name = NewPresetName.Text.Trim();
			if(string.IsNullOrWhiteSpace(name)) {
				ErrorText.Text = "Cannot be empty";
				ErrorText.Visibility = Visibility.Visible;
				NameValid = false;
			} else if(CurrentSelected == null) {
				ErrorText.Text = "No group selected";
				ErrorText.Visibility = Visibility.Visible;
				NameValid = false;
			} else if(existNames[CurrentSelected.Name].Contains(name)) {
				ErrorText.Text = "Name exists";
				ErrorText.Visibility = Visibility.Visible;
				NameValid = false;
			} else {
				ErrorText.Text = "";
				ErrorText.Visibility = Visibility.Collapsed;
				NameValid = true;
			}
			NewNameText = name;
		}

		private void NewPresetName_KeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Enter && ConfirmButton.IsEnabled) {
				DialogResult = true;
			}
		}

		private void GroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems.Count > 0 && e.AddedItems[0] is TextBlock text && text.Tag is ElementPresetsGroup group) {
				this.CurrentSelected = group;
				CalculateNameValid();
			}
		}
	}
}
