using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMap.Entities {
	public static class Icons {
		public static TextBlock CreateIcon(string icon, double fontsize = 10) {
			return new TextBlock() {
				FontFamily = new FontFamily("Segoe MDL2 Assets"),
				Text = icon,
				FontSize = fontsize,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = TextAlignment.Center,
			};
		}
	}
}
