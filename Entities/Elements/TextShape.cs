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
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public abstract class TextShape: Element, ITextGrid, IBorderBasedStyle {
		public TextBox MyTextBox { get; set; } = new();
		public TextBlock MyTextBlock { get; set; } = new();
		public abstract Shape MyShape { get; }

		public string Text {
			get => BaseProperties.text;
			set {
				BaseProperties.text = value;
			}
		}
		public FontFamily FontFamily {
			get => BaseProperties.fontFamily;
			set {
				BaseProperties.fontFamily = value;
			}
		}
		public FontWeight FontWeight {
			get => BaseProperties.fontWeight;
			set {
				BaseProperties.fontWeight = value;
			}
		}
		public double FontSize {
			get => BaseProperties.fontSize;
			set {
				BaseProperties.fontSize = value;
			}
		}
		public Color FontColor {
			get => BaseProperties.fontColor;
			set {
				BaseProperties.fontColor = value;
			}
		}
		public Brush Background {
			get => BaseProperties.background;
			set {
				BaseProperties.background = value;
			}
		}
		public Brush BorderColor {
			get => BaseProperties.borderColor;
			set {
				BaseProperties.borderColor = value;
			}
		}
		public Thickness BorderThickness {
			get => BaseProperties.borderThickness;
			set {
				BaseProperties.borderThickness = value;
			}
		}

		private readonly Grid _root = new();
		public override FrameworkElement Target => _root;

		protected abstract class BaseProperty: IProperty {
			public string text = "(Hello World)";
			public FontFamily fontFamily = new("Microsoft YaHei UI");
			public FontWeight fontWeight = FontWeights.Normal;
			public double fontSize = 14;
			public Color fontColor = Colors.Black;
			public Brush background = Brushes.Gray;
			public Brush borderColor = Brushes.Aquamarine;
			public Thickness borderThickness = new(10);
			public abstract IProperty Translate(string json);
		}

		protected abstract BaseProperty BaseProperties { get; }
		public override IProperty Properties => BaseProperties;

		public TextShape(MindMapPage parent) : base(parent) {

		}

		public abstract override Panel CreateElementProperties();

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

		public void ShowTextBlock() {

		}

		public void ShowTextBox() {

		}

		protected override void UpdateStyle() {
			MyTextBlock.Text = Text;
			MyTextBlock.Foreground = new SolidColorBrush(FontColor);
			MyTextBlock.FontFamily = FontFamily;
			MyTextBlock.FontWeight = FontWeight;
			MyTextBlock.FontSize = FontSize;
			MyTextBlock.Padding = new Thickness(10);
			MyTextBlock.TextWrapping = TextWrapping.Wrap;
			MyTextBlock.TextAlignment = TextAlignment.Center;
			MyTextBlock.VerticalAlignment = VerticalAlignment.Center;
			MyTextBlock.HorizontalAlignment = HorizontalAlignment.Center;

			MyTextBox.Text = Text;
			MyTextBox.Foreground = new SolidColorBrush(FontColor);
			MyTextBox.FontFamily = FontFamily;
			MyTextBox.FontWeight = FontWeight;
			MyTextBox.FontSize = FontSize;
			MyTextBox.Padding = new Thickness(10);
			MyTextBox.TextWrapping = TextWrapping.Wrap;
			MyTextBox.TextAlignment = TextAlignment.Center;
			MyTextBox.VerticalAlignment = VerticalAlignment.Stretch;
			MyTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
			MyTextBox.AcceptsReturn = true;
			MyTextBox.AcceptsTab = true;

			//MyShape.
		}
	}
}
