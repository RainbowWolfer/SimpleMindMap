using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MindMap.Entities.Elements.Interfaces {
	public interface ITextShadow {
		DropShadowEffect TextShadowEffect { get; set; }
		bool EnableTextShadow { get; set; }
		double TextShadowBlurRadius { get; set; }
		double TextShadowDepth { get; set; }
		double TextShadowDirection { get; set; }
		Color TextShadowColor { get; set; }
		double TextShadowOpacity { get; set; }
	}
}
