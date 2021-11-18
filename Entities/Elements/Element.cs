using MindMap.Entities.Frames;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public abstract class Element {
		protected MindMapPage parent;

		protected ConnectionsFrame? connectionsFrame;
		protected Canvas MainCanvas => parent.MainCanvas;
		public Element(MindMapPage parent) {
			this.parent = parent;
		}

		public void CreateConnectionsFrame() {
			if(Target != null) {
				connectionsFrame = new ConnectionsFrame(this.parent, Target);
			}
		}
		public void UpdateConnectionsFrame() {
			connectionsFrame?.UpdateConnections();
		}

		public void SetConnectionsFrameVisible(bool visible) => connectionsFrame?.SetVisible(visible);
		public List<Ellipse> GetAllConnectionDots() {
			if(connectionsFrame == null) {
				return new List<Ellipse>();
			}
			return connectionsFrame.topDots.Concat(
				connectionsFrame.botDots.Concat(
					connectionsFrame.leftDots.Concat(
						connectionsFrame.rightDots
					)
				)
			).ToList();
		}

		public abstract FrameworkElement CreateFramework();
		public abstract FrameworkElement? Target { get; }

		public abstract void DoubleClick();
		public abstract void LeftClick();
		public abstract void MiddleClick();
		public abstract void RightClick();
		public abstract void Deselect();

		public virtual void Delete() => parent.RemoveElement(this);
	}
}
