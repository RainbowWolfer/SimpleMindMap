using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Identifications {
	public class Identity {
		public event Action<string, string>? OnNameChanged;

		private string name;

		public string ID { get; set; }
		public string Name {
			get => name;
			set {
				string oldValue = name;
				name = value;
				OnNameChanged?.Invoke(oldValue, value);
			}
		}

		public Identity(string id, string name) {
			ID = id;
			this.name = name;
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
