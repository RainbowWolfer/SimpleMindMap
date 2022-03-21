using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Entities.Services;
using MindMap.Entities.Tags;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

/**
 * <Grid VerticalAlignment="Center" HorizontalAlignment="Center" >
 *       <Path StrokeThickness="2"
 *           Stroke="Red"
 *           Fill="{Binding Stroke, RelativeSource={RelativeSource Self}}">
 *           <Path.Data>
 *               <PathGeometry>
 *                   <PathFigure StartPoint="0 0" IsFilled="False">
 *                       <LineSegment Point="20 0"/>
 *                       <LineSegment Point="20 100"/>
 *                       <LineSegment Point="0 100"/>
 *                   </PathFigure>
 *                   <PathFigure StartPoint="0 100" IsClosed="False">
 *                       <LineSegment Point="10 95"/>
 *                       <LineSegment Point="10 105"/>
 *                   </PathFigure>
 *               </PathGeometry>
 *           </Path.Data>
 *       </Path>
 *   </Grid>
 */

namespace MindMap.Entities.Connections {
	public class ConnectionPath: IPropertiesContainer, IIdentityContainer {
		public Identity Identity { get; set; }

		//public Identity FromElement { get; set; }
		//public Identity FromControl { get; set; }
		//public Identity? ToElement { get; set; }
		//public Identity? ToControl { get; set; }

		public ConnectionControl from;
		public ConnectionControl? to;

		private readonly ConnectionsManager? _connectionsManager;
		private readonly MindMapPage _parent;

		public Canvas MainCanvas => _parent.MainCanvas;

		private class Property: IProperty {
			public double strokeThickess = 3;
			public Color strokeColor = Colors.Gray;
			public DuoNumber dashStroke = new DuoNumber(5, 0);

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

		//public Style PathStyle {
		//	get {
		//		Style style = new(typeof(Path));
		//		style.Setters.Add(new Setter(Path.StrokeThicknessProperty, StrokeThickess));
		//		style.Setters.Add(new Setter(Path.StrokeProperty, new SolidColorBrush(StrokeColor)));
		//		return style;
		//	}
		//}

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

		public bool IsPreview { get; private set; }

		public ConnectionPath(MindMapPage parent, ConnectionsManager connectionsManager, ConnectionControl from, ConnectionControl to, Identity? identity = null) {
			this.IsPreview = false;
			this._parent = parent;
			this._connectionsManager = connectionsManager;
			this.from = from;
			this.to = to;
			this.Path = CreatePath(from.GetPosition(), to.GetPosition());
			this.Identity = identity ?? new Identity(InitializeID(), InitializeDefaultName());
			this.DashStroke = new DuoNumber(5, 0);
			this.Initialize();
		}

		public ConnectionPath(MindMapPage parent, ConnectionControl from, Vector2 to) {
			this.IsPreview = true;
			this._parent = parent;
			this.from = from;
			this.to = null;
			this.Path = CreatePath(from.GetPosition(), to);
			this.Identity = new Identity($"Preview_Connection_({Methods.GetTick()})", "Preview Conncetion");
			this.DashStroke = new DuoNumber(5, 2);
			this.Initialize();
		}

		private string InitializeID() => $"Connection_({Methods.GetTick()})";

		private string InitializeDefaultName() {
			return $"Connection ({from.Parent.Identity.Name} - {to?.Parent.Identity.Name ?? "None"})";
		}

		public Identity GetIdentity() => Identity;

		private Vector2 lastMousePosition;
		private bool isRightClick;
		private bool isLeftClick;

		public void Initialize(bool initializeProperty = true) {
			MainCanvas.Children.Add(this.Path);
			if(!IsPreview) {
				if(initializeProperty) {
					property.strokeThickess = 3;
					property.strokeColor = Colors.Gray;
				}
				_backgroundPath = new() {
					Data = Path.Data,
					Visibility = Visibility.Collapsed,
				};
				MainCanvas.Children.Insert(MainCanvas.Children.IndexOf(Path), _backgroundPath);
				FlyoutMenu.CreateBase(this.Path, (s, e) => _connectionsManager?.RemoveConnection(this));
				Path.MouseDown += (s, e) => {
					if(e.MouseDevice.RightButton == MouseButtonState.Pressed) {
						isRightClick = true;
						lastMousePosition = e.GetPosition(MainCanvas);
					} else {
						isRightClick = false;
					}
					if(e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
						isLeftClick = true;
						lastMousePosition = e.GetPosition(MainCanvas);
					} else {
						isLeftClick = false;
					}
				};
				Path.MouseUp += (s, e) => {
					if(e.GetPosition(MainCanvas) == lastMousePosition) {
						if(isRightClick) {
							Path.ContextMenu.IsOpen = true;
						}
						if(isLeftClick) {
							Select();
							_connectionsManager?.ShowProperties(this);
						}
					}
				};
				Path.MouseEnter += (s, e) => MouseEnter();
				Path.MouseLeave += (s, e) => MouseExit();
				UpdateStyle();
				UpdateBackgroundStyle();//must be after UpdateStyle()
			} else {
				property.strokeThickess = 3;
				property.strokeColor = Colors.Black;
				UpdateStyle();
			}
		}

		public void SetProperty(IProperty property) {
			this.property = (Property)property;
			UpdateStyle();
		}

		public void SetProperty(string propertyJson) {
			this.property = (Property)property.Translate(propertyJson);
			UpdateStyle();
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
			panel.Children.Add(PropertiesPanel.DuoNumberInputs("Stroke Dash Array", DashStroke, args => IPropertiesContainer.PropertyChangedHandler(this, () => {
				DashStroke = args.NewValue;
			}, (oldP, newP) => {
				_parent.editHistory.SubmitByElementPropertyChanged(TargetType.ConnectionPath, this, oldP, newP, "Stroke Dash");
			})));
			return panel;
		}

		public void UpdateStyle() {
			//Path.Style = PathStyle;
			Path.StrokeThickness = StrokeThickess;
			Path.Stroke = new SolidColorBrush(StrokeColor);
			Path.StrokeDashArray = new DoubleCollection(DashStroke.ToArray());
		}

		public void UpdateBackgroundStyle() {
			_backgroundPath.Data = Path.Data;
			//_backdgroundPath.Stroke = Path.Stroke;
			_backgroundPath.Stroke = new SolidColorBrush(Invert(((SolidColorBrush)Path.Stroke).Color, 130));
			_backgroundPath.StrokeThickness = Path.StrokeThickness * 1.7;
		}

		public void Update(Vector2 to) {
			this.Path.Data = CreateGeometry(from.GetPosition(), to);
		}

		public void Update() {
			if(to == null) {
				throw new Exception("to is null");
			}
			this.Path.Data = CreateGeometry(from.GetPosition(), to.GetPosition());
		}

		public void ClearBackground() {
			if(!MainCanvas.Children.Contains(_backgroundPath)) {
				return;
			}
			MainCanvas.Children.Remove(_backgroundPath);
		}

		public override string ToString() {
			return $"Conection: {from.Parent_ID}-{to?.Parent_ID ?? "None"}";
		}

		public Path CreatePath(Vector2 from, Vector2 to) {
			return new Path() {
				Data = CreateGeometry(from, to),
				Tag = new ConnectionPathFrameworkTag(this),
			};
		}

		public static Geometry CreateGeometry(Vector2 from, Vector2 to) {
			return Geometry.Parse(
				$"M {from.X},{from.Y} " +
				$"C {from.X + (to.X - from.X) * 0.5},{from.Y} " +
				$"{from.X + (to.X - from.X) * 0.5},{to.Y} " +
				$"{to.X},{to.Y}");
		}

	}
}
