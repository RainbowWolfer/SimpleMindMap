using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MindMap.Entities.Elements.Interfaces {
	public interface IGridShadow {
		DropShadowEffect ShadowEffect { get; set; }
		bool EnableShadow { get; set; }
		double ShadowBlurRadius { get; set; }
		double ShadowDepth { get; set; }
		double ShadowDirection { get; set; }
		Color ShadowColor { get; set; }
		double ShadowOpacity { get; set; }
	}
}
