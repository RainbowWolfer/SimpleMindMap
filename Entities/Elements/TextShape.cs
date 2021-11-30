﻿using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public abstract class TextShape: Element, ITextGrid, IBorderBasedStyle {
		public TextBox MyTextBox { get; set; } = new();
		public TextBlock MyTextBlock { get; set; } = new();

		public string Text {
			get => BaseProperties.text;
			set {
				if(BaseProperties.text != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.text = value;
				UpdateStyle();
			}
		}
		public FontFamily FontFamily {
			get => BaseProperties.fontFamily;
			set {
				if(BaseProperties.fontFamily != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.fontFamily = value;
				UpdateStyle();
			}
		}
		public FontWeight FontWeight {
			get => BaseProperties.fontWeight;
			set {
				if(BaseProperties.fontWeight != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.fontWeight = value;
				UpdateStyle();
			}
		}
		public double FontSize {
			get => BaseProperties.fontSize;
			set {
				if(BaseProperties.fontSize != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.fontSize = value;
				UpdateStyle();
			}
		}
		public Color FontColor {
			get => BaseProperties.fontColor;
			set {
				if(BaseProperties.fontColor != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.fontColor = value;
				UpdateStyle();
			}
		}
		public Brush Background {
			get => BaseProperties.background;
			set {
				if(BaseProperties.background != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.background = value;
				UpdateStyle();
			}
		}
		public Brush BorderColor {
			get => BaseProperties.borderColor;
			set {
				if(BaseProperties.borderColor != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.borderColor = value;
				UpdateStyle();
			}
		}
		public Thickness BorderThickness {
			get => BaseProperties.borderThickness;
			set {
				if(BaseProperties.borderThickness != value) {
					SubmitPropertyChangedEditHistory(BaseProperties);
				}
				BaseProperties.borderThickness = value;
				UpdateStyle();
			}
		}

		protected readonly Grid root = new();
		public override FrameworkElement Target => root;

		protected abstract class BaseProperty: IProperty {
			public string text = "(Hello World)";
			public FontFamily fontFamily = new("Microsoft YaHei UI");
			public FontWeight fontWeight = FontWeights.Normal;
			public double fontSize = 14;
			public Color fontColor = Colors.Black;
			public Brush background = Brushes.Gray;
			public Brush borderColor = Brushes.Aquamarine;
			public Thickness borderThickness = new(2);
			public abstract IProperty Translate(string json);
			public abstract object Clone();
		}

		protected abstract BaseProperty BaseProperties { get; }
		public override IProperty Properties => BaseProperties;

		public TextShape(MindMapPage parent) : base(parent) {
			MyTextBox.KeyDown += (s, e) => {
				if(e.Key == Key.Escape) {
					Deselect();
				}
			};
		}

		public abstract override Panel CreateElementProperties();

		public override void Deselect() {
			ShowTextBlock();
			SubmitTextChange();
		}

		public override void DoubleClick() {
			ShowTextBox();
		}

		public override void LeftClick() {

		}

		public override void MiddleClick() {

		}

		public override void RightClick() {
			root.ContextMenu.IsOpen = true;
		}

		public override void SetFramework() {
			root.Children.Clear();
			if(!MainCanvas.Children.Contains(root)) {
				MainCanvas.Children.Add(root);
			}
			root.Children.Add(MyTextBlock);
			root.Children.Add(MyTextBox);
			ShowTextBlock();
			UpdateStyle();
		}

		public void SubmitTextChange() {
			Text = MyTextBox.Text;
			MyTextBlock.Text = Text;
		}

		public void ShowTextBlock() {
			MyTextBlock.Visibility = Visibility.Visible;
			MyTextBox.Visibility = Visibility.Collapsed;
		}

		public void ShowTextBox() {
			MyTextBlock.Visibility = Visibility.Collapsed;
			MyTextBox.Visibility = Visibility.Visible;
		}

		protected override void UpdateStyle() {
			MyTextBlock.Text = Text;
			MyTextBlock.Foreground = new SolidColorBrush(FontColor);
			MyTextBlock.FontFamily = FontFamily;
			MyTextBlock.FontWeight = FontWeight;
			MyTextBlock.FontSize = FontSize;
			MyTextBlock.Padding = new Thickness(10);
			MyTextBlock.TextWrapping = TextWrapping.Wrap;
			MyTextBlock.TextAlignment = TextAlignment.Center;
			MyTextBlock.VerticalAlignment = VerticalAlignment.Center;
			MyTextBlock.HorizontalAlignment = HorizontalAlignment.Center;

			MyTextBox.Text = Text;
			MyTextBox.Foreground = new SolidColorBrush(FontColor);
			MyTextBox.FontFamily = FontFamily;
			MyTextBox.FontWeight = FontWeight;
			MyTextBox.FontSize = FontSize;
			MyTextBox.Padding = new Thickness(10);
			MyTextBox.TextWrapping = TextWrapping.Wrap;
			MyTextBox.TextAlignment = TextAlignment.Center;
			MyTextBox.VerticalAlignment = VerticalAlignment.Stretch;
			MyTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
			MyTextBox.AcceptsReturn = true;
			MyTextBox.AcceptsTab = true;
		}
	}
}