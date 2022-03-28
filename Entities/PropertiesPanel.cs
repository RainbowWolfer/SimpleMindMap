using MindMap.Entities.Elements;
using MindMap.Entities.Icons;
using MindMap.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace MindMap.Entities {
	public static class PropertiesPanel {
		private static StackPanel CreateBase(string title) {
			StackPanel panel = new();
			panel.Children.Add(new TextBlock() {
				Text = $"{title}:",
			});
			return panel;
		}

		public static StackPanel SectionTitle(string title, Action<string>? onNameChanged = null) {
			StackPanel panel = new();
			panel.Children.Add(new Separator());
			var nameText = new RenamableTextDisplay() {
				Text = title,
				EnableInput = onNameChanged != null,
			};
			nameText.TextChanged += onNameChanged;
			panel.Children.Add(nameText);
			return panel;
		}

		//public static StackPanel TextInput(string title, Action<string> OnTextChanged) {
		//	StackPanel panel = CreateBase(title);
		//	TextBox box = new();
		//	box.TextChanged += (s, e) => {
		//		OnTextChanged?.Invoke(box.Text);
		//	};
		//	panel.Children.Add(box);
		//	return panel;
		//}

		//public static StackPanel DropdownMenu(string title, Action OnSelectionChanged, params string[] colors) {

		//	return new StackPanel();
		//}

		public static StackPanel SliderInput(string title, double initValue, double min, double max, Action<ValueChangedArgs<double>> OnValueChanged, double gap = 0, int displayPresision = 2) {
			StackPanel panel = CreateBase(title);
			Slider slider = new() {
				Minimum = min,
				Maximum = max,
				Value = initValue,
				AutoToolTipPlacement = AutoToolTipPlacement.BottomRight,
				AutoToolTipPrecision = displayPresision,
				TickFrequency = gap,
				IsSnapToTickEnabled = gap != 0,
			};
			CreateContextMenu(slider, () => {
				slider.Value = initValue;
			});
			slider.ValueChanged += (s, e) => {
				OnValueChanged.Invoke(new ValueChangedArgs<double>(e.OldValue, e.NewValue));
				ToolTipService.SetToolTip(slider, $"{slider.Value:0.00}");
			};
			ToolTipService.SetToolTip(slider, $"{slider.Value:0.00}");
			panel.Children.Add(slider);
			return panel;
		}

		public static StackPanel ColorInput(string title, Color initColor, Action<ValueChangedArgs<Color>> onColorChanged, Action? onLostFocus = null) {
			StackPanel panel = CreateBase(title);
			ColorPicker picker = new() {
				SelectedColor = initColor,
			};
			CreateContextMenu(picker, () => {
				picker.SelectedColor = initColor;
			});
			bool changed = false;
			picker.SelectedColorChanged += (s, e) => {
				Color oldColor = default;
				if(e.OldValue != null) {
					oldColor = e.OldValue.Value;
				}
				Color newColor = default;
				if(e.NewValue != null) {
					newColor = e.NewValue.Value;
				}
				changed = true;
				onColorChanged.Invoke(new ValueChangedArgs<Color>(oldColor, newColor));
			};
			//it is meant to instant seal delayed change after lost focus
			picker.LostFocus += (s, e) => {
				if(changed) {
					onLostFocus?.Invoke();//seems not working
				}
			};
			panel.Children.Add(picker);
			return panel;
		}
		public static StackPanel ColorInput(string title, Brush initBrush, Action<ValueChangedArgs<Color>> onColorChanged, Action? onLostFocus = null) {
			return ColorInput(title, initBrush is SolidColorBrush solid ? solid.Color : Colors.White, onColorChanged, onLostFocus);
		}

		public static StackPanel FontSelector(string title, FontFamily initFont, Action<ValueChangedArgs<FontFamily>> onFontChanged, params string[] availableFonts) {
			StackPanel panel = CreateBase(title);
			ComboBox comboBox = new() {
				SelectedIndex = availableFonts.ToList().IndexOf(initFont.Source),
			};
			CreateContextMenu(comboBox, () => {
				comboBox.SelectedIndex = availableFonts.ToList().IndexOf(initFont.Source);
			});
			foreach(string item in availableFonts) {
				comboBox.Items.Add(new ComboBoxItem() { Content = item });
			}
			comboBox.SelectionChanged += (s, e) => {
				if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item) {
					FontFamily? old = null;
					if(e.RemovedItems.Count > 0 && e.RemovedItems[0] is ComboBoxItem remove) {
						old = new FontFamily(remove.Content as string);
					}
					onFontChanged.Invoke(new ValueChangedArgs<FontFamily>(old, new FontFamily(item.Content as string)));
				}
			};
			panel.Children.Add(comboBox);
			return panel;
		}

		public static StackPanel ComboSelector<T>(string title, T initData, Action<ValueChangedArgs<T>> onValueChanged, params T[] selections) {
			StackPanel panel = CreateBase(title);
			ComboBox comboBox = new() {
				SelectedIndex = selections.ToList().IndexOf(initData),
			};
			foreach(T item in selections) {
				comboBox.Items.Add(new ComboBoxItem() { Content = item });
			}
			CreateContextMenu(comboBox, () => {
				comboBox.SelectedIndex = selections.ToList().IndexOf(initData);
			});
			comboBox.SelectionChanged += (s, e) => {
				if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item) {
					T? old = default;
					if(e.RemovedItems != null && e.RemovedItems.Count > 0 && e.RemovedItems[0] is ComboBoxItem remove) {
						old = (T?)remove.Content;
					}
					onValueChanged.Invoke(new ValueChangedArgs<T>(old, (T?)item.Content));
				}
			};
			panel.Children.Add(comboBox);
			return panel;
		}

		public static StackPanel DuoNumberInputs(string title, DuoNumber initial, Action<ValueChangedArgs<DuoNumber>> onValueChanged) {
			StackPanel panel = CreateBase(title);
			Grid grid = new();
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			NumberInput input_left = new(initial.Left);
			NumberInput input_right = new(initial.Right);
			input_left.OnNumberChanged += args => {
				DuoNumber oldV = new(args.From, input_right.GetNumber());
				DuoNumber newV = new(args.To, input_right.GetNumber());
				if(oldV == newV) {
					return;
				}
				onValueChanged.Invoke(new ValueChangedArgs<DuoNumber>(oldV, newV));
			};
			input_right.OnNumberChanged += args => {
				DuoNumber oldV = new(input_left.GetNumber(), args.From);
				DuoNumber newV = new(input_left.GetNumber(), args.To);
				if(oldV == newV) {
					return;
				}
				onValueChanged.Invoke(new ValueChangedArgs<DuoNumber>(oldV, newV));
			};
			Grid.SetColumn(input_right, 1);
			grid.Children.Add(input_left);
			grid.Children.Add(input_right);
			panel.Children.Add(grid);
			return panel;
		}

		public static StackPanel DirectionSelector(string title, Direction initialData, Action<ValueChangedArgs<Direction>> onValueChanged, DirectionArgs allowedDirections) {
			StackPanel panel = CreateBase(title);
			var selector = new DirectionSelector(initialData) {
				OnValueChanged = (oldV, newV) => {
					onValueChanged.Invoke(new ValueChangedArgs<Direction>(oldV, newV));
				}
			};
			CreateContextMenu(selector, () => {
				selector.Select(initialData);
			});
			panel.Children.Add(selector);
			return panel;
		}

		public static StackPanel StackIconsSelector(string title, List<Pair<IconElement, string>> data, int initialIndex, Action<ValueChangedArgs<Pair<IconElement, string>>> onValueChanged) {
			StackPanel panel = CreateBase(title);
			var selector = new StackIconsSelector(data, initialIndex) {
				OnItemClick = (oldV, newV) => {
					onValueChanged.Invoke(new ValueChangedArgs<Pair<IconElement, string>>(oldV, newV));
				},
			};
			CreateContextMenu(selector, () => {
				//selector.Select(initialData);
			});
			panel.Children.Add(selector);
			return panel;
		}

		public static StackPanel DuoColumnIconsSelector(string title, bool initialIsLeft, Action<ValueChangedArgs<bool>> onValueChanged) {
			StackPanel panel = CreateBase(title);
			var selector = new DuoColumnIconsSelector(initialIsLeft) {
				OnChanged = (from, to) => {
					onValueChanged.Invoke(new ValueChangedArgs<bool>(from, to));
				},
			};
			panel.Children.Add(selector);
			return panel;
		}

		public static StackPanel ActionButton(bool showTitle, string title, Action onClickAction, out Button button, string? icon = null, int fontSize = 15, bool leftToRight = true) {
			StackPanel panel = showTitle ? CreateBase(title) : new();
			button = new Button();
			if(string.IsNullOrWhiteSpace(icon)) {
				button.Content = title;
				button.FontSize = fontSize;
			} else {
				StackPanel content = new() {
					HorizontalAlignment = HorizontalAlignment.Center,
					Orientation = Orientation.Horizontal,
				};
				FrameworkElement iconElement = new FontIcon(icon, (int)(fontSize * 1.2)).Generate();
				iconElement.VerticalAlignment = VerticalAlignment.Center;
				TextBlock text = new() {
					Text = title,
					FontSize = fontSize,
					VerticalAlignment = VerticalAlignment.Center,
				};
				if(leftToRight) {
					content.Children.Add(iconElement);
					content.Children.Add(text);
					text.Margin = new Thickness(7, 0, 0, 0);
				} else {
					content.Children.Add(text);
					content.Children.Add(iconElement);
					iconElement.Margin = new Thickness(0, 0, 7, 0);
				}

				button.Content = content;
			}
			button.Click += (s, e) => {
				onClickAction.Invoke();
			};
			panel.Children.Add(button);
			return panel;
		}

		public static StackPanel CheckBox(string title) {
			StackPanel panel = new StackPanel();
			CheckBox checkBox = new CheckBox() {
				Content = title,
			};
			panel.Children.Add(checkBox);
			return panel;
		}

		private static void CreateContextMenu(FrameworkElement target, Action onReset) {
			ContextMenu menu = new();
			MenuItem item_reset = new() {
				Header = "Reset To Default",
				Icon = new FontIcon("\uE72C").Generate(),
			};
			item_reset.Click += (s, e) => onReset.Invoke();
			menu.Items.Add(item_reset);
			target.ContextMenu = menu;
		}
	}

	public class ValueChangedArgs<T> {
		public T? OldValue { get; set; }
		public T? NewValue { get; set; }
		public ValueChangedArgs(T? oldValue, T? newValue) {
			OldValue = oldValue;
			NewValue = newValue;
		}
		public override string ToString() {
			return $"({typeof(T)}) {OldValue} -> {NewValue}";
		}
	}

	public struct DuoNumber {
		public int Left { get; set; } = 1;
		public int Right { get; set; } = 1;
		public DuoNumber(int left, int right) {
			Left = left;
			Right = right;
		}

		public double[] ToArray() {
			return new double[] { Left, Right };
		}

		public override string ToString() {
			return $"Duo ({Left}|{Right})";
		}

		public override bool Equals([NotNullWhen(true)] object? obj) {
			return obj is DuoNumber duo && duo.Left == Left && duo.Right == Right;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static bool operator ==(DuoNumber a, DuoNumber b) {
			return a.Equals(b);
		}

		public static bool operator !=(DuoNumber a, DuoNumber b) {
			return !a.Equals(b);
		}
	}
}
