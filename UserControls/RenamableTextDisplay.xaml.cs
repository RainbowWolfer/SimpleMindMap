using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MindMap.UserControls {
	public partial class RenamableTextDisplay: UserControl {
		public event Action<string>? TextChanged;

		private bool isInputing = false;
		private string text = "Default";

		public bool IsInputing {
			get => isInputing;
			set {
				isInputing = EnableInput && value;
				Block.Visibility = !isInputing ? Visibility.Visible : Visibility.Collapsed;
				Box.Visibility = isInputing ? Visibility.Visible : Visibility.Collapsed;
				if(isInputing) {
					Box.Focus();
					Box.SelectionLength = Block.Text.Length;
				} else {
					string newInput = Box.Text.Trim();
					if(string.IsNullOrWhiteSpace(newInput)) {
						return;
					}
					Block.Text = newInput;
					TextChanged?.Invoke(newInput);
				}
			}
		}

		public string Text {
			get => text;
			set {
				text = value;
				Box.Text = value;
				Block.Text = value;
			}
		}

		public bool EnableInput { get; set; }

		public RenamableTextDisplay() {
			InitializeComponent();
		}

		private void Box_LostFocus(object sender, RoutedEventArgs e) {
			IsInputing = false;
		}

		private void Block_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			IsInputing = true;
		}

		private void Box_PreviewKeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Enter) {
				IsInputing = false;
			}
		}
	}
}
