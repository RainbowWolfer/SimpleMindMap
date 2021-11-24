using MindMap.Entities.Elements;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Frames {
	public class ResizeFrame {
		public static ResizeFrame? Current { get; set; }
		public const double SIZE = 15;
		public const double STROKE = 3;
		public static ResizeFrame Create(MindMapPage parent, FrameworkElement target, Element element) {
			Canvas mainCanvas = parent.MainCanvas;
			Current?.ClearResizeFrame(mainCanvas);

			Style style_line = new(typeof(Line));
			style_line.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
			style_line.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 0.5 })));
			style_line.Setters.Add(new Setter(Shape.StrokeThicknessProperty, STROKE));

			//line pattern cannot handle mousedown, consider using rectangle -> stroke
			Line top = new() { Style = style_line };
			Line bottom = new() { Style = style_line };
			Line left = new() { Style = style_line };
			Line right = new() { Style = style_line };

			mainCanvas.Children.Add(top);
			mainCanvas.Children.Add(bottom);
			mainCanvas.Children.Add(left);
			mainCanvas.Children.Add(right);

			Style style_rect = new(typeof(Rectangle));
			style_rect.Setters.Add(new Setter(Shape.FillProperty, Brushes.Transparent));
			style_rect.Setters.Add(new Setter(FrameworkElement.WidthProperty, SIZE));
			style_rect.Setters.Add(new Setter(FrameworkElement.HeightProperty, SIZE));
			style_rect.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));
			style_rect.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
			style_rect.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 1 })));

			Rectangle top_left = new() { Style = style_rect };
			Rectangle top_right = new() { Style = style_rect };
			Rectangle bot_left = new() { Style = style_rect };
			Rectangle bot_right = new() { Style = style_rect };

			mainCanvas.Children.Add(top_left);
			mainCanvas.Children.Add(top_right);
			mainCanvas.Children.Add(bot_left);
			mainCanvas.Children.Add(bot_right);

			Current = new ResizeFrame(parent, target, element, top, bottom, left, right, top_left, top_right, bot_left, bot_right);
			Current.UpdateResizeFrame();

			return Current;
		}

		public readonly FrameworkElement target;
		public readonly Element element;

		public readonly Line top;
		public readonly Line bot;
		public readonly Line left;
		public readonly Line right;

		public readonly Rectangle top_left;
		public readonly Rectangle top_right;
		public readonly Rectangle bot_left;
		public readonly Rectangle bot_right;

#pragma warning disable IDE0052 // Remove unread private members
		private readonly ResizeControl _control_top;
		private readonly ResizeControl _control_bot;
		private readonly ResizeControl _control_left;
		private readonly ResizeControl _control_right;
		private readonly ResizeControl _control_top_left;
		private readonly ResizeControl _control_top_right;
		private readonly ResizeControl _control_bot_left;
		private readonly ResizeControl _control_bot_right;
#pragma warning restore IDE0052 // Remove unread private members

		public ResizeFrame(MindMapPage parent, FrameworkElement target, Element element, Line top, Line bot, Line left, Line right, Rectangle top_left, Rectangle top_right, Rectangle bot_left, Rectangle bot_right) {
			this.target = target;
			this.element = element;
			this.top = top;
			this.bot = bot;
			this.left = left;
			this.right = right;
			this.top_left = top_left;
			this.top_right = top_right;
			this.bot_left = bot_left;
			this.bot_right = bot_right;

			_control_top = new ResizeControl(parent, target, element, top, ResizeControl.Direction.T);
			_control_bot = new ResizeControl(parent, target, element, bot, ResizeControl.Direction.B);
			_control_left = new ResizeControl(parent, target, element, left, ResizeControl.Direction.L);
			_control_right = new ResizeControl(parent, target, element, right, ResizeControl.Direction.R);

			_control_top_left = new ResizeControl(parent, target, element, top_left, ResizeControl.Direction.LT);
			_control_top_right = new ResizeControl(parent, target, element, top_right, ResizeControl.Direction.RT);
			_control_bot_left = new ResizeControl(parent, target, element, bot_left, ResizeControl.Direction.LB);
			_control_bot_right = new ResizeControl(parent, target, element, bot_right, ResizeControl.Direction.RB);
		}

		public bool IsValid => target != null && top != null && bot != null && left != null && right != null;

		public void ClearResizeFrame(Canvas mainCanvas) {
			void Remove(Shape shape) {
				if(mainCanvas.Children.Contains(shape)) {
					mainCanvas.Children.Remove(shape);
				}
			}
			Remove(this.top);
			Remove(this.bot);
			Remove(this.left);
			Remove(this.right);
			Remove(this.top_left);
			Remove(this.top_right);
			Remove(this.bot_left);
			Remove(this.bot_right);
		}

		public void UpdateResizeFrame() {
			if(Current == null || !Current.IsValid) {
				return;
			}
			Vector2 startPoint = new(Canvas.GetLeft(Current.target), Canvas.GetTop(Current.target));
			Vector2 size = new(Current.target.Width, Current.target.Height);

			top.X1 = startPoint.X;
			top.Y1 = startPoint.Y;
			top.X2 = startPoint.X + size.X;
			top.Y2 = startPoint.Y;

			bot.X1 = startPoint.X;
			bot.Y1 = startPoint.Y + size.Y;
			bot.X2 = startPoint.X + size.X;
			bot.Y2 = startPoint.Y + size.Y;

			left.X1 = startPoint.X;
			left.Y1 = startPoint.Y;
			left.X2 = startPoint.X;
			left.Y2 = startPoint.Y + size.Y;

			right.X1 = startPoint.X + size.X;
			right.Y1 = startPoint.Y;
			right.X2 = startPoint.X + size.X;
			right.Y2 = startPoint.Y + size.Y;

			Canvas.SetLeft(top_left, startPoint.X - top_left.Width / 2);
			Canvas.SetTop(top_left, startPoint.Y - top_left.Height / 2);

			Canvas.SetLeft(top_right, startPoint.X + size.X - top_right.Width / 2);
			Canvas.SetTop(top_right, startPoint.Y - top_right.Height / 2);

			Canvas.SetLeft(bot_left, startPoint.X - bot_left.Width / 2);
			Canvas.SetTop(bot_left, startPoint.Y + size.Y - bot_left.Height / 2);

			Canvas.SetLeft(bot_right, startPoint.X + size.X - bot_right.Width / 2);
			Canvas.SetTop(bot_right, startPoint.Y + size.Y - bot_right.Height / 2);

		}



		private class ResizeControl {
			public readonly FrameworkElement target;
			public readonly Shape shape;
			public readonly Direction direction;

			private Vector2 startMousePos;
			private Vector2 startSize;
			private Vector2 startPos;
			private bool _drag;
			public ResizeControl(MindMapPage parent, FrameworkElement target, Element element, Shape shape, Direction direction) {
				this.target = target;
				this.shape = shape;
				this.direction = direction;
				shape.MouseEnter += (s, e) => {
					parent.Cursor = GetCursor();
				};
				shape.MouseLeave += (s, e) => {
					parent.Cursor = null;
				};
				shape.MouseDown += (s, e) => {
					_drag = true;
					startMousePos = e.GetPosition(parent);
					startSize = new Vector2(target.Width, target.Height);
					startPos = new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
					Mouse.Capture(shape);
				};
				parent.MainCanvas.MouseMove += (s, e) => {
					if(!_drag) {
						return;
					}
					Vector2 delta = e.GetPosition(parent) - startMousePos;
					bool holdShift = true;
					if(holdShift) {
						double max = Math.Min(delta.X, delta.Y);
						delta.X = max;
						delta.Y = max;
					}
					switch(direction) {
						case Direction.L:
							target.Width = Math.Clamp(startSize.X - delta.X, 5, double.MaxValue);
							Canvas.SetLeft(target, Math.Clamp(startPos.X + delta.X, double.MinValue, startPos.X + startSize.X - 5));
							break;
						case Direction.LT:
							target.Width = Math.Clamp(startSize.X - delta.X, 5, double.MaxValue);
							Canvas.SetLeft(target, Math.Clamp(startPos.X + delta.X, double.MinValue, startPos.X + startSize.X - 5));
							target.Height = Math.Clamp(startSize.Y - delta.Y, 5, double.MaxValue);
							Canvas.SetTop(target, Math.Clamp(startPos.Y + delta.Y, double.MinValue, startPos.Y + startSize.Y - 5));
							break;
						case Direction.T:
							target.Height = Math.Clamp(startSize.Y - delta.Y, 5, double.MaxValue);
							Canvas.SetTop(target, Math.Clamp(startPos.Y + delta.Y, double.MinValue, startPos.Y + startSize.Y - 5));
							break;
						case Direction.RT:
							target.Height = Math.Clamp(startSize.Y - delta.Y, 5, double.MaxValue);
							Canvas.SetTop(target, Math.Clamp(startPos.Y + delta.Y, double.MinValue, startPos.Y + startSize.Y - 5));
							target.Width = Math.Clamp(startSize.X + delta.X, 5, double.MaxValue);
							break;
						case Direction.R:
							target.Width = Math.Clamp(startSize.X + delta.X, 5, double.MaxValue);
							break;
						case Direction.RB:
							target.Width = Math.Clamp(startSize.X + delta.X, 5, double.MaxValue);
							target.Height = Math.Clamp(startSize.Y + delta.Y, 5, double.MaxValue);
							break;
						case Direction.B:
							target.Height = Math.Clamp(startSize.Y + delta.Y, 5, double.MaxValue);
							break;
						case Direction.LB:
							target.Height = Math.Clamp(startSize.Y + delta.Y, 5, double.MaxValue);
							target.Width = Math.Clamp(startSize.X - delta.X, 5, double.MaxValue);
							Canvas.SetLeft(target, Math.Clamp(startPos.X + delta.X, double.MinValue, startPos.X + startSize.X - 5));
							break;
						default:
							throw new Exception("Direction Type Error");
					}
					Current?.UpdateResizeFrame();
					element.UpdateConnectionsFrame();
					if(element is IUpdate update) {
						update.Udpate();
					}
				};
				parent.MainCanvas.MouseUp += (s, e) => {
					_drag = false;
					Mouse.Capture(null);
				};
			}

			private Cursor GetCursor() => direction switch {
				Direction.L => Cursors.SizeWE,
				Direction.LT => Cursors.SizeNWSE,
				Direction.T => Cursors.SizeNS,
				Direction.RT => Cursors.SizeNESW,
				Direction.R => Cursors.SizeWE,
				Direction.RB => Cursors.SizeNWSE,
				Direction.B => Cursors.SizeNS,
				Direction.LB => Cursors.SizeNESW,
				_ => throw new Exception("Cursor Type Error"),
			};

			public enum Direction {
				L, LT, T, RT, R, RB, B, LB
			}
		}
	}
}
