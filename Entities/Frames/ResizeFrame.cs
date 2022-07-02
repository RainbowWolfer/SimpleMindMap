using MindMap.Entities.Elements;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Tags;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Frames {
	public class ResizeFrame {
		public static ResizeFrame? Current { get; set; }

		public static ResizeFrame Create(MindMapPage parent, params Element[] elements) {
			Current?.ClearResizeFrame();
			Current = new ResizeFrame(parent, elements);
			return Current;
		}

		public const double SIZE = 15;
		public const double STROKE = 3;

		private readonly MindMapPage _parent;
		private readonly Canvas _mainCanvas;

		public readonly List<Element> elements;

		public readonly Line top;
		public readonly Line bot;
		public readonly Line left;
		public readonly Line right;

		public readonly Rectangle top_left;
		public readonly Rectangle top_right;
		public readonly Rectangle bot_left;
		public readonly Rectangle bot_right;

		private readonly ResizeControl _control_top;
		private readonly ResizeControl _control_bot;
		private readonly ResizeControl _control_left;
		private readonly ResizeControl _control_right;
		private readonly ResizeControl _control_top_left;
		private readonly ResizeControl _control_top_right;
		private readonly ResizeControl _control_bot_left;
		private readonly ResizeControl _control_bot_right;

		private Style PathStyle {
			get {
				Style style_line = new(typeof(Line));
				style_line.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
				style_line.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 0.5 })));
				style_line.Setters.Add(new Setter(Shape.StrokeThicknessProperty, STROKE));
				return style_line;
			}
		}

		private Style RectStyle {
			get {
				Style style_rect = new(typeof(Rectangle));
				style_rect.Setters.Add(new Setter(Shape.FillProperty, Brushes.Transparent));
				style_rect.Setters.Add(new Setter(FrameworkElement.WidthProperty, SIZE));
				style_rect.Setters.Add(new Setter(FrameworkElement.HeightProperty, SIZE));
				style_rect.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));
				style_rect.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
				style_rect.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 1 })));
				return style_rect;
			}
		}

		public ResizeFrame(MindMapPage parent, params Element[] elements) {
			this._parent = parent;
			this._mainCanvas = parent.MainCanvas;
			this.elements = elements.ToList();

			top = new() {
				Style = PathStyle,
				Tag = new ResizeFrameworkTag(),
			};
			bot = new() {
				Style = PathStyle,
				Tag = new ResizeFrameworkTag(),
			};
			left = new() {
				Style = PathStyle,
				Tag = new ResizeFrameworkTag(),
			};
			right = new() {
				Style = PathStyle,
				Tag = new ResizeFrameworkTag(),
			};

			top_left = new() {
				Style = RectStyle,
				Tag = new ResizeFrameworkTag(),
			};
			top_right = new() {
				Style = RectStyle,
				Tag = new ResizeFrameworkTag(),
			};
			bot_left = new() {
				Style = RectStyle,
				Tag = new ResizeFrameworkTag(),
			};
			bot_right = new() {
				Style = RectStyle,
				Tag = new ResizeFrameworkTag(),
			};

			AddIntoCanvas();

			_control_top = new ResizeControl(parent, this, top, Direction.T);
			_control_bot = new ResizeControl(parent, this, bot, Direction.B);
			_control_left = new ResizeControl(parent, this, left, Direction.L);
			_control_right = new ResizeControl(parent, this, right, Direction.R);

			_control_top_left = new ResizeControl(parent, this, top_left, Direction.LT);
			_control_top_right = new ResizeControl(parent, this, top_right, Direction.RT);
			_control_bot_left = new ResizeControl(parent, this, bot_left, Direction.LB);
			_control_bot_right = new ResizeControl(parent, this, bot_right, Direction.RB);

			UpdateResizeFrame();
		}

		public void AddElement(Element element) {
			this.elements.Add(element);
			UpdateResizeFrame();
		}

		public void AddIntoCanvas() {
			void Add(Shape shape) {
				if(!_mainCanvas.Children.Contains(shape)) {
					_mainCanvas.Children.Add(shape);
				}
			}
			Add(top);
			Add(bot);
			Add(left);
			Add(right);

			Add(top_left);
			Add(top_right);
			Add(bot_left);
			Add(bot_right);
		}

		public void ClearResizeFrame() {
			void Remove(Shape shape) {
				if(_mainCanvas.Children.Contains(shape)) {
					_mainCanvas.Children.Remove(shape);
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

			elements.Clear();
		}

		public void UpdateResizeFrame() {
			if(elements.Count == 0) {
				return;
			}
			GetBoundSize(out Vector2 startPoint, out Vector2 endPoint);
			top.X1 = startPoint.X;
			top.Y1 = startPoint.Y;
			top.X2 = endPoint.X;
			top.Y2 = startPoint.Y;

			bot.X1 = startPoint.X;
			bot.Y1 = endPoint.Y;
			bot.X2 = endPoint.X;
			bot.Y2 = endPoint.Y;

			left.X1 = startPoint.X;
			left.Y1 = startPoint.Y;
			left.X2 = startPoint.X;
			left.Y2 = endPoint.Y;

			right.X1 = endPoint.X;
			right.Y1 = startPoint.Y;
			right.X2 = endPoint.X;
			right.Y2 = endPoint.Y;

			Canvas.SetLeft(top_left, startPoint.X - top_left.Width / 2);
			Canvas.SetTop(top_left, startPoint.Y - top_left.Height / 2);

			Canvas.SetLeft(top_right, endPoint.X - top_right.Width / 2);
			Canvas.SetTop(top_right, startPoint.Y - top_right.Height / 2);

			Canvas.SetLeft(bot_left, startPoint.X - bot_left.Width / 2);
			Canvas.SetTop(bot_left, endPoint.Y - bot_left.Height / 2);

			Canvas.SetLeft(bot_right, endPoint.X - bot_right.Width / 2);
			Canvas.SetTop(bot_right, endPoint.Y - bot_right.Height / 2);

		}

		public void GetBoundSize(out Vector2 startPoint, out Vector2 endPoint) {
			if(elements.Count == 0) {
				startPoint = Vector2.Zero;
				endPoint = Vector2.Zero;
				return;
			}

			IEnumerable<Vector2> all = elements.SelectMany(e => e.GetBoundPoints());

			double minX = all.Min(p => p.X);
			double minY = all.Min(p => p.Y);
			double maxX = all.Max(p => p.X);
			double maxY = all.Max(p => p.Y);
			startPoint = new Vector2(minX, minY);
			endPoint = new Vector2(maxX, maxY);
		}

		private class ResizeControl {
			private readonly ResizeFrame frame;
			public readonly Shape shape;
			public readonly Direction direction;

			private Vector2 startMousePos;
			private List<ResizeElementInfo> infos = new();
			private Vector2 startFrameSize;
			private bool _drag;
			public ResizeControl(MindMapPage parent, ResizeFrame frame, Shape shape, Direction direction) {
				this.frame = frame;
				this.shape = shape;
				this.direction = direction;
				shape.MouseEnter += (s, e) => {
					parent.Cursor = GetCursor(direction);
				};
				shape.MouseLeave += (s, e) => {
					parent.Cursor = null;
				};
				shape.MouseDown += (s, e) => {
					_drag = true;
					startMousePos = e.GetPosition(parent);
					infos.Clear();
					frame.GetBoundSize(out Vector2 startPoint, out Vector2 endPoint);
					startFrameSize = endPoint - startPoint;
					foreach(Element item in frame.elements) {
						infos.Add(new ResizeElementInfo(item, item.GetSize(), item.GetPosition(), startPoint, endPoint));
					}
					Mouse.Capture(shape);
				};
				parent.MainCanvas.MouseMove += (s, e) => {
					if(!_drag) {
						return;
					}
					Vector2 mousePosition = e.GetPosition(parent);
					Vector2 delta = mousePosition - startMousePos;
					void NormalizeA(bool active) {
						if(active) {
							double d = Math.Min(delta.X, delta.Y);
							delta.X = d;
							delta.Y = d;
						}
					}
					void NormalizeB(bool active) {
						if(active) {
							double min = Math.Min(delta.X, delta.Y);
							double max = Math.Max(delta.X, delta.Y);
							delta.X = mousePosition.X < startMousePos.X ? min : max;
							delta.Y = mousePosition.X < startMousePos.X ? -min : -max;
						}
					}
					bool holdShift = parent.holdShift;

					void LeftMovement() {
						double desiredFrameSizeX = startFrameSize.X - delta.X;
						foreach(ResizeElementInfo item in infos) {
							double ratio = item.StartSize.X / startFrameSize.X;
							item.targetSize.X = desiredFrameSizeX * (1 - item.LeftRatio - item.RightRatio);
							item.targetPosition.X = item.StartPos.X + delta.X * ratio + delta.X * item.RightRatio;
						}
					}

					void RightMovement() {
						double desiredFrameSizeX = startFrameSize.X + delta.X;
						foreach(ResizeElementInfo item in infos) {
							item.targetSize.X = desiredFrameSizeX * (1 - item.LeftRatio - item.RightRatio);
							item.targetPosition.X = item.StartPos.X + delta.X * item.LeftRatio;
						}
					}

					void TopMovement() {
						double desiredFrameSizeY = startFrameSize.Y - delta.Y;
						foreach(ResizeElementInfo item in infos) {
							double ratio = item.StartSize.Y / startFrameSize.Y;
							item.targetSize.Y = desiredFrameSizeY * (1 - item.TopRatio - item.BottomRatio);
							item.targetPosition.Y = item.StartPos.Y + delta.Y * ratio + delta.Y * item.BottomRatio;
						}
					}

					void BottomMovement() {
						double desiredFrameSizeY = startFrameSize.Y + delta.Y;
						foreach(ResizeElementInfo item in infos) {
							item.targetSize.Y = desiredFrameSizeY * (1 - item.TopRatio - item.BottomRatio);
							item.targetPosition.Y = item.StartPos.Y + delta.Y * item.TopRatio;
						}
					}

					switch(direction) {
						case Direction.L: {
							LeftMovement();
							break;
						}
						case Direction.LT: {
							NormalizeA(holdShift);
							LeftMovement();
							TopMovement();
							break;
						}
						case Direction.T: {
							TopMovement();
							break;
						}
						case Direction.RT: {
							NormalizeB(holdShift);
							RightMovement();
							TopMovement();
							break;
						}
						case Direction.R: {
							RightMovement();
							break;
						}
						case Direction.RB: {
							NormalizeA(holdShift);
							RightMovement();
							BottomMovement();
							break;
						}
						case Direction.B: {
							BottomMovement();
							break;
						}
						case Direction.LB: {
							NormalizeB(holdShift);
							LeftMovement();
							BottomMovement();
							break;
						}
						default:
							throw new Exception($"Direction ({direction}) Type Error");
					}
					foreach(ResizeElementInfo item in infos) {
						if(item.targetSize.X > Element.MIN_SIZE && item.targetSize.Y > Element.MIN_SIZE) {
							item.Target.SetSize(item.targetSize);
							item.Target.SetPosition(item.targetPosition);
						}
					}
					Current?.UpdateResizeFrame();
					foreach(Element element in frame.elements) {
						element.UpdateConnectionsFrame();
						if(element is IUpdate update) {
							update.Update();
						}
					}
				};
				parent.MainCanvas.MouseUp += (s, e) => {
					if(_drag) {
						List<EditHistory.ElementFrameworkChange.FrameworkChangeItem> items = new();
						foreach(ResizeElementInfo item in infos) {
							Vector2 endPos = item.Target.GetPosition();
							Vector2 endSize = item.Target.GetSize();
							if(item.StartPos == endPos && item.StartSize == endSize) {
								continue;
							}
							items.Add(new EditHistory.ElementFrameworkChange.FrameworkChangeItem() {
								Identity = item.Target.Identity,
								FromPosition = item.StartPos,
								FromSize = item.StartSize,
								ToPosition = endPos,
								ToSize = endSize,
							});
						}
						parent.editHistory.SubmitByElementFrameworkChanged(
							items,
							EditHistory.FrameChangeType.Resize
						);
					}
					_drag = false;
					Mouse.Capture(null);
				};
			}

			private Cursor GetCursor(Direction direction) => direction switch {
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

			private class ResizeElementInfo {
				public Element Target { get; set; }
				public Vector2 StartSize { get; set; }
				public Vector2 StartPos { get; set; }

				public double LeftRatio { get; private set; }
				public double RightRatio { get; private set; }
				public double TopRatio { get; private set; }
				public double BottomRatio { get; private set; }

				public Vector2 targetSize;
				public Vector2 targetPosition;

				public ResizeElementInfo(Element target, Vector2 startSize, Vector2 startPos, Vector2 frameStartPoint, Vector2 frameEndPoint) {
					Target = target;
					StartSize = startSize;
					StartPos = startPos;
					targetSize = startSize;
					targetPosition = startPos;

					Vector2 frameSize = frameEndPoint - frameStartPoint;

					LeftRatio = (startPos.X - frameStartPoint.X) / frameSize.X;
					RightRatio = (frameEndPoint.X - (startPos.X + startSize.X)) / frameSize.X;
					TopRatio = (startPos.Y - frameStartPoint.Y) / frameSize.Y;
					BottomRatio = (frameEndPoint.Y - (startPos.Y + startSize.Y)) / frameSize.Y;
				}

				public string DebugInfo() {
					return $"{Target.Identity.Name} - " +
						$"<{Math.Round(LeftRatio, 2)} " +
						$">{Math.Round(RightRatio, 2)} " +
						$"^{Math.Round(TopRatio, 2)} " +
						$"v{Math.Round(BottomRatio, 2)}";
				}
			}
		}
		private enum Direction {
			L, LT, T, RT, R, RB, B, LB
		}
	}
}
