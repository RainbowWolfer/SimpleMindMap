using MindMap.Entities;
using MindMap.Entities.Locals;
using MindMap.Pages;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MindMap.SubWindows {
	public partial class SettingsWindow: Window {
		public bool Intializing { get; private set; } = true;
		private readonly MindMapPage? mindMapPage;
		private bool hasBackgroundStyleChanged = false;
		private bool hasCoordinatonRuleStyleChanged = false;
		public SettingsWindow(Window onwer, MindMapPage? mindMapPage = null) {
			InitializeComponent();
			this.Owner = onwer;
			this.mindMapPage = mindMapPage;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			Initialize();
		}

		private void Initialize() {
			Intializing = true;
			if(AppSettings.Current == null) {
				ErrorPanel.Visibility = Visibility.Visible;
				SettingsGrid.Visibility = Visibility.Collapsed;
				Intializing = false;
				return;
			}
			DefaultElementHeightText.Text = $"{Math.Round(AppSettings.Current.ElementDefaultHeight, 0)}";
			DefaultElementHeightSlider.Value = AppSettings.Current.ElementDefaultHeight;
			DefaultPlacePositionViewCenteredToggle.IsChecked = AppSettings.Current.EnableElementDefaultPositionCentered;
			DefaultPlacePositionCustomToggle.IsChecked = !AppSettings.Current.EnableElementDefaultPositionCentered;
			DefaultPlacePositionXBox.IsEnabled = !AppSettings.Current.EnableElementDefaultPositionCentered;
			DefaultPlacePositionYBox.IsEnabled = !AppSettings.Current.EnableElementDefaultPositionCentered;
			DefaultPlacePositionXBox.Text = $"{AppSettings.Current.ElementDefaultPosition.X}";
			DefaultPlacePositionYBox.Text = $"{AppSettings.Current.ElementDefaultPosition.Y}";
			DefaultElementText.Text = AppSettings.Current.ElementDefaultText;
			switch(AppSettings.Current.BackgroundStyle) {
				case BackgroundStyle.Dot:
					BackgroundStyleDotToggle.IsChecked = true;
					BackgroundStyleHeartToggle.IsChecked = false;
					BackgroundStyleRectToggle.IsChecked = false;
					break;
				case BackgroundStyle.Rect:
					BackgroundStyleDotToggle.IsChecked = false;
					BackgroundStyleRectToggle.IsChecked = true;
					BackgroundStyleHeartToggle.IsChecked = false;
					break;
				case BackgroundStyle.Heart:
					BackgroundStyleDotToggle.IsChecked = false;
					BackgroundStyleRectToggle.IsChecked = false;
					BackgroundStyleHeartToggle.IsChecked = true;
					break;
				default:
					throw new Exception($"Style ({AppSettings.Current.BackgroundStyle}) Not Found");
			}
			BackgroundShapSizeText.Text = $"{Math.Round(AppSettings.Current.BackgroundShapeSize, 0)}";
			BackgroundShapSizeSlider.Value = AppSettings.Current.BackgroundShapeSize;
			BackgroundShapGapText.Text = $"{Math.Round(AppSettings.Current.BackgroundShapeGap, 0)}";
			BackgroundShapGapSlider.Value = AppSettings.Current.BackgroundShapeGap;

			RulesEnableToggle.IsChecked = AppSettings.Current.EnableCanvasRule;
			RulesDisableToggle.IsChecked = !AppSettings.Current.EnableCanvasRule;
			CanvasRuleGapSlider.IsEnabled = AppSettings.Current.EnableCanvasRule;

			CanvasRuleGapText.Text = $"{Math.Round(AppSettings.Current.CanvasRuleGap, 0)}";
			CanvasRuleGapSlider.Value = AppSettings.Current.CanvasRuleGap;

			Intializing = false;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.Key == Key.Escape) {
				this.Close();
			}
		}

		protected override async void OnClosed(EventArgs e) {
			base.OnClosed(e);
			await Local.SaveAppSettings();
			if(hasBackgroundStyleChanged) {
				mindMapPage?.NotifyBackgroundStyleChanged();
			}
			if(hasCoordinatonRuleStyleChanged) {
				mindMapPage?.NotifyCoordinationRuleStyleChanged();
			}
		}

		private void DefaultPlacePositionViewCenteredToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			DefaultPlacePositionViewCenteredToggle.IsChecked = true;
			DefaultPlacePositionCustomToggle.IsChecked = false;
			DefaultPlacePositionXBox.IsEnabled = false;
			DefaultPlacePositionYBox.IsEnabled = false;
			AppSettings.Current.EnableElementDefaultPositionCentered = true;
		}

		private void DefaultPlacePositionCustomToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			DefaultPlacePositionViewCenteredToggle.IsChecked = false;
			DefaultPlacePositionCustomToggle.IsChecked = true;
			DefaultPlacePositionXBox.IsEnabled = true;
			DefaultPlacePositionYBox.IsEnabled = true;
			AppSettings.Current.EnableElementDefaultPositionCentered = false;
		}

		private void DefaultElementHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.ElementDefaultHeight = e.NewValue;
			DefaultElementHeightText.Text = $"{Math.Round(e.NewValue, 0)}";
		}

		private void RulesEnableToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.EnableCanvasRule = true;
			RulesEnableToggle.IsChecked = true;
			RulesDisableToggle.IsChecked = false;
			CanvasRuleGapSlider.IsEnabled = true;
			hasCoordinatonRuleStyleChanged = true;
		}

		private void RulesDisableToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.EnableCanvasRule = false;
			RulesEnableToggle.IsChecked = false;
			RulesDisableToggle.IsChecked = true;
			CanvasRuleGapSlider.IsEnabled = false;
			hasCoordinatonRuleStyleChanged = true;
		}

		private void CanvasRuleGapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.CanvasRuleGap = e.NewValue;
			CanvasRuleGapText.Text = $"{Math.Round(e.NewValue, 0)}";
			hasCoordinatonRuleStyleChanged = true;
		}

		private void BackgroundShapGapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.BackgroundShapeGap = e.NewValue;
			BackgroundShapGapText.Text = $"{Math.Round(e.NewValue, 0)}";
			hasBackgroundStyleChanged = true;
		}

		private void BackgroundShapSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.BackgroundShapeSize = e.NewValue;
			BackgroundShapSizeText.Text = $"{Math.Round(e.NewValue, 0)}";
			hasBackgroundStyleChanged = true;
		}

		private void BackgroundStyleHeartToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.BackgroundStyle = BackgroundStyle.Heart;
			BackgroundStyleDotToggle.IsChecked = false;
			BackgroundStyleRectToggle.IsChecked = false;
			BackgroundStyleHeartToggle.IsChecked = true;
			hasBackgroundStyleChanged = true;
		}

		private void BackgroundStyleRectToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.BackgroundStyle = BackgroundStyle.Rect;
			BackgroundStyleDotToggle.IsChecked = false;
			BackgroundStyleRectToggle.IsChecked = true;
			BackgroundStyleHeartToggle.IsChecked = false;
			hasBackgroundStyleChanged = true;
		}

		private void BackgroundStyleDotToggle_Click(object sender, RoutedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.BackgroundStyle = BackgroundStyle.Dot;
			BackgroundStyleDotToggle.IsChecked = true;
			BackgroundStyleRectToggle.IsChecked = false;
			BackgroundStyleHeartToggle.IsChecked = false;
			hasBackgroundStyleChanged = true;
		}

		private void DefaultElementText_TextChanged(object sender, TextChangedEventArgs e) {
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.ElementDefaultText = DefaultElementText.Text;
		}

		private void DefaultPlacePositionXBox_TextChanged(object sender, TextChangedEventArgs e) {
			int num = Limit((TextBox)sender);
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.ElementDefaultPosition = new Vector2(num, AppSettings.Current.ElementDefaultPosition.Y);
		}

		private void DefaultPlacePositionXBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
			if(e.Text.Any(t => !char.IsDigit(t))) {
				e.Handled = true;
				return;
			}
		}

		private void DefaultPlacePositionYBox_TextChanged(object sender, TextChangedEventArgs e) {
			int num = Limit((TextBox)sender);
			if(AppSettings.Current == null || Intializing) {
				return;
			}
			AppSettings.Current.ElementDefaultPosition = new Vector2(AppSettings.Current.ElementDefaultPosition.X, num);
		}

		private void DefaultPlacePositionYBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
			if(e.Text.Any(t => !char.IsDigit(t))) {
				e.Handled = true;
				return;
			}
		}

		private int Limit(TextBox box) {
			if(int.TryParse(box.Text, out int num)) {
				if(num < 0) {
					num = 0;
				} else if(num > 999) {
					num = 999;
				}
				box.Text = num.ToString();
				box.SelectionStart = box.Text.Length;
			}
			return num;
		}

	}
}
