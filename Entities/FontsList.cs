using System.Windows;

namespace MindMap.Entities {
	public static class FontsList {
		public static string[] AvailableFonts => new string[] {
			"Microsoft YaHei UI",
			"Consolas",
			"Cascadia Code",
			"Lucida Calligraphy",
		};

		public static FontWeight[] AllFontWeights => new FontWeight[] {
			FontWeights.Black, FontWeights.Normal, FontWeights.Bold, FontWeights.Medium, FontWeights.ExtraBold, FontWeights.SemiBold, FontWeights.Thin, FontWeights.ExtraLight, FontWeights.DemiBold, FontWeights.ExtraBlack, FontWeights.Regular, FontWeights.UltraLight, FontWeights.UltraBold, FontWeights.UltraBlack, FontWeights.Heavy, FontWeights.Light
		};
	}
}