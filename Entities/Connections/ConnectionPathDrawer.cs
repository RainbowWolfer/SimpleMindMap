using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using static MindMap.Entities.Connections.ConnectionPath;

namespace MindMap.Entities.Connections {
	public static class ConnectionPathDrawer {
		public const int MIN_GAP = 40;
		private static LineSegment NewLine(Vector2 position) {
			return new LineSegment(position.ToPoint(), true);
		}
		private static LineSegment NewLine(double x, double y) {
			return new LineSegment(new Point(x, y), true);
		}

		public static List<LineSegment> LeftLeft(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> TopTop(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> BotBot(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> RightRight(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}

		public static List<LineSegment> LeftRight(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			List<LineSegment> segments = new();
			if(from.X - MIN_GAP < to.X + MIN_GAP) {
				double mid = from.Y + (to.Y - from.Y) / 2;
				if(from.Y > to.Y) {
					if(from.Y - from.Height / 2 - MIN_GAP < mid
						|| to.Y + to.Height / 2 + MIN_GAP > mid) {
						double toEdge = to.Y + to.Height / 2 + MIN_GAP;
						double fromEdge = from.Y - from.Height / 2 - MIN_GAP;
						double mid_X = from.X + (to.X - from.X) / 2;
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, fromEdge));
						segments.Add(NewLine(mid_X, fromEdge));
						segments.Add(NewLine(mid_X, toEdge));
						segments.Add(NewLine(to.X + MIN_GAP, toEdge));
						segments.Add(NewLine(to.X + MIN_GAP, to.Y));
					} else {
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, mid));
						segments.Add(NewLine(to.X + MIN_GAP, mid));
						segments.Add(NewLine(to.X + MIN_GAP, to.Y));
					}
				} else {
					if(from.Y + from.Height / 2 + MIN_GAP > mid
						|| to.Y - to.Height / 2 - MIN_GAP < mid) {
						double toEdge = to.Y - to.Height / 2 - MIN_GAP;
						double fromEdge = from.Y + from.Height / 2 + MIN_GAP;
						double mid_X = from.X + (to.X - from.X) / 2;
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, fromEdge));
						segments.Add(NewLine(mid_X, fromEdge));
						segments.Add(NewLine(mid_X, toEdge));
						segments.Add(NewLine(to.X + MIN_GAP, toEdge));
						segments.Add(NewLine(to.X + MIN_GAP, to.Y));
					} else {
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, mid));
						segments.Add(NewLine(to.X + MIN_GAP, mid));
						segments.Add(NewLine(to.X + MIN_GAP, to.Y));
					}
				}
			} else {
				double joint = from.X + (to.X - from.X) * extendLengthPercentage / 100;
				segments.Add(NewLine(joint, from.Y));
				segments.Add(NewLine(joint, to.Y));
			}
			return segments;
		}
		public static List<LineSegment> LeftTop(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> LeftBot(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> RightTop(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> RightBot(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
		public static List<LineSegment> TopBot(ConnectionDotInfo from, ConnectionDotInfo to, int extendLengthPercentage) {
			return new List<LineSegment>();
		}
	}
}
