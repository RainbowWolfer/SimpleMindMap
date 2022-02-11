using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MindMap.Entities.Icons {
	public class ImageIcon: IconElement {
		public double Height { get; set; } = 15;
		public double Width { get; set; } = 15;
		public string Path { get; set; }
		public ImageIcon(string path) {
			Path = path;
		}

		public override FrameworkElement Generate() {
			return new Image() {
				Source = new BitmapImage(new Uri(Path)),
				Height = Height,
				Width = Width,
			};
		}

	}
}
