﻿using MindMap.Entities.Frames;
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
		public long ID { get; private set; }
		protected MindMapPage parent;

		protected ConnectionsFrame? connectionsFrame;
		protected Canvas MainCanvas => parent.MainCanvas;

		public abstract FrameworkElement Target { get; }

		public Element(MindMapPage parent) {
			this.parent = parent;
			ID = DateTime.Now.Ticks + GetHashCode();
		}

		public void CreateConnectionsFrame() {
			connectionsFrame = new ConnectionsFrame(this.parent, this);
		}

		public void UpdateConnectionsFrame() {//also includes connected dots
			connectionsFrame?.UpdateConnections();
		}

		public void SetConnectionsFrameVisible(bool visible) => connectionsFrame?.SetVisible(visible);

		public List<ConnectionControl> GetAllConnectionDots() => connectionsFrame == null ? new List<ConnectionControl>() : connectionsFrame.AllDots;

		//public abstract FrameworkElement CreateFramework();

		public virtual void CreateFlyoutMenu() {
			FlyoutMenu.CreateBase(Target, (s, e) => Delete());
		}

		public abstract List<Panel> CreatePropertiesList();

		public abstract void UpdateStyle();

		public abstract void DoubleClick();
		public abstract void LeftClick();
		public abstract void MiddleClick();
		public abstract void RightClick();
		public abstract void Deselect();

		public virtual void Delete() {
			parent.RemoveElement(this);
			connectionsFrame?.ClearConnections();
		}
	}
}
