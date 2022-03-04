using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Entities.Tags;
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

namespace MindMap.Entities.Elements.TextShapes {
	public abstract class TextShape: TextRelated, ITextGrid, IBorderBasedStyle {
		public abstract FrameworkElement Shape { get; }
		public TextBox MyTextBox { get; set; } = new();
		public TextBlock MyTextBlock { get; set; } = new();

		public Brush Background {
			get => BaseProperties.background;
			set {
				BaseProperties.background = value;
				UpdateStyle();
			}
		}
		public Brush BorderColor {
			get => BaseProperties.borderColor;
			set {
				BaseProperties.borderColor = value;
				UpdateStyle();
			}
		}
		public Thickness BorderThickness {
			get => BaseProperties.borderThickness;
			set {
				BaseProperties.borderThickness = value;
				UpdateStyle();
			}
		}

		protected readonly Grid root = new();
		public override FrameworkElement Target => root;

		protected abstract class TextShapeProperty: TextRelatedProperty {
			public Brush background = Brushes.Gray;
			public Brush borderColor = Brushes.Aquamarine;
			public Thickness borderThickness = new(2);
		}

		protected abstract TextShapeProperty BaseProperties { get; }
		public override IProperty Properties => BaseProperties;

		public TextShape(MindMapPage parent, Identity? identity = null) : base(parent, identity) {
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
			MyTextBox.Focus();
			MyTextBox.SelectionStart = MyTextBox.Text.Length;
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

			if(Shape is Border border) {
				border.Background = Background;
				border.BorderBrush = BorderColor;
				border.BorderThickness = BorderThickness;
			} else if(Shape is Shape shape) {
				shape.Fill = Background;
				shape.Stroke = BorderColor;
				shape.StrokeThickness = BorderThickness.Top;
			}
		}
	}
}
