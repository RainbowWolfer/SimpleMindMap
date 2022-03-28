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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MindMap.Entities.Elements {
	public abstract class ShadowBasedElement: Element, IGridShadow {
		protected abstract class ElementBaseProperty: IProperty {
			public bool enableShadow = true;
			public DropShadowEffect shadowEffect = new();
			public double shadowBlurRadius = 4;
			public double shadowShadowDepth = 4;
			public double shadowDirection = 315;
			public Color shadowColor = Colors.Black;
			public double shadowOpacity = 0.5;

			public abstract object Clone();
			public abstract IProperty Translate(string json);
		}

		protected abstract ElementBaseProperty ElementBaseProperties { get; }

		public DropShadowEffect ShadowEffect {
			get => ElementBaseProperties.shadowEffect;
			set {
				ElementBaseProperties.shadowEffect = value;
				UpdateStyle();
			}
		}
		public bool EnableShadow {
			get => ElementBaseProperties.enableShadow;
			set {
				ElementBaseProperties.enableShadow = value;
				UpdateStyle();
			}
		}
		public double ShadowBlurRadius {
			get => ElementBaseProperties.shadowBlurRadius;
			set {
				ElementBaseProperties.shadowBlurRadius = value;
				UpdateStyle();
			}
		}
		public double ShadowDepth {
			get => ElementBaseProperties.shadowShadowDepth;
			set {
				ElementBaseProperties.shadowShadowDepth = value;
				UpdateStyle();
			}
		}
		public double ShadowDirection {
			get => ElementBaseProperties.shadowDirection;
			set {
				ElementBaseProperties.shadowDirection = value;
				UpdateStyle();
			}
		}
		public Color ShadowColor {
			get => ElementBaseProperties.shadowColor;
			set {
				ElementBaseProperties.shadowColor = value;
				UpdateStyle();
			}
		}
		public double ShadowOpacity {
			get => ElementBaseProperties.shadowOpacity;
			set {
				ElementBaseProperties.shadowOpacity = value;
				UpdateStyle();
			}
		}

		public ShadowBasedElement(MindMapPage parent, Identity? identity = null) : base(parent, identity) {
			//Target.Effect = ShadowEffect;
		}

		protected override void UpdateStyle() {
			ShadowEffect.BlurRadius = ShadowBlurRadius;
			ShadowEffect.ShadowDepth = ShadowDepth;
			ShadowEffect.Direction = ShadowDirection;
			ShadowEffect.Color = ShadowColor;
			ShadowEffect.Opacity = EnableShadow ? ShadowOpacity : 0;
			Target.Effect = ShadowEffect;
		}

	}
}
