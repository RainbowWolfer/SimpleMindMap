using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Locals;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MindMap.Entities.Connections {
	public class ConnectionsManager {
		private List<Connection> Connections { get; } = new();
		private readonly Canvas _mainCanvas;
		private readonly MindMapPage _parent;

		public int Count => Connections.Count;
		public ConnectionPath? CurrentSelection { get; set; }

		public ConnectionsManager(MindMapPage mindMapPage) {
			_parent = mindMapPage;
			_mainCanvas = mindMapPage.MainCanvas;
		}

		public bool CheckDuplicate(ConnectionControl from, ConnectionControl to) {
			return Connections.Any(c =>
				(c.From == from && c.To == to) ||
				(c.From == to && c.To == from)
			);
		}

		public ConnectionPath AddConnection(Identity identity, Identity fromElement, Identity fromDot, Identity toElement, Identity toDot, bool submitHistory = true) {
			
			return null;
		}

		//public void AddConnection(ConnectionPath path, bool submitHistory = true, string? propertyJson = null) {
		//	if(path.to == null || CheckDuplicate(path.from, path.to)) {
		//		return;
		//	}
		//	Connections.Add(new Connection(path, path.from, path.to));
		//	path.Initialize(false);
		//	_parent.UpdateCount();
		//	if(submitHistory) {
		//		_parent.editHistory.SubmitByConnectionCreated(path);
		//	}
		//}

		public ConnectionPath? AddConnection(ConnectionControl from, ConnectionControl to, bool submitHistory = true) {
			if(CheckDuplicate(from, to)) {
				return null;
			}
			var connectionPath = new ConnectionPath(_parent, _parent.connectionsManager, from, to);
			Connections.Add(new Connection(connectionPath, from, to));
			_parent.UpdateCount();
			if(submitHistory) {
				_parent.editHistory.SubmitByConnectionCreated(connectionPath);
			}
			return connectionPath;
		}

		public void RemoveConnetion(ConnectionPath path, bool submitHistory = true) {
			if(path.to == null) {
				return;
			}
			Connection? found = Connections.Find(c => c.Path == path);
			if(found != null) {
				if(submitHistory) {
					_parent.editHistory.SubmitByConnectionDeleted(found.Path);
				}
				found.Path.ClearBackground();
				found.Path.ClearFromCanvas();
				Connections.Remove(found);
			}
			_parent.UpdateCount();
		}

		public void RemoveFrame(ConnectionsFrame frame) {
			List<Connection> founds = new();
			foreach(ConnectionControl item in frame.AllDots) {
				foreach(Connection c in Connections.Where(c => c.From == item || c.To == item)) {
					c.Path.ClearFromCanvas();
					founds.Add(c);
				}
			}
			Connections.RemoveAll(c => founds.Contains(c));
		}

		public void Update(ConnectionControl dot) {
			foreach(Connection item in Connections.Where(c => c.From == dot || c.To == dot)) {
				item.Update();
			}
		}

		public void ShowProperties(ConnectionPath path) {
			_parent.ShowConnectionPathProperties(path);
		}

		public void DeselectAllBackgroundPaths() {
			Connections.ForEach(c => c.Path.Deselect());
		}

		public List<ConnectionPath> CalculateRelatedConnections(ConnectionsFrame frame) {
			List<ConnectionPath> result = new();
			foreach(Connection connection in Connections) {
				if(connection.Path.to == null) {
					continue;
				}
				if(frame.AllDots.Contains(connection.Path.from) || frame.AllDots.Contains(connection.Path.to)) {
					result.Add(connection.Path);
				}
			}
			return result;
		}

		public void DebugConnections() {
			Debug.WriteLine("START");
			foreach(Connection c in Connections) {
				Debug.WriteLine($"{c.Path} | {c.From.Identity} | {c.To.Identity}");
			}
			Debug.WriteLine("END");
		}

		public ConnectionInfo[] ConvertInfo() {
			List<ConnectionInfo> result = new();
			foreach(Connection item in Connections) {
				result.Add(new ConnectionInfo(
					item.From.Parent.Identity,
					item.From.Identity,
					item.To.Parent.Identity,
					item.To.Identity,
					JsonConvert.SerializeObject(item.Path.Properties)
				));
			}
			return result.ToArray();
		}

		private class Connection {
			public ConnectionPath Path { get; private set; }
			public ConnectionControl From { get; private set; }
			public ConnectionControl To { get; private set; }
			public Identity Identity => Path.Identity;

			public Connection(ConnectionPath path, ConnectionControl from, ConnectionControl to) {
				Path = path;
				From = from;
				To = to;
			}

			public void Update() => Path.Update();
		}
	}

}
