using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Properties {
	public class LocalProperty {
		public string PropertyJson { get; private set; }
		//[JsonIgnore]
		//public IProperty Property { get; private set; }

		//[JsonConstructor]
		//public LocalProperty(string propertyJson) {
		//	PropertyJson = propertyJson;
		//}

		//public void SetPropertyJson(string propertyJson) {
		//	PropertyJson = propertyJson;
		//	Property =
		//}

	}
}
