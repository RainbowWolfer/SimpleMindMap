using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace MindMap.Entities.Elements.Interfaces {
	public interface ITextContainer {
		string Text { get; set; }
		FontFamily FontFamily { get; set; }
		FontWeight FontWeight { get; set; }
		double FontSize { get; set; }
		Color FontColor { get; set; }
	}
}
