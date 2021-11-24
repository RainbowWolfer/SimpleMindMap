using MindMap.Entities.Elements.Interfaces;
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
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyEllipse: Element, ITextGrid, IBorderBasedStyle {
		public override string ID { get; protected set; }
		public override long TypeID => ID_Ellipse;
		private readonly Grid _root;
		private readonly Ellipse _ellipse;

		private struct Property: IProperty {
			public string text;
			public FontFamily fontFamily;
			public FontWeight fontWeight;
			public double fontSize;
			public Brush background;
			public Brush borderColor;
			public Thickness borderThickness;
			public Color fontColor;
			public IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json);
			}

			public void Udpate() {

			}
		}
		private Property property = new();
		public override IProperty Properties => property;

		public override FrameworkElement Target => _root;

		public TextBox MyTextBox { get; set; }
		public TextBlock MyTextBlock { get; set; }
		public string Text {
			get => property.text;
			set {
				property.text = value;
				UpdateText();
			}
		}
		public FontFamily FontFamily {
			get => property.fontFamily;
			set {
				property.fontFamily = value;
				UpdateStyle();
			}
		}
		public FontWeight FontWeight {
			get => property.fontWeight;
			set {
				property.fontWeight = value;
				UpdateStyle();
			}
		}
		public double FontSize {
			get => property.fontSize;
			set {
				property.fontSize = value;
				UpdateStyle();
			}
		}
		public Color FontColor {
			get => property.fontColor;
			set {
				property.fontColor = value;
				UpdateStyle();
			}
		}
		public Brush Background {
			get => property.background;
			set {
				property.background = value;
				UpdateStyle();
			}
		}
		public Brush BorderColor {
			get => property.borderColor;
			set {
				property.borderColor = value;
				UpdateStyle();
			}
		}
		public Thickness BorderThickness {
			get => property.borderThickness;
			set {
				property.borderThickness = value;
				UpdateStyle();
			}
		}

		public Style TextBlockStyle {
			get {
				Style style = new(typeof(TextBlock));
				style.Setters.Add(new Setter(TextBlock.FontFamilyProperty, FontFamily));
				style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeight));
				style.Setters.Add(new Setter(TextBlock.FontSizeProperty, FontSize));
				style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(10)));
				style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FontColor)));
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
				style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FontColor)));
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
				style.Setters.Add(new Setter(Shape.StrokeThicknessProperty, BorderThickness.Top));
				return style;
			}
		}

		public MyEllipse(MindMapPage parent) : base(parent) {
			ID = AssignID("Ellipes");
			property.text = "(Hello World)";
			property.background = Brushes.Gray;
			property.borderColor = Brushes.SkyBlue;
			property.borderThickness = new Thickness(2);
			property.fontFamily = new FontFamily("Microsoft YaHei UI");
			property.fontWeight = FontWeights.Regular;
			property.fontSize = 15;
			property.fontColor = Colors.Black;

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
		public MyEllipse(MindMapPage parent, string id, string propertiesJson) : base(parent) {
			ID = id;
			property = (Property)property.Translate(propertiesJson);
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

		public override Panel CreateElementProperties() {
			return new StackPanel();
		}
	}
}
