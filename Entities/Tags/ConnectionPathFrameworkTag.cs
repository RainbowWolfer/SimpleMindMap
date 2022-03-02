using MindMap.Entities.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Tags {
	public class ConnectionPathFrameworkTag: ITag {
		public ConnectionPath Target { get; set; }

		public ConnectionPathFrameworkTag(ConnectionPath target) {
			Target = target;
		}
	}
}
