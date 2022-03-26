using MindMap.Entities.Elements;
using MindMap.Entities.Tags;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Interactions {
	public class MultiSelectionFrame {
		private readonly MindMapPage _parent;
		private Canvas MainCanvas => _parent.MainCanvas;

		public readonly Line top;
		public readonly Line bot;
		public readonly Line left;
		public readonly Line right;

		private Vector2 Start { get; set; }
		private Vector2 End { get; set; }

		public MultiSelectionFrame(MindMapPage parent, Action<object, MouseButtonEventArgs> mouseDown, Action<object, MouseEventArgs> mouseMove, Action<object, MouseButtonEventArgs> mouseUp) {
			this._parent = parent;

			Style style_line = new(typeof(Line));
			style_line.Setters.Add(new Setter(Shape.StrokeProperty, Brushes.Black));
			style_line.Setters.Add(new Setter(Shape.StrokeDashArrayProperty, new DoubleCollection(new double[] { 3, 0.5 })));
			style_line.Setters.Add(new Setter(Shape.StrokeThicknessProperty, (double)2));

			top = new Line() {
				Style = style_line,
				Tag = new MultiSelectionFrameworkTag(),
			};
			bot = new Line() {
				Style = style_line,
				Tag = new MultiSelectionFrameworkTag(),
			};
			left = new Line() {
				Style = style_line,
				Tag = new MultiSelectionFrameworkTag(),
			};
			right = new Line() {
				Style = style_line,
				Tag = new MultiSelectionFrameworkTag(),
			};

			MainCanvas.Children.Add(top);
			MainCanvas.Children.Add(bot);
			MainCanvas.Children.Add(left);
			MainCanvas.Children.Add(right);

			top.MouseDown += (s, e) => mouseDown.Invoke(s, e);
			top.MouseMove += (s, e) => mouseMove.Invoke(s, e);
			top.MouseUp += (s, e) => mouseUp.Invoke(s, e);

			bot.MouseDown += (s, e) => mouseDown.Invoke(s, e);
			bot.MouseMove += (s, e) => mouseMove.Invoke(s, e);
			bot.MouseUp += (s, e) => mouseUp.Invoke(s, e);

			left.MouseDown += (s, e) => mouseDown.Invoke(s, e);
			left.MouseMove += (s, e) => mouseMove.Invoke(s, e);
			left.MouseUp += (s, e) => mouseUp.Invoke(s, e);

			right.MouseDown += (s, e) => mouseDown.Invoke(s, e);
			right.MouseMove += (s, e) => mouseMove.Invoke(s, e);
			right.MouseUp += (s, e) => mouseUp.Invoke(s, e);
		}

		public bool IsMouseOver => left.IsMouseOver
			|| right.IsMouseOver
			|| top.IsMouseOver
			|| bot.IsMouseOver;

		public void Appear() {
			PutOnTop();
			top.Visibility = Visibility.Visible;
			bot.Visibility = Visibility.Visible;
			left.Visibility = Visibility.Visible;
			right.Visibility = Visibility.Visible;
			ClearLines();
		}

		public void ClearLines() {
			Update(Vector2.Zero, Vector2.Zero);
		}

		public void PutOnTop() {
			if(MainCanvas.Children.Contains(top)) {
				MainCanvas.Children.Remove(top);
				MainCanvas.Children.Add(top);
			}
			if(MainCanvas.Children.Contains(bot)) {
				MainCanvas.Children.Remove(bot);
				MainCanvas.Children.Add(bot);
			}
			if(MainCanvas.Children.Contains(left)) {
				MainCanvas.Children.Remove(left);
				MainCanvas.Children.Add(left);
			}
			if(MainCanvas.Children.Contains(right)) {
				MainCanvas.Children.Remove(right);
				MainCanvas.Children.Add(right);
			}
		}

		public void Update(Vector2 start, Vector2 end) {
			top.X1 = start.X;
			top.Y1 = start.Y;
			top.X2 = end.X;
			top.Y2 = start.Y;

			left.X1 = start.X;
			left.Y1 = start.Y;
			left.X2 = start.X;
			left.Y2 = end.Y;

			right.X1 = end.X;
			right.Y1 = start.Y;
			right.X2 = end.X;
			right.Y2 = end.Y;

			bot.X1 = start.X;
			bot.Y1 = end.Y;
			bot.X2 = end.X;
			bot.Y2 = end.Y;

			Start = start;
			End = end;
		}

		public void Disappear() {
			top.Visibility = Visibility.Collapsed;
			bot.Visibility = Visibility.Collapsed;
			left.Visibility = Visibility.Collapsed;
			right.Visibility = Visibility.Collapsed;
		}

		// Can only select Elements for now
		public List<Element> GetSelected(IEnumerable<Element> sources) {
			if((Start - End).Magnitude < 1) {
				return new();
			}
			CalculateBound(out Vector2 topLeft, out Vector2 botRight);
			//_parent.SetTestPoints(true, topLeft, botRight);
			List<Element> result = new();
			foreach(Element item in sources) {
				if(IsInBound(item, topLeft, botRight)) {
					result.Add(item);
				}
			}
			return result;
		}

		private bool IsInBound(Element element, Vector2 topLeft, Vector2 botRight) {
			//_parent.SetTestPoints(false, element.GetBoundPoints());
			foreach(Vector2 point in element.GetBoundPoints()) {
				if(point.X < topLeft.X || point.X > botRight.X
					|| point.Y < topLeft.Y || point.Y > botRight.Y) {
					return false;
				}
			}
			return true;
		}

		private void CalculateBound(out Vector2 topLeft, out Vector2 botRight) {
			if(End.X < Start.X && End.Y < Start.Y) {
				topLeft = End;
				botRight = Start;
			} else if(End.X < Start.X && End.Y > Start.Y) {
				topLeft = new Vector2(End.X, Start.Y);
				botRight = new Vector2(Start.X, End.Y);
			} else if(End.X > Start.X && End.Y < Start.Y) {
				topLeft = new Vector2(Start.X, End.Y);
				botRight = new Vector2(End.X, Start.Y);
			} else {
				topLeft = Start;
				botRight = End;
			}
		}
	}
}
