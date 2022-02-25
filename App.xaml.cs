﻿using MindMap.Entities;
using MindMap.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using MindMap.Attributes;
using System.Windows.Controls;
using System.Reflection;
using Newtonsoft.Json;
using MindMap.Entities.Identifications;

namespace MindMap {
	public partial class App: Application {

		public App() {
			TypeInfo typeInfo = typeof(MyClass).GetTypeInfo();
			Debug.WriteLine("The assembly qualified name of MyClass is " + typeInfo.AssemblyQualifiedName);

			SubTest v1 = new("1", "1", "1");
			SubTest v2 = (SubTest)v1.Clone();
			//v2.name = "2";
			//Debug.WriteLine($"Start {v1.}");
			Identity a = new Identity("1", "123");
			Identity b = new Identity("2", "123");
			Debug.WriteLine(a == b);
		}
	}

	[MySpecial("2", 1)]
	public class MyClass {
		//[MySpecial("2", 1)]
		public int a;
		[MySpecial("2", 1)]
		public int A { get; set; }
	}


	public class Test: ICloneable {
		public string id;
		public string name;

		public Test(string id, string name) {
			this.id = id;
			this.name = name;
		}

		public object Clone() => MemberwiseClone();

		public override string ToString() {
			return $"id: {id} | name: {name}";
		}
	}

	public class SubTest: Test {
		public string description;

		public SubTest(string id, string name, string description) : base(id, name) {
			this.description = description;
		}
		public override string ToString() {
			return $"id: {id} | name: {name} | description:{description}";
		}
	}
}
