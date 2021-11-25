using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MindMap.Entities {
	public class ComboKeyManager {
		private readonly Dictionary<Action, Key[]> pool = new();

		private readonly List<Key> current = new();
		public ComboKeyManager(Window target) {
			target.KeyDown += (s, e) => {
				if(!current.Contains(e.Key)) {
					current.Add(e.Key);
				}
			};

			target.KeyUp += (s, e) => {
				if(current.Contains(e.Key)) {
					current.Remove(e.Key);
				}
			};
			DebugKeys();
		}
		private async void DebugKeys() {
			while(true) {
				string result = "";
				current.ForEach(c => result += c + " ");
				Debug.WriteLine(result);
				await Task.Delay(10);
			}
		}

		public void Register(Action action, params Key[] combo) {

		}
	}
}
