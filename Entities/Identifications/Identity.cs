using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Identifications {
	public class Identity {
		public string ID { get; set; }
		public string Name { get; set; }

		public Identity(string iD, string name) {
			ID = iD;
			Name = name;
		}
	}
}
