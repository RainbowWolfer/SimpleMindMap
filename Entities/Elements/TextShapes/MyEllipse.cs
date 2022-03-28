using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements.TextShapes {
	public class MyEllipse: TextShape {
		public override long TypeID => ID_Ellipse;
		public override string ElementTypeName => "Ellipse";
		public override (string icon, string fontFamily) Icon => ("\uE91F", "Segoe Fluent Icons");
		private class Property: TextShapeProperty {

			public override object Clone() {
				return MemberwiseClone();
			}

			public override IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}
		}
		private Property property = new();
		protected override TextShapeProperty TextShapeProperties => property;
		protected override TextRelatedProperty TextRelatedProperties => property;


		private readonly Ellipse _ellipse = new();
		public override FrameworkElement Shape => _ellipse;

		public MyEllipse(MindMapPage parent, Identity? identity = null, string? propertyJson = null) : base(parent, identity) {
			if(!string.IsNullOrWhiteSpace(propertyJson)) {
				SetProperty(propertyJson);
			}
		}

		public override void SetFramework() {
			base.SetFramework();
			root.Children.Insert(0, _ellipse);
		}
		public override Panel CreateElementProperties() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle(Identity.Name, newName => Identity.Name = newName));
			return panel;
		}

		public override void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
		}

		public override void SetProperty(string propertyJson) {
			this.property = (Property)property.Translate(propertyJson);
			UpdateStyle();
		}

		protected override void UpdateStyle() {
			base.UpdateStyle();

		}

	}
}
