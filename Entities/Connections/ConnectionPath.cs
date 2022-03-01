using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Entities.Services;
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

		private struct Property: IProperty {
			public double strokeThickess;
			public Color strokeColor;

			public object Clone() {
				return MemberwiseClone();
			}

			public IProperty Translate(string json) {
				return JsonConvert.DeserializeObject<Property>(json);
			}
		}
		private Property property;
		public IProperty Properties => property;

		public Path Path { get; private set; }
		public bool IsSelected { get; private set; }
		private Path _backdgroundPath = new();

		public Style PathStyle {
			get {
				Style style = new(typeof(Path));
				style.Setters.Add(new Setter(Path.StrokeThicknessProperty, StrokeThickess));
				style.Setters.Add(new Setter(Path.StrokeProperty, new SolidColorBrush(StrokeColor)));
				return style;
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
			this.Initialize();
		}

		public ConnectionPath(MindMapPage parent, ConnectionControl from, Vector2 to) {
			this.IsPreview = true;
			this._parent = parent;
			this.from = from;
			this.to = null;
			this.Path = CreatePath(from.GetPosition(), to);
			this.Identity = new Identity($"Preview_Connection_({Methods.GetTick()})", "Preview Conncetion");
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
				_backdgroundPath = new() {
					Data = Path.Data,
					Visibility = Visibility.Collapsed,
				};
				MainCanvas.Children.Insert(MainCanvas.Children.IndexOf(Path), _backdgroundPath);
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
			_backdgroundPath.Visibility = Visibility.Visible;
			_backdgroundPath.StrokeDashArray = new DoubleCollection(new double[] { 2, 0.5 });
			UpdateBackgroundStyle();
		}

		public void MouseExit() {
			if(IsSelected) {
				return;
			}
			_backdgroundPath.Visibility = Visibility.Collapsed;
		}

		public void Select() {
			if(_connectionsManager != null) {
				_connectionsManager.CurrentSelection = this;
			}
			IsSelected = true;
			_backdgroundPath.Visibility = Visibility.Visible;
			_backdgroundPath.StrokeDashArray = new DoubleCollection(new double[] { 1, 0 });
			UpdateBackgroundStyle();
		}

		public void Deselect() {
			if(_connectionsManager != null) {
				_connectionsManager.CurrentSelection = null;
			}
			IsSelected = false;
			_backdgroundPath.Visibility = Visibility.Collapsed;
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
			return panel;
		}

		public void UpdateStyle() {
			//Path.Style = PathStyle;
			Path.StrokeThickness = StrokeThickess;
			Path.Stroke = new SolidColorBrush(StrokeColor);
		}

		public void UpdateBackgroundStyle() {
			_backdgroundPath.Data = Path.Data;
			_backdgroundPath.Stroke = Path.Stroke;
			_backdgroundPath.Stroke = new SolidColorBrush(Invert(((SolidColorBrush)Path.Stroke).Color));
			_backdgroundPath.StrokeThickness = Path.StrokeThickness * 1.7;
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
			if(!MainCanvas.Children.Contains(_backdgroundPath)) {
				return;
			}
			MainCanvas.Children.Remove(_backdgroundPath);
		}

		public override string ToString() {
			return $"Conection: {from.Parent_ID}-{to?.Parent_ID ?? "None"}";
		}

		public static Path CreatePath(Vector2 from, Vector2 to) {
			return new Path() {
				Data = CreateGeometry(from, to),
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
