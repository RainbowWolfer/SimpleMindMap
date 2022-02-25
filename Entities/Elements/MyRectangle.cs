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
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyRectangle: TextShape {
		public override long TypeID => ID_Rectangle;
		public override string ElementTypeName => "Rectangle";
		private class Property: BaseProperty {
			public CornerRadius cornerRadius = new(0);

			public override object Clone() {
				return MemberwiseClone();
			}

			public override IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}

		}
		private Property property = new();
		public CornerRadius CornerRadius {
			get => property.cornerRadius;
			set {
				property.cornerRadius = value;
				UpdateStyle();
			}
		}

		protected override BaseProperty BaseProperties => property;


		private readonly Border _rect = new();

		public MyRectangle(MindMapPage parent, Identity? identity = null, string? propertyJson = null) : base(parent, identity) {
			if(!string.IsNullOrWhiteSpace(propertyJson)) {
				SetProperty(propertyJson);
			}
		}

		public override void SetFramework() {
			base.SetFramework();
			root.Children.Insert(0, _rect);//must be before textbox and textblock
		}

		public override Panel CreateElementProperties() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle(Identity.Name));
			panel.Children.Add(PropertiesPanel.SliderInput("Cornder Radius", CornerRadius.TopLeft, 0, 100,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					CornerRadius = new CornerRadius(args.NewValue);
				}, (oldP, newP) => {
					parent.editHistory.SubmitByElementPropertyDelayedChanged(this, oldP, newP, "Cornder Radius");
				})
			));
			return panel;
		}

		public override void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
		}
		public override void SetProperty(string propertyJson) {
			property = (Property)property.Translate(propertyJson);
			UpdateStyle();
		}

		protected override void UpdateStyle() {
			base.UpdateStyle();
			_rect.Background = Background;
			_rect.BorderBrush = BorderColor;
			_rect.BorderThickness = BorderThickness;
			_rect.CornerRadius = CornerRadius;
		}
	}
}
