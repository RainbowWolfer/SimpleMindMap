using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Icons;
using MindMap.Entities.Identifications;
using MindMap.Entities.Locals;
using MindMap.Entities.Properties;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.Entities {
	public class EditHistory {
		public event Action<List<Change>>? OnHistoryChanged;
		public event Action<Change>? OnUndo;
		public event Action<Change>? OnRedo;

		private readonly List<Change> previous = new();
		private readonly List<Change> future = new();

		private readonly MindMapPage _parent;
		public EditHistory(MindMapPage parent) {
			_parent = parent;
			OnHistoryChanged += list => {
				future.Clear();
				string content = JsonConvert.SerializeObject(list);
				//Debug.WriteLine(content);
				Local.SaveTmpFile("tmp.json", content);
			};

			OnUndo += change => {
				parent.connectionsManager.DeselectAllBackgroundPaths();
			};

			OnRedo += change => {
				parent.connectionsManager.DeselectAllBackgroundPaths();
			};
		}

		public List<Change> GetHistory() => previous.ToArray().Reverse().ToList();

		public void SubmitByElementCreated(Element target) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementCreatedOrDeleted(
				CreateOrDelete.Create,
				target.Identity,
				target.Properties,
				target.GetPosition(),
				target.GetSize()
			));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByElementDeleted(Element target, List<ConnectionPath>? relatedConnections = null) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementCreatedOrDeleted(
				CreateOrDelete.Delete,
				target.Identity,
				target.Properties,
				target.GetPosition(),
				target.GetSize(),
				relatedConnections?.Select(c => c.Identity).ToList() ?? new List<Identity>()
			));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByElementPropertyChanged(IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint = null) {
			InstantSealLastDelayedChange();
			IProperty.MakeClone(ref oldProperty);
			IProperty.MakeClone(ref newProperty);
			previous.Add(new PropertyChange(target.GetIdentity(), oldProperty, newProperty, propertyTargetHint));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public async void SubmitByElementPropertyDelayedChanged(IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint = null) {
			const int DELAY = 500;
			IProperty.MakeClone(ref oldProperty);
			IProperty.MakeClone(ref newProperty);

			PropertyDelayedChange? change = null;
			if(previous.LastOrDefault() is PropertyDelayedChange last && !last.IsSealed) {
				if(target == last.Identity) {
					change = last;
				} else {
					InstantSealLastDelayedChange(last);
				}
			}
			if(change == null) {
				change = new PropertyDelayedChange(target.GetIdentity(), oldProperty, newProperty, propertyTargetHint);
				previous.Add(change);
			}

			change.NewProperty = newProperty;
			if(change.Cts != null) {
				change.Cts.Cancel();
				change.Cts.Dispose();
			}
			change.Cts = new CancellationTokenSource();
			try {
				await Task.Delay(DELAY, change.Cts.Token);
				change.IsSealed = true;
				OnHistoryChanged?.Invoke(GetHistory());
			} catch(TaskCanceledException) {
				if(change.InstanceCancel) {
					change.IsSealed = true;
					OnHistoryChanged?.Invoke(GetHistory());
				}
			}
		}
		public void SubmitByElementFrameworkChanged(Element target, Vector2 fromSize, Vector2 fromPosition, Vector2 toSize, Vector2 toPosition) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementFrameworkChange(
				target.Identity,
				fromSize,
				fromPosition,
				toSize,
				toPosition
			));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByConnectionCreated(ConnectionPath path) {
			if(path.to == null) {
				return;
			}
			InstantSealLastDelayedChange();
			previous.Add(new ConnectionCreateOrDelete(
				CreateOrDelete.Create,
				path.Identity,
				path.from.Parent.Identity,
				path.from.Identity,
				path.to.Parent.Identity,
				path.to.Identity,
				path.Properties
			));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByConnectionDeleted(ConnectionPath path) {
			if(path.to == null) {
				return;
			}
			InstantSealLastDelayedChange();
			previous.Add(new ConnectionCreateOrDelete(
				CreateOrDelete.Delete,
				path.Identity,
				path.from.Parent.Identity,
				path.from.Identity,
				path.to.Parent.Identity,
				path.to.Identity,
				path.Properties
			));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void InstantSealLastDelayedChange() {
			InstantSealLastDelayedChange(null);
		}

		private void InstantSealLastDelayedChange(PropertyDelayedChange? change = null) {
			if(change == null && previous.LastOrDefault() is PropertyDelayedChange last && !last.IsSealed) {
				change = last;
			}
			if(change == null) {
				return;
			}
			change.InstanceCancel = true;
			try {
				change.Cts?.Cancel();
			} catch(Exception) { }
		}

		public void Undo() {
			if(previous.Count < 1) {
				return;
			}
			Change last = previous[^1];
			if(last is ElementCreatedOrDeleted cod) {
				switch(cod.Type) {
					case CreateOrDelete.Create:
						_parent.RemoveElement(cod.Identity, false);
						break;
					case CreateOrDelete.Delete:
						Element created = _parent.AddElement(cod.TypeID, cod.Identity, cod.Position, cod.Size, false);
						created.SetProperty(cod.Property);
						//Debug.WriteLine(cod.RelatedConnections.Count);
						//foreach(ConnectionPath item in cod.RelatedConnections) {
						//	_parent.connectionsManager.Add(item, false);
						//}
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(last is PropertyChange pc) {
				_parent.FindElementByIdentity(pc.Identity)?.SetProperty(pc.OldProperty);
			} else if(last is ElementFrameworkChange fc) {
				Element? element = _parent.FindElementByIdentity(fc.Identity);
				if(element != null) {
					element.SetPosition(fc.FromPosition);
					element.SetSize(fc.FromSize);
					element.UpdateConnectionsFrame();
					ResizeFrame.Current?.UpdateResizeFrame();
				}
			} else if(last is ConnectionCreateOrDelete ccd) {
				switch(ccd.Type) {
					case CreateOrDelete.Create:
						//_parent.connectionsManager.Remove(ccd.Path, false);
						break;
					case CreateOrDelete.Delete:
						//_parent.connectionsManager.Add(ccd.Path, false);
						break;
					default:
						throw new Exception($"{ccd.Type} not found");
				}
			}
			previous.RemoveAt(previous.Count - 1);
			future.Insert(0, last);
			OnUndo?.Invoke(last);
		}

		public void Redo() {
			if(future.Count < 1) {
				return;
			}
			Change first = future[0];
			if(first is ElementCreatedOrDeleted cod) {
				switch(cod.Type) {
					case CreateOrDelete.Create:
						//go create
						Element created = _parent.AddElement(cod.TypeID, cod.Identity, cod.Position, cod.Size, false);
						created.SetProperty(cod.Property);
						//Debug.WriteLine(cod.RelatedConnections.Count);
						//foreach(ConnectionPath item in cod.RelatedConnections) {
						//	_parent.connectionsManager.Add(item, false);
						//}
						break;
					case CreateOrDelete.Delete:
						_parent.RemoveElement(cod.Identity, false);
						//go delete
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(first is PropertyChange pc) {
				_parent.FindElementByIdentity(pc.Identity)?.SetProperty(pc.OldProperty);
			} else if(first is ElementFrameworkChange fc) {
				Element? element = _parent.FindElementByIdentity(fc.Identity);
				if(element != null) {
					element.SetPosition(fc.FromPosition);
					element.SetSize(fc.FromSize);
					element.UpdateConnectionsFrame();
					ResizeFrame.Current?.UpdateResizeFrame();
				}
			} else if(first is ConnectionCreateOrDelete ccd) {
				switch(ccd.Type) {
					case CreateOrDelete.Create:
						//_parent.connectionsManager.Add(ccd.Path, false);
						break;
					case CreateOrDelete.Delete:
						//_parent.connectionsManager.Remove(ccd.Path, false);
						break;
					default:
						throw new Exception($"{ccd.Type} not found");
				}
			}
			future.RemoveAt(0);
			previous.Add(first);
			OnRedo?.Invoke(first);
		}

		public abstract class Change {
			public abstract Identity Identity { get; protected set; }
			public abstract IconElement Icon { get; }
			public abstract DateTime Date { get; protected set; }
			public abstract string GetDetail();
		}

		public class ConnectionCreateOrDelete: Change {
			public override DateTime Date { get; protected set; }
			public override IconElement Icon => Type switch {
				CreateOrDelete.Create => new ImageIcon("pack://application:,,,/Icons/Connection_Add.png"),
				CreateOrDelete.Delete => new ImageIcon("pack://application:,,,/Icons/Connection_Remove.png"),
				_ => new FontIcon("\uE11B"),
			};
			public override Identity Identity { get; protected set; }

			public CreateOrDelete Type { get; protected set; }

			public Identity FromElement { get; protected set; }
			public Identity FromDot { get; protected set; }
			public Identity ToElement { get; protected set; }
			public Identity ToDot { get; protected set; }

			public IProperty Property { get; protected set; }

			public ConnectionCreateOrDelete(CreateOrDelete createOrDeleteType, Identity target, Identity fromElement, Identity fromDot, Identity toElement, Identity toDot, IProperty property) {
				Type = createOrDeleteType;
				Identity = target;
				FromElement = fromElement;
				FromDot = fromDot;
				ToElement = toElement;
				ToDot = toDot;
				Property = property;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return Type switch {
					CreateOrDelete.Create => $"Created Connection {Identity.Name}",
					CreateOrDelete.Delete => $"Deleted Connection {Identity.Name}",
					_ => throw new Exception($"({Type}) Type not found"),
				};
			}

			public override string GetDetail() {
				return "";
			}
		}

		public class ElementCreatedOrDeleted: Change {
			public override DateTime Date { get; protected set; }
			public override IconElement Icon => new FontIcon("\uE11B");
			public override Identity Identity { get; protected set; }

			public CreateOrDelete Type { get; protected set; }

			public long TypeID { get; protected set; }
			public IProperty Property { get; protected set; }
			public Vector2 Position { get; protected set; }
			public Vector2 Size { get; protected set; }
			public Dictionary<Direction, int> ConnetionControls { get; protected set; }
			public List<Identity> RelatedConnections { get; protected set; }

			public ElementCreatedOrDeleted(CreateOrDelete type, Identity target, IProperty property, Vector2 position, Vector2 size, List<Identity>? relatedConnections = null) {
				Type = type;
				Identity = target;
				Property = property;
				Position = position;
				Size = size;
				RelatedConnections = relatedConnections ?? new();
				Date = DateTime.Now;
			}

			public override string ToString() {
				return Type switch {
					CreateOrDelete.Create => $"Created {Identity.ID}",
					CreateOrDelete.Delete => $"Deleted {Identity.ID}",
					_ => throw new Exception($"({Type}) Type not found"),
				};
			}

			public override string GetDetail() {
				return "";
			}
		}

		public class PropertyChange: Change {
			public override DateTime Date { get; protected set; }
			public override IconElement Icon => new FontIcon("\uE11B");
			public override Identity Identity { get; protected set; }

			public IProperty OldProperty { get; set; }
			public IProperty NewProperty { get; set; }
			public string? PropertyTargetHint { get; protected set; }

			public PropertyChange(Identity target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint) {
				Identity = target;
				OldProperty = oldProperty;
				NewProperty = newProperty;
				PropertyTargetHint = propertyTargetHint;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return $"{Identity.Name} - {PropertyTargetHint ?? "Properties"} - Changed";
			}

			public override string GetDetail() {
				return "";
			}
		}

		private class PropertyDelayedChange: PropertyChange {
			public CancellationTokenSource? Cts { get; set; }
			public bool IsSealed { get; set; } = false;
			public bool InstanceCancel { get; set; } = false;

			public PropertyDelayedChange(Identity target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint) : base(target, oldProperty, newProperty, propertyTargetHint) { }


		}

		//public class ElementPosotionChange: Change {
		//	public override DateTime Date { get; protected set; }
		//	public override IconElement Icon => new FontIcon("\uE11B");
		//	public override Identity Identity { get; protected set; }

		//	public Vector2 FromPosition { get; private set; }
		//	public Vector2 ToPosition { get; private set; }

		//	public ElementPosotionChange(Identity target, Vector2 fromPosition, Vector2 toPosition) {
		//		Identity = target;
		//		FromPosition = fromPosition;
		//		ToPosition = toPosition;
		//		Date = DateTime.Now;
		//	}

		//	public override string ToString() {
		//		return $"{Identity.ID} - Position Changed";
		//	}

		//	public override string GetDetail() {
		//		return "";
		//	}
		//}

		public class ElementFrameworkChange: Change {
			public override DateTime Date { get; protected set; }
			public override IconElement Icon => new FontIcon("\uE11B");
			public override Identity Identity { get; protected set; }

			public Vector2 FromSize { get; private set; }
			public Vector2 FromPosition { get; private set; }
			public Vector2 ToSize { get; private set; }
			public Vector2 ToPosition { get; private set; }

			public ElementFrameworkChange(Identity target, Vector2 fromSize, Vector2 fromPosition, Vector2 toSize, Vector2 toPosition) {
				Identity = target;
				FromPosition = fromPosition;
				FromSize = fromSize;
				ToSize = toSize;
				ToPosition = toPosition;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return $"{Identity.ID} - Frame Changed";
			}

			public override string GetDetail() {
				return "";
			}
		}

		public enum CreateOrDelete {
			Create, Delete
		}
	}
}
