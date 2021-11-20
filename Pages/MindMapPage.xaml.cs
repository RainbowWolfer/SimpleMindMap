using MindMap.Entities;
using MindMap.Entities.Connections;
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

		private static ResizeFrame? Selection => ResizeFrame.Current;

		public void ClearResizePanel() {
			Selection?.ClearResizeFrame(MainCanvas);
			if(previous != null && elements.ContainsKey(previous)) {
				elements[previous].SetConnectionsFrameVisible(true);
				elements[previous].UpdateConnectionsFrame();
			}
		}

		public void Deselect() {
			if(Selection != null && elements.ContainsKey(Selection.target)) {
				elements[Selection.target].Deselect();
			}
		}

		private ConnectionPath? previewLine;

		public void UpdatePreviewLine(ConnectionControl from, Vector2 to) {
			if(previewLine == null) {
				previewLine = new ConnectionPath(MainCanvas, from, to);
			}
			previewLine.Update(to);
		}

		public void ClearPreviewLine() {
			if(previewLine == null) {
				return;
			}
			MainCanvas.Children.Remove(previewLine.Path);
			previewLine = null;
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

		public readonly Dictionary<FrameworkElement, Element> elements = new();

		public List<ConnectionControl> GetAllConnectionDots(Element? self) {
			List<ConnectionControl> result = new();
			foreach(Element item in elements.Values) {
				if(item == self) {
					continue;
				}
				item.GetAllConnectionDots().ForEach(result.Add);
			}
			return result;
		}

		private void AddToElementsDictionary(Element value) {
			FrameworkElement key = value.CreateFramework();
			value.CreateConnectionsFrame();
			value.CreateFlyoutMenu();
			key.MouseDown += Element_MouseDown;
			elements.Add(key, value);
		}

		private void AddElement(Type type) {
			if(type == typeof(MyRectangle)) {
				AddToElementsDictionary(new MyRectangle(this));
			} else if(type == typeof(TextGrid)) {
				AddToElementsDictionary(new TextGrid(this));
			} else if(type == typeof(MyEllipse)) {
				AddToElementsDictionary(new MyEllipse(this));
			}
		}

		public void RemoveElement(Element element) {
			if(element.Target == null || !elements.ContainsKey(element.Target) || !MainCanvas.Children.Contains(element.Target)) {
				return;
			}
			Deselect();
			ClearResizePanel();
			elements.Remove(element.Target);
			MainCanvas.Children.Remove(element.Target);
		}

		private void Element_MouseDown(object sender, MouseButtonEventArgs e) {
			FrameworkElement? target = sender as FrameworkElement;
			current = target;
			if(current != Selection?.target) {
				ClearResizePanel();
				Deselect();
			}
			startPos = e.GetPosition(MainCanvas);
			offset = startPos - new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
			Mouse.Capture(sender as UIElement);
			hasMoved = false;
			if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Left;
			} else if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Right;
			} else if(e.MouseDevice.MiddleButton == MouseButtonState.Pressed) {
				mouseType = MouseType.Middle;
			}

		}

		private void AddRectableButton_Click(object sender, RoutedEventArgs e) {
			AddElement(typeof(MyRectangle));
		}

		private void AddGridButton_Click(object sender, RoutedEventArgs e) {
			AddElement(typeof(TextGrid));
		}

		private void AddEllipseButton_Click(object sender, RoutedEventArgs e) {
			AddElement(typeof(MyEllipse));
		}

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

			Selection?.UpdateResizeFrame();

			if(current != null && elements.ContainsKey(current)) {
				elements[current].UpdateConnectionsFrame();
			}
		}

		private FrameworkElement? previous;
		private int clickCount = 0;
		private int lastClickTimeStamp;
		private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			if(current != null && startPos == e.GetPosition(MainCanvas) && !hasMoved) {
				Element? element = elements.ContainsKey(current) ? elements[current] : null;
				switch(mouseType) {
					case MouseType.Left:
						ResizeFrame.Create(this, current);
						element?.SetConnectionsFrameVisible(false);
						Debug.WriteLine("left mouse");
						clickCount = previous == current && e.Timestamp - lastClickTimeStamp <= 500 ? clickCount + 1 : 0;
						lastClickTimeStamp = e.Timestamp;
						Debug.WriteLine(clickCount);
						if(clickCount == 1) {
							Debug.WriteLine("This is double click");
							element?.DoubleClick();
						} else {
							element?.LeftClick();//care
						}
						break;
					case MouseType.Middle:
						Debug.WriteLine("middle mouse");
						element?.MiddleClick();
						break;
					case MouseType.Right:
						Debug.WriteLine("flyout menu");
						element?.RightClick();
						break;
					default:
						throw new Exception($"Mouse Type Error {mouseType}");
				}
				previous = current;
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


		private enum MouseType {
			Left, Middle, Right
		}

	}
}