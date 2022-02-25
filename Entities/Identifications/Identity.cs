using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Identifications {
	public class Identity {
		public string ID { get; set; }
		public string Name { get; set; }

		public Identity(string id, string name) {
			ID = id;
			Name = name;
		}

		public override string ToString() {
			return $"ID({ID}) Name({Name})";
		}

		public override bool Equals(object? obj) {
			return obj is Identity i && i.ID == this.ID;
		}

		public bool RefEquals(object? obj) {
			return obj is Identity i && i.ID == this.ID && i.Name == this.Name;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
	}
}
