using System;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.UserControls {
	public partial class DuoColumnIconsSelector: UserControl {
		public Action<bool, bool>? OnChanged;
		private bool isLeft;
		public DuoColumnIconsSelector(bool initialIsLeft) {
			InitializeComponent();
			Select(initialIsLeft);
		}

		private void LeftButton_Click(object sender, RoutedEventArgs e) {
			Select(true);
		}

		private void RightButton_Click(object sender, RoutedEventArgs e) {
			Select(false);
		}

		public void Select(bool isLeft) {
			OnChanged?.Invoke(this.isLeft, isLeft);
			this.isLeft = isLeft;
			LeftButton.IsChecked = isLeft;
			RightButton.IsChecked = !isLeft;
		}
	}
}
