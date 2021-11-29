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

namespace MindMap.Entities.Elements {
	public class MyRectangle2: TextShape {
		private class Property: BaseProperty {
			public CornerRadius cornerRadius = new(0);
			public override IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}

		}
		private Property property = new();
		public MyRectangle2(MindMapPage parent) : base(parent) {
			ID = AssignID("Rectangle");

		}

		public CornerRadius CornerRadius {
			get => property.cornerRadius;
			set {
				//submit edit history
				property.cornerRadius = value;
				UpdateStyle();
			}
		}

		public override long TypeID => ID_Rectangle;

		public override string ID { get; protected set; }

		protected override BaseProperty BaseProperties => property;

		public override Panel CreateElementProperties() {
			return new StackPanel();
		}

	}
}
