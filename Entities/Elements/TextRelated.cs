using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Identifications;
using MindMap.Entities.Locals;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MindMap.Entities.Elements {
	public abstract class TextRelated: ShadowBasedElement, ITextContainer, ITextShadow {
		protected abstract class TextRelatedProperty: ElementBaseProperty {
			public string text = AppSettings.Current?.ElementDefaultText ?? "( Hello World )";
			public FontFamily fontFamily = new("Microsoft YaHei UI");
			public FontWeight fontWeight = FontWeights.Normal;
			public double fontSize = 14;
			public Color fontColor = Colors.Black;
			public DropShadowEffect textShadowEffect = new();
			public bool enableTextShadow = true;
			public double textShadowBlurRadius = 4;
			public double textShadowDepth = 4;
			public double textShadowDirection = 315;
			public Color textShadowColor = Colors.Black;
			public double textShadowOpacity = 0.3;
		}

		protected abstract TextRelatedProperty TextRelatedProperties { get; }
		protected override ElementBaseProperty ElementBaseProperties => TextRelatedProperties;

		public DropShadowEffect TextShadowEffect {
			get => TextRelatedProperties.textShadowEffect;
			set {
				TextRelatedProperties.textShadowEffect = value;
				UpdateStyle();
			}
		}
		public bool EnableTextShadow {
			get => TextRelatedProperties.enableTextShadow;
			set {
				TextRelatedProperties.enableTextShadow = value;
				UpdateStyle();
			}
		}
		public double TextShadowBlurRadius {
			get => TextRelatedProperties.textShadowBlurRadius;
			set {
				TextRelatedProperties.textShadowBlurRadius = value;
				UpdateStyle();
			}
		}
		public double TextShadowDepth {
			get => TextRelatedProperties.textShadowDepth;
			set {
				TextRelatedProperties.textShadowDepth = value;
				UpdateStyle();
			}
		}
		public double TextShadowDirection {
			get => TextRelatedProperties.textShadowDirection;
			set {
				TextRelatedProperties.textShadowDirection = value;
				UpdateStyle();
			}
		}
		public Color TextShadowColor {
			get => TextRelatedProperties.textShadowColor;
			set {
				TextRelatedProperties.textShadowColor = value;
				UpdateStyle();
			}
		}
		public double TextShadowOpacity {
			get => TextRelatedProperties.textShadowOpacity;
			set {
				TextRelatedProperties.textShadowOpacity = value;
				UpdateStyle();
			}
		}
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

		protected override void UpdateStyle() {
			base.UpdateStyle();
		}
	}
}
