using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMap.Entities.Elements {
	public abstract class TextShape: Element, ITextGrid, IBorderBasedStyle {
		public TextBox MyTextBox { get; set; }
		public TextBlock MyTextBlock { get; set; }
		public string Text { get; set; }
		public FontFamily FontFamily { get; set; }
		public FontWeight FontWeight { get; set; }
		public double FontSize { get; set; }
		public Color FontColor { get; set; }
		public Brush Background { get; set; }
		public Brush BorderColor { get; set; }
		public Thickness BorderThickness { get; set; }

		public override FrameworkElement Target => _root;

		private readonly Grid _root;

		protected abstract class BaseProperty: IProperty {
			public abstract IProperty Translate(string json);

		}

		public TextShape(MindMapPage parent) : base(parent) {
			_root = new Grid();
		}

		public override Panel CreateElementProperties() {
			return new StackPanel();
		}

		public override void Deselect() {

		}

		public override void DoubleClick() {

		}

		public override void LeftClick() {

		}

		public override void MiddleClick() {

		}

		public override void RightClick() {

		}

		public override void SetFramework() {

		}

		public override void SetProperty(IProperty property) {

		}

		public void ShowTextBlock() {

		}

		public void ShowTextBox() {

		}

		public void UpdateText() {

		}

		protected override void UpdateStyle() {

		}
	}
}
