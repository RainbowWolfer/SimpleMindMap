using MindMap.Entities.Elements;
using MindMap.Entities.Icons;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MindMap.UserControls {
	public partial class StackIconsSelector: UserControl {
		public Action<Pair<IconElement, string>?, Pair<IconElement, string>>? OnItemClick;
		private readonly int initialIndex;
		private readonly List<LockableToggleButton> lockableToggles = new();
		private Pair<IconElement, string>? current;
		public StackIconsSelector(List<Pair<IconElement, string>> pairs, int initialIndex, int fontSize = 14) {
			InitializeComponent();
			this.initialIndex = initialIndex;
			for(int i = 0; i < pairs.Count; i++) {
				Pair<IconElement, string> item = pairs[i];
				Grid grid = new();
				grid.ColumnDefinitions.Add(new ColumnDefinition() {
					Width = new GridLength(0, GridUnitType.Auto),
				});
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				FrameworkElement icon = item.Key.Generate();
				icon.Margin = new Thickness(2);
				TextBlock textBlock = new() {
					Text = item.Value,
					FontSize = fontSize,
					Margin = new Thickness(10, 0, 0, 0),
					VerticalAlignment = VerticalAlignment.Center,
				};
				Grid.SetColumn(textBlock, 1);
				grid.Children.Add(icon);
				grid.Children.Add(textBlock);
				LockableToggleButton button = new() {
					Content = grid,
					LockToggle = true,
					IsChecked = initialIndex == i,
					HorizontalContentAlignment = HorizontalAlignment.Stretch,
					Padding = new Thickness(2),
				};
				lockableToggles.Add(button);
				MyStack.Children.Add(button);
				button.Click += (s, e) => {
					OnItemClick?.Invoke(current, item);
					foreach(LockableToggleButton b in lockableToggles) {
						b.IsChecked = b == button;
					}
					current = item;
				};
				if(initialIndex == i) {
					current = item;
				}
			}
		}
	}
}
