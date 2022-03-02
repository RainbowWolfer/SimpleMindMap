using MindMap.Entities.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Tags {
	public class ElementFrameworkTag {
		public Element Target { get; set; }

		public ElementFrameworkTag(Element element) {
			Target = element;
		}
	}
}
