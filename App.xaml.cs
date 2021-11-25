using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MindMap {
	public partial class App: Application {
		public App() {
			switch(Keyboard.GetKeyStates(Key.LeftCtrl)) {
				case KeyStates.None:
					Debug.WriteLine(1);
					break;
				case KeyStates.Down:
					Debug.WriteLine(2);
					break;
				case KeyStates.Toggled:
					Debug.WriteLine(3);
					break;
				default:
					Debug.WriteLine(4);
					break;
			}
		}
	}
}
