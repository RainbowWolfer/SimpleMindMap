using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MindMap.Entities {
	public enum Direction {
		Left, Right, Top, Bottom
	}

	public class DirectionArgs {
		public bool Left;
		public bool Right;
		public bool Top;
		public bool Bottom;

		public static DirectionArgs All => new DirectionArgs(true, true, true, true);

		public DirectionArgs(IEnumerable<Direction> directions) {
			foreach(Direction item in directions) {
				Assign(item);
			}
		}

		public DirectionArgs(Direction direction) {
			Assign(direction);
		}

		public DirectionArgs(bool left, bool right, bool top, bool bottom) {
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}

		private void Assign(Direction direction) {
			switch(direction) {
				case Direction.Left:
					Left = true;
					break;
				case Direction.Right:
					Right = true;
					break;
				case Direction.Top:
					Top = true;
					break;
				case Direction.Bottom:
					Bottom = true;
					break;
				default:
					throw new Exception($"Direction ({direction}) Not Found");
			}
		}
	}
}
