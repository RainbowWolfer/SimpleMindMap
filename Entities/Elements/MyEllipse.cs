using MindMap.Entities.Elements.Interfaces;
using MindMap.Pages;
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

namespace MindMap.Entities.Elements {
	public class MyEllipse: Element, ITextGrid, IBorderBasedStyle {
		private readonly Grid _root;
		private readonly Ellipse _ellipse;

		private string text;
		private FontFamily fontFamily;
		private FontWeight fontWeight;
		private double fontSize;
		private Brush background;
		private Brush borderColor;
		private Thickness borderThickness;

		public override FrameworkElement Target => _root;

		public TextBox MyTextBox { get; set; }
		public TextBlock MyTextBlock { get; set; }
		public string Text {
			get => text;
			set => text = value;
		}
		public FontFamily FontFamily {
			get => fontFamily;
			set => fontFamily = value;
		}
		public FontWeight FontWeight {
			get => fontWeight;
			set => fontWeight = value;
		}
		public double FontSize {
			get => fontSize;
			set => fontSize = value;
		}
		public Brush Background {
			get => background;
			set => background = value;
		}
		public Brush BorderColor {
			get => borderColor;
			set => borderColor = value;
		}
		public Thickness BorderThickness {
			get => borderThickness;
			set => borderThickness = value;
		}

		public Style TextBlockStyle {
			get {
				Style style = new(typeof(TextBlock));
				style.Setters.Add(new Setter(TextBlock.FontFamilyProperty, FontFamily));
				style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeight));
				style.Setters.Add(new Setter(TextBlock.FontSizeProperty, FontSize));
				style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(10)));
				style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
				style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
				style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center));
				style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
				return style;
			}
		}

		public Style TextBoxStyle {
			get {
				Style style = new(typeof(TextBox));
				style.Setters.Add(new Setter(Control.FontFamilyProperty, FontFamily));
				style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeight));
				style.Setters.Add(new Setter(Control.FontSizeProperty, FontSize));
				style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10)));
				style.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
				style.Setters.Add(new Setter(TextBox.TextAlignmentProperty, TextAlignment.Center));
				style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
				style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch));
				style.Setters.Add(new Setter(TextBoxBase.AcceptsReturnProperty, true));
				style.Setters.Add(new Setter(TextBoxBase.AcceptsTabProperty, true));
				//style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(20)));
				return style;
			}
		}

		public Style EllipseStyle {
			get {
				Style style = new(typeof(Ellipse));
				style.Setters.Add(new Setter(Shape.FillProperty, Background));
				style.Setters.Add(new Setter(Shape.StrokeProperty, BorderColor));
				return style;
			}
		}

		public MyEllipse(MindMapPage parent) : base(parent) {
			this.text = "(Hello World)";
			this.background = Brushes.Gray;
			this.borderColor = Brushes.SkyBlue;
			this.borderThickness = new Thickness(2);
			this.fontFamily = new FontFamily("Microsoft YaHei UI");
			this.fontWeight = FontWeights.Regular;
			this.fontSize = 15;

			_root = new Grid() {
				Width = 250,
				Height = 250,
			};
			_root.SetValue(Canvas.TopProperty, 0.0);
			_root.SetValue(Canvas.LeftProperty, 0.0);

			_ellipse = new Ellipse();
			MyTextBlock = new TextBlock();
			MyTextBox = new TextBox();

			_root.Children.Add(_ellipse);
			ShowTextBlock();

			MainCanvas.Children.Add(_root);

			UpdateStyle();
			UpdateText();

		}


		public override void Deselect() {
			ShowTextBlock();
		}

		public override void DoubleClick() {
			ShowTextBox();
		}

		public override void LeftClick() {

		}

		public override void MiddleClick() {

		}

		public override void RightClick() {
			_root.ContextMenu.IsOpen = true;
		}

		public override void UpdateStyle() {
			MyTextBlock.Style = TextBlockStyle;
			MyTextBox.Style = TextBoxStyle;
			_ellipse.Style = EllipseStyle;
		}

		public void UpdateText() {
			MyTextBlock.Text = Text;
			MyTextBox.Text = Text;
		}

		public void ShowTextBox() {
			if(_root.Children.Contains(MyTextBlock)) {
				_root.Children.Remove(MyTextBlock);
				Text = MyTextBlock.Text;
			}
			if(!_root.Children.Contains(MyTextBox)) {
				_root.Children.Add(MyTextBox);
			}
		}

		public void ShowTextBlock() {
			if(_root.Children.Contains(MyTextBox)) {
				_root.Children.Remove(MyTextBox);
				Text = MyTextBox.Text;
			}
			if(!_root.Children.Contains(MyTextBlock)) {
				_root.Children.Add(MyTextBlock);
			}
		}

		public override List<Panel> CreatePropertiesList() {
			throw new NotImplementedException();
		}
	}
}
