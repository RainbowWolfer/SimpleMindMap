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
		public readonly List<ConnectionControl> topDots = new();
		public readonly List<ConnectionControl> botDots = new();
		public readonly List<ConnectionControl> leftDots = new();
		public readonly List<ConnectionControl> rightDots = new();

		public List<ConnectionControl> AllDots => topDots.Concat(botDots.Concat(leftDots.Concat(rightDots))).ToList();

		//public readonly List<ConnectionPath> connected = new();//by others

		public const short SIZE = 10;

		public readonly Element _target;
		public FrameworkElement Framework => _target.Target;

		public ConnectionsFrame(MindMapPage parent, Element target) {
			this._target = target;

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

			topDots.Add(new ConnectionControl(this, parent, top));
			botDots.Add(new ConnectionControl(this, parent, bot));
			leftDots.Add(new ConnectionControl(this, parent, left));
			rightDots.Add(new ConnectionControl(this, parent, right));

			UpdateConnections();
		}

		public ConnectionControl? GetControlByID(string id) {
			return AllDots.Find(d => d.ID == id);
		}

		private static void CalculateStartPositionAndSize(FrameworkElement target, out Vector2 startPos, out Vector2 size) {
			startPos = new(Canvas.GetLeft(target), Canvas.GetTop(target));
			size = new(target.Width, target.Height);
		}

		public void UpdateConnections() {
			CalculateStartPositionAndSize(Framework, out Vector2 startPos, out Vector2 size);
			if(topDots.Count >= 1) {
				foreach(ConnectionControl top in topDots) {
					Canvas.SetLeft(top.target, startPos.X + size.X / (topDots.Count + 1) - SIZE / 2);
					Canvas.SetTop(top.target, startPos.Y - SIZE / 2);
					ConnectionsManager.Update(top);
				}
			}
			if(botDots.Count >= 1) {
				foreach(ConnectionControl bot in botDots) {
					Canvas.SetLeft(bot.target, startPos.X + size.X / (botDots.Count + 1) - SIZE / 2);
					Canvas.SetTop(bot.target, startPos.Y + size.Y - SIZE / 2);
					ConnectionsManager.Update(bot);
				}
			}
			if(leftDots.Count >= 1) {
				foreach(ConnectionControl left in leftDots) {
					Canvas.SetLeft(left.target, startPos.X - SIZE / 2);
					Canvas.SetTop(left.target, startPos.Y + size.Y / (leftDots.Count + 1) - SIZE / 2);
					ConnectionsManager.Update(left);
				}
			}
			if(rightDots.Count >= 1) {
				foreach(ConnectionControl right in rightDots) {
					Canvas.SetLeft(right.target, startPos.X + size.X - SIZE / 2);
					Canvas.SetTop(right.target, startPos.Y + size.Y / (rightDots.Count + 1) - SIZE / 2);
					ConnectionsManager.Update(right);
				}
			}
		}

		public void ClearConnections() {
			leftDots.ForEach(d => d.ClearDot());
			rightDots.ForEach(d => d.ClearDot());
			topDots.ForEach(d => d.ClearDot());
			botDots.ForEach(d => d.ClearDot());
			ConnectionsManager.Remove(this);
		}

		public void SetVisible(bool visible) {
			foreach(ConnectionControl item in AllDots) {
				item.target.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
			}
		}

	}

	/// <summary>
	/// This represents dot around the element
	/// </summary>
	public class ConnectionControl {
		public string Parent_ID => container._target.ID;
		public string ID { get; private set; }
		public readonly Ellipse target;
		public readonly ConnectionsFrame container;

		private readonly Canvas _mainCanvas;
		private readonly MindMapPage _parent;
		private bool _drag;

		private List<ConnectionControl> _otherDots = new();
		private ConnectionControl? _desiredDot;
		private const double MIN_CONNECTION_DISTANCE = 5.0;
		public ConnectionControl(ConnectionsFrame container, MindMapPage parent, Ellipse target) {
			this.ID = $"{container.AllDots.Count + 1}";
			this.container = container;
			this._mainCanvas = parent.MainCanvas;
			this._parent = parent;
			this.target = target;
			this._mainCanvas.Children.Add(target);
			target.MouseDown += Target_MouseDown;
			target.MouseEnter += Target_MouseEnter;
			target.MouseLeave += Target_MouseLeave;
			parent.BackgroundRectangle.MouseMove += Canvas_MouseMove;
			parent.BackgroundRectangle.PreviewMouseUp += Canvas_MouseUp;
			parent.MainCanvas.MouseMove += Canvas_MouseMove;
			parent.MainCanvas.PreviewMouseUp += Canvas_MouseUp;
		}

		private void Target_MouseLeave(object sender, MouseEventArgs e) {
			if(!_drag) {
				_parent.Cursor = null;
			}
		}

		private void Target_MouseEnter(object sender, MouseEventArgs e) {
			if(!_drag) {
				_parent.Cursor = Cursors.Cross;
			}
		}

		public Vector2 GetPosition() {
			return new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target)) + new Vector2(ConnectionsFrame.SIZE) / 2;
		}

		private void Canvas_MouseUp(object sender, MouseButtonEventArgs e) {
			_drag = false;
			_parent.Cursor = null;
			_parent.ClearPreviewLine();
			if(_desiredDot != null) {
				ConnectionsManager.Add(this, _desiredDot);
				_desiredDot = null;
			}
		}

		public void ClearDot() {
			if(this._mainCanvas.Children.Contains(target)) {
				this._mainCanvas.Children.Remove(target);
			}
		}

		private void Canvas_MouseMove(object sender, MouseEventArgs e) {
			if(!_drag) {
				return;
			}
			bool found = false;
			Vector2 pos = e.GetPosition(_mainCanvas);
			foreach(ConnectionControl item in _otherDots) {
				Vector2 itemPos = new Vector2(Canvas.GetLeft(item.target), Canvas.GetTop(item.target)) + new Vector2(ConnectionsFrame.SIZE) / 2;
				double distance = Vector2.Distance(pos, itemPos);
				if(distance < MIN_CONNECTION_DISTANCE) {
					_desiredDot = item;
					found = true;
					break;
				}
				_desiredDot = null;
			}
			_parent.Cursor = found ? Cursors.Hand : Cursors.Cross;
			_parent.UpdatePreviewLine(this, e.GetPosition(_mainCanvas));
		}

		private void Target_MouseDown(object sender, MouseButtonEventArgs e) {
			_drag = true;
			_parent.ClearResizePanel();
			_otherDots = _parent.GetAllConnectionDots(container._target);
		}
	}
}
