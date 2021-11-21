using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MindMap.Entities.Elements.Interfaces {
	public interface IBorderBasedStyle {
		public Brush Background { get; set; }
		public Brush BorderColor { get; set; }
		public Thickness BorderThickness { get; set; }
	}
}
