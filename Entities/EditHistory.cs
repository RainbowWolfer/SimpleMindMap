using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Icons;
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
using System.Windows.Controls;

namespace MindMap.Entities {
	public class EditHistory {
		public event Action<List<IChange>>? OnHistoryChanged;
		public event Action<IChange>? OnUndo;
		public event Action<IChange>? OnRedo;

		private readonly List<IChange> previous = new();
		private readonly List<IChange> future = new();

		private readonly MindMapPage _parent;
		public EditHistory(MindMapPage parent) {
			_parent = parent;
			OnHistoryChanged += list => {
				future.Clear();
			};

			OnUndo += change => {
				parent.connectionsManager.DeselectAllBackgroundPaths();
			};

			OnRedo += change => {
				parent.connectionsManager.DeselectAllBackgroundPaths();
			};
		}

		public List<IChange> GetHistory() => previous.ToArray().Reverse().ToList();

		public void SubmitByElementCreated(Element target) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementCreatedOrDeleted(CreateOrDelete.Create, target));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByElementDeleted(Element target, List<ConnectionPath>? relatedConnections = null) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementCreatedOrDeleted(CreateOrDelete.Delete, target, relatedConnections ?? new List<ConnectionPath>()));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByElementPropertyChanged(IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint = null) {
			InstantSealLastDelayedChange();
			IProperty.MakeClone(ref oldProperty);
			IProperty.MakeClone(ref newProperty);
			previous.Add(new PropertyChange(target, oldProperty, newProperty, propertyTargetHint));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public async void SubmitByElementPropertyDelayedChanged(IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint = null) {
			const int DELAY = 500;
			IProperty.MakeClone(ref oldProperty);
			IProperty.MakeClone(ref newProperty);

			PropertyDelayedChange? change = null;
			if(previous.LastOrDefault() is PropertyDelayedChange last && !last.IsSealed) {
				if(target == last.Target) {
					change = last;
				} else {
					InstantSealLastDelayedChange(last);
				}
			}
			if(change == null) {
				change = new PropertyDelayedChange(target, oldProperty, newProperty, propertyTargetHint);
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
			previous.Add(new ElementFrameworkChange(target, fromSize, fromPosition, toSize, toPosition));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByElementPositionChanged(Element target, Vector2 fromPosition, Vector2 toPosition) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementPosotionChange(target, fromPosition, toPosition));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByConnectionCreated(ConnectionPath path) {
			InstantSealLastDelayedChange();
			previous.Add(new ConnectionCreateOrDelete(CreateOrDelete.Create, path));
			OnHistoryChanged?.Invoke(GetHistory());
		}

		public void SubmitByConnectionDeleted(ConnectionPath path) {
			InstantSealLastDelayedChange();
			previous.Add(new ConnectionCreateOrDelete(CreateOrDelete.Delete, path));
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
			IChange last = previous[^1];
			if(last is ElementCreatedOrDeleted cod) {
				switch(cod.Type) {
					case CreateOrDelete.Create:
						cod.Target.Delete(false);
						break;
					case CreateOrDelete.Delete:
						_parent.AddElementFromHistory(cod.Target);
						Debug.WriteLine(cod.RelatedConnections.Count);
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(last is PropertyChange pc) {
				pc.Target.SetProperty(pc.OldProperty);
			} else if(last is ElementFrameworkChange fc) {
				fc.Target.SetPosition(fc.FromPosition);
				fc.Target.SetSize(fc.FromSize);
				fc.Target.UpdateConnectionsFrame();
				ResizeFrame.Current?.UpdateResizeFrame();
			} else if(last is ElementPosotionChange posc) {
				posc.Target.SetPosition(posc.FromPosition);
				posc.Target.UpdateConnectionsFrame();
				ResizeFrame.Current?.UpdateResizeFrame();
			} else if(last is ConnectionCreateOrDelete ccd) {
				switch(ccd.Type) {
					case CreateOrDelete.Create:
						_parent.connectionsManager.Remove(ccd.Path, false);
						break;
					case CreateOrDelete.Delete:
						_parent.connectionsManager.Add(ccd.Path, false);
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
			IChange first = future[0];
			if(first is ElementCreatedOrDeleted cod) {
				switch(cod.Type) {
					case CreateOrDelete.Create:
						//go create
						_parent.AddElementFromHistory(cod.Target);
						Debug.WriteLine(cod.RelatedConnections.Count);
						break;
					case CreateOrDelete.Delete:
						cod.Target.Delete(false);
						//go delete
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(first is PropertyChange pc) {
				pc.Target.SetProperty(pc.NewProperty);
			} else if(first is ElementFrameworkChange fc) {
				fc.Target.SetPosition(fc.ToPosition);
				fc.Target.SetSize(fc.ToSize);
				fc.Target.UpdateConnectionsFrame();
				ResizeFrame.Current?.UpdateResizeFrame();
			} else if(first is ElementPosotionChange posc) {
				posc.Target.SetPosition(posc.ToPosition);
				posc.Target.UpdateConnectionsFrame();
				ResizeFrame.Current?.UpdateResizeFrame();
			} else if(first is ConnectionCreateOrDelete ccd) {
				switch(ccd.Type) {
					case CreateOrDelete.Create:
						_parent.connectionsManager.Add(ccd.Path, false);
						break;
					case CreateOrDelete.Delete:
						_parent.connectionsManager.Remove(ccd.Path, false);
						break;
					default:
						throw new Exception($"{ccd.Type} not found");
				}
			}
			future.RemoveAt(0);
			previous.Add(first);
			OnRedo?.Invoke(first);
		}

		public interface IChange {
			IconElement Icon { get; }
			DateTime Date { get; set; }
			string GetDetail();
		}

		public class ConnectionCreateOrDelete: IChange {
			public DateTime Date { get; set; }
			public IconElement Icon => Type switch {
				CreateOrDelete.Create => new ImageIcon("pack://application:,,,/Icons/Connection_Add.png"),
				CreateOrDelete.Delete => new ImageIcon("pack://application:,,,/Icons/Connection_Remove.png"),
				_ => new FontIcon("\uE11B"),
			};

			public CreateOrDelete Type { get; protected set; }
			public ConnectionPath Path { get; protected set; }

			public ConnectionCreateOrDelete(CreateOrDelete createOrDeleteType, ConnectionPath path) {
				Type = createOrDeleteType;
				Path = path;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return Type switch {
					CreateOrDelete.Create => $"Created Connection {Path.from.Parent_ID}-{Path.to?.Parent_ID ?? "None"}",
					CreateOrDelete.Delete => $"Deleted Connection {Path.from.Parent_ID}-{Path.to?.Parent_ID ?? "None"}",
					_ => throw new Exception($"({Type}) Type not found"),
				};
			}

			public string GetDetail() {
				return "";
			}
		}

		public class ElementCreatedOrDeleted: IChange {
			public DateTime Date { get; set; }
			public IconElement Icon => new FontIcon("\uE11B");
			public CreateOrDelete Type { get; protected set; }
			public Element Target { get; protected set; }
			public List<ConnectionPath> RelatedConnections { get; protected set; }

			public ElementCreatedOrDeleted(CreateOrDelete type, Element target, List<ConnectionPath>? relatedConnections = null) {
				Type = type;
				Target = target;
				Date = DateTime.Now;
				RelatedConnections = relatedConnections ?? new();
			}

			public override string ToString() {
				return Type switch {
					CreateOrDelete.Create => $"Created {Target.ID}",
					CreateOrDelete.Delete => $"Deleted {Target.ID}",
					_ => throw new Exception($"({Type}) Type not found"),
				};
			}

			public string GetDetail() {
				return "";
			}
		}

		public class PropertyChange: IChange {
			public DateTime Date { get; set; }
			public IconElement Icon => new FontIcon("\uE11B");
			public IPropertiesContainer Target { get; protected set; }
			public IProperty OldProperty { get; set; }
			public IProperty NewProperty { get; set; }
			public string? PropertyTargetHint { get; protected set; }

			public PropertyChange(IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint) {
				Target = target;
				OldProperty = oldProperty;
				NewProperty = newProperty;
				PropertyTargetHint = propertyTargetHint;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return $"{Target.GetID()} - {PropertyTargetHint ?? "Properties"} - Changed";
			}

			public string GetDetail() {
				return "";
			}
		}

		private class PropertyDelayedChange: PropertyChange {
			public CancellationTokenSource? Cts { get; set; }
			public bool IsSealed { get; set; } = false;
			public bool InstanceCancel { get; set; } = false;

			public PropertyDelayedChange(IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint) : base(target, oldProperty, newProperty, propertyTargetHint) { }


		}

		public class ElementPosotionChange: IChange {
			public DateTime Date { get; set; }
			public IconElement Icon => new FontIcon("\uE11B");
			public Element Target { get; protected set; }
			public Vector2 FromPosition { get; private set; }
			public Vector2 ToPosition { get; private set; }

			public ElementPosotionChange(Element target, Vector2 fromPosition, Vector2 toPosition) {
				Target = target;
				FromPosition = fromPosition;
				ToPosition = toPosition;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return $"{Target.ID} - Position Changed";
			}

			public string GetDetail() {
				return "";
			}
		}

		public class ElementFrameworkChange: IChange {
			public DateTime Date { get; set; }
			public IconElement Icon => new FontIcon("\uE11B");
			public Element Target { get; protected set; }
			public Vector2 FromSize { get; private set; }
			public Vector2 FromPosition { get; private set; }
			public Vector2 ToSize { get; private set; }
			public Vector2 ToPosition { get; private set; }

			public ElementFrameworkChange(Element target, Vector2 fromSize, Vector2 fromPosition, Vector2 toSize, Vector2 toPosition) {
				Target = target;
				FromPosition = fromPosition;
				FromSize = fromSize;
				ToSize = toSize;
				ToPosition = toPosition;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return $"{Target.ID} - Frame Changed";
			}

			public string GetDetail() {
				return "";
			}
		}

		public enum CreateOrDelete {
			Create, Delete
		}
	}
}
