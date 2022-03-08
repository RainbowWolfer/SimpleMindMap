using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MindMap.Entities.Elements {
	public class MyImage: TextRelated, IAnnotation, IBorderBasedStyle, IUpdate {
		public override long TypeID => ID_Image;

		public override string ElementTypeName => "Image";

		protected class Property: TextRelatedProperty {
			public Direction direction = Direction.Top;
			public int offsetPercentage = 0;

			public Brush background = Brushes.Gray;
			public Brush borderColor = Brushes.Aquamarine;
			public Thickness borderThickness = new(2);

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
		private readonly Image _image = new();
		public override FrameworkElement Target => _root;

		public TextBlock AnnotationTextBlock { get; set; } = new();
		public Direction Direction {
			get => property.direction;
			set {
				property.direction = value;
				UpdateStyle();
			}
		}
		public int OffsetPercentage {
			get => property.offsetPercentage;
			set {
				property.offsetPercentage = value;
				UpdateStyle();
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

		private Vector2 ImageSize { get; set; } = Vector2.Zero;
		private int ImageDefaultHeight = 150;

		public MyImage(MindMapPage parent, Identity? identity = null) : base(parent, identity) {
			BitmapImage bitmapImage = new(new Uri("pack://application:,,,/Images/20210620_130110.jpg"));
			ImageSize = new Vector2(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
			_image.Source = bitmapImage;
			_image.Stretch = Stretch.Uniform;
		}

		public override Vector2 DefaultSize {
			get {
				if(ImageSize == Vector2.Zero) {
					return new Vector2(150, 150);
				} else {
					double ratio = ImageSize.X / ImageSize.Y;
					return new Vector2(ImageDefaultHeight * ratio, ImageDefaultHeight);
				}
			}
		}

		public override Panel CreateElementProperties() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle(Identity.Name,
				newName => Identity.Name = newName
			));
			panel.Children.Add(PropertiesPanel.ActionButton(true, "Reset Ratio", () => {
				SetSize(DefaultSize);
				ResizeFrame.Current?.UpdateResizeFrame();
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

		}

		public override void DoubleClick() {

		}

		public override void LeftClick() {

		}

		public override void MiddleClick() {

		}

		public override void RightClick() {
			_root.ContextMenu.IsOpen = true;
		}

		public override void SetFramework() {
			_root.Children.Clear();
			if(!MainCanvas.Children.Contains(_root)) {
				MainCanvas.Children.Add(_root);
			}
			_root.Children.Add(_background);
			_root.Children.Add(_image);
			_root.Children.Add(AnnotationTextBlock);
			_root.Children.Add(_border);
			UpdateStyle();
		}

		public override void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
		}

		public override void SetProperty(string propertyJson) {
			property = (Property)property.Translate(propertyJson);
			UpdateStyle();
		}

		protected override void UpdateStyle() {
			_background.Fill = Background;
			_border.BorderBrush = BorderColor;
			_border.BorderThickness = BorderThickness;
		}

		public void Update() {
			Debug.WriteLine($"{_image.ActualWidth} - {_image.ActualHeight}");
		}
	}
}
