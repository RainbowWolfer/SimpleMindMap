﻿using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MindMap.Entities {
	public class EditHistory {
		private readonly List<IChange> previous = new();
		private readonly List<IChange> future = new();

		private readonly MindMapPage _parent;
		public EditHistory(MindMapPage parent) {
			_parent = parent;
		}

		public void SubmitByElementCreated(Element target) {
			previous.Add(new ElementCreatedOrDeleted(CreateOrDelete.Create, target));
			future.Clear();
		}
		public void SubmitByElementDeleted(Element target) {
			previous.Add(new ElementCreatedOrDeleted(CreateOrDelete.Delete, target));
			future.Clear();
		}
		public void SubmitByElementPropertyChanged(Element target, IProperty property) {
			previous.Add(new ElementPropertyChange(target, property));
		}
		public void SubmitByElementFrameworkChanged(Element target, Vector2 size, Vector2 position) {
			previous.Add(new ElementFrameworkChange(target, size, position));
		}

		public void SumbitByConnectionCreated(ConnectionPath path) {
			//previous.Add(new ConnectionCreateOrDelete(CreateOrDelete.Create, path));
		}
		public void SumbitByConnectionDeleted(ConnectionPath path) {
			//previous.Add(new ConnectionCreateOrDelete(CreateOrDelete.Delete, path));
		}
		public void SubmitByConnectionChange() {

		}

		public void Undo() {
			if(previous.Count < 1) {
				return;
			}
			IChange last = previous[^1];
			if(last is ElementCreatedOrDeleted cod) {
				switch(cod.Type) {
					case CreateOrDelete.Create:
						cod.Target.Delete(false);
						break;
					case CreateOrDelete.Delete:
						_parent.AddElementFromHistory(cod.Target);
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(last is ElementPropertyChange pc) {
				IProperty TargetProperty = pc.Property;
				pc.Property = (IProperty)pc.Target.Properties.Clone();
				pc.Target.SetProperty(TargetProperty);
			} else if(last is ElementFrameworkChange fc) {
				fc.Target.SetPosition(fc.Position);
				fc.Target.SetSize(fc.Size);
			}
			previous.RemoveAt(previous.Count - 1);
			future.Insert(0, last);
		}

		public void Redo() {
			if(future.Count < 1) {
				return;
			}
			IChange first = future[0];
			if(first is ElementCreatedOrDeleted cod) {
				switch(cod.Type) {
					case CreateOrDelete.Create:
						//go create
						_parent.AddElementFromHistory(cod.Target);
						break;
					case CreateOrDelete.Delete:
						cod.Target.Delete();
						//go delete
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(first is ElementPropertyChange pc) {
				IProperty TargetProperty = pc.Property;
				pc.Property = (IProperty)pc.Target.Properties.Clone();
				pc.Target.SetProperty(TargetProperty);
			}
			future.RemoveAt(0);
			previous.Add(first);
		}

		private interface IChange {

		}

		private class ConnectionCreateOrDelete: IChange {
			public CreateOrDelete CreateOrDeleteType { get; protected set; }
			public ConnectionPath Path { get; protected set; }
		}

		private class ConnectionChange: IChange {
			public ConnectionPath Target { get; protected set; }
			public IProperty Property { get; private set; }
		}

		private class ElementCreatedOrDeleted: IChange {
			public CreateOrDelete Type { get; protected set; }
			public Element Target { get; protected set; }
			public ElementCreatedOrDeleted(CreateOrDelete type, Element target) {
				Type = type;
				Target = target;
			}
		}

		private class ElementPropertyChange: IChange {
			public Element Target { get; protected set; }
			public IProperty Property { get; set; }

			public ElementPropertyChange(Element target, IProperty property) {
				Target = target;
				Property = property;
			}
		}

		private class ElementFrameworkChange: IChange {
			public Element Target { get; protected set; }
			public Vector2 Size { get; private set; }
			public Vector2 Position { get; private set; }

			public ElementFrameworkChange(Element target, Vector2 size, Vector2 position) {
				Target = target;
				Size = size;
				Position = position;
			}
		}

		private enum CreateOrDelete {
			Create, Delete
		}
	}
}