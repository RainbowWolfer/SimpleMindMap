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
		public TextBox MyTextBox { get; set; } = new() {
			TextWrapping = TextWrapping.Wrap,
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
			AcceptsReturn = true,
			AcceptsTab = true,
		};
		public TextBlock MyTextBlock { get; set; } = new() {
			TextWrapping = TextWrapping.Wrap,
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
		};

		public Brush Background {
			get => TextShapeProperties.background;
			set {
				TextShapeProperties.background = value;
				UpdateStyle();
			}
		}
		public Brush BorderColor {
			get => TextShapeProperties.borderColor;
			set {
				TextShapeProperties.borderColor = value;
				UpdateStyle();
			}
		}
		public Thickness BorderThickness {
			get => TextShapeProperties.borderThickness;
			set {
				TextShapeProperties.borderThickness = value;
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

		protected abstract TextShapeProperty TextShapeProperties { get; }
		public override IProperty Properties => TextShapeProperties;

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

		public override void LeftClick(MouseButtonEventArgs e) {

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
			base.UpdateStyle();
			MyTextBlock.Text = Text;
			MyTextBlock.Foreground = new SolidColorBrush(FontColor);
			MyTextBlock.FontFamily = FontFamily;
			MyTextBlock.FontWeight = FontWeight;
			MyTextBlock.FontSize = FontSize;
			MyTextBlock.Padding = new Thickness(10);

			MyTextBox.Text = Text;
			MyTextBox.Foreground = new SolidColorBrush(FontColor);
			MyTextBox.FontFamily = FontFamily;
			MyTextBox.FontWeight = FontWeight;
			MyTextBox.FontSize = FontSize;
			MyTextBox.Padding = new Thickness(10);

			if(Shape is Border border) {
				border.Background = Background;
				border.BorderBrush = BorderColor;
				border.BorderThickness = BorderThickness;
			} else if(Shape is Shape shape) {
				shape.Fill = Background;
				shape.Stroke = BorderColor;
				shape.StrokeThickness = BorderThickness.Top;
			}

			TextShadowEffect.BlurRadius = TextShadowBlurRadius;
			TextShadowEffect.ShadowDepth = TextShadowDepth;
			TextShadowEffect.Direction = TextShadowDirection;
			TextShadowEffect.Color = TextShadowColor;
			TextShadowEffect.Opacity = EnableTextShadow ? TextShadowOpacity : 0;
			MyTextBlock.Effect = TextShadowEffect;
			MyTextBox.Effect = TextShadowEffect;
		}
	}
}
