using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Properties;
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
using Xceed.Wpf.Toolkit;
using Newtonsoft.Json;

namespace MindMap.Entities.Elements {
	public class MyRectangle: Element, ITextGrid, IBorderBasedStyle {
		public override long TypeID => ID_Rectangle;
		public override string ID { get; protected set; }

		private readonly Border _root;
		private readonly Grid _container;
		private struct Property: IProperty {
			public string text;
			public Brush background;
			public Brush borderColor;
			public Thickness borderThickness;
			public FontFamily fontFamily;
			public FontWeight fontWeight;
			public double fontSize;
			public Color fontColor;
			public CornerRadius cornerRadius;
			public void Udpate() {

			}
			public IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json);
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

		public CornerRadius CornerRadius {
			get => property.cornerRadius;
			set {
				property.cornerRadius = value;
				UpdateStyle();
			}
		}

		public Style TextBlockStyle {
			get {
				Style style = new(typeof(TextBlock));
				style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(FontColor)));
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
				style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(FontColor)));
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
				style.Setters.Add(new Setter(Border.CornerRadiusProperty, CornerRadius));
				return style;
			}
		}


		public MyRectangle(MindMapPage parent) : base(parent) {
			ID = AssignID("Rectangle");
			property.text = "(Hello World)";
			property.background = Brushes.Gray;
			property.borderColor = Brushes.SkyBlue;
			property.borderThickness = new Thickness(2);
			property.fontFamily = new FontFamily("Microsoft YaHei UI");
			property.fontWeight = FontWeights.Regular;
			property.fontSize = 15;
			property.fontColor = Colors.Black;
			property.cornerRadius = new CornerRadius(0);

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

		public MyRectangle(MindMapPage parent, string id, string propertiesJson) : base(parent) {
			ID = id;
			property = (Property)property.Translate(propertiesJson);
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

		public override Panel CreateElementProperties() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle($"{ID}"));
			panel.Children.Add(PropertiesPanel.SliderInput("Cornder Radius", CornerRadius.TopLeft, 0, 100,
				value => CornerRadius = new CornerRadius(value)
			));
			return panel;
		}
	}
}
