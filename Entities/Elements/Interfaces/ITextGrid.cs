using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMap.Entities.Elements.Interfaces {
	public interface ITextGrid {
		public TextBox MyTextBox { get; set; }
		public TextBlock MyTextBlock { get; set; }

		public string Text { get; set; }
		public FontFamily FontFamily { get; set; }
		public FontWeight FontWeight { get; set; }
		public double FontSize { get; set; }
		public Color FontColor { get; set; }

		public void UpdateText();
		public void ShowTextBox();
		public void ShowTextBlock();
	}
}
