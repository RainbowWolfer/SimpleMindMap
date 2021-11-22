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

namespace MindMap.Entities.Locals {
	public static class Local {

		static Local() {

		}


		public static async void Save(List<Element> elements) {
			string json = JsonConvert.SerializeObject(elements.Select(e => new ElementInfo(e)));

			await File.WriteAllTextAsync("WriteText.txt", json);
		}

		public static ElementInfo[] Load(string json) {

			return JsonConvert.DeserializeObject<ElementInfo[]>(json) ?? Array.Empty<ElementInfo>();
		}

		public class ConnectionInfo {
			
		}

		public class ElementInfo {
			public long element_id;
			public string propertyJson;
			public Vector2 position;
			public Vector2 size;
			[JsonConstructor]
			public ElementInfo(long element_id, string propertyJson, Vector2 position, Vector2 size) {
				this.element_id = element_id;
				this.propertyJson = propertyJson;
				this.position = position;
				this.size = size;
			}
			public ElementInfo(Element e) {
				this.element_id = e.TypeID;
				this.position = e.GetPosition();
				this.size = e.GetSize();
				this.propertyJson = JsonConvert.SerializeObject(e.Properties);
			}
		}
	}
}
