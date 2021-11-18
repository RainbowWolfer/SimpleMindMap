using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyRectangle: Element {
		private Rectangle? _rect;
		public MyRectangle(MindMapPage parent) : base(parent) {

		}

		public override FrameworkElement? Target => _rect;

		public override FrameworkElement CreateFramework(Canvas mainCanvas) {
			_rect = new Rectangle() {
				Width = 50,
				Height = 50,
				Fill = new SolidColorBrush(Colors.Red),
				RenderTransform = new TranslateTransform(0, 0),
			};
			_rect.SetValue(Canvas.TopProperty, 0.0);
			_rect.SetValue(Canvas.LeftProperty, 0.0);

			mainCanvas.Children.Add(_rect);
			return _rect;
		}

		public override void Deselect() {
			Debug.WriteLine($"Rectangle Deselect");
		}

		public override void DoubleClick() {
			Debug.WriteLine($"Rectangle Double");
		}

		public override void LeftClick() {
			Debug.WriteLine($"Rectangle Left");
		}

		public override void MiddleClick() {
			Debug.WriteLine($"Rectangle Middle");
		}

		public override void RightClick() {
			Debug.WriteLine($"Rectangle Right");
		}
	}
}
