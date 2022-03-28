using MindMap.Entities.Connections;

namespace MindMap.Entities.Tags {
	public class ConnectionPathFrameworkTag: ITag {
		public ConnectionPath Target { get; set; }

		public ConnectionPathFrameworkTag(ConnectionPath target) {
			Target = target;
		}
	}
}
