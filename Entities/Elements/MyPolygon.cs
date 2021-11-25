using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Properties;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyPolygon: Element, ITextGrid, IBorderBasedStyle, IUpdate {
		public override long TypeID => ID_Polygon;

		public override string ID { get; protected set; }

		public override FrameworkElement Target => _root;
		private struct Property: IProperty {
			public Brush background;
			public Brush borderColor;
			public Thickness borderThickness;
			public string text;
			public FontFamily fontFamily;
			public FontWeight fontWeight;
			public double fontSize;
			public Color fontColor;
			public int pointCount;
			public IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json);
			}
		}
		private Property property = new();

		public override IProperty Properties => property;

		public TextBox MyTextBox { get; set; }
		public TextBlock MyTextBlock { get; set; }

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
		public int PointCount {
			get => property.pointCount;
			set {
				property.pointCount = value;
				Update();
			}
		}


		private readonly Grid _root;
		private readonly Polygon _polygon;
		public MyPolygon(MindMapPage parent) : base(parent) {
			ID = AssignID("Polygon");

			property.background = new SolidColorBrush(Colors.Gray);
			property.borderColor = new SolidColorBrush(Colors.Azure);
			property.borderThickness = new Thickness(2);
			property.text = "(Hello World)";
			property.fontFamily = new FontFamily("Microsoft YaHei UI");
			property.fontWeight = FontWeights.Regular;
			property.fontSize = 15;
			property.fontColor = Colors.Black;
			property.pointCount = 6;

			_root = new Grid() {
				Height = 250,
				Width = 250,
			};
			_root.SetValue(Canvas.TopProperty, 0.0);
			_root.SetValue(Canvas.LeftProperty, 0.0);
			_polygon = new Polygon();
			_root.Children.Add(_polygon);
			MyTextBlock = new TextBlock();
			MyTextBox = new TextBox();
			MainCanvas.Children.Add(_root);
			ShowTextBlock();
			UpdateStyle();
			UpdateText();
			DrawPolygon(PointCount);
		}

		public MyPolygon(MindMapPage parent, string id, string propertiesJson) : base(parent) {
			ID = id;
			property = (Property)property.Translate(propertiesJson);

			_root = new Grid() {
				Height = 250,
				Width = 250,
			};
			_root.SetValue(Canvas.TopProperty, 0.0);
			_root.SetValue(Canvas.LeftProperty, 0.0);
			_polygon = new Polygon();
			_root.Children.Add(_polygon);
			MyTextBlock = new TextBlock();
			MyTextBox = new TextBox();
			MainCanvas.Children.Add(_root);
			ShowTextBlock();
			UpdateStyle();
			UpdateText();
			DrawPolygon(PointCount);
		}

		public void DrawPolygon(int pointCount) {
			Point[] points = new Point[pointCount + 1];
			double anglePerGon = Math.PI * 2 / pointCount;
			double radiusX = _root.Width / 2;
			double radiusY = _root.Height / 2;
			Point point = new(radiusX, 0);
			for(int i = 0; i < points.Length; i++) {
				points[i] = new Point(point.X, point.Y);
				point.X = radiusX * Math.Cos(i * anglePerGon);
				point.Y = radiusY * Math.Sin(i * anglePerGon);
			}
			for(int i = 0; i < points.Length; i++) {
				points[i].X += radiusX;
				points[i].Y += radiusY;
			}
			_polygon.Points = new PointCollection(points);
		}

		public override Panel CreateElementProperties() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle($"{ID}"));
			panel.Children.Add(PropertiesPanel.SliderInput("Polygon Points", PointCount, 3, 20,
				value => PointCount = (int)value
			, 1, 0));
			return panel;
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

		protected override void UpdateStyle() {
			_polygon.Fill = property.background;
			_polygon.Stroke = property.borderColor;
			_polygon.StrokeThickness = property.borderThickness.Top;

			MyTextBlock.FontFamily = property.fontFamily;
			MyTextBlock.FontSize = property.fontSize;
			MyTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
			MyTextBlock.VerticalAlignment = VerticalAlignment.Center;
			MyTextBlock.FontWeight = property.fontWeight;
			MyTextBlock.Foreground = new SolidColorBrush(property.fontColor);
			MyTextBlock.TextWrapping = TextWrapping.Wrap;
			MyTextBlock.TextAlignment = TextAlignment.Center;
			MyTextBlock.Padding = new Thickness(10);

			MyTextBox.FontFamily = property.fontFamily;
			MyTextBox.FontSize = property.fontSize;
			MyTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
			MyTextBox.VerticalAlignment = VerticalAlignment.Stretch;
			MyTextBox.FontWeight = property.fontWeight;
			MyTextBox.Foreground = new SolidColorBrush(property.fontColor);
			MyTextBox.TextWrapping = TextWrapping.Wrap;
			MyTextBox.TextAlignment = TextAlignment.Center;
			MyTextBox.Padding = new Thickness(10);
			MyTextBox.AcceptsReturn = true;
			MyTextBox.AcceptsTab = true;
		}

		public void UpdateText() {
			MyTextBlock.Text = Text;
			MyTextBox.Text = Text;
		}

		public void Update() {
			DrawPolygon(PointCount);
		}
	}
}
