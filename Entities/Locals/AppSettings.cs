using MindMap.Entities.Presets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MindMap.Entities.Locals {
	public class AppSettings {
		public static AppSettings? Current { get; set; }

		public bool EnableElementDefaultPositionCentered = true;
		public Vector2 ElementDefaultPosition { get; set; } = Vector2.Zero;
		public double ElementDefaultHeight { get; set; } = 150;
		public string ElementDefaultText { get; set; } = "(Default Text)";
		public BackgroundStyle BackgroundStyle { get; set; } = BackgroundStyle.Dot;
		public double BackgroundShapeSize { get; set; } = 4;
		public double BackgroundShapeGap { get; set; } = 20;
		public bool EnableCanvasRule { get; set; } = true;
		public double CanvasRuleGap { get; set; } = 100;

		public List<(string name, string path, DateTime time)> RecentOpenFilesList { get; set; } = new();
		public List<ElementPresetsGroup> ElementPresetsGroups { get; set; } = new();

		public string ToJson() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public AppSettings() { }

		public static void Load(string json) {
			AppSettings? obj = JsonConvert.DeserializeObject<AppSettings>(json);
			Current = obj ?? GetDefault();
			if(Current.ElementPresetsGroups.Count <= 1) {
				Current.ElementPresetsGroups = ElementPresetsGroup.GetDefaultList();
			}
		}

		public static AppSettings GetDefault() {
			return new AppSettings() {
				EnableElementDefaultPositionCentered = true,
				ElementDefaultPosition = Vector2.Zero,
				ElementDefaultHeight = 150,
				ElementDefaultText = "(Default Text)",
				BackgroundStyle = BackgroundStyle.Dot,
				BackgroundShapeSize = 5,
				BackgroundShapeGap = 20,
				EnableCanvasRule = true,
				CanvasRuleGap = 100,
				ElementPresetsGroups = ElementPresetsGroup.GetDefaultList(),
				RecentOpenFilesList = new List<(string, string, DateTime)>(),
			};
		}

		public List<(string, string, DateTime)> GetDefaultTestValues() {
			List<(string, string, DateTime)> result = new();
			result.Add(new("1.mp", "C:\\test\\UWU", new DateTime(2021, 1, 1, 1, 1, 1)));
			result.Add(new("My Test Map.mp", "C:\\test\\OWO", new DateTime(2022, 2, 12, 1, 1, 1)));
			result.Add(new("My Test Map1.mp", "C:\\test\\OWO", new DateTime(2022, 3, 12, 1, 1, 1)));
			result.Add(new("My Test Map2.mp", "C:\\test\\OWO", new DateTime(2022, 3, 28, 1, 1, 1)));
			result.Add(new("My Test Map3.mp", "C:\\test\\OWO", new DateTime(2022, 3, 29, 1, 1, 1)));
			result.Add(new("My Test Map3.mp", "C:\\test\\OWO", new DateTime(2022, 3, 29, 1, 1, 1)));
			result.Add(new("My Test Map3.mp", "C:\\test\\OWO", new DateTime(2022, 3, 29, 1, 1, 1)));
			return result;
		}

	}

	public enum BackgroundStyle {
		Dot, Rect, Heart
	}
}
