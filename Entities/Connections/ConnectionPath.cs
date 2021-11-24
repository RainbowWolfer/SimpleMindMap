using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Connections {
	public class ConnectionPath {
		//public Element Parent { get; private set; }
		public readonly ConnectionControl from;
		public ConnectionControl? to;
		private readonly Canvas _mainCanvas;

		private struct Property: IProperty {
			public double strokeThickess;
			public Color strokeColor;
			public IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json);
			}
		}
		private Property property;
		public IProperty Properties => property;

		public Path Path { get; private set; }
		private Path _backdgroundPath = new();
		private bool _isSelected;

		public Style PathStyle {
			get {
				Style style = new(typeof(Path));
				style.Setters.Add(new Setter(Path.StrokeThicknessProperty, StrokeThickess));
				style.Setters.Add(new Setter(Path.StrokeProperty, new SolidColorBrush(StrokeColor)));
				return style;
			}
		}

		public double StrokeThickess {
			get => property.strokeThickess;
			set {
				property.strokeThickess = value;
				UpdateStyle();
			}
		}

		public Color StrokeColor {
			get => property.strokeColor;
			set {
				property.strokeColor = value;
				UpdateStyle();
			}
		}

		public bool IsPreview { get; private set; }
		public ConnectionPath(Canvas mainCanvas, ConnectionControl from, ConnectionControl to) {
			this.IsPreview = false;
			this._mainCanvas = mainCanvas;
			this.from = from;
			this.to = to;
			this.Path = CreatePath(from.GetPosition(), to.GetPosition());
			Initialize();
		}

		public ConnectionPath(Canvas mainCanvas, ConnectionControl from, Vector2 to) {
			this.IsPreview = true;
			this._mainCanvas = mainCanvas;
			this.from = from;
			this.to = null;
			this.Path = CreatePath(from.GetPosition(), to);
			Initialize();
		}
		//public ConnectionPath(Canvas mainCanvas, string propertiesJson) {
		//	this.IsPreview = true;
		//	this._mainCanvas = mainCanvas;
		//	this.from = from;
		//	this.to = null;
		//	this.Path = CreatePath(from.GetPosition(), to);
		//	Initialize();
		//}

		private Vector2 lastMousePosition;
		private bool isRightClick;
		private bool isLeftClick;

		private void Initialize() {
			property.strokeThickess = 2;
			property.strokeColor = Colors.Black;
			_mainCanvas.Children.Add(this.Path);
			_backdgroundPath = new() {
				Data = Path.Data,
				Stroke = new SolidColorBrush(Invert(((SolidColorBrush)Path.Stroke).Color)),
				StrokeThickness = Path.StrokeThickness * 1.5,
				Visibility = Visibility.Collapsed,
			};
			_mainCanvas.Children.Insert(_mainCanvas.Children.IndexOf(Path), _backdgroundPath);
			if(!IsPreview) {
				FlyoutMenu.CreateBase(this.Path, (s, e) => ConnectionsManager.Remove(this));
				Path.MouseDown += (s, e) => {
					if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
						isRightClick = true;
						lastMousePosition = e.GetPosition(_mainCanvas);
					} else {
						isRightClick = false;
					}
					if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
						isLeftClick = true;
						lastMousePosition = e.GetPosition(_mainCanvas);
					} else {
						isLeftClick = false;
					}
				};
				Path.MouseUp += (s, e) => {
					if(e.GetPosition(_mainCanvas) == lastMousePosition) {
						if(isRightClick) {
							Path.ContextMenu.IsOpen = true;
						}
						if(isLeftClick) {
							Select();
							ConnectionsManager.ShowProperties(this);
						}
					}
				};
				Path.MouseEnter += (s, e) => MouseEnter();
				Path.MouseLeave += (s, e) => MouseExit();
			}
		}

		public void ClearFromCanvas() {
			if(_mainCanvas.Children.Contains(Path)) {
				_mainCanvas.Children.Remove(Path);
			}
		}

		public void MouseEnter() {
			if(_isSelected) {
				return;
			}
			_backdgroundPath.Visibility = Visibility.Visible;
			_backdgroundPath.StrokeDashArray = new DoubleCollection(new double[] { 5, 1 });
			UpdateBackgroundStyle();
		}

		public void MouseExit() {
			if(_isSelected) {
				return;
			}
			_backdgroundPath.Visibility = Visibility.Collapsed;
		}

		public void Select() {
			ConnectionsManager.CurrentSelection = this;
			_isSelected = true;
			_backdgroundPath.Visibility = Visibility.Visible;
			_backdgroundPath.StrokeDashArray = new DoubleCollection(new double[] { 1, 0 });
			UpdateBackgroundStyle();
		}

		public void Deselect() {
			ConnectionsManager.CurrentSelection = null;
			_isSelected = false;
			_backdgroundPath.Visibility = Visibility.Collapsed;
		}

		public static Color Invert(Color color, byte alpha = 255) {
			return Color.FromArgb(alpha, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
		}

		public Panel CreatePropertiesPanel() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle("Connection Path"));
			panel.Children.Add(PropertiesPanel.SliderInput("Strock Thickness", StrokeThickess, 0.5, 5,
				value => StrokeThickess = value
			, 0.1, 1));
			panel.Children.Add(PropertiesPanel.ColorInput("Strock Color", StrokeColor,
				value => StrokeColor = value
			));
			return panel;
		}

		public void UpdateStyle() {
			//Path.Style = PathStyle;
			Path.StrokeThickness = StrokeThickess;
			Path.Stroke = new SolidColorBrush(StrokeColor);
		}

		public void UpdateBackgroundStyle() {
			_backdgroundPath.Stroke = Path.Stroke;
			_backdgroundPath.StrokeThickness = Path.StrokeThickness;
		}

		public void Update(Vector2 to) {
			this.Path.Data = CreateGeometry(from.GetPosition(), to);
		}

		public void Update() {
			if(to == null) {
				throw new Exception("to is null");
			}
			this.Path.Data = CreateGeometry(from.GetPosition(), to.GetPosition());
		}

		public static Path CreatePath(Vector2 from, Vector2 to) {
			return new Path() {
				Data = CreateGeometry(from, to),
				Stroke = Brushes.Black,
				StrokeThickness = 2,
			};
		}
		public static Geometry CreateGeometry(Vector2 from, Vector2 to) {
			return Geometry.Parse(
						$"M {from.X},{from.Y} " +
						$"C {from.X + (to.X - from.X) * 0.5},{from.Y} " +
						$"{from.X + (to.X - from.X) * 0.5},{to.Y} " +
						$"{to.X},{to.Y}");
		}


	}
}
