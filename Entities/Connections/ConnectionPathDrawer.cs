using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static MindMap.Entities.Connections.ConnectionPath;

namespace MindMap.Entities.Connections {
	public static class ConnectionPathDrawer {
		public const int MIN_GAP = 40;
		public static List<LineSegment> LeftLeft(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> TopTop(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> BotBot(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> RightRight(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}

		public static List<LineSegment> LeftRight(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> LeftTop(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> LeftBot(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> RightTop(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> RightBot(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> TopBot(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new List<LineSegment>();
		}
	}
}
