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

		public static async Task<string> Save(List<Element> elements, ConnectionsManager connectionsManager, string filePath = "") {
			if(string.IsNullOrEmpty(filePath)) {
				SaveFileDialog dialog = new() {
					Filter = FILTER,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				};
				if(dialog.ShowDialog() == true) {
					filePath = dialog.FileName;
				} else {
					return filePath;
				}
			}

			MapInfo info = new(
				elements.Select(e => new ElementInfo(e)).ToArray(),
				connectionsManager.ConvertInfo()
			);
			string json = JsonConvert.SerializeObject(info);
			string converted = StringToBinary(json);
			await File.WriteAllTextAsync(filePath, converted);

			return filePath;
		}

		public static string StringToBinary(string data) {
			string result = "";
			foreach(char c in data.ToCharArray()) {
				result += Convert.ToString(c, 2).PadLeft(8, '0');
			}
			return result;
		}

		public static string BinaryToString(string data) {
			List<byte> byteList = new();
			for(int i = 0; i < data.Length; i += 8) {
				byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
			}
			return Encoding.ASCII.GetString(byteList.ToArray());
		}

		public static async Task<LocalInfo?> Load() {
			OpenFileDialog openFileDialog = new() {
				Filter = FILTER,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			};
			if(openFileDialog.ShowDialog() == true) {
				Debug.WriteLine(openFileDialog.FileName);
				string json = await File.ReadAllTextAsync(openFileDialog.FileName);
				string converted = BinaryToString(json);
				LocalInfo info = new(
					JsonConvert.DeserializeObject<MapInfo>(converted),
					openFileDialog.SafeFileName,
					openFileDialog.FileName,
					File.GetCreationTime(openFileDialog.FileName)
				);
				return info;
			} else {
				return null;
			}
		}

		public class LocalInfo {
			public MapInfo? MapInfo { get; private set; }
			public FileInfo FileInfo { get; private set; }

			public LocalInfo(MapInfo? info, string filename, string path, DateTime date) {
				MapInfo = info;
				FileInfo = new FileInfo(filename, path, date);
			}
		}

		public class MapInfo {
			public readonly ElementInfo[] elements;
			public readonly ConnectionInfo[] connections;
			public MapInfo(ElementInfo[] elements, ConnectionInfo[] connections) {
				this.elements = elements;
				this.connections = connections;
			}
		}

		public class ConnectionInfo {
			public readonly string from_parent_id;
			public readonly string from_dot_id;
			public readonly string to_parent_id;
			public readonly string to_dot_id;
			public readonly string propertyJson;
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
			public readonly long type_id;
			public readonly string element_id;
			public readonly string propertyJson;
			public readonly Vector2 position;
			public readonly Vector2 size;
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
