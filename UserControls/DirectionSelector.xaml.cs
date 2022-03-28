using MindMap.Entities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.UserControls {
	public partial class DirectionSelector: UserControl {
		private const Direction DEFAULT_DIRECTION = Direction.Left;
		public Action<Direction, Direction>? OnValueChanged { get; set; }

		public DirectionSelector(Direction defaultDirection = DEFAULT_DIRECTION) {
			InitializeComponent();
			Select(defaultDirection, false);
		}

		private void LeftButton_Click(object sender, RoutedEventArgs e) {
			Select(Direction.Left);
		}

		private void RightButton_Click(object sender, RoutedEventArgs e) {
			Select(Direction.Right);
		}

		private void TopButton_Click(object sender, RoutedEventArgs e) {
			Select(Direction.Top);
		}

		private void BottomButton_Click(object sender, RoutedEventArgs e) {
			Select(Direction.Bottom);
		}

		public void Select(Direction direction, bool submit = true) {
			if(submit) {
				Direction current = GetCurrent();
				if(current != direction) {
					OnValueChanged?.Invoke(current, direction);
				}
			}
			LeftButton.IsChecked = Direction.Left == direction;
			RightButton.IsChecked = Direction.Right == direction;
			TopButton.IsChecked = Direction.Top == direction;
			BottomButton.IsChecked = Direction.Bottom == direction;
		}

		private Direction GetCurrent() {
			if(LeftButton.IsChecked == true) {
				return Direction.Left;
			} else if(RightButton.IsChecked == true) {
				return Direction.Right;
			} else if(TopButton.IsChecked == true) {
				return Direction.Top;
			} else if(BottomButton.IsChecked == true) {
				return Direction.Bottom;
			} else {
				return DEFAULT_DIRECTION;
			}
		}
	}
}
