using System.Windows;
using System.Windows.Media;

namespace MindMap.Entities.Elements.Interfaces {
	public interface IBorderBasedStyle {
		Brush Background { get; set; }
		Brush BorderColor { get; set; }
		Thickness BorderThickness { get; set; }
	}
}
