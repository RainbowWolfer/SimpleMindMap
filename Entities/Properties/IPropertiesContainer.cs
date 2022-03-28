using MindMap.Entities.Identifications;
using System;

namespace MindMap.Entities.Properties {
	public interface IPropertiesContainer {
		IProperty Properties { get; }
		void SetProperty(IProperty property);
		void SetProperty(string propertyJson);
		Identity GetIdentity();

		public static void PropertyChangedHandler(IPropertiesContainer target, Action assign, Action<IProperty, IProperty> submit) {
			IProperty oldProperty = IProperty.MakeClone(target.Properties);
			assign.Invoke();
			IProperty newProperty = IProperty.MakeClone(target.Properties);
			submit.Invoke(oldProperty, newProperty);
		}
	}
}
