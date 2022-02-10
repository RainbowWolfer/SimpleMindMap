using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MindMap.Entities {
	public class EditHistory {
		public event Action? OnHistoryChanged;
		private readonly List<IChange> previous = new();
		private readonly List<IChange> future = new();

		private readonly MindMapPage _parent;
		public EditHistory(MindMapPage parent) {
			_parent = parent;
		}

		public void SubmitByElementCreated(Element target) {
			previous.Add(new ElementCreatedOrDeleted(CreateOrDelete.Create, target));
			future.Clear();
			OnHistoryChanged?.Invoke();
		}
		public void SubmitByElementDeleted(Element target) {
			previous.Add(new ElementCreatedOrDeleted(CreateOrDelete.Delete, target));
			future.Clear();
			OnHistoryChanged?.Invoke();
		}
		public void SubmitByElementPropertyChanged(Element target, IProperty oldProperty, IProperty newProperty) {
			IProperty.MakeClone(ref oldProperty);
			IProperty.MakeClone(ref newProperty);
			previous.Add(new ElementPropertyChange(target, oldProperty, newProperty));
			OnHistoryChanged?.Invoke();
		}
		public async void SubmitByElementPropertyDelayedChanged(Element target, IProperty oldProperty, IProperty newProperty) {
			const int DELAY = 1000;
			IProperty.MakeClone(ref oldProperty);
			IProperty.MakeClone(ref newProperty);
			if(previous.LastOrDefault() is ElementPropertyDelayedChange delayedChange && !delayedChange.IsSealed) {
				delayedChange.NewProperty = newProperty;
				if(delayedChange.Cts != null) {
					delayedChange.Cts.Cancel();
					delayedChange.Cts.Dispose();
				}
				delayedChange.Cts = new CancellationTokenSource();
				try {
					await Task.Delay(DELAY, delayedChange.Cts.Token);
					delayedChange.IsSealed = true;
				} catch(TaskCanceledException) { }
			} else {
				previous.Add(new ElementPropertyDelayedChange(target, oldProperty, newProperty));
			}
			OnHistoryChanged?.Invoke();
		}
		public void SubmitByElementFrameworkChanged(Element target, Vector2 size, Vector2 position) {
			previous.Add(new ElementFrameworkChange(target, size, position));
			OnHistoryChanged?.Invoke();
		}

		public void SumbitByConnectionCreated(ConnectionPath path) {
			//previous.Add(new ConnectionCreateOrDelete(CreateOrDelete.Create, path));
			OnHistoryChanged?.Invoke();
		}
		public void SumbitByConnectionDeleted(ConnectionPath path) {
			//previous.Add(new ConnectionCreateOrDelete(CreateOrDelete.Delete, path));
			OnHistoryChanged?.Invoke();
		}
		public void SubmitByConnectionChange() {

			OnHistoryChanged?.Invoke();
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
				pc.Target.SetProperty(pc.OldProperty);
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
				pc.Target.SetProperty(pc.OldProperty);
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
			public IProperty OldProperty { get; set; }
			public IProperty NewProperty { get; set; }

			public ElementPropertyChange(Element target, IProperty oldProperty, IProperty newProperty) {
				Target = target;
				OldProperty = oldProperty;
				NewProperty = newProperty;
			}
		}

		private class ElementPropertyDelayedChange: ElementPropertyChange {//later
			public CancellationTokenSource? Cts { get; set; }
			public bool IsSealed { get; set; } = false;

			public ElementPropertyDelayedChange(Element target, IProperty oldProperty, IProperty newProperty) : base(target, oldProperty, newProperty) {

			}

			public void ContinuesSubmit(IProperty newProperty) {

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
