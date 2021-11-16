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

			UpdateBackgroundDot();
			DebugMousePosition();
		}

		private async void DebugMousePosition() {
			while(true) {
				CurrentMousePositionText.Text = (Vector2)Mouse.GetPosition(MainCanvas);
				await Task.Delay(10);
			}
		}

		private void CreateResizePanel() {

		}

		private void UpdateBackgroundDot() {
			for(int i = 0; i < 100; i++) {
				for(int j = 0; j < 100; j++) {
					var t = new Ellipse() {
						Width = 2,
						Height = 2,
						Fill = new SolidColorBrush(Colors.Gray),
					};
					Canvas.SetLeft(t, i * 10);
					Canvas.SetTop(t, j * 10);
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

		private Vector2 offset;
		private Vector2 startPos;//check for click
		private Shape current;
		private bool hasMoved;
		private void Rect_MouseMove(object sender, MouseEventArgs e) {
			if(current == null || e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
				return;
			}
			hasMoved = true;
			Vector2 mouse_position = e.GetPosition(MainCanvas);

			Canvas.SetLeft(current, mouse_position.X - offset.X);
			Canvas.SetTop(current, mouse_position.Y - offset.Y);

			CurrentPositionText.Text = new Vector2(Canvas.GetLeft(current), Canvas.GetTop(current));
		}

		private void Rect_MouseDown(object sender, MouseButtonEventArgs e) {
			if(e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
				return;
			}
			var target = sender as Shape;
			current = target;
			startPos = e.GetPosition(MainCanvas);
			offset = startPos - new Vector2(Canvas.GetLeft(target), Canvas.GetTop(target));
			Mouse.Capture(sender as UIElement);
			hasMoved = false;
		}

		private void Rect_MouseUp(object sender, MouseButtonEventArgs e) {
			current = null;
			Mouse.Capture(null);
			if(startPos == e.GetPosition(MainCanvas) && !hasMoved) {
				Debug.WriteLine("This is a click");
			}
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
		}

		private void BackgroundRectangle_MouseUp(object sender, MouseButtonEventArgs e) {
			_drag = false;
		}
	}
}
