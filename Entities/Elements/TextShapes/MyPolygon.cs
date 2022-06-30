using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements.TextShapes {
	public class MyPolygon: TextShape, IUpdate {
		public override long TypeID => ElementGenerator.ID_Polygon;
		public override string ElementTypeName => "Polygon";
		public override (string icon, string fontFamily) Icon => ("\u2B22", "Segoe UI Symbol");
		private class Property: TextShapeProperty {
			public int pointCount = 6;

			public override object Clone() {
				return MemberwiseClone();
			}

			public override IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}
		}
		private Property property = new();

		public int PointCount {
			get => property.pointCount;
			set {
				property.pointCount = value;
				Update();
			}
		}

		protected override TextShapeProperty TextShapeProperties => property;
		protected override TextRelatedProperty TextRelatedProperties => property;


		private readonly Grid _root = new();
		private readonly Polygon _polygon = new();
		public override FrameworkElement Shape => _polygon;

		public MyPolygon(MindMapPage? parent, Identity? identity = null, string? propertyJson = null) : base(parent, identity) {
			if(!string.IsNullOrWhiteSpace(propertyJson)) {
				SetProperty(propertyJson);
			}
		}

		public override void SetFramework() {
			base.SetFramework();
			root.Children.Insert(0, _polygon);
			Update();
		}

		public void DrawPolygon(int pointCount) {
			//Debug.WriteLine("Start");
			Point[] points = new Point[pointCount + 1];
			double anglePerGon = Math.PI * 2 / pointCount;
			Vector2 size = GetSize();
			double radiusX = size.X / 2;
			double radiusY = size.Y / 2;
			Point point = new(radiusX, 0);
			for(int i = 0; i < points.Length; i++) {
				points[i] = new Point(point.X, point.Y);
				point.X = radiusX * Math.Cos(i * anglePerGon);
				point.Y = radiusY * Math.Sin(i * anglePerGon);
			}
			double dY = points.Max(p => p.Y) - points.Min(p => p.Y);
			double dX = points.Max(p => p.X) - points.Min(p => p.X);
			double deltaY = (size.Y - dY) / 2;
			for(int i = 0; i < points.Length; i++) {
				if((int)points[i].Y < 0) {
					points[i].Y -= deltaY;
				} else if((int)points[i].Y > 0) {
					points[i].Y += deltaY;
				}
				points[i].X = (points[i].X - dX / 2) * size.X / dX + dX / 2;// not working correctly
			}

			for(int i = 0; i < points.Length; i++) {
				points[i].X += radiusX;
				points[i].Y += radiusY;
			}
			_polygon.Points = new PointCollection(points);
		}

		public override Panel CreateElementProperties() {
			if(parent == null) {
				throw BeyondLimitException;
			}
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle($"{Identity.Name}", newName => Identity.Name = newName));
			panel.Children.Add(PropertiesPanel.SliderInput("Polygon Points", PointCount, 3, 20,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					PointCount = (int)args.NewValue;
				}, (oldP, newP) => {
					parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Polygon Points");
				})
			, 1, 0));
			return panel;
		}

		public override void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
			Update();
		}

		public override void SetProperty(string propertyJson) {
			this.property = (Property)Properties.Translate(propertyJson);
			UpdateStyle();
			Update();
		}

		protected override void UpdateStyle() {
			base.UpdateStyle();
		}

		public void Update() {
			DrawPolygon(PointCount);
		}

		public static string GetDefaultPropertyJson() {
			return JsonConvert.SerializeObject(new Property(), Formatting.Indented);
		}
	}
}
