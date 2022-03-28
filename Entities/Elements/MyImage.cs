using Microsoft.Win32;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Locals;
using MindMap.Entities.Properties;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyImage: TextRelated, IAnnotation, IBorderBasedStyle {
		public override long TypeID => ElementGenerator.ID_Image;

		public override string ElementTypeName => "Image";
		public override (string icon, string fontFamily) Icon => ("\uE91B", "Segoe Fluent Icons");

		protected class Property: TextRelatedProperty {
			public Direction direction = Direction.Bottom;
			public int offsetPercentage = 0;

			public Brush background = Brushes.Gray;
			public Brush borderColor = Brushes.Aquamarine;
			public Thickness borderThickness = new(2);

			public string? imageKey = null;

			public override object Clone() {
				return MemberwiseClone();
			}

			public override IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}
		}

		private Property property = new();
		public override IProperty Properties => property;
		protected override TextRelatedProperty TextRelatedProperties => property;

		private readonly Grid _root = new();
		private readonly Rectangle _background = new() {
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
		};
		private readonly Border _border = new() {
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Background = new SolidColorBrush() {
				Color = Colors.Transparent,
			},
		};
		private readonly Image _image = new() {
			Stretch = Stretch.Uniform,
		};
		public override FrameworkElement Target => _root;

		public Grid AnnotationGrid { get; set; } = new() {
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};
		public TranslateTransform GridTransform { get; set; } = new();
		public TextBlock AnnotationTextBlock { get; set; } = new() {
			TextWrapping = TextWrapping.Wrap,
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		public TextBox AnnotationTextBox { get; set; } = new() {
			TextWrapping = TextWrapping.Wrap,
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
			AcceptsReturn = true,
			AcceptsTab = true,
		};

		public Direction Direction {
			get => property.direction;
			set {
				property.direction = value;
				SizeUpdate();
			}
		}
		/// <summary> Range From -100 to 100 </summary>
		public int OffsetPercentage {
			get => property.offsetPercentage;
			set {
				property.offsetPercentage = value;
				SizeUpdate();
			}
		}
		public Brush Background {
			get => property.background;
			set {
				property.background = value;
				UpdateStyle();
			}
		}
		public Brush BorderColor {
			get => property.borderColor;
			set {
				property.borderColor = value;
				UpdateStyle();
			}
		}
		public Thickness BorderThickness {
			get => property.borderThickness;
			set {
				property.borderThickness = value;
				UpdateStyle();
			}
		}

		public string? ImageKey {
			get => property.imageKey;
			set {
				property.imageKey = value;
				SizeUpdate();
				UpdateStyle();
			}
		}

		public override Vector2 DefaultSize {
			get {
				if(AppSettings.Current == null || ImageSize == Vector2.Zero) {
					return base.DefaultSize;
				} else {
					double ratio = ImageSize.X / ImageSize.Y;
					double imageDefaultHeight = AppSettings.Current.ElementDefaultHeight;
					return new Vector2(imageDefaultHeight * ratio, imageDefaultHeight);
				}
			}
		}

		private Vector2 ImageSize { get; set; } = Vector2.Zero;

		//private int ImageDefaultHeight = 150;

		public MyImage(MindMapPage parent, Identity? identity = null) : base(parent, identity) {
			BitmapImage bitmapImage = new(new Uri("pack://application:,,,/Images/DefaultImage.png"));
			ImageSize = new Vector2(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
			_image.Source = bitmapImage;
			_image.SizeChanged += (s, e) => {
				SizeUpdate();
			};
			AnnotationTextBlock.SizeChanged += (s, e) => {
				SizeUpdate();
			};
			AnnotationTextBox.SizeChanged += (s, e) => {
				SizeUpdate();
			};
		}

		public override Panel CreateElementProperties() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle(Identity.Name,
				newName => Identity.Name = newName
			));
			panel.Children.Add(PropertiesPanel.ActionButton(true, "Image Source", () => {
				OpenFileDialog dialog = new OpenFileDialog() {
					Filter = "Image (*.jpg, *.png) | *.jpg; *.png;",
				};
				if(dialog.ShowDialog() == true) {
					byte[] bytes = File.ReadAllBytes(dialog.FileName);
					string b64 = Convert.ToBase64String(bytes);
					string key = parent.imagesAssets.AddImage(b64);
					IPropertiesContainer.PropertyChangedHandler(this, () => {
						SetImageByKey(key);
					}, (oldP, newP) => {
						parent.editHistory.SubmitByElementPropertyChanged(TargetType.Element, this, oldP, newP, "Image Source");
					});
				}
			}, out Button imageSourceButton, "\uEB9F"));
			panel.Children.Add(PropertiesPanel.ActionButton(true, "Reset Ratio", () => {
				Vector2 before_size = GetSize();
				Vector2 before_pos = GetPosition();
				if(before_size == DefaultSize) {
					return;
				}
				SetSize(DefaultSize);
				Vector2 after_size = GetSize();
				Vector2 after_pos = GetPosition();
				ResizeFrame.Current?.UpdateResizeFrame();
				parent.editHistory.SubmitByElementFrameworkChanged(this, before_size, before_pos, after_size, after_pos, EditHistory.FrameChangeType.Resize);
			}, out Button resetRatioButton, "\uF577"));
			panel.Children.Add(PropertiesPanel.DirectionSelector("Annotation Direction", Direction,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					Direction = args.NewValue;
				}, (oldP, newP) => {
					parent.editHistory.SubmitByElementPropertyChanged(TargetType.Element, this, oldP, newP, "Annotation Direction");
				}),
				DirectionArgs.All
			));
			panel.Children.Add(PropertiesPanel.SliderInput("Offset Percentage", OffsetPercentage, -100, 100, args => IPropertiesContainer.PropertyChangedHandler(this, () => {
				OffsetPercentage = (int)args.NewValue;
			}, (oldP, newP) => {
				parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Offset Percentage");
			}), 1, 0));
			return panel;
		}

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
			_root.ContextMenu.IsOpen = true;
		}

		public override void SetFramework() {
			_root.Children.Clear();
			AnnotationGrid.Children.Clear();
			if(!MainCanvas.Children.Contains(_root)) {
				MainCanvas.Children.Add(_root);
			}
			_root.Children.Add(_background);
			_root.Children.Add(_image);
			_root.Children.Add(AnnotationGrid);
			AnnotationGrid.Children.Add(AnnotationTextBlock);
			AnnotationGrid.Children.Add(AnnotationTextBox);
			_root.Children.Add(_border);

			AnnotationGrid.RenderTransform = GridTransform;

			ShowTextBlock();
			UpdateStyle();
		}

		public override void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
			SizeUpdate();
			UpdateImage();
		}

		public override void SetProperty(string propertyJson) {
			property = (Property)property.Translate(propertyJson);
			UpdateStyle();
			SizeUpdate();
			UpdateImage();
		}

		protected override void UpdateStyle() {
			base.UpdateStyle();
			_background.Fill = Background;
			_border.BorderBrush = BorderColor;
			_border.BorderThickness = BorderThickness;

			AnnotationTextBlock.Text = Text;
			AnnotationTextBlock.Foreground = new SolidColorBrush(FontColor);
			AnnotationTextBlock.FontFamily = FontFamily;
			AnnotationTextBlock.FontWeight = FontWeight;
			AnnotationTextBlock.FontSize = FontSize;
			AnnotationTextBlock.Padding = new Thickness(10);

			AnnotationTextBox.Text = Text;
			AnnotationTextBox.Foreground = new SolidColorBrush(FontColor);
			AnnotationTextBox.FontFamily = FontFamily;
			AnnotationTextBox.FontWeight = FontWeight;
			AnnotationTextBox.FontSize = FontSize;
			AnnotationTextBox.Padding = new Thickness(10);
		}

		public void SizeUpdate() {
			double w = _image.ActualWidth;
			double h = _image.ActualHeight;
			double selfWidth = AnnotationTextBlock.ActualWidth;
			double selfHeight = AnnotationTextBlock.ActualHeight;
			switch(Direction) {
				case Direction.Left:
					GridTransform.X = -w / 2 - selfWidth / 2;
					GridTransform.Y = OffsetPercentage * h / 200;
					break;
				case Direction.Right:
					GridTransform.X = w / 2 + selfWidth / 2;
					GridTransform.Y = OffsetPercentage * h / 200;
					break;
				case Direction.Top:
					GridTransform.X = OffsetPercentage * w / 200;
					GridTransform.Y = -h / 2 - selfHeight / 2;
					break;
				case Direction.Bottom:
					GridTransform.X = OffsetPercentage * w / 200;
					GridTransform.Y = h / 2 + selfHeight / 2;
					break;
				default:
					throw new Exception($"Direction ({Direction}) Not Found");
			}
		}

		public void SubmitTextChange() {
			Text = AnnotationTextBox.Text;
			AnnotationTextBlock.Text = Text;
		}

		public void ShowTextBox() {
			AnnotationTextBlock.Visibility = Visibility.Collapsed;
			AnnotationTextBox.Visibility = Visibility.Visible;
			AnnotationTextBox.Focus();
			AnnotationTextBox.SelectionStart = AnnotationTextBox.Text.Length;
		}

		public void ShowTextBlock() {
			AnnotationTextBlock.Visibility = Visibility.Visible;
			AnnotationTextBox.Visibility = Visibility.Collapsed;
		}

		public void UpdateImage() {
			if(ImageKey == null) {
				SetImageByBase64(null);
			} else {
				string? base64 = parent.imagesAssets.FindBase64(ImageKey);
				SetImageByBase64(base64);
			}
		}

		public void SetImageByKey(string key) {
			ImageKey = key;
			string? base64 = parent.imagesAssets.FindBase64(key);
			SetImageByBase64(base64);
		}

		public void SetImageByBase64(string? base64) {
			if(string.IsNullOrWhiteSpace(base64)) {
				BitmapImage bitmapImage = new(new Uri("pack://application:,,,/Images/DefaultImage.png"));
				_image.Source = bitmapImage;
			} else {
				byte[] binaryData = Convert.FromBase64String(base64);
				BitmapImage bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.StreamSource = new MemoryStream(binaryData);
				bitmap.EndInit();
				_image.Source = bitmap;
				ImageSize = new Vector2(bitmap.PixelWidth, bitmap.PixelHeight);
			}
		}
	}
}
