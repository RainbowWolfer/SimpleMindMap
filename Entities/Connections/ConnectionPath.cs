using MindMap.Entities.Elements;
using MindMap.Entities.Frames;
using MindMap.Entities.Icons;
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
			public Color strokeColor = Colors.ForestGreen;
			public DuoNumber dashStroke = new DuoNumber(5, 0);
			public int extendLengthPercentage = 50;
			public PathType pathType = PathType.Linear;

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
			//this.Path = CreatePath(from.GetPosition(), to.GetPosition(), from.Direction);
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
			//this.Path = CreatePath(from.GetPosition(), to, from.Direction);
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

		private Vector2 lastMousePosition;
		private bool isRightClick;
		private bool isLeftClick;

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
			panel.Children.Add(PropertiesPanel.StackIconsSelector("Path Type", pathTypesData, 0,
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
			return panel;
		}

		public void UpdateStyle() {
			//Path.Style = PathStyle;
			Path.StrokeThickness = StrokeThickess;
			Path.Stroke = new SolidColorBrush(StrokeColor);
			Path.StrokeDashArray = new DoubleCollection(DashStroke.ToArray());
			UpdateBackgroundStyle();
		}

		public void UpdateBackgroundStyle() {
			_backgroundPath.Data = Path.Data;
			//_backdgroundPath.Stroke = Path.Stroke;
			_backgroundPath.Stroke = new SolidColorBrush(Invert(((SolidColorBrush)Path.Stroke).Color, 130));
			_backgroundPath.StrokeThickness = Path.StrokeThickness * 1.7;
		}

		public void Update(Vector2 to, ConnectionControl? target) {
			this.Path.Data = CreateGeometry(new ConnectionDotInfo(from), target == null ? new ConnectionDotInfo(to) : new ConnectionDotInfo(target));
			UpdateBackgroundStyle();
		}

		public void Update() {
			if(to == null) {
				throw new Exception("to is null");
			}
			this.Path.Data = CreateGeometry(new ConnectionDotInfo(from), new ConnectionDotInfo(to));
			UpdateBackgroundStyle();
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

		//private Path CreatePath(Vector2 from, Vector2 to, Direction direction) {
		//	return new Path() {
		//		Data = CreateLinearGeometry(from, to, direction),
		//		Tag = new ConnectionPathFrameworkTag(this),
		//	};
		//}

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
			const int MIN_GAP = 40;
			List<LineSegment> segments = new();
			if(from.Direction == Direction.Left) {
				if(!to.IsExisted || to.Direction == Direction.Right) {// Default & Left -> Right
					if(from.X - MIN_GAP < to.X + MIN_GAP) {
						double mid = from.Y + (to.Y - from.Y) / 2;
						if(from.Y > to.Y) {
							if(from.Y - from.Height / 2 - MIN_GAP < mid
								|| to.Y + to.Height / 2 + MIN_GAP > mid) {
								double toEdge = to.Y + to.Height / 2 + MIN_GAP;
								double fromEdge = from.Y - from.Height / 2 - MIN_GAP;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(from.X - MIN_GAP, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X + MIN_GAP, toEdge));
								segments.Add(NewLine(to.X + MIN_GAP, to.Y));
							} else {
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(from.X - MIN_GAP, mid));
								segments.Add(NewLine(to.X + MIN_GAP, mid));
								segments.Add(NewLine(to.X + MIN_GAP, to.Y));
							}
						} else {
							if(from.Y + from.Height / 2 + MIN_GAP > mid
								|| to.Y - to.Height / 2 - MIN_GAP < mid) {
								double toEdge = to.Y - to.Height / 2 - MIN_GAP;
								double fromEdge = from.Y + from.Height / 2 + MIN_GAP;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(from.X - MIN_GAP, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X + MIN_GAP, toEdge));
								segments.Add(NewLine(to.X + MIN_GAP, to.Y));
							} else {
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(from.X - MIN_GAP, mid));
								segments.Add(NewLine(to.X + MIN_GAP, mid));
								segments.Add(NewLine(to.X + MIN_GAP, to.Y));
							}
						}
					} else {
						double joint = from.X + (to.X - from.X) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Left) {// Left -> Left
					double topEdge = to.Y - to.Height / 2 - MIN_GAP;
					double botEdge = to.Y + to.Height / 2 + MIN_GAP;
					if(topEdge < from.Y && from.Y < botEdge) {
						double joint;
						double edge;
						if(from.X > to.X) {
							joint = Math.Min(to.X, from.X) - MIN_GAP;
							if(from.Y > to.Y) {
								edge = to.Y + to.Height / 2 + MIN_GAP;
							} else {
								edge = to.Y - to.Height / 2 - MIN_GAP;
							}
						} else {
							joint = to.X - MIN_GAP;
							if(from.Y < to.Y) {
								edge = from.Y + from.Height / 2 + MIN_GAP;
							} else {
								edge = from.Y - from.Height / 2 - MIN_GAP;
							}
						}
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, edge));
						segments.Add(NewLine(joint, edge));
						segments.Add(NewLine(joint, to.Y));
					} else {
						double joint = Math.Min(to.X, from.X) - MIN_GAP;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Top) {// Left -> Top
					if(to.Y - MIN_GAP < from.Y) {
						if(from.Y > to.Y + to.Height + MIN_GAP
							&& from.X - MIN_GAP < to.X + to.Width / 2 + MIN_GAP
							&& from.X - MIN_GAP > to.X - to.Width / 2 - MIN_GAP) {
							if(from.X - MIN_GAP > to.X) {
								double joint_height = to.Y + to.Height + MIN_GAP;
								double joint_width = to.X + to.Width / 2 + MIN_GAP;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(from.X - MIN_GAP, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							} else {
								double joint_width = to.X - to.Width / 2 - MIN_GAP;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							}
						} else {
							segments.Add(NewLine(from.X - MIN_GAP, from.Y));
							segments.Add(NewLine(from.X - MIN_GAP, to.Y - MIN_GAP));
							segments.Add(NewLine(to.X, to.Y - MIN_GAP));
						}
					} else if(from.X - MIN_GAP < to.X) {
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, to.Y - MIN_GAP));
						segments.Add(NewLine(to.X, to.Y - MIN_GAP));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else if(to.Direction == Direction.Bottom) {// Left -> Bottom
					if(to.Y + MIN_GAP > from.Y) {
						if(from.Y < to.Y - to.Height - MIN_GAP
							&& from.X - MIN_GAP < to.X + to.Width / 2 + MIN_GAP
							&& from.X - MIN_GAP > to.X - to.Width / 2 - MIN_GAP) {
							if(from.X - MIN_GAP > to.X) {
								double joint_height = to.Y - to.Height - MIN_GAP;
								double joint_width = to.X + to.Width / 2 + MIN_GAP;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(from.X - MIN_GAP, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							} else {
								double joint_width = to.X - to.Width / 2 - MIN_GAP;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							}
						} else {
							segments.Add(NewLine(from.X - MIN_GAP, from.Y));
							segments.Add(NewLine(from.X - MIN_GAP, to.Y + MIN_GAP));
							segments.Add(NewLine(to.X, to.Y + MIN_GAP));
						}
					} else if(from.X - MIN_GAP < to.X) {
						segments.Add(NewLine(from.X - MIN_GAP, from.Y));
						segments.Add(NewLine(from.X - MIN_GAP, to.Y + MIN_GAP));
						segments.Add(NewLine(to.X, to.Y + MIN_GAP));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else if(from.Direction == Direction.Right) {
				if(!to.IsExisted || to.Direction == Direction.Left) {// Default & Right -> Left
					if(from.X + MIN_GAP > to.X - MIN_GAP) {
						double mid = from.Y + (to.Y - from.Y) / 2;
						if(from.Y > to.Y) {
							if(from.Y - from.Height / 2 - MIN_GAP < mid
								|| to.Y + to.Height / 2 + MIN_GAP > mid) {
								double toEdge = to.Y + to.Height / 2 + MIN_GAP;
								double fromEdge = from.Y - from.Height / 2 - MIN_GAP;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(from.X + MIN_GAP, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X - MIN_GAP, toEdge));
								segments.Add(NewLine(to.X - MIN_GAP, to.Y));
							} else {
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(from.X + MIN_GAP, mid));
								segments.Add(NewLine(to.X - MIN_GAP, mid));
								segments.Add(NewLine(to.X - MIN_GAP, to.Y));
							}
						} else {
							if(from.Y + from.Height / 2 + MIN_GAP > mid
								|| to.Y - to.Height / 2 - MIN_GAP < mid) {
								double toEdge = to.Y - to.Height / 2 - MIN_GAP;
								double fromEdge = from.Y + from.Height / 2 + MIN_GAP;
								double mid_X = from.X + (to.X - from.X) / 2;
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(from.X + MIN_GAP, fromEdge));
								segments.Add(NewLine(mid_X, fromEdge));
								segments.Add(NewLine(mid_X, toEdge));
								segments.Add(NewLine(to.X - MIN_GAP, toEdge));
								segments.Add(NewLine(to.X - MIN_GAP, to.Y));
							} else {
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(from.X + MIN_GAP, mid));
								segments.Add(NewLine(to.X - MIN_GAP, mid));
								segments.Add(NewLine(to.X - MIN_GAP, to.Y));
							}
						}
					} else {
						double joint = from.X + (to.X - from.X) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Right) {// Right -> Right
					double topEdge = to.Y - to.Height / 2 - MIN_GAP;
					double botEdge = to.Y + to.Height / 2 + MIN_GAP;
					if(topEdge < from.Y && from.Y < botEdge) {
						double joint;
						double edge;
						if(from.X < to.X) {
							joint = Math.Max(to.X, from.X) + MIN_GAP;
							if(from.Y > to.Y) {
								edge = to.Y + to.Height / 2 + MIN_GAP;
							} else {
								edge = to.Y - to.Height / 2 - MIN_GAP;
							}
						} else {
							joint = to.X + MIN_GAP;
							if(from.Y < to.Y) {
								edge = from.Y + from.Height / 2 + MIN_GAP;
							} else {
								edge = from.Y - from.Height / 2 - MIN_GAP;
							}
						}
						segments.Add(NewLine(from.X + MIN_GAP, from.Y));
						segments.Add(NewLine(from.X + MIN_GAP, edge));
						segments.Add(NewLine(joint, edge));
						segments.Add(NewLine(joint, to.Y));
					} else {
						double joint = Math.Max(to.X, from.X) + MIN_GAP;
						segments.Add(NewLine(joint, from.Y));
						segments.Add(NewLine(joint, to.Y));
					}
				} else if(to.Direction == Direction.Top) {// Right -> Top
					if(to.Y - MIN_GAP < from.Y) {
						if(from.Y > to.Y + to.Height + MIN_GAP
							&& from.X + MIN_GAP > to.X - to.Width / 2 - MIN_GAP
							&& from.X + MIN_GAP < to.X + to.Width / 2 + MIN_GAP) {
							if(from.X + MIN_GAP < to.X) {
								double joint_height = to.Y + to.Height + MIN_GAP;
								double joint_width = to.X - to.Width / 2 - MIN_GAP;
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(from.X + MIN_GAP, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							} else {
								double joint_width = to.X + to.Width / 2 + MIN_GAP;
								segments.Add(NewLine(from.X - MIN_GAP, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							}
						} else {
							segments.Add(NewLine(from.X + MIN_GAP, from.Y));
							segments.Add(NewLine(from.X + MIN_GAP, to.Y - MIN_GAP));
							segments.Add(NewLine(to.X, to.Y - MIN_GAP));
						}
					} else if(from.X + MIN_GAP > to.X) {
						segments.Add(NewLine(from.X + MIN_GAP, from.Y));
						segments.Add(NewLine(from.X + MIN_GAP, to.Y - MIN_GAP));
						segments.Add(NewLine(to.X, to.Y - MIN_GAP));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else if(to.Direction == Direction.Bottom) {// Right -> Bottom
					if(to.Y + MIN_GAP > from.Y) {
						if(from.Y < to.Y - to.Height - MIN_GAP
							&& from.X + MIN_GAP > to.X - to.Width / 2 - MIN_GAP
							&& from.X + MIN_GAP < to.X + to.Width / 2 + MIN_GAP) {
							if(from.X + MIN_GAP < to.X) {
								double joint_height = to.Y - to.Height - MIN_GAP;
								double joint_width = to.X - to.Width / 2 - MIN_GAP;
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(from.X + MIN_GAP, joint_height));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(joint_width, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							} else {
								double joint_width = to.X + to.Width / 2 + MIN_GAP;
								segments.Add(NewLine(from.X + MIN_GAP, from.Y));
								segments.Add(NewLine(joint_width, from.Y));
								segments.Add(NewLine(joint_width, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							}
						} else {
							segments.Add(NewLine(from.X + MIN_GAP, from.Y));
							segments.Add(NewLine(from.X + MIN_GAP, to.Y + MIN_GAP));
							segments.Add(NewLine(to.X, to.Y + MIN_GAP));
						}
					} else if(from.X + MIN_GAP > to.X) {
						segments.Add(NewLine(from.X + MIN_GAP, from.Y));
						segments.Add(NewLine(from.X + MIN_GAP, to.Y + MIN_GAP));
						segments.Add(NewLine(to.X, to.Y + MIN_GAP));
					} else {
						segments.Add(NewLine(to.X, from.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else if(from.Direction == Direction.Top) {
				if(!to.IsExisted || to.Direction == Direction.Bottom) {// Default & Top -> Bottom
					if(from.Y - MIN_GAP < to.Y + MIN_GAP) {
						double mid = from.X + (to.X - from.X) / 2;
						if(from.X > to.X) {
							if(from.X - from.Width / 2 - MIN_GAP < mid
								|| to.X + to.Width / 2 + MIN_GAP > mid) {
								double toEdge = to.X + to.Width / 2 + MIN_GAP;
								double fromEdge = from.X - from.Width / 2 - MIN_GAP;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(fromEdge, from.Y - MIN_GAP));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							} else {
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(mid, from.Y - MIN_GAP));
								segments.Add(NewLine(mid, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							}
						} else {
							if(from.X + from.Width / 2 + MIN_GAP > mid
								|| to.X - to.Width / 2 - MIN_GAP < mid) {
								double toEdge = to.X - to.Width / 2 - MIN_GAP;
								double fromEdge = from.X + from.Width / 2 + MIN_GAP;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(fromEdge, from.Y - MIN_GAP));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							} else {
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(mid, from.Y - MIN_GAP));
								segments.Add(NewLine(mid, to.Y + MIN_GAP));
								segments.Add(NewLine(to.X, to.Y + MIN_GAP));
							}
						}
					} else {
						double joint = from.Y + (to.Y - from.Y) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Top) {// Top -> Top
					double leftEdge = to.X - to.Width / 2 - MIN_GAP;
					double rightEdge = to.X + to.Width / 2 + MIN_GAP;
					if(leftEdge < from.X && from.X < rightEdge) {
						double joint;
						double edge;
						if(from.Y > to.Y) {
							joint = Math.Min(to.Y, from.Y) - MIN_GAP;
							if(from.X > to.X) {
								edge = to.X + to.Width / 2 + MIN_GAP;
							} else {
								edge = to.X - to.Width / 2 - MIN_GAP;
							}
						} else {
							joint = to.Y - MIN_GAP;
							if(from.X < to.X) {
								edge = from.X + from.Width / 2 + MIN_GAP;
							} else {
								edge = from.X - from.Width / 2 - MIN_GAP;
							}
						}
						segments.Add(NewLine(from.X, from.Y - MIN_GAP));
						segments.Add(NewLine(edge, from.Y - MIN_GAP));
						segments.Add(NewLine(edge, joint));
						segments.Add(NewLine(to.X, joint));
					} else {
						double joint = Math.Min(to.Y, from.Y) - MIN_GAP;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Left) {// Top -> Left
					if(to.Y + MIN_GAP > from.Y) {
						if(to.Y > from.Y - from.Height - MIN_GAP
							&& to.X - MIN_GAP < from.X + from.Width / 2 + MIN_GAP
							&& to.X - MIN_GAP > from.X - from.Width / 2 - MIN_GAP) {
							if(to.X - MIN_GAP > from.X) {
								double joint_width = from.X + from.Width / 2 + MIN_GAP;
								double joint_height = from.Y + from.Height + MIN_GAP;
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X - MIN_GAP, joint_height));
								segments.Add(NewLine(to.X - MIN_GAP, to.Y));
							} else {
								double joint_width = from.X - from.Width / 2 - MIN_GAP;
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y - MIN_GAP));
							segments.Add(NewLine(to.X - MIN_GAP, from.Y - MIN_GAP));
							segments.Add(NewLine(to.X - MIN_GAP, to.Y));
						}
					} else if(from.X + MIN_GAP > to.X) {
						segments.Add(NewLine(from.X, from.Y - MIN_GAP));
						segments.Add(NewLine(to.X - MIN_GAP, from.Y - MIN_GAP));
						segments.Add(NewLine(to.X - MIN_GAP, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else if(to.Direction == Direction.Right) {// Top -> Right
					if(to.Y + MIN_GAP > from.Y) {
						if(to.Y > from.Y - from.Height - MIN_GAP
							&& to.X + MIN_GAP > from.X - from.Width / 2 - MIN_GAP
							&& to.X + MIN_GAP < from.X + from.Width / 2 + MIN_GAP) {
							if(to.X < from.X) {
								double joint_width = from.X - from.Width / 2 - MIN_GAP;
								double joint_height = from.Y + from.Height + MIN_GAP;
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X + MIN_GAP, joint_height));
								segments.Add(NewLine(to.X + MIN_GAP, to.Y));
							} else {
								double joint_width = from.X + from.Width / 2 + MIN_GAP;
								segments.Add(NewLine(from.X, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y - MIN_GAP));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y - MIN_GAP));
							segments.Add(NewLine(to.X + MIN_GAP, from.Y - MIN_GAP));
							segments.Add(NewLine(to.X + MIN_GAP, to.Y));
						}
					} else if(from.X - MIN_GAP < to.X) {
						segments.Add(NewLine(from.X, from.Y - MIN_GAP));
						segments.Add(NewLine(to.X + MIN_GAP, from.Y - MIN_GAP));
						segments.Add(NewLine(to.X + MIN_GAP, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else if(from.Direction == Direction.Bottom) {
				if(!to.IsExisted || to.Direction == Direction.Top) {// Default & Bottom -> Top
					if(from.Y + MIN_GAP > to.Y - MIN_GAP) {
						double mid = from.X + (to.X - from.X) / 2;
						if(from.X > to.X) {
							if(from.X - from.Width / 2 - MIN_GAP < mid
								|| to.X + to.Width / 2 + MIN_GAP > mid) {
								double toEdge = to.X + to.Width / 2 + MIN_GAP;
								double fromEdge = from.X - from.Width / 2 - MIN_GAP;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(fromEdge, from.Y + MIN_GAP));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							} else {
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(mid, from.Y + MIN_GAP));
								segments.Add(NewLine(mid, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							}
						} else {
							if(from.X + from.Width / 2 + MIN_GAP > mid
								|| to.X - to.Width / 2 - MIN_GAP < mid) {
								double toEdge = to.X - to.Width / 2 - MIN_GAP;
								double fromEdge = from.X + from.Width / 2 + MIN_GAP;
								double mid_Y = from.Y + (to.Y - from.Y) / 2;
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(fromEdge, from.Y + MIN_GAP));
								segments.Add(NewLine(fromEdge, mid_Y));
								segments.Add(NewLine(toEdge, mid_Y));
								segments.Add(NewLine(toEdge, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							} else {
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(mid, from.Y + MIN_GAP));
								segments.Add(NewLine(mid, to.Y - MIN_GAP));
								segments.Add(NewLine(to.X, to.Y - MIN_GAP));
							}
						}
					} else {
						double joint = from.Y + (to.Y - from.Y) * ExtendLengthPercentage / 100;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Bottom) {// Bottom -> Bottom
					double leftEdge = to.X - to.Width / 2 - MIN_GAP;
					double rightEdge = to.X + to.Width / 2 + MIN_GAP;
					if(leftEdge < from.X && from.X < rightEdge) {
						double joint;
						double edge;
						if(from.Y < to.Y) {
							joint = Math.Max(to.Y, from.Y) + MIN_GAP;
							if(from.X > to.X) {
								edge = to.X + to.Width / 2 + MIN_GAP;
							} else {
								edge = to.X - to.Width / 2 - MIN_GAP;
							}
						} else {
							joint = to.Y + MIN_GAP;
							if(from.X < to.X) {
								edge = from.X + from.Width / 2 + MIN_GAP;
							} else {
								edge = from.X - from.Width / 2 - MIN_GAP;
							}
						}
						segments.Add(NewLine(from.X, from.Y + MIN_GAP));
						segments.Add(NewLine(edge, from.Y + MIN_GAP));
						segments.Add(NewLine(edge, joint));
						segments.Add(NewLine(to.X, joint));
					} else {
						double joint = Math.Max(to.Y, from.Y) + MIN_GAP;
						segments.Add(NewLine(from.X, joint));
						segments.Add(NewLine(to.X, joint));
					}
				} else if(to.Direction == Direction.Left) {// Bottom -> Left
					if(to.Y - MIN_GAP < from.Y) {
						if(to.Y < from.Y + from.Height + MIN_GAP
							&& to.X - MIN_GAP < from.X + from.Width / 2 + MIN_GAP
							&& to.X - MIN_GAP > from.X - from.Width / 2 - MIN_GAP) {
							if(to.X - MIN_GAP > from.X) {
								double joint_width = from.X + from.Width / 2 + MIN_GAP;
								double joint_height = from.Y - from.Height - MIN_GAP;
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X - MIN_GAP, joint_height));
								segments.Add(NewLine(to.X - MIN_GAP, to.Y));
							} else {
								double joint_width = from.X - from.Width / 2 - MIN_GAP;
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y + MIN_GAP));
							segments.Add(NewLine(to.X - MIN_GAP, from.Y + MIN_GAP));
							segments.Add(NewLine(to.X - MIN_GAP, to.Y));
						}
					} else if(from.X + MIN_GAP > to.X) {
						segments.Add(NewLine(from.X, from.Y + MIN_GAP));
						segments.Add(NewLine(to.X - MIN_GAP, from.Y + MIN_GAP));
						segments.Add(NewLine(to.X - MIN_GAP, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else if(to.Direction == Direction.Right) {// Bottom -> Right
					if(to.Y - MIN_GAP < from.Y) {
						if(to.Y < from.Y + from.Height + MIN_GAP
							&& to.X + MIN_GAP > from.X - from.Width / 2 - MIN_GAP
							&& to.X + MIN_GAP < from.X + from.Width / 2 + MIN_GAP) {
							if(to.X + MIN_GAP < from.X) {
								double joint_width = from.X - from.Width / 2 - MIN_GAP;
								double joint_height = from.Y - from.Height - MIN_GAP;
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, joint_height));
								segments.Add(NewLine(to.X + MIN_GAP, joint_height));
								segments.Add(NewLine(to.X + MIN_GAP, to.Y));
							} else {
								double joint_width = from.X + from.Width / 2 + MIN_GAP;
								segments.Add(NewLine(from.X, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, from.Y + MIN_GAP));
								segments.Add(NewLine(joint_width, to.Y));
							}
						} else {
							segments.Add(NewLine(from.X, from.Y + MIN_GAP));
							segments.Add(NewLine(to.X + MIN_GAP, from.Y + MIN_GAP));
							segments.Add(NewLine(to.X + MIN_GAP, to.Y));
						}
					} else if(from.X - MIN_GAP < to.X) {
						segments.Add(NewLine(from.X, from.Y + MIN_GAP));
						segments.Add(NewLine(to.X + MIN_GAP, from.Y + MIN_GAP));
						segments.Add(NewLine(to.X + MIN_GAP, to.Y));
					} else {
						segments.Add(NewLine(from.X, to.Y));
					}
				} else {
					throw new DirectionNotFoundException(to.Direction);
				}
			} else {
				throw new DirectionNotFoundException(to.Direction);
			}

			List<PathSegment> paths = new();
			paths.AddRange(segments);
			paths.Add(new LineSegment(to.Position.ToPoint(), true));
			PathFigure lines = new PathFigure(from.Position.ToPoint(), paths, false);
			lines.IsFilled = false;
			//PathFigure pointer = new PathFigure(to.Position.ToPoint(), new PathSegment[] {

			//}, false);
			return new PathGeometry(new PathFigure[] {
				lines
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
