using MindMap.Entities.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

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
