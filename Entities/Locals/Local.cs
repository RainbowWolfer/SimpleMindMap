using Microsoft.Win32;
using MindMap.Entities.Connections;
using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MindMap.Entities.Locals {
	public static class Local {
		public const string APP_FOLDER_NAME = "SimpleMindMap";
		public const string APP_SETTINGS_FILE_NAME = "Settings.json";
		public const string FILTER = "MindMap file (*.mp) | *.mp";

		private static string GetAppSettingsFilePath() {
			string documentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string folderPath = Path.Combine(documentFolder, APP_FOLDER_NAME);
			Directory.CreateDirectory(folderPath);
			string filePath = Path.Combine(folderPath, APP_SETTINGS_FILE_NAME);
			return filePath;
		}

		public static async ValueTask SaveAppSettings() {
			string filePath = GetAppSettingsFilePath();
			await File.WriteAllTextAsync(filePath, (AppSettings.Current ?? AppSettings.GetDefault()).ToJson());
		}

		public static async Task ReadAppSettings() {
			string filePath = GetAppSettingsFilePath();
			if(!File.Exists(filePath)) {
				await SaveAppSettings();
			}
			string text = await File.ReadAllTextAsync(filePath);
			AppSettings.Load(text);
		}

		public static async Task<string?> Save(List<Element> elements, ConnectionsManager connectionsManager, EditHistory editHistory, ImagesAssets imagesAssets, string? filePath = null) {
			if(string.IsNullOrWhiteSpace(filePath)) {
				SaveResult result = WindowsService.CreateSaveFileDialog(FILTER);
				if(result.Success) {
					filePath = result.Path;
				}
			}
			if(string.IsNullOrWhiteSpace(filePath)) {
				return null;
			}
			MapInfo info = new(
				elements.Select(e => new ElementInfo(e)).ToArray(),
				connectionsManager.ConvertInfo(),
				editHistory.GetHistoryInfo(),
				imagesAssets.GetAssets()
			);
			string json = JsonConvert.SerializeObject(info, Formatting.Indented);
			//string converted = StringToBinary(json);
			await File.WriteAllTextAsync(filePath, json);
			return filePath;
		}

		public static async Task<LocalInfo?> Load(string? path = null) {
			OpenFileDialog openFileDialog = new() {
				Filter = FILTER,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			};
			if(string.IsNullOrWhiteSpace(path)) {
				if(openFileDialog.ShowDialog() == true) {
					string json = await File.ReadAllTextAsync(openFileDialog.FileName);
					//string converted = BinaryToString(json);
					LocalInfo info = new(
						JsonConvert.DeserializeObject<MapInfo>(json),
						openFileDialog.SafeFileName,
						openFileDialog.FileName,
						File.GetCreationTime(openFileDialog.FileName)
					);
					return info;
				} else {
					return null;
				}
			} else {
				try {
					string json = await File.ReadAllTextAsync(path);
					LocalInfo info = new(
						JsonConvert.DeserializeObject<MapInfo>(json),
						Path.GetFileName(path),
						path,
						File.GetCreationTime(path)
					);
					return info;
				} catch(FileNotFoundException) {
					return null;
				}
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
		public readonly HistoryInfo history;
		public readonly List<ImageAsset> imagesAssets;

		public MapInfo(ElementInfo[] elements, ConnectionInfo[] connections, HistoryInfo history, List<ImageAsset> imagesAssets) {
			this.elements = elements;
			this.connections = connections;
			this.history = history;
			this.imagesAssets = imagesAssets;
		}
	}

	public class HistoryInfo {
		public string HistoryJson { get; private set; }
		public int Index { get; private set; }

		public HistoryInfo(string historyJson, int index) {
			HistoryJson = historyJson;
			Index = index;
		}
	}

	public class ConnectionInfo {
		public readonly Identity identity;
		public readonly Identity from_element;
		public readonly Identity from_dot;
		public readonly Identity to_element;
		public readonly Identity to_dot;
		public readonly string propertyJson;

		[JsonConstructor]
		public ConnectionInfo(Identity identity, Identity from_element, Identity from_dot, Identity to_element, Identity to_dot, string propertyJson) {
			this.identity = identity;
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
		public readonly ControlsInfo connectionControls;

		[JsonConstructor]
		public ElementInfo(long type_id, Identity identity, string propertyJson, Vector2 position, Vector2 size, ControlsInfo connectionControls) {
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
			this.propertyJson = JsonConvert.SerializeObject(element.Properties, Formatting.Indented);
			this.connectionControls = element.ConnectionsFrame?.GetControlsInfo() ?? new();
		}
	}
}
