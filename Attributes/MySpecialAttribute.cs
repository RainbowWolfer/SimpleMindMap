using System;

namespace MindMap.Attributes {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
	public class MySpecialAttribute: Attribute {
		public MySpecialAttribute(string s, int i) {

		}
	}
}
