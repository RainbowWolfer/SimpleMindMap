using Microsoft.Win32;
using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
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

		public static async void SaveTmpFile(string fileName, string content) {
			await File.WriteAllTextAsync($"F:\\Documents\\MindMap\\{fileName}", content);
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
		//history info

		public MapInfo(ElementInfo[] elements, ConnectionInfo[] connections) {
			this.elements = elements;
			this.connections = connections;
		}
	}

	public class ConnectionInfo {
		public readonly Identity from_element;
		public readonly Identity from_dot;
		public readonly Identity to_element;
		public readonly Identity to_dot;
		public readonly string propertyJson;

		[JsonConstructor]
		public ConnectionInfo(Identity from_element, Identity from_dot, Identity to_element, Identity to_dot, string propertyJson) {
			this.from_element = from_element;
			this.from_dot = from_dot;
			this.to_element = to_element;
			this.to_dot = to_dot;
			this.propertyJson = propertyJson;
		}
	}

	public class ElementInfo {
		public readonly long type_id;
		public readonly Identity identity;
		public readonly string propertyJson;
		public readonly Vector2 position;
		public readonly Vector2 size;
		public readonly Dictionary<Direction, int> connectionControls;

		[JsonConstructor]
		public ElementInfo(long type_id, Identity identity, string propertyJson, Vector2 position, Vector2 size, Dictionary<Direction, int> connectionControls) {
			this.type_id = type_id;
			this.identity = identity;
			this.propertyJson = propertyJson;
			this.position = position;
			this.size = size;
			this.connectionControls = connectionControls;
		}

		public ElementInfo(Element element) {
			this.type_id = element.TypeID;
			this.identity = element.Identity;
			this.position = element.GetPosition();
			this.size = element.GetSize();
			this.propertyJson = JsonConvert.SerializeObject(element.Properties);
			this.connectionControls = element.ConnectionsFrame?.GetControlsInfo() ?? new();
		}
	}
}
