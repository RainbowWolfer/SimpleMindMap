using MindMap.Entities.Presets;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MindMap.Entities.Locals {
	public class AppSettings {
		public static AppSettings? Current { get; set; }

		public bool EnableElementDefaultPositionCentered = true;
		public Vector2 ElementDefaultPosition { get; set; } = Vector2.Zero;
		public double ElementDefaultHeight { get; set; } = -1;
		public string ElementDefaultText { get; set; } = "";
		public BackgroundStyle BackgroundStyle { get; set; } = BackgroundStyle.Dot;
		public double BackgroundShapeSize { get; set; } = -1;
		public double BackgroundShapeGap { get; set; } = -1;
		public bool EnableCanvasRule { get; set; } = true;
		public double CanvasRuleGap { get; set; } = 100;

		public List<ElementPresetsGroup> ElementPresetsGroups { get; set; } = new();

		public string ToJson() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public AppSettings() { }

		public static void Load(string json) {
			AppSettings? obj = JsonConvert.DeserializeObject<AppSettings>(json);
			Current = obj ?? new AppSettings() {
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
			};
		}

	}

	public enum BackgroundStyle {
		Dot, Rect, Heart
	}
}
