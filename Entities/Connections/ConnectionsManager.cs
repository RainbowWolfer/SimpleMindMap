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

		public ConnectionPath? CurrentSelection { get; set; }

		public ConnectionsManager(MindMapPage mindMapPage) {
			_parent = mindMapPage;
			_mainCanvas = mindMapPage.MainCanvas;
		}

		public bool CheckDuplicate(ConnectionControl from, ConnectionControl to) {
			return Connections.Any(c => c.From == from && c.To == to);
		}

		public void Add(ConnectionControl from, ConnectionControl to, string? propertyJson = null) {
			if(_mainCanvas == null || CheckDuplicate(from, to)) {
				return;
			}
			Connections.Add(new Connection(string.IsNullOrEmpty(propertyJson) ?
				new ConnectionPath(_mainCanvas, _parent.connectionsManager, from, to) :
				new ConnectionPath(_mainCanvas, _parent.connectionsManager, from, to, propertyJson)
			, from, to));
		}

		public void Remove(ConnectionPath path) {
			if(_mainCanvas == null || path.to == null) {
				return;
			}
			Connection? found = Connections.Find(c => c.Path == path);
			if(found != null) {
				found.Path.ClearFromCanvas();
				found.Path.ClearBackground();
				Connections.Remove(found);
			}
		}

		public void Remove(ConnectionsFrame frame) {
			if(_mainCanvas == null) {
				return;
			}
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
			_parent?.ShowConnectionPathProperties(path);
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
