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
		private readonly List<Combo> pool = new();

		private readonly List<Key> current = new();
		public ComboKeyManager(Window target) {
			target.KeyDown += (s, e) => {
				if(!current.Contains(e.Key)) {
					current.Add(e.Key);
				}
				Listen(KeyState.Pressed);
			};

			target.KeyUp += (s, e) => {
				Listen(KeyState.Released);
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

		private void Listen(KeyState state) {
			foreach(Combo item in pool) {
				if(current.SequenceEqual(item.keys)) {
					switch(state) {
						case KeyState.Pressed:
							item.keyDown.Invoke();
							break;
						case KeyState.Released:
							item.keyUp.Invoke();
							break;
						default:
							throw new Exception($"KeyState({state}) not found");
					}
				}
			}
		}

		public void Register(Action keyDown, Action keyUp, params Key[] keys) {
			if(CheckKeysExisted(keys)) {
				return;
			}
			pool.Add(new Combo(keyDown, keyUp, keys));
		}

		private bool CheckKeysExisted(Key[] keys) => FindByKeys(keys) != null;

		private Combo? FindByKeys(Key[] keys) {
			foreach(Combo combo in pool) {
				if(keys.Length != combo.keys.Length) {
					continue;
				}
				bool match = true;
				foreach(Key item in keys) {
					if(!combo.keys.Contains(item)) {
						match = false;
						break;
					}
				}
				if(match) {
					return combo;
				}
			}
			return null;
		}

		public void Dispose(params Key[] keys) {
			Combo? found = FindByKeys(keys);
			if(found == null) {
				return;
			}
			pool.Remove(found);
		}

		public enum KeyState {
			Pressed, Released
		}

		private class Combo {
			public Action keyDown;
			public Action keyUp;
			public Key[] keys;
			public Combo(Action keyDown, Action keyUp, Key[] keys) {
				this.keyDown = keyDown;
				this.keyUp = keyUp;
				this.keys = keys;
			}
		}
	}
}
