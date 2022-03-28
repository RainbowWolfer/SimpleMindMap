using System.Windows.Input;

namespace MindMap.Entities.Interactions {
	public interface IInteractive {


		void DoubleClick();
		void LeftClick(MouseButtonEventArgs e);
		void MiddleClick();
		void RightClick();
		void Deselect();
	}
}
