using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace MindMap.Entities {
	[Serializable]
	public struct Vector2 {
		public double X { get; set; }
		public double Y { get; set; }

		[JsonIgnore]
		public int X_Int => (int)Math.Floor(X);
		[JsonIgnore]
		public int Y_Int => (int)Math.Floor(Y);

		public static readonly Vector2 Zero = new();
		public static readonly Vector2 One = new(1, 1);
		public static readonly Vector2 Max = new(double.MaxValue, double.MaxValue);

		public double Magnitude {
			get {
				return (double)Math.Sqrt(X * X + Y * Y);
			}
		}

		public static double Distance(Vector2 a, Vector2 b) {
			return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
		}

		public Vector2() {
			X = 0;
			Y = 0;
		}

		public Vector2(double size) {
			X = size;
			Y = size;
		}

		public Vector2(double x, double y) {
			this.X = x;
			this.Y = y;
		}

		public Vector2(Point point) {
			this.X = point.X;
			this.Y = point.Y;
		}

		public Vector2(Vector vector) {
			this.X = vector.X;
			this.Y = vector.Y;
		}

		public Vector2 Clone() => new(X, Y);

		public Vector2 Bound(Vector2 from, Vector2 to) {
			var result = this.Clone();
			if(result.X < from.X) {
				result.X = from.X;
			} else if(result.X > to.X) {
				result.X = to.X;
			}
			if(result.Y < from.Y) {
				result.Y = from.Y;
			} else if(result.Y > to.Y) {
				result.Y = to.Y;
			}
			return result;
		}

		public Vector2 ToInt() {
			return new Vector2(Math.Floor(X), Math.Floor(Y));
		}

		public Point ToPoint() {
			return new Point(X, Y);
		}

		public string ToString(int lengthAfterDot) {//"bug": does not work with negative number
			return $"Vector2: (" +
			$"{Math.Round(X, lengthAfterDot)}," +
			$"{Math.Round(Y, lengthAfterDot)})";
		}

		public override string ToString() {
			return $"Vector2: ({X},{Y})";
		}

		public override bool Equals(object? obj) {
			return obj is Vector2 v && v.X == X && v.Y == Y;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static implicit operator Vector2(Point point) {
			return new Vector2(point.X, point.Y);
		}

		public static implicit operator Vector2(Vector vec) {
			return new Vector2(vec.X, vec.Y);
		}

		public static implicit operator Vector2(Size size) {
			return new Vector2(size.Width, size.Height);
		}

		public static implicit operator string(Vector2 vec) => vec.ToString();

		public static bool operator ==(Vector2 v1, Vector2 v2) => v1.Equals(v2);
		public static bool operator !=(Vector2 v1, Vector2 v2) => !v1.Equals(v2);

		public static Vector2 operator +(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
		}
		public static Vector2 operator -(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
		}
		public static Vector2 operator -(Vector2 v) {
			return new Vector2(-v.X, -v.Y);
		}
		public static Vector2 operator *(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X * v2.X, v1.Y * v2.Y);
		}
		public static Vector2 operator /(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X / v2.X, v1.Y / v2.Y);
		}
		public static Vector2 operator *(Vector2 v1, double d) {
			return new Vector2(v1.X * d, v1.Y * d);
		}
		public static Vector2 operator /(Vector2 v1, double d) {
			return new Vector2(v1.X / d, v1.Y / d);
		}
	}
}
