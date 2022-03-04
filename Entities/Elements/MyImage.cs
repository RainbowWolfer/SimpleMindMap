using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
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
using System.Windows.Media;

namespace MindMap.Entities.Elements {
	public class MyImage: TextRelated, IAnnotation, IBorderBasedStyle {
		public override long TypeID => Element.ID_Image;

		public override string ElementTypeName => "Image";

		public override FrameworkElement Target => throw new NotImplementedException();

		protected class Property: TextRelatedProperty {
			public Brush background = Brushes.Gray;
			public Brush borderColor = Brushes.Aquamarine;
			public Thickness borderThickness = new(2);

			public override object Clone() {
				return MemberwiseClone();
			}

			public override IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}
		}

		public override IProperty Properties => throw new NotImplementedException();
		protected override TextRelatedProperty TextRelatedProperties => throw new NotImplementedException();

		public TextBlock AnnotationTextBlock { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public string Text { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Direction Direction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Brush Background { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Brush BorderColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Thickness BorderThickness { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public FontFamily FontFamily { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public FontWeight FontWeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public double FontSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Color FontColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


		public MyImage(MindMapPage parent, Identity? identity = null) : base(parent, identity) {

		}


		public override Panel CreateElementProperties() {
			throw new NotImplementedException();
		}

		public override void Deselect() {
			throw new NotImplementedException();
		}

		public override void DoubleClick() {
			throw new NotImplementedException();
		}

		public override void LeftClick() {
			throw new NotImplementedException();
		}

		public override void MiddleClick() {
			throw new NotImplementedException();
		}

		public override void RightClick() {
			throw new NotImplementedException();
		}

		public override void SetFramework() {
			throw new NotImplementedException();
		}

		public override void SetProperty(IProperty property) {
			throw new NotImplementedException();
		}

		public override void SetProperty(string propertyJson) {
			throw new NotImplementedException();
		}

		protected override void UpdateStyle() {
			throw new NotImplementedException();
		}
	}
}
