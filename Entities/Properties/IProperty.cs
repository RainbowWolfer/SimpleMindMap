using System;

namespace MindMap.Entities.Properties {
	public interface IProperty: ICloneable {
		public IProperty Translate(string json);

		public static void MakeClone(ref IProperty property) {
			property = (IProperty)property.Clone();
		}

		public static IProperty MakeClone(IProperty property) {
			return (IProperty)property.Clone();
		}
	}

}
