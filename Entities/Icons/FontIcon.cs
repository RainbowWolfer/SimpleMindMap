using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMap.Entities.Icons {
	public class FontIcon: IconElement {
		public const string SEGOE_ASSETS = "Segoe MDL2 Assets";
		public const string SEGOE_FLUENT = "Segoe Fluent Icons";
		public const string SEGOE_SYMBOL = "Segoe UI Symbol";
		public string FontFamily { get; set; }
		public double FontSize { get; set; }
		public string Icon { get; set; }
		public Thickness Margin { get; set; } = new Thickness(0);
		public FontIcon(string icon, double fontSize = 12, string fontFamily = SEGOE_ASSETS) {
			FontSize = fontSize;
			Icon = icon;
			FontFamily = fontFamily;
		}

		public override FrameworkElement Generate() {
			return new TextBlock() {
				FontFamily = new FontFamily(FontFamily),
				Text = Icon,
				FontSize = FontSize,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = TextAlignment.Center,
				Margin = Margin,
			};
		}
	}
}

/**
 * public static class Icons {
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
 */