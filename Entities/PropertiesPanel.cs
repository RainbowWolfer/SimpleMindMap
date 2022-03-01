using MindMap.Pages;
using MindMap.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			//var textblock = new TextBlock() {
			//	Text = title,
			//	FontSize = 14,
			//	HorizontalAlignment = HorizontalAlignment.Center,
			//};
			//var textbox = new TextBox() {
			//	Text = title,
			//	FontSize = 14,
			//	HorizontalAlignment = HorizontalAlignment.Center,
			//	//Visibility = Visibility.Collapsed,
			//};
			//textblock.
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
		public static StackPanel ColorInput(string title, Brush initBrush, Action<ValueChangedArgs<Color>> OnColorChanged) {
			return ColorInput(title, initBrush is SolidColorBrush solid ? solid.Color : Colors.White, OnColorChanged);
		}

		public static StackPanel FontSelector(string title, FontFamily initFont, Action<ValueChangedArgs<FontFamily>> OnFontChanged, params string[] availableFonts) {
			StackPanel panel = CreateBase(title);
			ComboBox comboBox = new() {
				SelectedIndex = availableFonts.ToList().IndexOf(initFont.Source),
			};

			foreach(string item in availableFonts) {
				comboBox.Items.Add(new ComboBoxItem() { Content = item });
			}
			comboBox.SelectionChanged += (s, e) => {
				if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item) {
					FontFamily? old = null;
					if(e.RemovedItems.Count > 0 && e.RemovedItems[0] is ComboBoxItem remove) {
						old = new FontFamily(remove.Content as string);
					}
					OnFontChanged.Invoke(new ValueChangedArgs<FontFamily>(old, new FontFamily(item.Content as string)));
				}
			};
			panel.Children.Add(comboBox);
			return panel;
		}

		public static StackPanel ComboSelector<T>(string title, T initData, Action<ValueChangedArgs<T>> OnValueChanged, params T[] selections) {
			StackPanel panel = CreateBase(title);
			ComboBox comboBox = new() {
				SelectedIndex = selections.ToList().IndexOf(initData),
			};
			foreach(T item in selections) {
				comboBox.Items.Add(new ComboBoxItem() { Content = item });
			}
			comboBox.SelectionChanged += (s, e) => {
				if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item) {
					T? old = default;
					if(e.RemovedItems != null && e.RemovedItems.Count > 0 && e.RemovedItems[0] is ComboBoxItem remove) {
						old = (T?)remove.Content;
					}
					OnValueChanged.Invoke(new ValueChangedArgs<T>(old, (T?)item.Content));
				}
			};
			panel.Children.Add(comboBox);
			return panel;
		}

		public static StackPanel DuoNumberInput(string title) {
			StackPanel panel = CreateBase(title);
			return panel;
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
}
