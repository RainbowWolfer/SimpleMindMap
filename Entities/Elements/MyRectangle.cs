using MindMap.Entities.Elements.Interfaces;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyRectangle: Element, ITextGrid, IBorderBasedStyle {
		private readonly Border _root;
		private readonly Grid _container;

		private string text;
		private Brush background;
		private Brush borderColor;
		private Thickness borderThickness;
		private FontFamily fontFamily;
		private FontWeight fontWeight;
		private double fontSize;

		public override FrameworkElement Target => _root;

		public TextBox MyTextBox { get; set; }
		public TextBlock MyTextBlock { get; set; }
		public string Text {
			get => text;
			set {
				text = value;
				UpdateText();
			}
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
				return style;
			}
		}

		public Style BorderStyle {
			get {
				Style style = new(typeof(Border));
				style.Setters.Add(new Setter(Border.BackgroundProperty, Background));
				style.Setters.Add(new Setter(Border.BorderBrushProperty, BorderColor));
				style.Setters.Add(new Setter(Border.BorderThicknessProperty, BorderThickness));
				return style;
			}
		}

		public MyRectangle(MindMapPage parent) : base(parent) {
			this.text = "(Hello World)";
			this.background = Brushes.Gray;
			this.borderColor = Brushes.SkyBlue;
			this.borderThickness = new Thickness(2);
			this.fontFamily = new FontFamily("Microsoft YaHei UI");
			this.fontWeight = FontWeights.Regular;
			this.fontSize = 15;

			_root = new Border() {
				Width = 250,
				Height = 250,
			};
			_root.SetValue(Canvas.TopProperty, 0.0);
			_root.SetValue(Canvas.LeftProperty, 0.0);

			_container = new Grid();
			_root.Child = _container;

			MyTextBox = new TextBox();
			MyTextBlock = new TextBlock();

			ShowTextBlock();

			MainCanvas.Children.Add(_root);

			UpdateStyle();
			UpdateText();
		}

		public override void UpdateStyle() {
			_root.Style = BorderStyle;
			MyTextBlock.Style = TextBlockStyle;
			MyTextBox.Style = TextBoxStyle;
		}

		public override void Deselect() {
			ShowTextBlock();
		}

		public override void DoubleClick() {
			ShowTextBox();
		}

		public override void LeftClick() {
			Debug.WriteLine($"Rectangle Left");
		}

		public override void MiddleClick() {
			Debug.WriteLine($"Rectangle Middle");
		}

		public override void RightClick() {
			_root.ContextMenu.IsOpen = true;
		}

		public void ShowTextBox() {
			if(_container.Children.Contains(MyTextBlock)) {
				_container.Children.Remove(MyTextBlock);
				Text = MyTextBlock.Text;
			}
			if(!_container.Children.Contains(MyTextBox)) {
				_container.Children.Add(MyTextBox);
			}
		}

		public void ShowTextBlock() {
			if(_container.Children.Contains(MyTextBox)) {
				_container.Children.Remove(MyTextBox);
				Text = MyTextBox.Text;
			}
			if(!_container.Children.Contains(MyTextBlock)) {
				_container.Children.Add(MyTextBlock);
			}
		}

		public void UpdateText() {
			MyTextBlock.Text = Text;
			MyTextBox.Text = Text;
		}
	}
}
