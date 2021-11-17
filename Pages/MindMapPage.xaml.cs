using MindMap.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MindMap.Pages {
	public partial class MindMapPage: Page {//Editor Page

		public MindMapPage() {
			InitializeComponent();

			MainCanvas.MouseMove += MainCanvas_MouseMove;
			MainCanvas.MouseUp += MainCanvas_MouseUp;

			SizeChanged += (s, e) => UpdateBackgroundDot();
		}

		private ResizePanel? _selection;
		private void ClearResizePanel() {
			if(_selection == null) {
				return;
			}
			void Remove(Shape shape) {
				if(MainCanvas.Children.Contains(shape)) {
					MainCanvas.Children.Remove(shape);
				}
			}
			Remove(_selection.top);
			Remove(_selection.bot);
			Remove(_selection.left);
			Remove(_selection.right);
			Remove(_selection.top_left);
			Remove(_selection.top_right);
			Remove(_selection.bot_left);
			Remove(_selection.bot_right);
		}

		private void Deselect() {
			if(_selection != null && _selection.target is Grid grid) {
				if(grid.Children.Cast<UIElement>().FirstOrDefault(i => i is TextBox box) is TextBox box) {
					grid.Children.Add(new TextBlock() { Text = box.Text, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 15 });
					grid.Children.Remove(box);
				}
			}
		}

		private void CreateResizePanel(FrameworkElement target) {
			ClearResizePanel();
			Style style_line = new(typeof(Line));
			style_line.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Gray));
			style_line.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 0.5 })));
			style_line.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));
			//line pattern cannot handle mousedown, consider using rectangle -> stroke
			Line top = new() { Style = style_line };
			Line bottom = new() { Style = style_line };
			Line left = new() { Style = style_line };
			Line right = new() { Style = style_line };
			MainCanvas.Children.Add(top);
			MainCanvas.Children.Add(bottom);
			MainCanvas.Children.Add(left);
			MainCanvas.Children.Add(right);

			Style style_rect = new(typeof(Rectangle));
			style_rect.Setters.Add(new Setter(Shape.FillProperty, Brushes.Transparent));
			style_rect.Setters.Add(new Setter(WidthProperty, (double)10));
			style_rect.Setters.Add(new Setter(HeightProperty, (double)10));
			style_rect.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));
			style_rect.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Gray));
			style_rect.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 1 })));

			Rectangle top_left = new() { Style = style_rect };
			Rectangle top_right = new() { Style = style_rect };
			Rectangle bot_left = new() { Style = style_rect };
			Rectangle bot_right = new() { Style = style_rect };
			MainCanvas.Children.Add(top_left);
			MainCanvas.Children.Add(top_right);
			MainCanvas.Children.Add(bot_left);
			MainCanvas.Children.Add(bot_right);

			_selection = new ResizePanel(this, target, top, bottom, left, right, top_left, top_right, bot_left, bot_right);
			UpdateResizePanel();
		}

		private void UpdateResizePanel() {
			if(_selection == null || !_selection.IsValid) {
				return;
			}
			Vector2 startPoint = new(Canvas.GetLeft(_selection.target), Canvas.GetTop(_selection.target));
			Vector2 size = new(_selection.target.Width, _selection.target.Height);

			_selection.top.X1 = startPoint.X;
			_selection.top.Y1 = startPoint.Y;
			_selection.top.X2 = startPoint.X + size.X;
			_selection.top.Y2 = startPoint.Y;

			_selection.bot.X1 = startPoint.X;
			_selection.bot.Y1 = startPoint.Y + size.Y;
			_selection.bot.X2 = startPoint.X + size.X;
			_selection.bot.Y2 = startPoint.Y + size.Y;

			_selection.left.X1 = startPoint.X;
			_selection.left.Y1 = startPoint.Y;
			_selection.left.X2 = startPoint.X;
			_selection.left.Y2 = startPoint.Y + size.Y;

			_selection.right.X1 = startPoint.X + size.X;
			_selection.right.Y1 = startPoint.Y;
			_selection.right.X2 = startPoint.X + size.X;
			_selection.right.Y2 = startPoint.Y + size.Y;

			Canvas.SetLeft(_selection.top_left, startPoint.X - _selection.top_left.Width / 2);
			Canvas.SetTop(_selection.top_left, startPoint.Y - _selection.top_left.Height / 2);

			Canvas.SetLeft(_selection.top_right, startPoint.X + size.X - _selection.top_right.Width / 2);
			Canvas.SetTop(_selection.top_right, startPoint.Y - _selection.top_right.Height / 2);

			Canvas.SetLeft(_selection.bot_left, startPoint.X - _selection.bot_left.Width / 2);
			Canvas.SetTop(_selection.bot_left, startPoint.Y + size.Y - _selection.bot_left.Height / 2);

			Canvas.SetLeft(_selection.bot_right, startPoint.X + size.X - _selection.bot_right.Width / 2);
			Canvas.SetTop(_selection.bot_right, startPoint.Y + size.Y - _selection.bot_right.Height / 2);

		}

		private readonly Dictionary<Vector2, Shape> backgroundPool = new();
		private const int SIZE = 4;
		private const int GAP = 20;
		private void UpdateBackgroundDot() {
			var offset = -new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y).ToInt() / GAP;
			var size = new Vector2(MainCanvas.ActualWidth, MainCanvas.ActualHeight).ToInt() / GAP;
			for(int i = offset.X_Int; i < size.X + offset.X; i++) {
				for(int j = offset.Y_Int; j < size.Y + offset.Y; j++) {
					if(backgroundPool.ContainsKey(new Vector2(i, j))) {
						continue;
					}
					Ellipse t = new() {
						Width = SIZE,
						Height = SIZE,
						Fill = new SolidColorBrush(Colors.Gray),
					};
					Canvas.SetLeft(t, i * GAP);
					Canvas.SetTop(t, j * GAP);
					backgroundPool.Add(new Vector2(i, j), t);
					BackgroundCanvas.Children.Add(t);
				}
			}
		}

		private void AddRectableButton_Click(object sender, RoutedEventArgs e) {
			Rectangle rect = new() {
				Width = 50,
				Height = 50,
				Fill = new SolidColorBrush(Colors.Red),
				RenderTransform = new TranslateTransform(0, 0),
			};
			rect.SetValue(Canvas.TopProperty, 0.0);
			rect.SetValue(Canvas.LeftProperty, 0.0);
			rect.MouseDown += Rect_MouseDown;
			MainCanvas.Children.Add(rect);
		}

		private void AddGridButton_Click(object sender, RoutedEventArgs e) {
			Grid grid = new() {
				Width = 200,
				Height = 200,
				Background = Brushes.Gray,
				RenderTransform = new TranslateTransform(0, 0),
			};
			grid.SetValue(Canvas.TopProperty, 0.0);
			grid.SetValue(Canvas.LeftProperty, 0.0);
			grid.MouseDown += Rect_MouseDown;
			grid.Children.Add(new TextBlock() { Text = "Input Your Text", TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 15 });
			MainCanvas.Children.Add(grid);
		}

		private int clickCount = 0;
		private int lastClickTimeStamp;
		private Vector2 offset;
		private Vector2 startPos;//check for click
		private FrameworkElement? current;
		private bool hasMoved;
		private MouseType mouseType;
		private void MainCanvas_MouseMove(object sender, MouseEventArgs e) {
			if(current == null || e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
				return;
			}
			hasMoved = true;
			Vector2 mouse_position = e.GetPosition(MainCanvas);

			Canvas.SetLeft(current, mouse_position.X - offset.X);
			Canvas.SetTop(current, mouse_position.Y - offset.Y);

			UpdateResizePanel();
		}

		private void Rect_MouseDown(object sender, MouseButtonEventArgs e) {
			FrameworkElement? target = sender as FrameworkElement;
			current = target;
			if(current != _selection?.target) {
				ClearResizePanel();
				Deselect();
			}
			startPos = e.GetPosition(MainCanvas);
			offset = startPos - new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
			Mouse.Capture(sender as UIElement);
			hasMoved = false;
			if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Left;
				clickCount = e.Timestamp - lastClickTimeStamp <= 200 ? clickCount + 1 : 0;
				lastClickTimeStamp = e.Timestamp;
				if(clickCount == 1) {
					Debug.WriteLine("This is double click");
					if(current is Grid grid) {
						if(grid.Children.Cast<UIElement>().FirstOrDefault(i => i is TextBlock) is TextBlock tb) {
							var box = new TextBox() {
								Text = tb.Text,
								TextWrapping = TextWrapping.Wrap,
								HorizontalAlignment = HorizontalAlignment.Center,
								VerticalAlignment = VerticalAlignment.Center,
								FontSize = 15,
								AcceptsReturn = true,
								AcceptsTab = true,
							};
							box.KeyDown += (s, e) => {
								if(e.Key == Key.Escape) {
									ClearResizePanel();
									Deselect();
								}
							};
							grid.Children.Add(box);
							grid.Children.Remove(tb);
							box.Focus();
						}
					}
				}
			} else if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Right;
			} else if(e.MouseDevice.MiddleButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Middle;
			}
		}

		private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			if(current != null && startPos == e.GetPosition(MainCanvas) && !hasMoved) {
				switch(mouseType) {
					case MouseType.Left:
						CreateResizePanel(current);
						break;
					case MouseType.Middle:
						Debug.WriteLine("middle mouse");
						break;
					case MouseType.Right:
						Debug.WriteLine("flyout menu");
						break;
					default:
						throw new Exception($"Mouse Type Error {mouseType}");
				}
			}
			current = null;
			Mouse.Capture(null);
		}

		private bool _drag;
		private Vector2 _dragStartPos;
		private Vector2 _translatStartPos;
		private void BackgroundRectangle_MouseDown(object sender, MouseButtonEventArgs e) {
			_dragStartPos = e.GetPosition(this);
			_translatStartPos = new Vector2(MainCanvas_TranslateTransform.X, MainCanvas_TranslateTransform.Y);
			_drag = true;
		}

		private void BackgroundRectangle_MouseMove(object sender, MouseEventArgs e) {
			if(!_drag) {
				return;
			}
			Vector2 delta = e.GetPosition(this) - _dragStartPos;
			MainCanvas_TranslateTransform.X = delta.X + _translatStartPos.X;
			MainCanvas_TranslateTransform.Y = delta.Y + _translatStartPos.Y;

			UpdateBackgroundDot();
		}

		private void BackgroundRectangle_MouseUp(object sender, MouseButtonEventArgs e) {
			_drag = false;
			ClearResizePanel();
			Deselect();
		}

		private void MainCanvas_Loaded(object sender, RoutedEventArgs e) {
			UpdateBackgroundDot();
		}


		private class ResizePanel {
			public FrameworkElement target;//FrameworkElement
			public Line top;
			public Line bot;
			public Line left;
			public Line right;

			public Rectangle top_left;
			public Rectangle top_right;
			public Rectangle bot_left;
			public Rectangle bot_right;

			public ResizeControl control_top;
			public ResizeControl control_bot;
			public ResizeControl control_left;
			public ResizeControl control_right;
			public ResizeControl control_top_left;
			public ResizeControl control_top_right;
			public ResizeControl control_bot_left;
			public ResizeControl control_bot_right;

			public ResizePanel(MindMapPage parent, FrameworkElement target, Line top, Line bot, Line left, Line right, Rectangle top_left, Rectangle top_right, Rectangle bot_left, Rectangle bot_right) {
				this.target = target;
				this.top = top;
				this.bot = bot;
				this.left = left;
				this.right = right;
				this.top_left = top_left;
				this.top_right = top_right;
				this.bot_left = bot_left;
				this.bot_right = bot_right;

				control_top = new ResizeControl(parent, target, top, ResizeControl.Direction.T);
				control_bot = new ResizeControl(parent, target, bot, ResizeControl.Direction.B);
				control_left = new ResizeControl(parent, target, left, ResizeControl.Direction.L);
				control_right = new ResizeControl(parent, target, right, ResizeControl.Direction.R);

				control_top_left = new ResizeControl(parent, target, top_left, ResizeControl.Direction.LT);
				control_top_right = new ResizeControl(parent, target, top_right, ResizeControl.Direction.RT);
				control_bot_left = new ResizeControl(parent, target, bot_left, ResizeControl.Direction.LB);
				control_bot_right = new ResizeControl(parent, target, bot_right, ResizeControl.Direction.RB);

			}

			public bool IsValid => target != null && top != null && bot != null && left != null && right != null;
		}

		private class ResizeControl {
			public FrameworkElement target;
			public Shape shape;
			public Direction direction;

			private Vector2 startMousePos;
			private Vector2 startSize;
			private Vector2 startPos;
			private bool _drag;
			public ResizeControl(MindMapPage parent, FrameworkElement target, Shape shape, Direction direction) {
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
					parent.UpdateResizePanel();
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
		private enum MouseType {
			Left, Middle, Right
		}

	}
}