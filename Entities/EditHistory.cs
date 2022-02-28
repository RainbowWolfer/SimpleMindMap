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
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
				string content = JsonConvert.SerializeObject(ConvertHistoriesJsonPair(list));
				Local.SaveTmpFile("tmp.json", content);
				var pairs = JsonConvert.DeserializeObject<List<Pair<int, string>>>(content) ?? new();
				//List<IChange> converted = ConvertBack(pairs);
			};

			OnUndo += change => {
				parent.connectionsManager.DeselectAllBackgroundPaths();
			};

			OnRedo += change => {
				parent.connectionsManager.DeselectAllBackgroundPaths();
			};
		}

		public List<IChange> GetPreviousHistories() => previous.ToArray().Reverse().ToList();

		public static List<IChange> ConvertBack(string pairsJson) {
			List<Pair<int, string>>? pairs = JsonConvert.DeserializeObject<List<Pair<int, string>>>(pairsJson);
			return pairs == null ? new() : ConvertBack(pairs);
		}

		public static List<IChange> ConvertBack(List<Pair<int, string>> pairs) {
			List<IChange> changes = new();
			IChange? change;
			foreach(Pair<int, string> item in pairs) {
				change = item.Key switch {
					ConnectionCreateOrDelete.JSONCONVERTID => JsonConvert.DeserializeObject<ConnectionCreateOrDelete>(item.Value),
					ElementCreatedOrDeleted.JSONCONVERTID => JsonConvert.DeserializeObject<ElementCreatedOrDeleted>(item.Value),
					PropertyDelayedChange.JSONCONVERTID => JsonConvert.DeserializeObject<PropertyDelayedChange>(item.Value),
					PropertyChange.JSONCONVERTID => JsonConvert.DeserializeObject<PropertyChange>(item.Value),
					ElementFrameworkChange.JSONCONVERTID => JsonConvert.DeserializeObject<ElementFrameworkChange>(item.Value),
					_ => null,
				};
				if(change == null) {
					continue;
				}
				changes.Add(change);
			}
			return changes;
		}

		public static List<Pair<int, string>> ConvertHistoriesJsonPair(List<IChange> list) {
			List<Pair<int, string>> changesJson = new();
			foreach(IChange change in list) {
				int id;
				string json;
				if(change is ConnectionCreateOrDelete ccod) {
					id = ConnectionCreateOrDelete.JSONCONVERTID;
					json = JsonConvert.SerializeObject(ccod);
				} else if(change is ElementCreatedOrDeleted ecod) {
					id = ElementCreatedOrDeleted.JSONCONVERTID;
					json = JsonConvert.SerializeObject(ecod);
				} else if(change is PropertyDelayedChange pdc) {
					id = PropertyDelayedChange.JSONCONVERTID;
					json = JsonConvert.SerializeObject(pdc);
				} else if(change is PropertyChange pc) {
					id = PropertyChange.JSONCONVERTID;
					json = JsonConvert.SerializeObject(pc);
				} else if(change is ElementFrameworkChange efc) {
					id = ElementFrameworkChange.JSONCONVERTID;
					json = JsonConvert.SerializeObject(efc);
				} else if(change is ElementConnectionFrameControlsChange ecfcc) {
					id = ElementConnectionFrameControlsChange.JSONCONVERTID;
					json = JsonConvert.SerializeObject(ecfcc);
				} else {
					throw new Exception();
				}
				changesJson.Add(new Pair<int, string>(id, json));
			}
			return changesJson;
		}

		public void SubmitByElementCreated(Element target) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementCreatedOrDeleted(
				CreateOrDelete.Create,
				target.TypeID,
				target.Identity,
				JsonConvert.SerializeObject(target.Properties),
				target.GetPosition(),
				target.GetSize(),
				target.ConnectionsFrame?.GetControlsInfo()
			));
			OnHistoryChanged?.Invoke(GetPreviousHistories());
		}

		public void SubmitByElementDeleted(Element target, List<ConnectionPath>? relatedConnections = null, ControlsInfo? connections = null) {
			InstantSealLastDelayedChange();
			List<ConnectionInfo> relatedInfos = new();
			foreach(ConnectionPath item in relatedConnections ?? new()) {
				if(item.to == null) {
					continue;
				}
				relatedInfos.Add(new ConnectionInfo(
					item.Identity,
					item.from.Parent.Identity,
					item.from.Identity,
					item.to.Parent.Identity,
					item.to.Identity,
					JsonConvert.SerializeObject(item.Properties)
				));
			}
			previous.Add(new ElementCreatedOrDeleted(
				CreateOrDelete.Delete,
				target.TypeID,
				target.Identity,
				JsonConvert.SerializeObject(target.Properties),
				target.GetPosition(),
				target.GetSize(),
				connections ?? target.ConnectionsFrame?.GetControlsInfo(),
				relatedInfos
			));
			OnHistoryChanged?.Invoke(GetPreviousHistories());
		}

		public void SubmitByElementPropertyChanged(TargetType targetType, IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint = null) {
			InstantSealLastDelayedChange();
			previous.Add(new PropertyChange(
				targetType,
				target.GetIdentity(),
				JsonConvert.SerializeObject(oldProperty),
				JsonConvert.SerializeObject(newProperty),
				propertyTargetHint));
			OnHistoryChanged?.Invoke(GetPreviousHistories());
		}

		public async void SubmitByElementPropertyDelayedChanged(TargetType targetType, IPropertiesContainer target, IProperty oldProperty, IProperty newProperty, string? propertyTargetHint = null) {
			Debug.WriteLine("!");
			const int DELAY = 500;
			string oldJson = JsonConvert.SerializeObject(oldProperty);
			string newJson = JsonConvert.SerializeObject(newProperty);

			PropertyDelayedChange? change = null;
			if(previous.LastOrDefault() is PropertyDelayedChange last && !last.IsSealed) {
				if(target.GetIdentity() == last.Identity) {
					change = last;
				} else {
					InstantSealLastDelayedChange(last);
				}
			}
			if(change == null) {
				change = new PropertyDelayedChange(
					targetType,
					target.GetIdentity(),
					oldJson,
					newJson,
					propertyTargetHint);
				previous.Add(change);
			}

			change.NewPropertyJson = newJson;
			if(change.Cts != null) {
				change.Cts.Cancel();
				change.Cts.Dispose();
			}
			change.Cts = new CancellationTokenSource();
			try {
				await Task.Delay(DELAY, change.Cts.Token);
				change.IsSealed = true;
				OnHistoryChanged?.Invoke(GetPreviousHistories());
			} catch(TaskCanceledException) {
				if(change.InstanceCancel) {
					change.IsSealed = true;
					OnHistoryChanged?.Invoke(GetPreviousHistories());
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
			OnHistoryChanged?.Invoke(GetPreviousHistories());
		}

		public void SubmitByElementConnectionControlsChanged(Element element, ControlsInfo from, ControlsInfo to) {
			InstantSealLastDelayedChange();
			previous.Add(new ElementConnectionFrameControlsChange(element.Identity, from, to));
			OnHistoryChanged?.Invoke(GetPreviousHistories());
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
				JsonConvert.SerializeObject(path.Properties)
			));
			OnHistoryChanged?.Invoke(GetPreviousHistories());
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
				JsonConvert.SerializeObject(path.Properties)
			));
			OnHistoryChanged?.Invoke(GetPreviousHistories());
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
						_parent.RemoveElement(cod.Identity, false);
						break;
					case CreateOrDelete.Delete:
						Element created = _parent.AddElement(cod.TypeID, cod.Identity, cod.Position, cod.Size, cod.ConnetionControls, false);
						created.SetProperty(cod.PropertyJson);
						foreach(ConnectionInfo item in cod.RelatedConnections) {
							_parent.connectionsManager.AddConnection(item, false);
						}
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(last is PropertyChange pc) {
				IPropertiesContainer? container = pc.TargetType switch {
					TargetType.Element => _parent.FindElementByIdentity(pc.Identity),
					TargetType.ConnectionPath => _parent.FindConnectionPathByIdentity(pc.Identity),
					_ => throw new Exception(),
				};
				if(container != null) {
					container.SetProperty(pc.OldPropertyJson);
				}
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
						_parent.connectionsManager.RemoveConnection(ccd.Identity, false);
						break;
					case CreateOrDelete.Delete:
						//_parent.connectionsManager.Add(ccd.Path, false);
						_parent.connectionsManager.AddConnection(
							ccd.Identity,
							ccd.FromElement,
							ccd.FromDot,
							ccd.ToElement,
							ccd.ToDot,
							ccd.PropertyJson,
							false
						);
						break;
					default:
						throw new Exception($"{ccd.Type} not found");
				}
			} else if(last is ElementConnectionFrameControlsChange ecfcc) {
				Element? element = _parent.FindElementByIdentity(ecfcc.Identity);
				if(element != null && element.ConnectionsFrame != null) {
					element.ConnectionsFrame.SetControls(ecfcc.From, false);
				}
			} else {
				throw new Exception($"Type({last.GetType()}) is not implemented");
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
						Element created = _parent.AddElement(cod.TypeID, cod.Identity, cod.Position, cod.Size, cod.ConnetionControls, false);
						created.SetProperty(cod.PropertyJson);
						foreach(ConnectionInfo item in cod.RelatedConnections) {
							_parent.connectionsManager.AddConnection(item, false);
						}
						break;
					case CreateOrDelete.Delete:
						_parent.RemoveElement(cod.Identity, false);
						//go delete
						break;
					default:
						throw new Exception($"{cod.Type} not found");
				}
			} else if(first is PropertyChange pc) {
				IPropertiesContainer? container = pc.TargetType switch {
					TargetType.Element => _parent.FindElementByIdentity(pc.Identity),
					TargetType.ConnectionPath => _parent.FindConnectionPathByIdentity(pc.Identity),
					_ => throw new Exception(),
				};
				if(container != null) {
					container.SetProperty(pc.NewPropertyJson);
				}
			} else if(first is ElementFrameworkChange fc) {
				Element? element = _parent.FindElementByIdentity(fc.Identity);
				if(element != null) {
					element.SetPosition(fc.ToPosition);
					element.SetSize(fc.ToSize);
					element.UpdateConnectionsFrame();
					ResizeFrame.Current?.UpdateResizeFrame();
				}
			} else if(first is ConnectionCreateOrDelete ccd) {
				switch(ccd.Type) {
					case CreateOrDelete.Create:
						//_parent.connectionsManager.Add(ccd.Path, false);
						_parent.connectionsManager.AddConnection(
							ccd.Identity,
							ccd.FromElement,
							ccd.FromDot,
							ccd.ToElement,
							ccd.ToDot,
							ccd.PropertyJson,
							false
						);
						break;
					case CreateOrDelete.Delete:
						//_parent.connectionsManager.Remove(ccd.Path, false);
						_parent.connectionsManager.RemoveConnection(ccd.Identity, false);
						break;
					default:
						throw new Exception($"{ccd.Type} not found");
				}
			} else if(first is ElementConnectionFrameControlsChange ecfcc) {
				Element? element = _parent.FindElementByIdentity(ecfcc.Identity);
				if(element != null && element.ConnectionsFrame != null) {
					element.ConnectionsFrame.SetControls(ecfcc.To, false);
					//update element related connection path
				}
			} else {
				throw new Exception($"Type({first.GetType()}) is not implemented");
			}
			future.RemoveAt(0);
			previous.Add(first);
			OnRedo?.Invoke(first);
		}

		public interface IChange {
			Identity Identity { get; set; }
			DateTime Date { get; set; }
			IconElement GetIcon();
			string GetDetail();
		}

		public class ConnectionCreateOrDelete: IChange {
			public const int JSONCONVERTID = 1;

			public DateTime Date { get; set; }
			public Identity Identity { get; set; }
			public IconElement GetIcon() => Type switch {
				CreateOrDelete.Create => new ImageIcon("pack://application:,,,/Icons/Connection_Add.png"),
				CreateOrDelete.Delete => new ImageIcon("pack://application:,,,/Icons/Connection_Remove.png"),
				_ => new FontIcon("\uE11B"),
			};

			public CreateOrDelete Type { get; protected set; }

			public Identity FromElement { get; protected set; }
			public Identity FromDot { get; protected set; }
			public Identity ToElement { get; protected set; }
			public Identity ToDot { get; protected set; }

			public string PropertyJson { get; protected set; }

			public ConnectionCreateOrDelete(CreateOrDelete createOrDeleteType, Identity target, Identity fromElement, Identity fromDot, Identity toElement, Identity toDot, string propertyJson) {
				Type = createOrDeleteType;
				Identity = target;
				FromElement = fromElement;
				FromDot = fromDot;
				ToElement = toElement;
				ToDot = toDot;
				PropertyJson = propertyJson;
				Date = DateTime.Now;
			}

			public override string ToString() {
				return Type switch {
					CreateOrDelete.Create => $"Created Connection {Identity.Name}",
					CreateOrDelete.Delete => $"Deleted Connection {Identity.Name}",
					_ => throw new Exception($"({Type}) Type not found"),
				};
			}

			public string GetDetail() {
				return "";
			}
		}

		public class ElementCreatedOrDeleted: IChange {
			public const int JSONCONVERTID = 2;
			public DateTime Date { get; set; }
			public Identity Identity { get; set; }
			public IconElement GetIcon() => new FontIcon("\uE11B");

			public CreateOrDelete Type { get; protected set; }

			public long TypeID { get; protected set; }
			public string PropertyJson { get; protected set; }
			public Vector2 Position { get; protected set; }
			public Vector2 Size { get; protected set; }
			public ControlsInfo ConnetionControls { get; protected set; }
			public List<ConnectionInfo> RelatedConnections { get; protected set; }

			public ElementCreatedOrDeleted(CreateOrDelete type, long typeID, Identity target, string propertyJson, Vector2 position, Vector2 size, ControlsInfo? connetionControls = null, List<ConnectionInfo>? relatedConnections = null) {
				TypeID = typeID;
				Type = type;
				Identity = target;
				PropertyJson = propertyJson;
				Position = position;
				Size = size;
				ConnetionControls = connetionControls ?? new();
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

			public string GetDetail() {
				return "";
			}
		}

		public class PropertyChange: IChange {
			public const int JSONCONVERTID = 3;
			public DateTime Date { get; set; }
			public Identity Identity { get; set; }
			public IconElement GetIcon() => new FontIcon("\uE11B");

			public TargetType TargetType { get; set; }

			public string OldPropertyJson { get; set; }
			public string NewPropertyJson { get; set; }
			public string? PropertyTargetHint { get; protected set; }

			public PropertyChange(TargetType targetType, Identity target, string oldPropertyJson, string newPropertyJson, string? propertyTargetHint) {
				Identity = target;
				OldPropertyJson = oldPropertyJson;
				NewPropertyJson = newPropertyJson;
				PropertyTargetHint = propertyTargetHint;
				Date = DateTime.Now;
				TargetType = targetType;
			}

			public override string ToString() {
				return $"{Identity.Name} - {PropertyTargetHint ?? "Properties"} - Changed";
			}

			public string GetDetail() {
				return "";
			}
		}

		private class PropertyDelayedChange: PropertyChange {
			public new const int JSONCONVERTID = 4;
			public CancellationTokenSource? Cts { get; set; }
			public bool IsSealed { get; set; } = false;
			public bool InstanceCancel { get; set; } = false;

			public PropertyDelayedChange(TargetType targetType,
				Identity target,
				string oldPropertyJson,
				string newPropertyJson,
				string? propertyTargetHint)
				: base(targetType, target, oldPropertyJson, newPropertyJson, propertyTargetHint) { }

		}

		public class ElementFrameworkChange: IChange {
			public const int JSONCONVERTID = 5;
			public DateTime Date { get; set; }
			public Identity Identity { get; set; }
			public IconElement GetIcon() => new FontIcon("\uE11B");

			public Vector2 FromSize { get; protected set; }
			public Vector2 FromPosition { get; protected set; }
			public Vector2 ToSize { get; protected set; }
			public Vector2 ToPosition { get; protected set; }

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

			public string GetDetail() {
				return "";
			}
		}

		public class ElementConnectionFrameControlsChange: IChange {
			public const int JSONCONVERTID = 6;
			public Identity Identity { get; set; }
			public DateTime Date { get; set; }
			public IconElement GetIcon() => new FontIcon("\uE11B");

			public ControlsInfo From { get; set; }
			public ControlsInfo To { get; set; }

			public ElementConnectionFrameControlsChange(Identity identity, ControlsInfo from, ControlsInfo to) {
				Identity = identity;
				From = from;
				To = to;
				Date = DateTime.Now;
			}

			public string GetDetail() {
				return "";
			}
		}

		public enum CreateOrDelete {
			Create, Delete
		}
	}
	public enum TargetType {
		Element, ConnectionPath
	}

}
