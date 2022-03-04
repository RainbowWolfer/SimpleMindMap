using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MindMap.Entities.Elements {
	public abstract class TextRelated: Element, ITextContainer {

		protected abstract class TextRelatedProperty: IProperty {
			public string text = "(Hello World)";
			public FontFamily fontFamily = new("Microsoft YaHei UI");
			public FontWeight fontWeight = FontWeights.Normal;
			public double fontSize = 14;
			public Color fontColor = Colors.Black;

			public abstract object Clone();
			public abstract IProperty Translate(string json);
		}

		protected abstract TextRelatedProperty TextRelatedProperties { get; }

		public string Text {
			get => TextRelatedProperties.text;
			set {
				TextRelatedProperties.text = value;
				UpdateStyle();
			}
		}
		public FontFamily FontFamily {
			get => TextRelatedProperties.fontFamily;
			set {
				TextRelatedProperties.fontFamily = value;
				UpdateStyle();
			}
		}
		public FontWeight FontWeight {
			get => TextRelatedProperties.fontWeight;
			set {
				TextRelatedProperties.fontWeight = value;
				UpdateStyle();
			}
		}
		public double FontSize {
			get => TextRelatedProperties.fontSize;
			set {
				TextRelatedProperties.fontSize = value;
				UpdateStyle();
			}
		}
		public Color FontColor {
			get => TextRelatedProperties.fontColor;
			set {
				TextRelatedProperties.fontColor = value;
				UpdateStyle();
			}
		}

		protected TextRelated(MindMapPage parent, Identity? identity = null) : base(parent, identity) {

		}

	}
}
