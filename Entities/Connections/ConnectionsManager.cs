using MindMap.Entities.Frames;
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

		public void Add(ConnectionPath path, bool submitHistory = true, string? propertyJson = null) {
			if(path.to == null || CheckDuplicate(path.from, path.to)) {
				return;
			}
			Connections.Add(new Connection(path, path.from, path.to));
			path.Initialize(false);
			_parent.UpdateCount();
			if(submitHistory) {
				_parent.editHistory.SubmitByConnectionCreated(path);
			}
		}

		public void Add(ConnectionControl from, ConnectionControl to, bool submitHistory = true, string? propertyJson = null) {
			if(CheckDuplicate(from, to)) {
				return;
			}
			var connectionPath = string.IsNullOrEmpty(propertyJson) ?
				new ConnectionPath(_parent, _mainCanvas, _parent.connectionsManager, from, to) :
				new ConnectionPath(_parent, _mainCanvas, _parent.connectionsManager, from, to, propertyJson);
			Connections.Add(new Connection(connectionPath, from, to));
			_parent.UpdateCount();
			if(submitHistory) {
				_parent.editHistory.SubmitByConnectionCreated(connectionPath);
			}
		}

		public void Remove(ConnectionPath path, bool submitHistory = true) {
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

		public void Remove(ConnectionsFrame frame) {
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
				Debug.WriteLine($"{c.Path} | {c.From.ID} | {c.To.ID}");
			}
			Debug.WriteLine("END");
		}

		public Local.ConnectionInfo[] ConvertInfo() {
			List<Local.ConnectionInfo> result = new();
			foreach(Connection item in Connections) {
				result.Add(new Local.ConnectionInfo(
					item.From.Parent_ID,
					item.From.ID,
					item.To.Parent_ID,
					item.To.ID,
					JsonConvert.SerializeObject(item.Path.Properties)
				));
			}
			return result.ToArray();
		}

		private class Connection {
			public ConnectionPath Path { get; private set; }
			public ConnectionControl From { get; private set; }
			public ConnectionControl To { get; private set; }

			public Connection(ConnectionPath path, ConnectionControl from, ConnectionControl to) {
				Path = path;
				From = from;
				To = to;
			}

			public void Update() => Path.Update();
		}
	}

}
