using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MindMap.Entities.Icons {
	public class ImageIcon: IconElement {
		public double Height { get; set; } = 20;
		public double Width { get; set; } = 20;
		public string Path { get; set; }
		public ImageIcon(string path) {
			Path = path;
		}

		public override FrameworkElement Generate() {
			Border border = new() {
				Width = Height,
				Height = Width,
				Child = new Image() {
					Source = new BitmapImage(new Uri(Path)),
					Height = Height,
					Width = Width,
				}
			};
			return border;
		}

	}
}
