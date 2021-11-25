using Microsoft.Win32;
using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace MindMap.Entities.Locals {
	public static class Local {
		public const string FILTER = "MindMap file (*.mp) | *.mp";
		static Local() {

		}

		public static async void Save(List<Element> elements, ConnectionsManager connectionsManager) {
			SaveFileDialog dialog = new() {
				Filter = FILTER,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			};
			if(dialog.ShowDialog() == true) {
				EverythingInfo info = new(
					elements.Select(e => new ElementInfo(e)).ToArray(),
					connectionsManager.ConvertInfo()
				);
				string json = JsonConvert.SerializeObject(info);
				await File.WriteAllTextAsync(dialog.FileName, json);
			}
		}

		public static async Task<FileInfo?> Load() {
			OpenFileDialog openFileDialog = new() {
				Filter = FILTER,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			};
			if(openFileDialog.ShowDialog() == true) {
				Debug.WriteLine(openFileDialog.FileName);
				string json = await File.ReadAllTextAsync(openFileDialog.FileName);
				FileInfo info = new(JsonConvert.DeserializeObject<EverythingInfo>(json), openFileDialog.SafeFileName);
				return info;
			} else {
				return null;
			}
		}

		public class FileInfo {
			public EverythingInfo? Info { get; set; }
			public string Filename { get; set; }

			public FileInfo(EverythingInfo? info, string filename) {
				Info = info;
				Filename = filename;
			}
		}

		public class EverythingInfo {
			public ElementInfo[] elements;
			public ConnectionInfo[] connections;
			public EverythingInfo(ElementInfo[] elements, ConnectionInfo[] connections) {
				this.elements = elements;
				this.connections = connections;
			}
		}

		public class ConnectionInfo {
			public string from_parent_id;
			public string from_dot_id;
			public string to_parent_id;
			public string to_dot_id;
			public string propertyJson;
			[JsonConstructor]
			public ConnectionInfo(string from_parent_id, string from_dot_id, string to_parent_id, string to_dot_id, string propertyJson) {
				this.from_parent_id = from_parent_id;
				this.from_dot_id = from_dot_id;
				this.to_parent_id = to_parent_id;
				this.to_dot_id = to_dot_id;
				this.propertyJson = propertyJson;
			}
		}

		public class ElementInfo {
			public long type_id;
			public string element_id;
			public string propertyJson;
			public Vector2 position;
			public Vector2 size;
			[JsonConstructor]
			public ElementInfo(long type_id, string element_id, string propertyJson, Vector2 position, Vector2 size) {
				this.type_id = type_id;
				this.element_id = element_id;
				this.propertyJson = propertyJson;
				this.position = position;
				this.size = size;
			}
			public ElementInfo(Element e) {
				this.type_id = e.TypeID;
				this.element_id = e.ID;
				this.position = e.GetPosition();
				this.size = e.GetSize();
				this.propertyJson = JsonConvert.SerializeObject(e.Properties);
			}
		}
	}
}
