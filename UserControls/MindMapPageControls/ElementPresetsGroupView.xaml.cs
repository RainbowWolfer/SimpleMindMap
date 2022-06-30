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
	public partial class ElementPresetsGroupView: UserControl {
		private ElementPresetsGroup group;

		public ElementPresetsGroup Group {
			get => group;
			set {
				group = value;
				PresetsPanel.Children.Clear();
				foreach(ElementPreset item in value.Presets) {
					var child = new ElementPresetView(parent, item);
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
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e) {

		}
	}
}
