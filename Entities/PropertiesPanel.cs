using MindMap.Pages;
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

		public static StackPanel SectionTitle(string title) {
			StackPanel panel = new();
			panel.Children.Add(new Separator());
			panel.Children.Add(new TextBlock() {
				Text = title,
				FontSize = 14,
				HorizontalAlignment = HorizontalAlignment.Center,
			});
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

		public static StackPanel ColorInput(string title, Color initColor, Action<Color> OnColorChanged, Action<Color>? AfterValueSubumit = null) {
			StackPanel panel = CreateBase(title);
			ColorPicker picker = new() {
				SelectedColor = initColor,
			};
			Color valueBefore = (Color)picker.SelectedColor;
			bool changing = false;
			picker.SelectedColorChanged += (s, e) => {
				if(e.NewValue != null) {
					changing = true;
					OnColorChanged.Invoke(e.NewValue.Value);
				}
			};
			picker.PreviewMouseDown += (s, e) => {
				valueBefore = (Color)picker.SelectedColor;
			};
			picker.PreviewMouseUp += (s, e) => {
				if(changing) {
					Debug.WriteLine(picker.SelectedColor);
					AfterValueSubumit?.Invoke(valueBefore);
					changing = false;
				}
			};
			picker.LostFocus += (s, e) => {
				if(changing) {
					Debug.WriteLine(picker.SelectedColor);
					AfterValueSubumit?.Invoke(valueBefore);
					changing = false;
				}
			};
			panel.Children.Add(picker);
			return panel;
		}
		public static StackPanel ColorInput(string title, Brush initBrush, Action<Color> OnColorChanged, Action<Color>? AfterValueSubumit = null) {
			return ColorInput(title, initBrush is SolidColorBrush solid ? solid.Color : Colors.White, OnColorChanged, AfterValueSubumit);
		}

		public static StackPanel FontSelector(string title, FontFamily initFont, Action<FontFamily> OnFontChanged, params string[] availableFonts) {
			StackPanel panel = CreateBase(title);
			ComboBox comboBox = new() {
				SelectedIndex = availableFonts.ToList().IndexOf(initFont.Source),
			};

			foreach(string item in availableFonts) {
				comboBox.Items.Add(new ComboBoxItem() { Content = item });
			}
			comboBox.SelectionChanged += (s, e) => {
				if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item) {
					OnFontChanged.Invoke(new FontFamily(item.Content as string));
				}
			};
			panel.Children.Add(comboBox);
			return panel;
		}

		public static StackPanel ComboSelector<T>(string title, T initData, Action<T> OnValueChanged, params T[] selections) {
			StackPanel panel = CreateBase(title);
			ComboBox comboBox = new() {
				SelectedIndex = selections.ToList().IndexOf(initData),
			};
			foreach(T item in selections) {
				comboBox.Items.Add(new ComboBoxItem() { Content = item });
			}
			comboBox.SelectionChanged += (s, e) => {
				if(e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item) {
					OnValueChanged.Invoke((T)item.Content);
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
