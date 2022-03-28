using System.Windows.Controls;

namespace MindMap.Entities.Elements.Interfaces {
	public interface ITextGrid: ITextContainer {
		TextBox MyTextBox { get; set; }
		TextBlock MyTextBlock { get; set; }

		void SubmitTextChange();
		void ShowTextBox();
		void ShowTextBlock();
	}
}
