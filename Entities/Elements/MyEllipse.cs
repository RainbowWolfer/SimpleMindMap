using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyEllipse: Element {
		private Ellipse? _ellipse;
		public MyEllipse(MindMapPage parent) : base(parent) {

		}

		public override FrameworkElement? Target => _ellipse;
		public override FrameworkElement CreateFramework(Canvas mainCanvas) {
			_ellipse = new Ellipse() {
				Width = 50,
				Height = 50,
				Fill = new SolidColorBrush(Colors.Red),
				RenderTransform = new TranslateTransform(0, 0),
			};
			_ellipse.SetValue(Canvas.TopProperty, 0.0);
			_ellipse.SetValue(Canvas.LeftProperty, 0.0);

			mainCanvas.Children.Add(_ellipse);
			return _ellipse;
		}

		public override void Deselect() {
			
		}

		public override void DoubleClick() {
			
		}

		public override void LeftClick() {
			
		}

		public override void MiddleClick() {
			
		}

		public override void RightClick() {
			
		}
	}
}
