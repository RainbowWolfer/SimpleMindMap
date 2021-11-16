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
	public partial class MindMapPage: Page {

		public MindMapPage() {
			InitializeComponent();

			MainCanvas.MouseMove += Rect_MouseMove;
			MainCanvas.MouseUp += Rect_MouseUp;

			SizeChanged += (s, e) => UpdateBackgroundDot();
		}

		private ResizePanel? _selection;
		private void ClearResizePanel() {
			if(_selection == null) {
				return;
			}
			void Remove(Line line) {
				if(MainCanvas.Children.Contains(line)) {
					MainCanvas.Children.Remove(line);
				}
			}
			Remove(_selection.top);
			Remove(_selection.bot);
			Remove(_selection.left);
			Remove(_selection.right);
		}

		private void CreateResizePanel(Shape target) {
			ClearResizePanel();
			Style style = new(typeof(Line));
			style.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Gray));
			style.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 2 })));
			style.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)3));
			Line top = new() { Style = style };
			Line bottom = new() { Style = style };
			Line left = new() { Style = style };
			Line right = new() { Style = style };
			MainCanvas.Children.Add(top);
			MainCanvas.Children.Add(bottom);
			MainCanvas.Children.Add(left);
			MainCanvas.Children.Add(right);
			_selection = new ResizePanel(target, top, bottom, left, right);
			UpdateResizePanel();
		}

		private void UpdateResizePanel() {
			if(_selection == null || !_selection.IsValid) {
				return;
			}
			Vector2 startPoint = new(Canvas.GetLeft(_selection.target), Canvas.GetTop(_selection.target));
			Vector2 size = new(_selection.target.ActualWidth, _selection.target.ActualHeight);

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
			//Debug.WriteLine(backgroundPool.Count);
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

		private Vector2 offset;
		private Vector2 startPos;//check for click
		private Shape? current;
		private bool hasMoved;
		private void Rect_MouseMove(object sender, MouseEventArgs e) {
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
			if(e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
				return;
			}
			Shape? target = sender as Shape;
			current = target;
			startPos = e.GetPosition(MainCanvas);
			offset = startPos - new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
			Mouse.Capture(sender as UIElement);
			hasMoved = false;
		}

		private void Rect_MouseUp(object sender, MouseButtonEventArgs e) {
			if(current != null && startPos == e.GetPosition(MainCanvas) && !hasMoved) {
				Debug.WriteLine("This is a click");
				//current.Width += 20;
				//current.Height += 20;
				CreateResizePanel(current);
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
		}

		private void MainCanvas_Loaded(object sender, RoutedEventArgs e) {
			UpdateBackgroundDot();
		}


		private class ResizePanel {
			public Shape target;
			public Line top;
			public Line bot;
			public Line left;
			public Line right;

			public ResizePanel(Shape target, Line top, Line bot, Line left, Line right) {
				this.target = target;
				this.top = top;
				this.bot = bot;
				this.left = left;
				this.right = right;
			}

			public bool IsValid => target != null && top != null && bot != null && left != null && right != null;
		}
	}
}
