using MindMap.Entities.Elements;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Entities.Icons;
using MindMap.Entities.Identifications;
using MindMap.Entities.Interactions;
using MindMap.Entities.Properties;
using MindMap.Entities.Services;
using MindMap.Entities.Tags;
using MindMap.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace MindMap.Entities.Connections {
	public class ConnectionPath: IPropertiesContainer, IIdentityContainer, ITextContainer, IInteractive, ITextShadow {
		public Identity Identity { get; set; }

		public ConnectionControl from;
		public ConnectionControl? to;

		private readonly ConnectionsManager? _connectionsManager;
		private readonly MindMapPage _parent;

		public Canvas MainCanvas => _parent.MainCanvas;

		private class Property: IProperty {
			public double strokeThickess = 3;
			public Color strokeColor = Colors.ForestGreen;
			public DuoNumber dashStroke = new DuoNumber(5, 0);
			public int extendLengthPercentage = 50;
			public int minimumGap = 30;
			public PathType pathType = PathType.Linear;

			public string text = "";
			public FontFamily fontFamily = new("Microsoft YaHei UI");
			public FontWeight fontWeight = FontWeights.Normal;
			public double fontSize = 14;
			public Color fontColor = Colors.Black;

			public bool enableTextShadow = true;
			public DropShadowEffect textShadowEffect = new();
			public double textShadowBlurRadius = 2;
			public double textShadowDepth = 2;
			public double textShadowDirection = 315;
			public Color textShadowColor = Colors.Black;
			public double textShadowOpacity = 0.5;

			public object Clone() {
				return MemberwiseClone();
			}

			public IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json) ?? new();
			}
		}
		private Property property = new();
		public IProperty Properties => property;

		public Path Path { get; private set; }
		public bool IsSelected { get; private set; }
		private Path _backgroundPath = new();

		private Vector2 PathCenterPoint { get; set; }

		private Grid grid = new() {
			Background = new SolidColorBrush(new Color() {
				A = 10,
				R = 100,
				G = 100,
				B = 100,
			}),
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		private TranslateTransform gridTransform = new();
		private TextBlock block = new() {
			TextWrapping = TextWrapping.Wrap,
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		private TextBox box = new() {
			TextWrapping = TextWrapping.Wrap,
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
			AcceptsReturn = true,
			AcceptsTab = true,
		};

		public PathType PathType {
			get => property.pathType;
			set {
				property.pathType = value;
				Update();
			}
		}

		public int ExtendLengthPercentage {
			get => property.extendLengthPercentage;
			set {
				property.extendLengthPercentage = value;
				Update();
			}
		}

		public int MinimumGap {
			get => property.minimumGap;
			set {
				property.minimumGap = value;
				Update();
				Update();
			}
		}

		public DuoNumber DashStroke {
			get => property.dashStroke;
			set {
				property.dashStroke = value;
				UpdateStyle();
			}
		}

		public double StrokeThickess {
			get => property.strokeThickess;
			set {
				property.strokeThickess = value;
				UpdateStyle();
			}
		}

		public Color StrokeColor {
			get => property.strokeColor;
			set {
				property.strokeColor = value;
				UpdateStyle();
			}
		}

		public string Text {
			get => property.text;
			set {
				property.text = value;
				UpdateStyle();
			}
		}

		public FontFamily FontFamily {
			get => property.fontFamily;
			set {
				property.fontFamily = value;
				UpdateStyle();
			}
		}

		public FontWeight FontWeight {
			get => property.fontWeight;
			set {
				property.fontWeight = value;
				UpdateStyle();
			}
		}

		public double FontSize {
			get => property.fontSize;
			set {
				property.fontSize = value;
				UpdateStyle();
			}
		}

		public Color FontColor {
			get => property.fontColor;
			set {
				property.fontColor = value;
				UpdateStyle();
			}
		}

		public DropShadowEffect TextShadowEffect {
			get => property.textShadowEffect;
			set {
				property.textShadowEffect = value;
				UpdateStyle();
			}
		}
		public bool EnableTextShadow {
			get => property.enableTextShadow;
			set {
				property.enableTextShadow = value;
				UpdateStyle();
			}
		}
		public double TextShadowBlurRadius {
			get => property.textShadowBlurRadius;
			set {
				property.textShadowBlurRadius = value;
				UpdateStyle();
			}
		}
		public double TextShadowDepth {
			get => property.textShadowDepth;
			set {
				property.textShadowDepth = value;
				UpdateStyle();
			}
		}
		public double TextShadowDirection {
			get => property.textShadowDirection;
			set {
				property.textShadowDirection = value;
				UpdateStyle();
			}
		}
		public Color TextShadowColor {
			get => property.textShadowColor;
			set {
				property.textShadowColor = value;
				UpdateStyle();
			}
		}
		public double TextShadowOpacity {
			get => property.textShadowOpacity;
			set {
				property.textShadowOpacity = value;
				UpdateStyle();
			}
		}

		public bool IsPreview { get; private set; }

		public ConnectionPath(MindMapPage parent, ConnectionsManager connectionsManager, ConnectionControl from, ConnectionControl to, Identity? identity = null) {
			this.IsPreview = false;
			this._parent = parent;
			this._connectionsManager = connectionsManager;
			this.from = from;
			this.to = to;
			this.Path = new Path() {
				Data = CreateGeometry(new ConnectionDotInfo(from), new ConnectionDotInfo(to)),
				Tag = new ConnectionPathFrameworkTag(this),
			};
			this.Identity = identity ?? new Identity(InitializeID(), InitializeDefaultName());
			this.DashStroke = new DuoNumber(5, 0);
			this.Initialize();
		}

		public ConnectionPath(MindMapPage parent, ConnectionControl from, Vector2 to) {
			this.IsPreview = true;
			this._parent = parent;
			this.from = from;
			this.to = null;
			this.Path = new Path() {
				Data = CreateGeometry(new ConnectionDotInfo(from), new ConnectionDotInfo(to)),
				Tag = new ConnectionPathFrameworkTag(this),
			};
			this.Identity = new Identity($"Preview_Connection_({Methods.GetTick()})", "Preview Conncetion");
			this.DashStroke = new DuoNumber(5, 2);
			this.Initialize();
		}

		private string InitializeID() => $"Connection_({Methods.GetTick()})";

		private string InitializeDefaultName() {
			return $"Connection ({from.Parent.Identity.Name} - {to?.Parent.Identity.Name ?? "None"})";
		}

		public Identity GetIdentity() => Identity;

		public void Initialize(bool initializeProperty = true) {
			MainCanvas.Children.Add(this.Path);
			if(!IsPreview) {
				if(initializeProperty) {
					property.strokeThickess = 3;
					property.strokeColor = Colors.ForestGreen;
				}
				_backgroundPath = new() {
					Data = Path.Data,
					Visibility = Visibility.Collapsed,
				};
				int index = MainCanvas.Children.IndexOf(Path);
				MainCanvas.Children.Insert(index, _backgroundPath);
				MainCanvas.Children.Insert(index + 2, grid);
				grid.RenderTransform = gridTransform;
				grid.Children.Add(block);
				grid.Children.Add(box);
				FlyoutMenu.CreateBase(this.Path, (s, e) => _connectionsManager?.RemoveConnection(this));
				MenuItem item_edit = new() {
					Header = "Edit Text",
					Icon = new FontIcon("\uE70F", 14).Generate(),
				};
				item_edit.Click += (s, e) => SelectText();

				this.Path.ContextMenu.Items.Add(item_edit);

				Path.PreviewMouseLeftButtonUp += (s, e) => LeftClick(e);
				Path.PreviewMouseRightButtonUp += (s, e) => RightClick();
				grid.PreviewMouseLeftButtonUp += (s, e) => LeftClick(e);
				grid.PreviewMouseRightButtonDown += (s, e) => RightClick();

				Path.MouseEnter += (s, e) => MouseEnter();
				Path.MouseLeave += (s, e) => MouseExit();
				grid.MouseEnter += (s, e) => MouseEnter();
				grid.MouseLeave += (s, e) => MouseExit();

				box.PreviewKeyDown += (s, e) => {
					if(e.Key == Key.Escape || e.Key == Key.Enter) {
						Deselect();
					}
				};

				grid.SizeChanged += (s, e) => UpdateTextGridPosition();
				UpdateStyle();
				UpdateBackgroundStyle();//must be after UpdateStyle()
				SubmitText();
				ShowText();
			} else {
				property.strokeThickess = 3;
				property.strokeColor = Colors.Black;
				UpdateStyle();
			}
		}

		private int clickTimeStamp;
		public void LeftClick(MouseButtonEventArgs e) {
			if(e.Timestamp - clickTimeStamp < 300) {
				DoubleClick();
			} else {
				Select();
				_connectionsManager?.ShowProperties(this);
			}
			clickTimeStamp = e.Timestamp;
		}

		public void RightClick() {
			Path.ContextMenu.IsOpen = true;
		}

		public void DoubleClick() {
			SelectText();
			_parent.PropertiesTabItem.IsSelected = true;
			_connectionsManager?.ShowProperties(this);
		}

		public void MiddleClick() {

		}


		public List<FrameworkElement> GetRelatedFrameworks() {
			return new List<FrameworkElement>() {
				_backgroundPath, Path, grid
			};
		}

		public void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
			Update();
		}

		public void SetProperty(string propertyJson) {
			this.property = (Property)property.Translate(propertyJson);
			UpdateStyle();
			Update();
		}

		public void ClearFromCanvas() {
			if(MainCanvas.Children.Contains(Path)) {
				MainCanvas.Children.Remove(Path);
			}
		}

		public void MouseEnter() {
			if(IsSelected) {
				return;
			}
			_backgroundPath.Visibility = Visibility.Visible;
			_backgroundPath.StrokeDashArray = new DoubleCollection(new double[] { 2, 0.5 });
			UpdateBackgroundStyle();
		}

		public void MouseExit() {
			if(IsSelected) {
				return;
			}
			_backgroundPath.Visibility = Visibility.Collapsed;
		}

		public void SelectText() {
			Select();
			_connectionsManager?.ShowProperties(this);
			ShowBox();
		}

		public void Select() {
			if(_connectionsManager != null) {
				_connectionsManager.CurrentSelection = this;
			}
			IsSelected = true;
			_backgroundPath.Visibility = Visibility.Visible;
			_backgroundPath.StrokeDashArray = new DoubleCollection(new double[] { 1, 0 });
			UpdateBackgroundStyle();
		}

		public void Deselect() {
			if(_connectionsManager != null) {
				_connectionsManager.CurrentSelection = null;
			}
			IsSelected = false;
			_backgroundPath.Visibility = Visibility.Collapsed;
			SubmitText();
			ShowText();
		}

		public static Color Invert(Color color, byte alpha = 255) {
			return Color.FromArgb(alpha, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
		}

		public Panel CreatePropertiesPanel() {
			StackPanel panel = new();
			panel.Children.Add(PropertiesPanel.SectionTitle("Connection Path", newName => Identity.Name = newName));
			panel.Children.Add(PropertiesPanel.SliderInput("Strock Thickness", StrokeThickess, 1, 8,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					StrokeThickess = args.NewValue;
				}, (oldP, newP) => {
					_parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.ConnectionPath, this, oldP, newP, "Strock Thickness");
				})
			, 0.1, 1));
			panel.Children.Add(PropertiesPanel.ColorInput("Strock Color", StrokeColor,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					StrokeColor = args.NewValue;
				}, (oldP, newP) => {
					_parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.ConnectionPath, this, oldP, newP, "Strock Color");
				}), () => {
					_parent.editHistory.InstantSealLastDelayedChange();
				}
			));
			panel.Children.Add(PropertiesPanel.DuoNumberInputs("Stroke Dash Array", DashStroke,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					DashStroke = args.NewValue;
				}, (oldP, newP) => {
					_parent.editHistory.SubmitByElementPropertyChanged(TargetType.ConnectionPath, this, oldP, newP, "Stroke Dash");
				})
			));

			List<Pair<IconElement, string>> pathTypesData = new() {
				new(new FontIcon("\uE123", 16), PathType.Linear.ToString()),
				new(new FontIcon("\uE123", 16), PathType.RightAngle.ToString()),
				new(new FontIcon("\uE123", 16), PathType.Straight.ToString()),
			};
			int pathTypeInitialIndex;
			switch(PathType) {
				case PathType.Linear:
					pathTypeInitialIndex = 0;
					break;
				case PathType.RightAngle:
					pathTypeInitialIndex = 1;
					break;
				case PathType.Straight:
					pathTypeInitialIndex = 2;
					break;
				default:
					pathTypeInitialIndex = 0;
					break;
			}
			panel.Children.Add(PropertiesPanel.StackIconsSelector("Path Type", pathTypesData, pathTypeInitialIndex,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					if(args.NewValue != null) {
						PathType = (PathType)Enum.Parse(typeof(PathType), args.NewValue.Value);
					}
				}, (oldP, newP) => {
					_parent.editHistory.SubmitByElementPropertyChanged(TargetType.ConnectionPath, this, oldP, newP, "Path Type");
				})
			));

			panel.Children.Add(PropertiesPanel.SliderInput("Extend Length", ExtendLengthPercentage, 0, 100,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					ExtendLengthPercentage = (int)args.NewValue;
				}, (oldP, newP) => {
					_parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.ConnectionPath, this, oldP, newP, "Extend Length");
				})
			, 5, 0));

			panel.Children.Add(PropertiesPanel.SliderInput("Minimun Gap", MinimumGap, 5, 50,
				args => IPropertiesContainer.PropertyChangedHandler(this, () => {
					MinimumGap = (int)args.NewValue;
				}, (oldP, newP) => {
					_parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.ConnectionPath, this, oldP, newP, "Minimun Gap");
				})
			, 5, 0));

			foreach(Panel p in Element.CreatePropertiesList(this, _parent.editHistory)) {
				panel.Children.Add(p);
			}
			return panel;
		}

		public void UpdateStyle() {
			Path.StrokeThickness = StrokeThickess;
			Path.Stroke = new SolidColorBrush(StrokeColor);
			Path.StrokeDashArray = new DoubleCollection(DashStroke.ToArray());
			UpdateTextShadow();
			UpdateBackgroundStyle();
			UpdateTextStyle();
		}

		public void UpdateTextShadow() {
			TextShadowEffect.BlurRadius = TextShadowBlurRadius;
			TextShadowEffect.ShadowDepth = TextShadowDepth;
			TextShadowEffect.Direction = TextShadowDirection;
			TextShadowEffect.Color = TextShadowColor;
			TextShadowEffect.Opacity = EnableTextShadow ? TextShadowOpacity : 0;
			block.Effect = TextShadowEffect;
			box.Effect = TextShadowEffect;
		}

		public void UpdateBackgroundStyle() {
			_backgroundPath.Data = Path.Data;
			_backgroundPath.Stroke = new SolidColorBrush(Invert(((SolidColorBrush)Path.Stroke).Color, 130));
			_backgroundPath.StrokeThickness = Path.StrokeThickness * 1.7;
		}

		public void UpdateTextStyle() {
			block.Text = Text;
			block.Foreground = new SolidColorBrush(FontColor);
			block.FontFamily = FontFamily;
			block.FontWeight = FontWeight;
			block.FontSize = FontSize;
			block.Padding = new Thickness(10);

			box.Text = Text;
			box.Foreground = new SolidColorBrush(FontColor);
			box.FontFamily = FontFamily;
			box.FontWeight = FontWeight;
			box.FontSize = FontSize;
			box.Padding = new Thickness(10);
		}

		public void Update(Vector2 to, ConnectionControl? target) {
			this.Path.Data = CreateGeometry(new ConnectionDotInfo(from), target == null ? new ConnectionDotInfo(to) : new ConnectionDotInfo(target));
			UpdateBackgroundStyle();
			UpdateTextGridPosition();
		}

		public void Update() {
			if(to == null) {
				throw new Exception("to is null");
			}
			this.Path.Data = CreateGeometry(new ConnectionDotInfo(from), new ConnectionDotInfo(to));
			UpdateBackgroundStyle();
			UpdateTextGridPosition();
		}

		public void ClearBackground() {
			if(!MainCanvas.Children.Contains(_backgroundPath)) {
				return;
			}
			MainCanvas.Children.Remove(_backgroundPath);
		}

		public Vector2 GetCenterPoint() {
			if(this.to == null) {
				return Vector2.Zero;
			}
			if(PathType == PathType.Linear || PathType == PathType.RightAngle) {
				return PathCenterPoint;
			}
			Vector2 from = this.from.GetPosition();
			Vector2 to = this.to.GetPosition();
			return (from + to) / 2;
		}

		public void UpdateTextGridPosition() {
			if(IsPreview) {
				return;
			}
			Vector2 center = GetCenterPoint();
			gridTransform.X = -grid.ActualWidth / 2;
			gridTransform.Y = -grid.ActualHeight / 2;
			Canvas.SetLeft(grid, center.X);
			Canvas.SetTop(grid, center.Y);
		}

		public void SubmitText() {
			Text = box.Text;
			block.Text = Text;
			if(string.IsNullOrWhiteSpace(Text)) {
				grid.Visibility = Visibility.Collapsed;
			}
		}

		public void ShowText() {
			block.Visibility = Visibility.Visible;
			box.Visibility = Visibility.Collapsed;
		}

		public void ShowBox() {
			grid.Visibility = Visibility.Visible;
			block.Visibility = Visibility.Collapsed;
			box.Visibility = Visibility.Visible;
			box.Focus();
			box.SelectionStart = box.Text.Length;
		}

		public void ClearText() {
			if(!MainCanvas.Children.Contains(grid)) {
				return;
			}
			MainCanvas.Children.Remove(grid);
		}

		public override string ToString() {
			return $"Conection: {from.Parent_ID}-{to?.Parent_ID ?? "None"}";
		}

		private Geometry CreateGeometry(ConnectionDotInfo from, ConnectionDotInfo to) {
			Geometry geometry;
			switch(PathType) {
				case PathType.Linear:
					geometry = CreateLinearGeometry(from, to);
					break;
				case PathType.RightAngle:
					geometry = CreatePathGeometry(from, to);
					break;
				case PathType.Straight:
					geometry = CreateStraightPathGeometry(from, to);
					break;
				default:
					throw new Exception($"PathType ({PathType}) not found");
			}
			return geometry;
		}

		private Geometry CreateStraightPathGeometry(ConnectionDotInfo from, ConnectionDotInfo to) {
			return new PathGeometry(new PathFigure[] {
				new PathFigure(from.Position.ToPoint(),
					new List<PathSegment>() {
						new LineSegment(to.Position.ToPoint(), true)
					}
				, false)
			});
		}

		private Geometry CreateLinearGeometry(ConnectionDotInfo from, ConnectionDotInfo to) {
			Vector2 startJoint;
			Vector2 endJoint;
			double length = ExtendLengthPercentage * 2;
			if(from.Direction == Direction.Left) {
				startJoint = new Vector2(from.X - length, from.Y);
			} else if(from.Direction == Direction.Right) {
				startJoint = new Vector2(from.X + length, from.Y);
			} else if(from.Direction == Direction.Top) {
				startJoint = new Vector2(from.X, from.Y - length);
			} else if(from.Direction == Direction.Bottom) {
				startJoint = new Vector2(from.X, from.Y + length);
			} else {
				throw new DirectionNotFoundException(from.Direction);
			}
			if(to.IsExisted) {
				if(to.Direction == Direction.Left) {
					endJoint = new Vector2(to.X - length, to.Y);
				} else if(to.Direction == Direction.Right) {
					endJoint = new Vector2(to.X + length, to.Y);
				} else if(to.Direction == Direction.Top) {
					endJoint = new Vector2(to.X, to.Y - length);
				} else if(to.Direction == Direction.Bottom) {
					endJoint = new Vector2(to.X, to.Y + length);
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else {
				if(from.Direction == Direction.Left) {
					endJoint = new Vector2(to.X + length, to.Y);
				} else if(from.Direction == Direction.Right) {
					endJoint = new Vector2(to.X - length, to.Y);
				} else if(from.Direction == Direction.Top) {
					endJoint = new Vector2(to.X, to.Y + length);
				} else if(from.Direction == Direction.Bottom) {
					endJoint = new Vector2(to.X, to.Y - length);
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			}
			PathCenterPoint = (startJoint + endJoint) / 2;
			return Geometry.Parse(
				$"M {from.Position.X},{from.Position.Y} " +
				$"C {startJoint.X},{startJoint.Y} " +
				$"	{endJoint.X},{endJoint.Y} " +
				$"	{to.Position.X},{to.Position.Y}");
		}

		private static LineSegment NewLine(double x, double y) {
			return new LineSegment(new Point(x, y), true);
		}

		private Geometry CreatePathGeometry(ConnectionDotInfo from, ConnectionDotInfo to) {
			int MinGap = MinimumGap;
			List<LineSegment> segments = new();
			if(from.Direction == Direction.Left) {
				if(!to.IsExisted || to.Direction == Direction.Right) {// Default & Left -> Right
					if(from.X - MinGap < to.X + MinGap) {
						double mid = from.Y + (to.Y - from.Y) / 2;
						if(from.Y > to.Y) {
							if(from.Y - from.Height / 2 - MinGap < mid
								|| to.Y + to.Height / 2 + MinGap > mid) {
								double toEdge = to.Y + to.Height / 2 + MinGap;
								double fromEdge = from.Y - from.Height / 2 - MinGap;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(from.X - MinGap, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X + MinGap, toEdge));
								segments.Add(NewLine(to.X + MinGap, to.Y));
							} else {
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(from.X - MinGap, mid));
								segments.Add(NewLine(to.X + MinGap, mid));
								segments.Add(NewLine(to.X + MinGap, to.Y));
							}
						} else {
							if(from.Y + from.Height / 2 + MinGap > mid
								|| to.Y - to.Height / 2 - MinGap < mid) {
								double toEdge = to.Y - to.Height / 2 - MinGap;
								double fromEdge = from.Y + from.Height / 2 + MinGap;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(from.X - MinGap, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X + MinGap, toEdge));
								segments.Add(NewLine(to.X + MinGap, to.Y));
							} else {
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(from.X - MinGap, mid));
								segments.Add(NewLine(to.X + MinGap, mid));
								segments.Add(NewLine(to.X + MinGap, to.Y));
							}
						}
					} else {
						double joint = from.X + (to.X - from.X) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Left) {// Left -> Left
					double topEdge = to.Y - to.Height / 2 - MinGap;
					double botEdge = to.Y + to.Height / 2 + MinGap;
					if(topEdge < from.Y && from.Y < botEdge) {
						double joint;
						double edge;
						if(from.X > to.X) {
							joint = Math.Min(to.X, from.X) - MinGap;
							if(from.Y > to.Y) {
								edge = to.Y + to.Height / 2 + MinGap;
							} else {
								edge = to.Y - to.Height / 2 - MinGap;
							}
						} else {
							joint = to.X - MinGap;
							if(from.Y < to.Y) {
								edge = from.Y + from.Height / 2 + MinGap;
							} else {
								edge = from.Y - from.Height / 2 - MinGap;
							}
						}
						segments.Add(NewLine(from.X - MinGap, from.Y));
						segments.Add(NewLine(from.X - MinGap, edge));
						segments.Add(NewLine(joint, edge));
						segments.Add(NewLine(joint, to.Y));
					} else {
						double joint = Math.Min(to.X, from.X) - MinGap;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Top) {// Left -> Top
					if(to.Y - MinGap < from.Y) {
						if(from.Y > to.Y + to.Height + MinGap
							&& from.X - MinGap < to.X + to.Width / 2 + MinGap
							&& from.X - MinGap > to.X - to.Width / 2 - MinGap) {
							if(from.X - MinGap > to.X) {
								double joint_height = to.Y + to.Height + MinGap;
								double joint_width = to.X + to.Width / 2 + MinGap;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(from.X - MinGap, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							} else {
								double joint_width = to.X - to.Width / 2 - MinGap;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							}
						} else {
							segments.Add(NewLine(from.X - MinGap, from.Y));
							segments.Add(NewLine(from.X - MinGap, to.Y - MinGap));
							segments.Add(NewLine(to.X, to.Y - MinGap));
						}
					} else if(from.X - MinGap < to.X) {
						segments.Add(NewLine(from.X - MinGap, from.Y));
						segments.Add(NewLine(from.X - MinGap, to.Y - MinGap));
						segments.Add(NewLine(to.X, to.Y - MinGap));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else if(to.Direction == Direction.Bottom) {// Left -> Bottom
					if(to.Y + MinGap > from.Y) {
						if(from.Y < to.Y - to.Height - MinGap
							&& from.X - MinGap < to.X + to.Width / 2 + MinGap
							&& from.X - MinGap > to.X - to.Width / 2 - MinGap) {
							if(from.X - MinGap > to.X) {
								double joint_height = to.Y - to.Height - MinGap;
								double joint_width = to.X + to.Width / 2 + MinGap;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(from.X - MinGap, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							} else {
								double joint_width = to.X - to.Width / 2 - MinGap;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							}
						} else {
							segments.Add(NewLine(from.X - MinGap, from.Y));
							segments.Add(NewLine(from.X - MinGap, to.Y + MinGap));
							segments.Add(NewLine(to.X, to.Y + MinGap));
						}
					} else if(from.X - MinGap < to.X) {
						segments.Add(NewLine(from.X - MinGap, from.Y));
						segments.Add(NewLine(from.X - MinGap, to.Y + MinGap));
						segments.Add(NewLine(to.X, to.Y + MinGap));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else if(from.Direction == Direction.Right) {
				if(!to.IsExisted || to.Direction == Direction.Left) {// Default & Right -> Left
					if(from.X + MinGap > to.X - MinGap) {
						double mid = from.Y + (to.Y - from.Y) / 2;
						if(from.Y > to.Y) {
							if(from.Y - from.Height / 2 - MinGap < mid
								|| to.Y + to.Height / 2 + MinGap > mid) {
								double toEdge = to.Y + to.Height / 2 + MinGap;
								double fromEdge = from.Y - from.Height / 2 - MinGap;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(from.X + MinGap, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X - MinGap, toEdge));
								segments.Add(NewLine(to.X - MinGap, to.Y));
							} else {
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(from.X + MinGap, mid));
								segments.Add(NewLine(to.X - MinGap, mid));
								segments.Add(NewLine(to.X - MinGap, to.Y));
							}
						} else {
							if(from.Y + from.Height / 2 + MinGap > mid
								|| to.Y - to.Height / 2 - MinGap < mid) {
								double toEdge = to.Y - to.Height / 2 - MinGap;
								double fromEdge = from.Y + from.Height / 2 + MinGap;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(from.X + MinGap, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X - MinGap, toEdge));
								segments.Add(NewLine(to.X - MinGap, to.Y));
							} else {
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(from.X + MinGap, mid));
								segments.Add(NewLine(to.X - MinGap, mid));
								segments.Add(NewLine(to.X - MinGap, to.Y));
							}
						}
					} else {
						double joint = from.X + (to.X - from.X) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Right) {// Right -> Right
					double topEdge = to.Y - to.Height / 2 - MinGap;
					double botEdge = to.Y + to.Height / 2 + MinGap;
					if(topEdge < from.Y && from.Y < botEdge) {
						double joint;
						double edge;
						if(from.X < to.X) {
							joint = Math.Max(to.X, from.X) + MinGap;
							if(from.Y > to.Y) {
								edge = to.Y + to.Height / 2 + MinGap;
							} else {
								edge = to.Y - to.Height / 2 - MinGap;
							}
						} else {
							joint = to.X + MinGap;
							if(from.Y < to.Y) {
								edge = from.Y + from.Height / 2 + MinGap;
							} else {
								edge = from.Y - from.Height / 2 - MinGap;
							}
						}
						segments.Add(NewLine(from.X + MinGap, from.Y));
						segments.Add(NewLine(from.X + MinGap, edge));
						segments.Add(NewLine(joint, edge));
						segments.Add(NewLine(joint, to.Y));
					} else {
						double joint = Math.Max(to.X, from.X) + MinGap;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Top) {// Right -> Top
					if(to.Y - MinGap < from.Y) {
						if(from.Y > to.Y + to.Height + MinGap
							&& from.X + MinGap > to.X - to.Width / 2 - MinGap
							&& from.X + MinGap < to.X + to.Width / 2 + MinGap) {
							if(from.X + MinGap < to.X) {
								double joint_height = to.Y + to.Height + MinGap;
								double joint_width = to.X - to.Width / 2 - MinGap;
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(from.X + MinGap, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							} else {
								double joint_width = to.X + to.Width / 2 + MinGap;
								segments.Add(NewLine(from.X - MinGap, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							}
						} else {
							segments.Add(NewLine(from.X + MinGap, from.Y));
							segments.Add(NewLine(from.X + MinGap, to.Y - MinGap));
							segments.Add(NewLine(to.X, to.Y - MinGap));
						}
					} else if(from.X + MinGap > to.X) {
						segments.Add(NewLine(from.X + MinGap, from.Y));
						segments.Add(NewLine(from.X + MinGap, to.Y - MinGap));
						segments.Add(NewLine(to.X, to.Y - MinGap));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else if(to.Direction == Direction.Bottom) {// Right -> Bottom
					if(to.Y + MinGap > from.Y) {
						if(from.Y < to.Y - to.Height - MinGap
							&& from.X + MinGap > to.X - to.Width / 2 - MinGap
							&& from.X + MinGap < to.X + to.Width / 2 + MinGap) {
							if(from.X + MinGap < to.X) {
								double joint_height = to.Y - to.Height - MinGap;
								double joint_width = to.X - to.Width / 2 - MinGap;
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(from.X + MinGap, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							} else {
								double joint_width = to.X + to.Width / 2 + MinGap;
								segments.Add(NewLine(from.X + MinGap, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							}
						} else {
							segments.Add(NewLine(from.X + MinGap, from.Y));
							segments.Add(NewLine(from.X + MinGap, to.Y + MinGap));
							segments.Add(NewLine(to.X, to.Y + MinGap));
						}
					} else if(from.X + MinGap > to.X) {
						segments.Add(NewLine(from.X + MinGap, from.Y));
						segments.Add(NewLine(from.X + MinGap, to.Y + MinGap));
						segments.Add(NewLine(to.X, to.Y + MinGap));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else if(from.Direction == Direction.Top) {
				if(!to.IsExisted || to.Direction == Direction.Bottom) {// Default & Top -> Bottom
					if(from.Y - MinGap < to.Y + MinGap) {
						double mid = from.X + (to.X - from.X) / 2;
						if(from.X > to.X) {
							if(from.X - from.Width / 2 - MinGap < mid
								|| to.X + to.Width / 2 + MinGap > mid) {
								double toEdge = to.X + to.Width / 2 + MinGap;
								double fromEdge = from.X - from.Width / 2 - MinGap;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(fromEdge, from.Y - MinGap));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							} else {
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(mid, from.Y - MinGap));
								segments.Add(NewLine(mid, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							}
						} else {
							if(from.X + from.Width / 2 + MinGap > mid
								|| to.X - to.Width / 2 - MinGap < mid) {
								double toEdge = to.X - to.Width / 2 - MinGap;
								double fromEdge = from.X + from.Width / 2 + MinGap;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(fromEdge, from.Y - MinGap));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							} else {
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(mid, from.Y - MinGap));
								segments.Add(NewLine(mid, to.Y + MinGap));
								segments.Add(NewLine(to.X, to.Y + MinGap));
							}
						}
					} else {
						double joint = from.Y + (to.Y - from.Y) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Top) {// Top -> Top
					double leftEdge = to.X - to.Width / 2 - MinGap;
					double rightEdge = to.X + to.Width / 2 + MinGap;
					if(leftEdge < from.X && from.X < rightEdge) {
						double joint;
						double edge;
						if(from.Y > to.Y) {
							joint = Math.Min(to.Y, from.Y) - MinGap;
							if(from.X > to.X) {
								edge = to.X + to.Width / 2 + MinGap;
							} else {
								edge = to.X - to.Width / 2 - MinGap;
							}
						} else {
							joint = to.Y - MinGap;
							if(from.X < to.X) {
								edge = from.X + from.Width / 2 + MinGap;
							} else {
								edge = from.X - from.Width / 2 - MinGap;
							}
						}
						segments.Add(NewLine(from.X, from.Y - MinGap));
						segments.Add(NewLine(edge, from.Y - MinGap));
						segments.Add(NewLine(edge, joint));
						segments.Add(NewLine(to.X, joint));
					} else {
						double joint = Math.Min(to.Y, from.Y) - MinGap;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Left) {// Top -> Left
					if(to.Y + MinGap > from.Y) {
						if(to.Y > from.Y - from.Height - MinGap
							&& to.X - MinGap < from.X + from.Width / 2 + MinGap
							&& to.X - MinGap > from.X - from.Width / 2 - MinGap) {
							if(to.X - MinGap > from.X) {
								double joint_width = from.X + from.Width / 2 + MinGap;
								double joint_height = from.Y + from.Height + MinGap;
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(joint_width, from.Y - MinGap));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X - MinGap, joint_height));
								segments.Add(NewLine(to.X - MinGap, to.Y));
							} else {
								double joint_width = from.X - from.Width / 2 - MinGap;
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(joint_width, from.Y - MinGap));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y - MinGap));
							segments.Add(NewLine(to.X - MinGap, from.Y - MinGap));
							segments.Add(NewLine(to.X - MinGap, to.Y));
						}
					} else if(from.X + MinGap > to.X) {
						segments.Add(NewLine(from.X, from.Y - MinGap));
						segments.Add(NewLine(to.X - MinGap, from.Y - MinGap));
						segments.Add(NewLine(to.X - MinGap, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else if(to.Direction == Direction.Right) {// Top -> Right
					if(to.Y + MinGap > from.Y) {
						if(to.Y > from.Y - from.Height - MinGap
							&& to.X + MinGap > from.X - from.Width / 2 - MinGap
							&& to.X + MinGap < from.X + from.Width / 2 + MinGap) {
							if(to.X < from.X) {
								double joint_width = from.X - from.Width / 2 - MinGap;
								double joint_height = from.Y + from.Height + MinGap;
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(joint_width, from.Y - MinGap));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X + MinGap, joint_height));
								segments.Add(NewLine(to.X + MinGap, to.Y));
							} else {
								double joint_width = from.X + from.Width / 2 + MinGap;
								segments.Add(NewLine(from.X, from.Y - MinGap));
								segments.Add(NewLine(joint_width, from.Y - MinGap));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y - MinGap));
							segments.Add(NewLine(to.X + MinGap, from.Y - MinGap));
							segments.Add(NewLine(to.X + MinGap, to.Y));
						}
					} else if(from.X - MinGap < to.X) {
						segments.Add(NewLine(from.X, from.Y - MinGap));
						segments.Add(NewLine(to.X + MinGap, from.Y - MinGap));
						segments.Add(NewLine(to.X + MinGap, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else if(from.Direction == Direction.Bottom) {
				if(!to.IsExisted || to.Direction == Direction.Top) {// Default & Bottom -> Top
					if(from.Y + MinGap > to.Y - MinGap) {
						double mid = from.X + (to.X - from.X) / 2;
						if(from.X > to.X) {
							if(from.X - from.Width / 2 - MinGap < mid
								|| to.X + to.Width / 2 + MinGap > mid) {
								double toEdge = to.X + to.Width / 2 + MinGap;
								double fromEdge = from.X - from.Width / 2 - MinGap;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(fromEdge, from.Y + MinGap));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							} else {
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(mid, from.Y + MinGap));
								segments.Add(NewLine(mid, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							}
						} else {
							if(from.X + from.Width / 2 + MinGap > mid
								|| to.X - to.Width / 2 - MinGap < mid) {
								double toEdge = to.X - to.Width / 2 - MinGap;
								double fromEdge = from.X + from.Width / 2 + MinGap;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(fromEdge, from.Y + MinGap));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							} else {
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(mid, from.Y + MinGap));
								segments.Add(NewLine(mid, to.Y - MinGap));
								segments.Add(NewLine(to.X, to.Y - MinGap));
							}
						}
					} else {
						double joint = from.Y + (to.Y - from.Y) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Bottom) {// Bottom -> Bottom
					double leftEdge = to.X - to.Width / 2 - MinGap;
					double rightEdge = to.X + to.Width / 2 + MinGap;
					if(leftEdge < from.X && from.X < rightEdge) {
						double joint;
						double edge;
						if(from.Y < to.Y) {
							joint = Math.Max(to.Y, from.Y) + MinGap;
							if(from.X > to.X) {
								edge = to.X + to.Width / 2 + MinGap;
							} else {
								edge = to.X - to.Width / 2 - MinGap;
							}
						} else {
							joint = to.Y + MinGap;
							if(from.X < to.X) {
								edge = from.X + from.Width / 2 + MinGap;
							} else {
								edge = from.X - from.Width / 2 - MinGap;
							}
						}
						segments.Add(NewLine(from.X, from.Y + MinGap));
						segments.Add(NewLine(edge, from.Y + MinGap));
						segments.Add(NewLine(edge, joint));
						segments.Add(NewLine(to.X, joint));
					} else {
						double joint = Math.Max(to.Y, from.Y) + MinGap;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Left) {// Bottom -> Left
					if(to.Y - MinGap < from.Y) {
						if(to.Y < from.Y + from.Height + MinGap
							&& to.X - MinGap < from.X + from.Width / 2 + MinGap
							&& to.X - MinGap > from.X - from.Width / 2 - MinGap) {
							if(to.X - MinGap > from.X) {
								double joint_width = from.X + from.Width / 2 + MinGap;
								double joint_height = from.Y - from.Height - MinGap;
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(joint_width, from.Y + MinGap));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X - MinGap, joint_height));
								segments.Add(NewLine(to.X - MinGap, to.Y));
							} else {
								double joint_width = from.X - from.Width / 2 - MinGap;
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(joint_width, from.Y + MinGap));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y + MinGap));
							segments.Add(NewLine(to.X - MinGap, from.Y + MinGap));
							segments.Add(NewLine(to.X - MinGap, to.Y));
						}
					} else if(from.X + MinGap > to.X) {
						segments.Add(NewLine(from.X, from.Y + MinGap));
						segments.Add(NewLine(to.X - MinGap, from.Y + MinGap));
						segments.Add(NewLine(to.X - MinGap, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else if(to.Direction == Direction.Right) {// Bottom -> Right
					if(to.Y - MinGap < from.Y) {
						if(to.Y < from.Y + from.Height + MinGap
							&& to.X + MinGap > from.X - from.Width / 2 - MinGap
							&& to.X + MinGap < from.X + from.Width / 2 + MinGap) {
							if(to.X + MinGap < from.X) {
								double joint_width = from.X - from.Width / 2 - MinGap;
								double joint_height = from.Y - from.Height - MinGap;
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(joint_width, from.Y + MinGap));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X + MinGap, joint_height));
								segments.Add(NewLine(to.X + MinGap, to.Y));
							} else {
								double joint_width = from.X + from.Width / 2 + MinGap;
								segments.Add(NewLine(from.X, from.Y + MinGap));
								segments.Add(NewLine(joint_width, from.Y + MinGap));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y + MinGap));
							segments.Add(NewLine(to.X + MinGap, from.Y + MinGap));
							segments.Add(NewLine(to.X + MinGap, to.Y));
						}
					} else if(from.X - MinGap < to.X) {
						segments.Add(NewLine(from.X, from.Y + MinGap));
						segments.Add(NewLine(to.X + MinGap, from.Y + MinGap));
						segments.Add(NewLine(to.X + MinGap, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else {
				throw new DirectionNotFoundException(to.Direction);
			}

			List<LineSegment> paths = new();
			paths.AddRange(segments);
			paths.Add(new LineSegment(to.Position.ToPoint(), true));
			PathFigure lines = new PathFigure(from.Position.ToPoint(), paths, false);
			lines.IsFilled = false;
			//PathFigure pointer = new PathFigure(to.Position.ToPoint(), new PathSegment[] {

			//}, false);

			var clone = paths.ToList();
			clone.Insert(0, new LineSegment(from.Position.ToPoint(), true));
			if(clone.Count % 2 == 0) {
				LineSegment first = clone[clone.Count / 2 - 1];
				LineSegment second = clone[clone.Count / 2];
				PathCenterPoint = (new Vector2(first.Point) + new Vector2(second.Point)) / 2;
			} else {
				List<(Vector2 dot1, Vector2 dot2, double distance)> list = new();
				for(int i = 0; i < clone.Count - 1; i++) {
					Vector2 dot1 = clone[i].Point;
					Vector2 dot2 = clone[i + 1].Point;
					double distance = Vector2.Distance(dot1, dot2);
					list.Add(new(dot1, dot2, distance));
				}
				var sorted = list.OrderByDescending(l => l.distance);
				var max = sorted.FirstOrDefault();
				PathCenterPoint = (max.dot1 + max.dot2) / 2;
			}

			return new PathGeometry(new PathFigure[] {
				lines, /*pointer*/
			});
		}
		public class ConnectionDotInfo {
			public Vector2 Position { get; set; }
			public double X => Position.X;
			public double Y => Position.Y;
			public bool IsExisted { get; set; }
			public Vector2 Size { get; set; }
			public double Width => Size.X;
			public double Height => Size.Y;
			public Direction Direction { get; set; }

			public ConnectionDotInfo(Vector2 position, Vector2 size, Direction direction) {
				Position = position;
				Size = size;
				Direction = direction;
				IsExisted = true;
			}

			public ConnectionDotInfo(ConnectionControl control) {
				Position = control.GetPosition();
				Size = control.Parent.GetSize();
				Direction = control.Direction;
				IsExisted = true;
			}
			public ConnectionDotInfo(Vector2 position) {
				Position = position;
				IsExisted = false;
				Size = default;
				Direction = default;
			}
		}
	}

	public enum PathType {
		Linear, RightAngle, Straight
	}
}
