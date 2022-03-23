using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
