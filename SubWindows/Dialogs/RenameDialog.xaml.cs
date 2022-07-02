using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMap.SubWindows.Dialogs {
	public partial class RenameDialog: Window {
		private readonly string[] exists;
		private bool error = false;
		private readonly string originName;
		public bool Error {
			get => error;
			set {
				error = value;
				ErrorText.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				ConfirmButton.IsEnabled = !value;
			}
		}

		public RenameDialog(Window onwer, IEnumerable<string> exists, string originName) {
			InitializeComponent();
			this.Owner = onwer;
			this.exists = exists.ToArray();
			this.originName = originName;
			this.KeyDown += (s, e) => {
				if(e.Key == Key.Escape) {
					this.Close();
				}
			};

			OriginNameText.Text = originName;
			NewNameBox.Text = originName;
			NewNameBox.Focus();
			NewNameBox.SelectAll();

			ConfirmButton.IsEnabled = false;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void NewNameBox_TextChanged(object sender, TextChangedEventArgs e) {
			string text = NewNameBox.Text.Trim();
			if(string.IsNullOrWhiteSpace(text)) {
				ErrorText.Text = "Cannot be empty";
				Error = true;
			} else if(originName == text) {
				ErrorText.Text = "Cannot be the same";
				Error = true;
			} else if(exists.Contains(text)) {
				ErrorText.Text = "Already exists";
				Error = true;
			} else {
				ErrorText.Text = "";
				Error = false;
			}
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			if(Error) {
				return;
			}
			DialogResult = true;
		}

		private void BackButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
		}

		private void NewNameBox_KeyDown(object sender, KeyEventArgs e) {
			if(e.Key == Key.Enter && !Error) {
				DialogResult = true;
			}
		}

		public string GetNewName() => NewNameBox.Text.Trim();
	}
}
