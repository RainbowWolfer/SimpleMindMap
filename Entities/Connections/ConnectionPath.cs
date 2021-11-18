using MindMap.Entities.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Connections {
	public class ConnectionPath {
		public Element from;
		public Element to;
		public ConnectionPath(Element from, Element to) {
			this.from = from;
			this.to = to;
		}
	}
}
