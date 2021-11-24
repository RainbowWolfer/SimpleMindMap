using MindMap.Entities.Frames;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static MindMap.Entities.Locals.Local;

namespace MindMap.Entities.Connections {
	public static class ConnectionsManager {
		private static List<Connection> Connections { get; } = new();
		private static Canvas? _mainCanvas;
		private static MindMapPage? _parent;

		public static ConnectionPath? CurrentSelection { get; set; }

		public static void Initialize(MindMapPage mindMapPage) {
			_parent = mindMapPage;
			_mainCanvas = mindMapPage.MainCanvas;
		}

		public static bool CheckDuplicate(ConnectionControl from, ConnectionControl to) {
			return Connections.Any(c => c.From == from && c.To == to);
		}

		public static void Add(ConnectionControl from, ConnectionControl to, string? propertyJson = null) {
			if(_mainCanvas == null || CheckDuplicate(from, to)) {
				return;
			}
			Connections.Add(new Connection(string.IsNullOrEmpty(propertyJson) ?
				new ConnectionPath(_mainCanvas, from, to) : 
				new ConnectionPath(_mainCanvas, from, to, propertyJson)
			, from, to));
		}

		public static void Remove(ConnectionPath path) {
			if(_mainCanvas == null || path.to == null) {
				return;
			}
			Connection? found = Connections.Find(c => c.Path == path);
			if(found != null) {
				found.Path.ClearFromCanvas();
				Connections.Remove(found);
			}
		}

		public static void Remove(ConnectionsFrame frame) {
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

		public static void Update(ConnectionControl dot) {
			foreach(Connection item in Connections.Where(c => c.From == dot || c.To == dot)) {
				item.Update();
			}
		}

		public static void ShowProperties(ConnectionPath path) {
			_parent?.ShowConnectionPathProperties(path);
		}

		public static void DebugConnections() {
			Debug.WriteLine("START");
			foreach(Connection c in Connections) {
				Debug.WriteLine($"{c.Path} | {c.From.ID} | {c.To.ID}");
			}
			Debug.WriteLine("END");
		}

		public static ConnectionInfo[] ConvertInfo() {
			List<ConnectionInfo> result = new();
			foreach(Connection item in Connections) {
				result.Add(new ConnectionInfo(
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
