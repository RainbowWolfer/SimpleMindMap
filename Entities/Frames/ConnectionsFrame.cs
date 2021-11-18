using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Pages;
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

namespace MindMap.Entities.Frames {
	public class ConnectionsFrame {
		public readonly List<Ellipse> topDots = new();
		public readonly List<Ellipse> botDots = new();
		public readonly List<Ellipse> leftDots = new();
		public readonly List<Ellipse> rightDots = new();

		public const short SIZE = 10;

		private FrameworkElement _target;
		private Canvas _mainCanvas;

		public ConnectionsFrame(MindMapPage parent, FrameworkElement target) {
			this._target = target;
			this._mainCanvas = parent.MainCanvas;

			Style style = new(typeof(Ellipse));
			style.Setters.Add(new Setter(FrameworkElement.HeightProperty, (double)SIZE));
			style.Setters.Add(new Setter(FrameworkElement.WidthProperty, (double)SIZE));
			style.Setters.Add(new Setter(Shape.FillProperty, Brushes.Transparent));
			style.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
			style.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));

			Ellipse top = new() { Style = style };
			Ellipse bot = new() { Style = style };
			Ellipse left = new() { Style = style };
			Ellipse right = new() { Style = style };

			topDots.Add(top);
			botDots.Add(bot);
			leftDots.Add(left);
			rightDots.Add(right);

			_ = new ConnectionControl(parent, top);
			_ = new ConnectionControl(parent, bot);
			_ = new ConnectionControl(parent, left);
			_ = new ConnectionControl(parent, right);

			this._mainCanvas.Children.Add(top);
			this._mainCanvas.Children.Add(bot);
			this._mainCanvas.Children.Add(left);
			this._mainCanvas.Children.Add(right);

			UpdateConnections();
		}

		private static void CalculateStartPositionAndSize(FrameworkElement target, out Vector2 startPos, out Vector2 size) {
			startPos = new(Canvas.GetLeft(target), Canvas.GetTop(target));
			size = new(target.Width, target.Height);
		}

		public void UpdateConnections() {
			CalculateStartPositionAndSize(_target, out Vector2 startPos, out Vector2 size);
			if(topDots.Count >= 1) {
				foreach(Ellipse top in topDots) {
					Canvas.SetLeft(top, startPos.X + size.X / (topDots.Count + 1) - SIZE / 2);
					Canvas.SetTop(top, startPos.Y - SIZE / 2);
				}
			}
			if(botDots.Count >= 1) {
				foreach(Ellipse bot in botDots) {
					Canvas.SetLeft(bot, startPos.X + size.X / (botDots.Count + 1) - SIZE / 2);
					Canvas.SetTop(bot, startPos.Y + size.Y - SIZE / 2);
				}
			}
			if(leftDots.Count >= 1) {
				foreach(Ellipse left in leftDots) {
					Canvas.SetLeft(left, startPos.X - SIZE / 2);
					Canvas.SetTop(left, startPos.Y + size.Y / (leftDots.Count + 1) - SIZE / 2);
				}
			}
			if(rightDots.Count >= 1) {
				foreach(Ellipse right in rightDots) {
					Canvas.SetLeft(right, startPos.X + size.X - SIZE / 2);
					Canvas.SetTop(right, startPos.Y + size.Y / (rightDots.Count + 1) - SIZE / 2);
				}
			}
		}

		public void SetVisible(bool visible) {
			foreach(Ellipse item in leftDots.Concat(rightDots).Concat(topDots).Concat(botDots)) {
				item.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
			}
		}

		private class ConnectionControl {
			private Canvas _mainCanvas;
			private bool _drag;
			private MindMapPage _parent;

			private List<(Element, List<Ellipse>)> dots = new();
			public ConnectionControl(MindMapPage parent, Ellipse target) {
				this._mainCanvas = parent.MainCanvas;
				this._parent = parent;
				target.MouseDown += Target_MouseDown;
				target.MouseEnter += Target_MouseEnter;
				target.MouseLeave += Target_MouseLeave;
				parent.MainCanvas.MouseMove += Canvas_MouseMove;
				parent.MainCanvas.MouseUp += Canvas_MouseUp;
			}

			private void Target_MouseLeave(object sender, MouseEventArgs e) {
				_parent.Cursor = null;
			}

			private void Target_MouseEnter(object sender, MouseEventArgs e) {
				_parent.Cursor = Cursors.Cross;
			}

			private void DrawPreviewLine() {

			}

			private void Canvas_MouseUp(object sender, MouseButtonEventArgs e) {
				_drag = false;
			}

			private void Canvas_MouseMove(object sender, MouseEventArgs e) {
				if(!_drag) {
					return;
				}
				Vector2 pos = e.GetPosition(_mainCanvas);

				foreach((Element, List<Ellipse>) list in dots) {
					foreach(Ellipse item in list.Item2) {
						Vector2 itemPos = new(Canvas.GetLeft(item), Canvas.GetTop(item));
						double distance = Vector2.Distance(pos, itemPos);
						Debug.WriteLine($"distance to {item} is {distance}");
					}
				}
			}

			private void Target_MouseDown(object sender, MouseButtonEventArgs e) {
				_drag = true;
				dots = _parent.GetAllConnectionDots();
			}
		}
	}
}
