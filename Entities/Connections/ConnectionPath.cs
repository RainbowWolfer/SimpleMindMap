using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Connections {
	public class ConnectionPath {
		//public Element Parent { get; private set; }
		public readonly ConnectionControl from;
		public ConnectionControl? to;
		private readonly Canvas _mainCanvas;

		public Path Path { get; private set; }

		public bool IsPreview { get; private set; }
		public ConnectionPath(Canvas mainCanvas, ConnectionControl from, ConnectionControl to) {
			this.IsPreview = false;
			this._mainCanvas = mainCanvas;
			this.from = from;
			this.to = to;
			this.Path = CreatePath(from.GetPosition(), to.GetPosition());
			mainCanvas.Children.Add(Path);
		}

		public ConnectionPath(Canvas mainCanvas, ConnectionControl from, Vector2 to) {
			this.IsPreview = true;
			this._mainCanvas = mainCanvas;
			this.from = from;
			this.to = null;
			this.Path = CreatePath(from.GetPosition(), to);
			mainCanvas.Children.Add(Path);
		}

		public void ClearFromCanvas() {
			if(_mainCanvas.Children.Contains(Path)) {
				_mainCanvas.Children.Remove(Path);
			}
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
						$"C {from.X + (to.X - from.X) * 0.8},{from.Y} " +
						$"{from.X + (to.X - from.X) * 0.2},{to.Y} " +
						$"{to.X},{to.Y}");
		}


	}
}
