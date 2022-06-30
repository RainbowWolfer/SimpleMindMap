using MindMap.Entities.Elements;
using MindMap.Entities.Elements.TextShapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Presets {
	public class ElementPresetsGroup {
		public string Name { get; set; }
		public List<ElementPreset> Presets { get; set; }

		public ElementPresetsGroup(string name, List<ElementPreset> presets) {
			Name = name;
			Presets = presets;
		}

		public static List<ElementPresetsGroup> GetDefaultList() {
			List<ElementPresetsGroup> result = new();
			result.Add(new ElementPresetsGroup("Default", new List<ElementPreset>() {
				new ElementPreset("Rectangle", ElementGenerator.ID_Rectangle, MyRectangle.GetDefaultPropertyJson()),
			}));
			return result;
		}
	}
}
