using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MindMap.Entities.Elements {
	public class TextGrid: Element {
		private Grid? _root;
		private TextBlock? _textBlock;
		private TextBox? _textBox;

		private Style TextBoxStyle {
			get {
				Style style = new(typeof(TextBox));
				style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
				style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch));
				style.Setters.Add(new Setter(Control.FontSizeProperty, (double)15));
				style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10)));
				style.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
				style.Setters.Add(new Setter(TextBox.TextAlignmentProperty, TextAlignment.Center));
				return style;
			}
		}

		private Style TextBlockStyle {
			get {
				Style style = new(typeof(TextBlock));
				style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center));
				style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
				style.Setters.Add(new Setter(TextBlock.FontSizeProperty, (double)15));
				style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(10)));
				style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
				style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
				return style;
			}
		}

		public TextGrid(MindMapPage parent) : base(parent) {

		}

		public override FrameworkElement? Target => _root;
		public override FrameworkElement CreateFramework(Canvas mainCanvas) {
			_root = new Grid() {
				Width = 200,
				Height = 200,
				Background = Brushes.Gray,
				RenderTransform = new TranslateTransform(0, 0),
			};
			_root.SetValue(Canvas.TopProperty, 0.0);
			_root.SetValue(Canvas.LeftProperty, 0.0);

			_textBlock = new TextBlock() {
				Text = "Input Your Text",
				Style = TextBlockStyle,
			};
			_root.Children.Add(_textBlock);

			_root.ContextMenu = new ContextMenu();
			MenuItem item = new() {
				Header = "Delete",
				Icon = Icons.CreateIcon("\uE74D"),
			};
			item.Click += (sender, args) => Delete();
			_root.ContextMenu.Items.Add(item);
			_root.ContextMenu.PlacementTarget = _root;

			mainCanvas.Children.Add(_root);
			return _root;
		}

		public override void Deselect() {
			if(_root == null) {
				return;
			}
			if(_textBox != null) {
				_textBlock = new TextBlock() {
					Text = string.IsNullOrEmpty(_textBox.Text) ? "(No Text)" : _textBox.Text,
					Style = TextBlockStyle,
				};
				_root.Children.Add(_textBlock);
				_root.Children.Remove(_textBox);
			}

		}

		public override void DoubleClick() {
			if(_root == null) {
				return;
			}
			if(_textBlock != null) {
				_textBox = new TextBox() {
					Text = _textBlock.Text,
					Style = TextBoxStyle,
					AcceptsReturn = true,
					AcceptsTab = true,
				};
				_textBox.KeyDown += (s, e) => {
					if(e.Key == Key.Escape) {
						parent.ClearResizePanel();
						parent.Deselect();
					}
				};
				_root.Children.Add(_textBox);
				_root.Children.Remove(_textBlock);
				_textBox.Focus();
			}
		}

		public override void LeftClick() {
			Debug.WriteLine($"TextGrid Left");
		}

		public override void MiddleClick() {
			Debug.WriteLine($"TextGrid Middle");
		}

		public override void RightClick() {
			Debug.WriteLine($"TextGrid Right");
			if(_root == null || _root.ContextMenu == null) {
				return;
			}
			_root.ContextMenu.IsOpen = true;
		}
	}
}
