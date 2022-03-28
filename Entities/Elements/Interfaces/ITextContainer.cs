using System.Windows;
using System.Windows.Media;

namespace MindMap.Entities.Elements.Interfaces {
	public interface ITextContainer {
		string Text { get; set; }
		FontFamily FontFamily { get; set; }
		FontWeight FontWeight { get; set; }
		double FontSize { get; set; }
		Color FontColor { get; set; }
	}
}
