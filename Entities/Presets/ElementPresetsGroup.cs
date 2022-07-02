using MindMap.Entities.Elements;
using MindMap.Entities.Elements.TextShapes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MindMap.Entities.Presets {
	public class ElementPresetsGroup {
		public string Name { get; set; }
		public bool Unchangable { get; set; } = false;
		public List<ElementPreset> Presets { get; set; }

		public ElementPresetsGroup(string name) {
			Name = name;
			Presets = new List<ElementPreset>();
		}

		[JsonConstructor]
		public ElementPresetsGroup(string name, List<ElementPreset> presets) {
			Name = name;
			Presets = presets;
		}

		public static List<ElementPresetsGroup> GetDefaultList() {
			List<ElementPresetsGroup> result = new();
			result.Add(new ElementPresetsGroup("Default", new List<ElementPreset>() {
				new ElementPreset("Rectangle", ElementGenerator.ID_Rectangle, MyRectangle.GetDefaultPropertyJson(), default),
				new ElementPreset("Ellipse", ElementGenerator.ID_Ellipse, MyEllipse.GetDefaultPropertyJson(), default),
				new ElementPreset("Polygon", ElementGenerator.ID_Polygon, MyPolygon.GetDefaultPropertyJson(), default),
				new ElementPreset("Image", ElementGenerator.ID_Image, MyImage.GetDefaultPropertyJson(), default),
			}) {
				Unchangable = true,
			});
			return result;
		}
	}
}
