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

			DebugMousePosition();
		}

		private async void DebugMousePosition() {
			while(true) {
				CurrentMousePositionText.Text = (Vector2)Mouse.GetPosition(MainCanvas);
				await Task.Delay(10);
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


		private bool _drag;
		private Vector2 offset;
		private void Rect_MouseMove(object sender, MouseEventArgs e) {
			if(!_drag) {
				return;
			}
			Rectangle? found = MainCanvas.Children.Cast<Shape>().FirstOrDefault(s => s is Rectangle) as Rectangle;

			Vector2 mouse_position = e.GetPosition(MainCanvas);

			Canvas.SetLeft(found, mouse_position.X);
			Canvas.SetTop(found, mouse_position.Y);

			CurrentPositionText.Text = new Vector2(Canvas.GetLeft(found), Canvas.GetTop(found));
		}

		private void Rect_MouseDown(object sender, MouseButtonEventArgs e) {
			_drag = true;
			//offset=
			Mouse.Capture(sender as UIElement);
		}

		private void Rect_MouseUp(object sender, MouseButtonEventArgs e) {
			_drag = false;
			Mouse.Capture(null);
		}

	}
}
