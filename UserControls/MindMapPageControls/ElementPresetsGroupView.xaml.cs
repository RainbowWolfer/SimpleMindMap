﻿using MindMap.Entities.Locals;
using MindMap.Entities.Presets;
using MindMap.Pages;
using MindMap.SubWindows.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.UserControls.MindMapPageControls {
	public partial class ElementPresetsGroupView: UserControl {
		private ElementPresetsGroup group;

		public ElementPresetsGroup Group {
			get => group;
			set {
				group = value;
				GroupTextBlock.Text = value.Name;
				GroupCountText.Text = $"({value.Presets.Count})";
				PresetsPanel.Children.Clear();
				foreach(ElementPreset item in value.Presets) {
					var child = new ElementPresetView(parent, item, group.Name, value.Unchangable);
					PresetsPanel.Children.Add(child);
				}
			}
		}

		private readonly MindMapPage parent;

		public ElementPresetsGroupView(MindMapPage parent, ElementPresetsGroup group) {
			InitializeComponent();
			this.parent = parent;
			this.group = group;
			Group = group;

			if(Group.Unchangable) {
				RenameButton.Visibility = Visibility.Collapsed;
				DeleteButton.Visibility = Visibility.Collapsed;
				RenameButton.IsEnabled = false;
				DeleteButton.IsEnabled = false;
			}
		}

		private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
			if(MainWindow.Instance == null || AppSettings.Current == null) {
				return;
			}
			if(MessageBox.Show(MainWindow.Instance, $"Are you sure to delete this group? ({Group.Name})", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
				parent.RemovePresetGroup(Group);
				AppSettings.Current.ElementPresetsGroups.Remove(Group);
				await Local.SaveAppSettings();
			}
		}

		private async void RenameButton_Click(object sender, RoutedEventArgs e) {
			if(MainWindow.Instance == null || AppSettings.Current == null) {
				return;
			}
			var exist = AppSettings.Current.ElementPresetsGroups.Select(x => x.Name);
			var dialog = new RenameDialog(MainWindow.Instance, exist, Group.Name);
			var result = dialog.ShowDialog();
			if(result == true) {
				string text = dialog.GetNewName();
				ElementPresetsGroup? found = AppSettings.Current.ElementPresetsGroups.Find(i => i == Group);
				if(found != null) {
					found.Name = text;
					Group = found;
					await Local.SaveAppSettings();
				}
			}
		}
	}
}
