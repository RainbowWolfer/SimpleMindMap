using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMap.Entities.Elements.Interfaces {
	public interface ITextGrid: ITextContainer {
		TextBox MyTextBox { get; set; }
		TextBlock MyTextBlock { get; set; }

		void SubmitTextChange();
		void ShowTextBox();
		void ShowTextBlock();
	}
}
