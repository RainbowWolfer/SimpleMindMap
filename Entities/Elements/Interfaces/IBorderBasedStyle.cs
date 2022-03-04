using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements.Interfaces {
	public interface IBorderBasedStyle {
		Brush Background { get; set; }
		Brush BorderColor { get; set; }
		Thickness BorderThickness { get; set; }
	}
}
