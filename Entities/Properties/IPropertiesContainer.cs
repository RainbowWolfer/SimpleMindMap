using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Properties {
	public interface IPropertiesContainer {
		IProperty Properties { get; }
		void SetProperty(IProperty property);
		string GetID();
		public static void PropertyChangedHandler(IPropertiesContainer target, Action assign, Action<IProperty, IProperty> submit) {
			IProperty oldProperty = IProperty.MakeClone(target.Properties);
			assign.Invoke();
			IProperty newProperty = IProperty.MakeClone(target.Properties);
			submit.Invoke(oldProperty, newProperty);
		}
	}
}
