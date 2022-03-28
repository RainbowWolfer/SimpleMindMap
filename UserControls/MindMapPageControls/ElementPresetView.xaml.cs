using MindMap.Entities.Elements;
using MindMap.Entities.Identifications;
using MindMap.Entities.Presets;
using MindMap.Pages;
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
		private bool edit;
		private readonly MindMapPage parent;

		public ElementPreset Preset { get; set; }

		public bool IsMouseOn {
			get => isMouseOn;
			set {
				isMouseOn = value;
				parent.Cursor = value ? Cursors.Hand : null;
			}
		}

		public bool Edit {
			get => edit;
			set {
				edit = value;
				if(value) {
					NameBox.Text = NameBlock.Text;
					NameBlock.Visibility = Visibility.Collapsed;
					NameBox.Visibility = Visibility.Visible;
					NameBox.Focus();
					NameBox.SelectionStart = NameBox.Text.Length;
				} else {
					if(string.IsNullOrWhiteSpace(NameBox.Text)) {
						NameBlock.Text = NameBox.Text;
						Preset.Name = NameBox.Text;
					}
					NameBlock.Visibility = Visibility.Visible;
					NameBox.Visibility = Visibility.Collapsed;
				}
			}
		}

		public ElementPresetView(MindMapPage parent, ElementPreset preset) {
			InitializeComponent();
			this.parent = parent;
			Preset = preset;
			NameBlock.Text = preset.Name;
			NameBox.Text = preset.Name;
			Edit = false;
			DrawElement();
		}

		private void DrawElement() {
			Identity identity = new("preset_id", "preset_name");
			Element element = ElementGenerator.GetElement(parent, Preset.TypeID, identity);
			element.SetFramework();
			element.SetProperty(Preset.PropertiesJson);
			PresetElementDisplayGrid.Children.Clear();
			PresetElementDisplayGrid.Children.Add(element.Target);
		}

		private void RenameMenuItem_Click(object sender, RoutedEventArgs e) {

		}

		private void TopMenuItem_Click(object sender, RoutedEventArgs e) {

		}

		private void PreviousMenuItem_Click(object sender, RoutedEventArgs e) {

		}

		private void NextMenuItem_Click(object sender, RoutedEventArgs e) {

		}

		private void DeleteMenuItem_Click(object sender, RoutedEventArgs e) {

		}

		private void MainButton_Click(object sender, RoutedEventArgs e) {

		}

		private void Grid_MouseEnter(object sender, MouseEventArgs e) {
			IsMouseOn = true;
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e) {
			IsMouseOn = false;
		}

		private void Grid_LostMouseCapture(object sender, MouseEventArgs e) {
			Edit = false;
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			Edit = true;
		}

		private void NameBox_PreviewKeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Escape || e.Key == Key.Enter) {
				Edit = false;
			}
		}

		private void NameBox_LostFocus(object sender, RoutedEventArgs e) {
			Edit = false;
		}
	}
}
