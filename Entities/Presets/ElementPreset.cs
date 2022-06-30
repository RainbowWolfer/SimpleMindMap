using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Presets {
	public class ElementPreset {
		public string Name { get; set; }
		public long TypeID { get; set; }
		public string PropertiesJson { get; set; }
		public Vector2 Size { get; set; }

		public ElementPreset(string name, long typeID, string propertiesJson, Vector2 size) {
			Name = name;
			TypeID = typeID;
			PropertiesJson = propertiesJson;
			Size = size;
		}
	}
}
