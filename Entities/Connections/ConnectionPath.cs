using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
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

		public Path Path { get; private set; }

		public Style PathStyle {
			get {
				Style style = new(typeof(Path));
				style.Setters.Add(new Setter(Shape.StrokeThicknessProperty, 2.0));
				return style;
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

		private Vector2 lastMousePosition;
		private bool isRightClick;
		private void Initialize() {
			_mainCanvas.Children.Add(this.Path);
			if(!IsPreview) {
				FlyoutMenu.CreateBase(this.Path, (s, e) => ConnectionsManager.Remove(this));
				Path.MouseDown += (s, e) => {
					if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
						isRightClick = true;
						lastMousePosition = e.GetPosition(_mainCanvas);
					} else {
						isRightClick = false;
					}
				};
				Path.MouseUp += (s, e) => {
					if(isRightClick && e.GetPosition(_mainCanvas) == lastMousePosition) {
						Path.ContextMenu.IsOpen = true;
					}
				};
				Path.MouseEnter += (s, e) => Path.Stroke = Brushes.Gray;
				Path.MouseLeave += (s, e) => Path.Stroke = Brushes.Black;
			}
		}

		public void ClearFromCanvas() {
			if(_mainCanvas.Children.Contains(Path)) {
				_mainCanvas.Children.Remove(Path);
			}
		}

		public Panel CreatePropertiesPanel() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle("Connection Path"));
			//panel.Children.Add(PropertiesPanel.SliderInput("Strock Thickness"));
			return panel;
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
