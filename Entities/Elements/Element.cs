using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Entities.Elements {
	public abstract class Element {
		protected MindMapPage parent;
		public Element(MindMapPage parent) {
			this.parent = parent;
		}

		public abstract FrameworkElement CreateFramework(Canvas mainCanvas);
		public abstract FrameworkElement? Target { get; }

		public abstract void DoubleClick();
		public abstract void LeftClick();
		public abstract void MiddleClick();
		public abstract void RightClick();
		public abstract void Deselect();

		public virtual void Delete() => parent.RemoveElement(this);
	}
}
