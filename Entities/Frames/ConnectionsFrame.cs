using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Icons;
using MindMap.Entities.Identifications;
using MindMap.Entities.Services;
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
		public const int MAXDOTSPERSIDE = 4;

		public readonly List<ConnectionControl> topDots = new();
		public readonly List<ConnectionControl> botDots = new();
		public readonly List<ConnectionControl> leftDots = new();
		public readonly List<ConnectionControl> rightDots = new();

		public List<ConnectionControl> AllDots {
			get {
				List<ConnectionControl> list = new();
				list.AddRange(topDots);
				list.AddRange(botDots);
				list.AddRange(leftDots);
				list.AddRange(rightDots);
				return list;
			}
		}

		//public readonly List<ConnectionPath> connected = new();//by others

		public const short SIZE = 10;

		public readonly MindMapPage _parent;
		public readonly Element _target;
		public FrameworkElement Framework => _target.Target;

		public static Style DefaultStyle {
			get {
				Style style = new(typeof(Ellipse));
				style.Setters.Add(new Setter(FrameworkElement.HeightProperty, (double)SIZE));
				style.Setters.Add(new Setter(FrameworkElement.WidthProperty, (double)SIZE));
				style.Setters.Add(new Setter(Shape.FillProperty, Brushes.Transparent));
				style.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
				style.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));
				return style;
			}
		}

		public ConnectionsFrame(MindMapPage parent, Element target, Dictionary<Direction, int>? initialControls = null) {
			this._parent = parent;
			this._target = target;
			if(initialControls == null ||
				initialControls.Count != 4 ||
				initialControls.Select(i => i.Value).Any(i => i <= 0)
			) {
				AddControl(Direction.Top);
				AddControl(Direction.Bottom);
				AddControl(Direction.Left);
				AddControl(Direction.Right);
			} else {
				foreach(KeyValuePair<Direction, int> item in initialControls) {
					for(int i = 0; i < item.Value; i++) {
						AddControl(item.Key);
					}
				}
			}
		}

		public List<ConnectionControl> GetList(Direction direction) {
			return direction switch {
				Direction.Left => leftDots,
				Direction.Right => rightDots,
				Direction.Top => topDots,
				Direction.Bottom => botDots,
				_ => throw new Exception("Direction Not Found"),
			};
		}

		public void AddControl(Direction direction) {
			switch(direction) {
				case Direction.Left:
					leftDots.Add(CreateControl(Direction.Left));
					break;
				case Direction.Right:
					rightDots.Add(CreateControl(Direction.Right));
					break;
				case Direction.Top:
					topDots.Add(CreateControl(Direction.Top));
					break;
				case Direction.Bottom:
					botDots.Add(CreateControl(Direction.Bottom));
					break;
				default:
					throw new Exception("Direction Not Found");
			}
			UpdateConnections();
		}

		public void RemoveControl(Direction direction, ConnectionControl control) {
			ConnectionControl? found = null;
			List<ConnectionControl> list;
			switch(direction) {
				case Direction.Left:
					found = leftDots.Find(d => d == control);
					list = leftDots;
					break;
				case Direction.Right:
					found = rightDots.Find(d => d == control);
					list = rightDots;
					break;
				case Direction.Top:
					found = topDots.Find(d => d == control);
					list = topDots;
					break;
				case Direction.Bottom:
					found = botDots.Find(d => d == control);
					list = botDots;
					break;
				default:
					throw new Exception("Direction Not Found");
			}
			if(found == null) {
				return;
			}
			found.ClearDot();
			list.Remove(found);
			UpdateConnections();
		}

		private ConnectionControl CreateControl(Direction direction) {
			return new ConnectionControl(this, _parent, new Ellipse() {
				Style = DefaultStyle,
			}, direction);
		}

		public Dictionary<Direction, int> GetControlsInfo() {
			Dictionary<Direction, int> result = new();
			result.Add(Direction.Left, leftDots.Count);
			result.Add(Direction.Right, rightDots.Count);
			result.Add(Direction.Top, topDots.Count);
			result.Add(Direction.Bottom, botDots.Count);
			return result;
		}

		public ConnectionControl? GetControlByID(string id) {
			return AllDots.Find(d => d.Identity.ID == id);
		}

		private static void CalculateStartPositionAndSize(FrameworkElement target, out Vector2 startPos, out Vector2 size) {
			startPos = new(Canvas.GetLeft(target), Canvas.GetTop(target));
			size = new(target.Width, target.Height);
		}

		public void UpdateConnections() {
			CalculateStartPositionAndSize(Framework, out Vector2 startPos, out Vector2 size);
			if(topDots.Count >= 1) {
				for(int i = 0; i < topDots.Count; i++) {
					ConnectionControl top = topDots[i];
					Canvas.SetLeft(
						top.target,
						startPos.X + size.X / (topDots.Count + 1) * (i + 1) - SIZE / 2
					);
					Canvas.SetTop(top.target, startPos.Y - SIZE / 2);
					_parent.connectionsManager.Update(top);
				}
			}
			if(botDots.Count >= 1) {
				for(int i = 0; i < botDots.Count; i++) {
					ConnectionControl bot = botDots[i];
					Canvas.SetLeft(
						bot.target,
						startPos.X + size.X / (botDots.Count + 1) * (i + 1) - SIZE / 2
					);
					Canvas.SetTop(bot.target, startPos.Y + size.Y - SIZE / 2);
					_parent.connectionsManager.Update(bot);
				}
			}
			if(leftDots.Count >= 1) {
				for(int i = 0; i < leftDots.Count; i++) {
					ConnectionControl left = leftDots[i];
					Canvas.SetLeft(left.target, startPos.X - SIZE / 2);
					Canvas.SetTop(
						left.target,
						startPos.Y + size.Y / (leftDots.Count + 1) * (i + 1) - SIZE / 2
					);
					_parent.connectionsManager.Update(left);
				}
			}
			if(rightDots.Count >= 1) {
				for(int i = 0; i < rightDots.Count; i++) {
					ConnectionControl right = rightDots[i];
					Canvas.SetLeft(right.target, startPos.X + size.X - SIZE / 2);
					Canvas.SetTop(
						right.target,
						startPos.Y + size.Y / (rightDots.Count + 1) * (i + 1) - SIZE / 2
					);
					_parent.connectionsManager.Update(right);
				}
			}
		}

		public void ClearConnections() {
			leftDots.ForEach(d => d.ClearDot());
			rightDots.ForEach(d => d.ClearDot());
			topDots.ForEach(d => d.ClearDot());
			botDots.ForEach(d => d.ClearDot());
			_parent.connectionsManager.RemoveFrame(this);
		}

		public void SetVisible(bool visible) {
			foreach(ConnectionControl item in AllDots) {
				item.target.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
			}
		}

		//public override string ToString() {
		//	return $"Connection Frame: {_target.Identity.Name}";
		//}
	}

	/// <summary>
	/// This represents dot around the element
	/// </summary>
	public class ConnectionControl: IIdentityContainer {
		public Element Parent => container._target;
		public string Parent_ID => Parent.Identity.ID;

		public Direction Direction { get; private set; }

		public Identity Identity { get; set; }

		public readonly Ellipse target;
		public readonly ConnectionsFrame container;

		private readonly Canvas _mainCanvas;
		private readonly MindMapPage _parent;
		private bool _drag;

		private List<ConnectionControl> _otherDots = new();
		private ConnectionControl? _desiredDot;
		private const double MIN_CONNECTION_DISTANCE = 5.0;

		public ConnectionControl(ConnectionsFrame container, MindMapPage parent, Ellipse target, Direction direction, Identity? identity = null) {
			//this.ID = $"{container.AllDots.Count + 1}";
			this.container = container;
			this._mainCanvas = parent.MainCanvas;
			this._parent = parent;
			this.Direction = direction;
			this.target = target;
			this.Identity = identity ?? new Identity(InitializeID(), InitializeDefaultName());
			this._mainCanvas.Children.Add(target);
			target.MouseDown += Target_MouseDown;
			target.MouseEnter += Target_MouseEnter;
			target.MouseLeave += Target_MouseLeave;
			target.MouseRightButtonUp += Target_MouseRightButtonUp;
			target.MouseLeftButtonUp += Target_MouseLeftButtonUp;
			parent.BackgroundRectangle.MouseMove += Canvas_MouseMove;
			parent.BackgroundRectangle.PreviewMouseUp += Canvas_MouseUp;
			parent.MainCanvas.MouseMove += Canvas_MouseMove;
			parent.MainCanvas.PreviewMouseUp += Canvas_MouseUp;
		}

		private int clickTime = 0;
		private void Target_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if(e.Timestamp - clickTime < 300) {
				if(container.GetList(Direction).Count < ConnectionsFrame.MAXDOTSPERSIDE) {
					container.AddControl(Direction);
				}
				clickTime = 0;
			} else {
				clickTime = e.Timestamp;
			}
		}

		private void Target_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
			target.ContextMenu = new ContextMenu();
			//add new one (max 3)
			//remove this (optional)
			MenuItem item_add = new() {
				Header = "Add New Dot",
				Icon = new FontIcon("\uE109", 14).Generate(),
			};
			MenuItem item_delete = new() {
				Header = "Delete This Dot",
				Icon = new FontIcon("\uE107", 14).Generate(),
			};
			int count = container.GetList(Direction).Count;
			if(count <= 1) {
				item_delete.IsEnabled = false;
			} else if(count >= ConnectionsFrame.MAXDOTSPERSIDE) {
				item_add.IsEnabled = false;
			}
			item_add.Click += (s, args) => {
				container.AddControl(Direction);
			};
			item_delete.Click += (s, args) => {
				container.RemoveControl(Direction, this);
			};
			target.ContextMenu.Items.Add(item_add);
			target.ContextMenu.Items.Add(item_delete);
			target.ContextMenu.PlacementTarget = target;
		}

		private string InitializeID() => $"ConnectionControl_({Methods.GetTick()})";

		private string InitializeDefaultName() => $"Dot_{Parent.Identity}";


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
				_parent.connectionsManager.AddConnection(this, _desiredDot);
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

		public override string ToString() {
			return $"Dot: {Parent_ID}";
		}
	}

	public enum Direction {
		Left, Right, Top, Bottom
	}
}
