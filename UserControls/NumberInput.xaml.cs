using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMap.UserControls {
	public partial class NumberInput: UserControl {
		public event Action<NumberChangedEventArgs>? OnNumberChanged;
		private bool edit;

		public bool Edit {
			get => edit;
			set {
				edit = value;
				if(value) {
					Text.Visibility = Visibility.Collapsed;
					Box.Visibility = Visibility.Visible;
					Box.Text = Text.Text;
					Box.Focus();
					Box.SelectionLength = Box.Text.Length;
				} else {
					Text.Visibility = Visibility.Visible;
					Box.Visibility = Visibility.Collapsed;
					Limit();
					if(int.TryParse(Box.Text, out int box) && int.TryParse(Text.Text, out int text)) {
						OnNumberChanged?.Invoke(new NumberChangedEventArgs(text, box));
					}
					Text.Text = Box.Text;
				}
			}
		}

		public NumberInput(int initial) {
			InitializeComponent();
			Text.Text = $"{initial}";
			Box.Text = $"{initial}";
		}

		public int GetNumber() {
			if(Edit) {
				if(int.TryParse(Box.Text, out int num)) {
					return num;
				} else {
					return -1;
				}
			} else {
				if(int.TryParse(Text.Text, out int num)) {
					return num;
				} else {
					return -1;
				}
			}
		}

		private void MainGrid_MouseEnter(object sender, MouseEventArgs e) {
			Mouse.OverrideCursor = Cursors.Hand;
		}

		private void MainGrid_MouseLeave(object sender, MouseEventArgs e) {
			Mouse.OverrideCursor = null;
		}

		private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			Edit = true;
		}

		private void Box_LostFocus(object sender, RoutedEventArgs e) {
			Edit = false;
		}

		private void Box_PreviewTextInput(object sender, TextCompositionEventArgs e) {
			if(e.Text.Any(t => !char.IsDigit(t))) {
				e.Handled = true;
				return;
			}
			if(e.Text.Length > 4) {
				e.Handled = true;
				return;
			}
			//string newText = Box.Text + e.Text;
			//if(int.TryParse(newText, out int num) && (num < 0 || num > 9)) {
			//	e.Handled = true;
			//	return;
			//}
		}

		private void Box_TextChanged(object sender, TextChangedEventArgs e) {
			Limit();
		}

		private void Box_PreviewKeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Enter || e.Key == Key.Escape) {
				Edit = false;
			}
		}
		private void Limit() {
			if(int.TryParse(Box.Text, out int num)) {
				if(num < 0) {
					num = 0;
				} else if(num > 9) {
					num = 9;
				}
				Box.Text = num.ToString();
				Box.SelectionStart = Box.Text.Length;
			}
		}
	}

	public class NumberChangedEventArgs {
		public int From { get; set; }
		public int To { get; set; }

		public NumberChangedEventArgs(int from, int to) {
			From = from;
			To = to;
		}
	}
}
