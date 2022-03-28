using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Presets {
	public class ElementPresetsGroup {
		public string Name { get; set; }
		public List<ElementPresetsGroup> Group { get; set; }

		public ElementPresetsGroup(string name, List<ElementPresetsGroup> group) {
			Name = name;
			Group = group;
		}

		public static List<ElementPresetsGroup> GetDefaultList() {
			List<ElementPresetsGroup> result = new();
			result.Add(new ElementPresetsGroup("", new List<ElementPresetsGroup>() {

			}));
			return result;
		}
	}
}
