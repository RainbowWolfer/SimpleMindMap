using MindMap.Entities;
using MindMap.Entities.Elements;
using MindMap.Entities.Identifications;
using MindMap.Entities.Locals;
using MindMap.Entities.Presets;
using MindMap.Pages;
using MindMap.SubWindows.Dialogs;
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

namespace MindMap.UserControls.MindMapPageControls {
	public partial class ElementPresetView: UserControl {
		private bool isMouseOn;
		private readonly MindMapPage parent;

		public ElementPreset Preset { get; set; }

		public bool IsMouseOn {
			get => isMouseOn;
			set {
				isMouseOn = value;
				parent.Cursor = value ? Cursors.Hand : null;
			}
		}
		private readonly string groupName;

		public ElementPresetView(MindMapPage parent, ElementPreset preset, string groupName, bool unchangable = false) {
			InitializeComponent();
			this.parent = parent;
			this.groupName = groupName;
			Preset = preset;
			NameBlock.Text = preset.Name;
			DrawElement();
			if(unchangable) {
				MainGrid.ContextMenu = null;
			}
		}

		private void DrawElement() {
			Identity identity = new(Preset.TypeID.ToString(), Preset.Name);
			Element element = ElementGenerator.GetElement(null, Preset.TypeID, identity);
			if(Preset.TypeID == ElementGenerator.ID_Polygon) {
				element.SetSize(new Vector2(60, 54));
			} else {
				element.SetSize(new Vector2(60, 60));
			}
			element.SetFramework();
			element.SetProperty(Preset.PropertiesJson);
			if(element is TextRelated text) {
				text.SetText(t => string.IsNullOrWhiteSpace(t) ? "" : "T");
			}
			if(element is MyImage image) {
				image.SetText(t => "");
			}
			PresetElementDisplayGrid.Children.Clear();
			PresetElementDisplayGrid.Children.Add(element.Target);
		}

		private async void RenameMenuItem_Click(object sender, RoutedEventArgs e) {
			if(MainWindow.Instance == null || AppSettings.Current == null) {
				return;
			}
			var group = AppSettings.Current.ElementPresetsGroups.Find(f => f.Name == groupName);
			if(group == null) {
				return;
			}
			var dialog = new RenameDialog(MainWindow.Instance, group.Presets.Select(p => p.Name), Preset.Name);
			if(dialog.ShowDialog() == true) {
				var name = dialog.GetNewName();
				Preset.Name = name;
				NameBlock.Text = Preset.Name;
				await Local.SaveAppSettings();
			}
		}

		private void TopMenuItem_Click(object sender, RoutedEventArgs e) {
			parent.MovePresetToTop(Preset, groupName);
		}

		private void PreviousMenuItem_Click(object sender, RoutedEventArgs e) {
			parent.MovePresetToLeft(Preset, groupName);
		}

		private void NextMenuItem_Click(object sender, RoutedEventArgs e) {
			parent.MovePresetToRight(Preset, groupName);
		}

		private void DeleteMenuItem_Click(object sender, RoutedEventArgs e) {
			parent.RemoveFromPresetGroup(Preset, groupName);
		}

		private void MainButton_Click(object sender, RoutedEventArgs e) {
			var isDefault = false;
			var found = AppSettings.Current?.ElementPresetsGroups.Find(i => i.Unchangable);
			if(found != null) {
				isDefault = found.Presets.Contains(Preset);
			}
			parent.AddElementByClick(Preset.TypeID, Preset.Size, Preset.PropertiesJson, isDefault);
		}

		private void Grid_MouseEnter(object sender, MouseEventArgs e) {
			IsMouseOn = true;
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e) {
			IsMouseOn = false;
		}
	}
}
