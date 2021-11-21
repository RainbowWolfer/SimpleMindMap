using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public static StackPanel TextInput(string title, Action<string> OnTextChanged) {
			StackPanel panel = CreateBase(title);
			TextBox box = new();
			box.TextChanged += (s, e) => OnTextChanged?.Invoke(box.Text);
			panel.Children.Add(box);
			return panel;
		}

		//public static StackPanel DropdownMenu(string title, Action OnSelectionChanged, params string[] colors) {

		//	return new StackPanel();
		//}

		public static StackPanel SliderInput(string title, double initValue, double min, double max, Action<double> OnValueChanged, double gap = 0, int displayPresision = 2) {
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
				OnValueChanged.Invoke(slider.Value);
				ToolTipService.SetToolTip(slider, string.Format("{0:0.00}", slider.Value));
			};
			ToolTipService.SetToolTip(slider, string.Format("{0:0.00}", slider.Value));
			panel.Children.Add(slider);
			return panel;
		}

		public static StackPanel ColorInput(string title, Color initColor, Action<Color> OnColorChanged) {
			StackPanel panel = CreateBase(title);
			ColorPicker picker = new() {
				SelectedColor = initColor,
			};
			picker.SelectedColorChanged += (s, e) => {
				if(e.NewValue != null) {
					OnColorChanged.Invoke(e.NewValue.Value);
				}
			};
			panel.Children.Add(picker);
			return panel;
		}
		public static StackPanel ColorInput(string title, Brush initBrush, Action<Color> OnColorChanged) => ColorInput(title, initBrush is SolidColorBrush solid ? solid.Color : Colors.White, OnColorChanged);


		public static StackPanel FontSelector(string title, string initFont, Action<string> OnFontChanged) {
			StackPanel panel = CreateBase(title);

			return panel;
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
	}
}
