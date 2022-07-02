using MindMap.Entities.Connections;
using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Entities.Icons;
using MindMap.Entities.Identifications;
using MindMap.Entities.Interactions;
using MindMap.Entities.Locals;
using MindMap.Entities.Properties;
using MindMap.Entities.Services;
using MindMap.Entities.Tags;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MindMap.Entities.Elements {
	public abstract class Element: IPropertiesContainer, IIdentityContainer, IInteractive {
		public abstract long TypeID { get; }

		public const double MIN_SIZE = 30;

		public abstract string ElementTypeName { get; }
		public abstract (string icon, string fontFamily) Icon { get; }

		public Identity Identity { get; set; }

		protected MindMapPage? parent;
		private bool isLocked = false;

		public ConnectionsFrame? ConnectionsFrame { get; protected set; }
		//protected Canvas? MainCanvas => parent?.MainCanvas;

		//public bool FullFunction => MainCanvas != null && parent != null;

		public abstract FrameworkElement Target { get; }

		public abstract IProperty Properties { get; }

		private readonly MenuItem menuItem_lock;

		public bool IsLocked {
			get => isLocked;
		}

		public void SetLocked(bool value, bool submitHistory = true) {
			if(parent == null) {
				throw BeyondLimitException;
			}
			if(submitHistory) {
				parent.editHistory.SubmitByElementLockStateChange(this, IsLocked, value);
			}
			isLocked = value;
			if(value) {
				menuItem_lock.Header = "Unlock";
				menuItem_lock.Icon = new FontIcon("\uE785", 14).Generate();
				Deselect();
				parent.Deselect();
				parent.ClearResizePanel();
			} else {
				menuItem_lock.Header = "Lock";
				menuItem_lock.Icon = new FontIcon("\uE72E", 14).Generate();
			}
		}

		public Element(MindMapPage? parent, Identity? identity = null) {
			this.parent = parent;
			Identity = identity ?? new Identity(IntializeID(GetType()), InitializeDefaultName());
			Identity.OnNameChanged += (n, o) => parent?.UpdateHistoryListView();
			Target.Tag = new ElementFrameworkTag(this);
			Debug();

			menuItem_lock = new() {
				Header = "Lock",
				Icon = new FontIcon("\uE72E", 14).Generate(),
			};
			menuItem_lock.Click += (s, e) => {
				SetLocked(!IsLocked);
			};
		}

		private async void Debug() {
			while(true) {
				ToolTipService.SetToolTip(Target, $"{Identity}");
				await Task.Delay(100);
			}
		}

		private string IntializeID(Type type) {
			return $"{type.Name}_({Methods.GetTick()})";
		}

		private string InitializeDefaultName() {
			return $"{ElementTypeName} ({new Random().Next(1, 100)})";
		}

		public Identity GetIdentity() => Identity;

		public Vector2 GetSize() => new(Target.Width, Target.Height);
		public void SetSize(Vector2 size) {
			size = size.Bound(new Vector2(MIN_SIZE, MIN_SIZE), Vector2.Max);
			Target.Width = size.X;
			Target.Height = size.Y;
		}
		public Vector2 GetPosition() => new(Canvas.GetLeft(Target), Canvas.GetTop(Target));
		public void SetPosition(Vector2 position) {
			Canvas.SetLeft(Target, position.X);
			Canvas.SetTop(Target, position.Y);
		}

		public Vector2[] GetBoundPoints() {
			Vector2 pos = GetPosition();
			Vector2 size = GetSize();
			return new Vector2[4]{
				new(pos.X, pos.Y),
				new(pos.X + size.X, pos.Y),
				new(pos.X, pos.Y + size.Y),
				new(pos.X + size.X, pos.Y + size.Y),
			};
		}

		public virtual Vector2 DefaultSize {
			get {
				if(AppSettings.Current == null) {
					return new Vector2(150, 150);
				} else {
					return new Vector2(AppSettings.Current.ElementDefaultHeight);
				}
			}
		}

		public abstract void SetProperty(IProperty property);
		public abstract void SetProperty(string propertyJson);

		public List<FrameworkElement> GetRelatedFrameworks() {
			List<FrameworkElement> result = new() { Target };
			if(ConnectionsFrame != null) {
				result.AddRange(ConnectionsFrame.AllDots.Select(d => d.target));
			}
			return result;
		}

		public void CreateConnectionsFrame(ControlsInfo? initialControls = null) {
			if(parent == null) {
				throw BeyondLimitException;
			}
			ConnectionsFrame = new ConnectionsFrame(this.parent, this, initialControls);
		}

		public List<ConnectionPath> GetRelatedPaths() {
			if(parent == null) {
				throw BeyondLimitException;
			}
			if(ConnectionsFrame == null) {
				return new();
			}
			return parent.connectionsManager.CalculateRelatedConnections(ConnectionsFrame);
		}

		public ConnectionControl? GetConnectionControlByID(string id) {
			return ConnectionsFrame?.GetControlByID(id);
		}

		public void UpdateConnectionsFrame() {//also includes connected dots
			ConnectionsFrame?.UpdateConnections();
		}

		public void SetConnectionsFrameVisible(bool visible) {
			return;
			//ConnectionsFrame?.SetVisible(visible);
		}

		public List<ConnectionControl> GetAllConnectionDots() {
			return ConnectionsFrame == null ? new List<ConnectionControl>() : ConnectionsFrame.AllDots;
		}

		public virtual void CreateFlyoutMenu() {
			FlyoutMenu.CreateBase(Target, (s, e) => Delete());
			Target.ContextMenu.Items.Add(menuItem_lock);
			MenuItem item_addPreset = new MenuItem() {
				Header = "Add to preset",
				Icon = new FontIcon("\uE109", 14).Generate(),
			};
			item_addPreset.Click += (s, e) => AddToPreset();
			Target.ContextMenu.Items.Add(item_addPreset);
		}

		public static List<Panel> CreatePropertiesList(IPropertiesContainer container, EditHistory editHistory) {
			List<Panel> panels = new();
			if(container is IBorderBasedStyle border) {
				var title = PropertiesPanel.SectionTitle("Border");
				var pro1 = PropertiesPanel.ColorInput("Background Color", border.Background,
					args => IPropertiesContainer.PropertyChangedHandler(container, () => {
						border.Background = new SolidColorBrush(args.NewValue);
					}, (oldP, newP) => {
						editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Background Color");
					}), () => {
						editHistory.InstantSealLastDelayedChange();
					}
				);
				var pro2 = PropertiesPanel.ColorInput("Border Color", border.BorderColor,
					args => IPropertiesContainer.PropertyChangedHandler(container, () => {
						border.BorderColor = new SolidColorBrush(args.NewValue);
					}, (oldP, newP) => {
						editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Border Color");
					}), () => {
						editHistory.InstantSealLastDelayedChange();
					}
				);
				var pro3 = PropertiesPanel.SliderInput("Border Thickness", border.BorderThickness.Left, 0, 5,
					args => IPropertiesContainer.PropertyChangedHandler(container, () => {
						border.BorderThickness = new Thickness(args.NewValue);
					}, (oldP, newP) => {
						editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Border Thickness");
					})
				);
				panels.AddRange(new Panel[] { title, pro1, pro2, pro3, });
			}
			if(container is ITextContainer text) {
				panels.AddRange(new Panel[] {
					PropertiesPanel.SectionTitle("Text"),
					PropertiesPanel.FontSelector("Font Family", text.FontFamily,
						args => IPropertiesContainer.PropertyChangedHandler(container, () => {
							if(args.NewValue == null){
								return;
							}
							text.FontFamily = args.NewValue;
						}, (oldP, newP) => {
							editHistory.SubmitByElementPropertyChanged(TargetType.Element, container, oldP, newP, "Font Family");
						}), FontsList.AvailableFonts),
					PropertiesPanel.ComboSelector("Font Weight", text.FontWeight,
						value => IPropertiesContainer.PropertyChangedHandler(container, ()=>{
							text.FontWeight = value.NewValue;
						}, (oldP, newP) => {
							editHistory.SubmitByElementPropertyChanged(TargetType.Element, container, oldP, newP, "Font Weight");
						})
					, FontsList.AllFontWeights),
					PropertiesPanel.SliderInput("Font Size", text.FontSize, 5, 42,
						args => IPropertiesContainer.PropertyChangedHandler(container, () => {
							text.FontSize = args.NewValue;
						}, (oldP, newP) => {
							editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Font Size");
						}), 1, 0),
					PropertiesPanel.ColorInput("Font Color", text.FontColor,
						args => IPropertiesContainer.PropertyChangedHandler(container, () => {
							text.FontColor = args.NewValue;
						}, (oldP, newP) => {
							editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Font Color");
						}), ()=>{
							editHistory.InstantSealLastDelayedChange();
						}
					),
				});
			}
			if(container is IGridShadow shadow) {
				StackPanel title = PropertiesPanel.SectionTitle("Grid Shadow");
				var pro1 = PropertiesPanel.DuoColumnIconsSelector("Enable Grid Shadow", shadow.EnableShadow, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					shadow.EnableShadow = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyChanged(TargetType.Element, container, oldP, newP, "Enable Shadow");
				}));
				var pro2 = PropertiesPanel.SliderInput("Blur Radius", shadow.ShadowBlurRadius, 0, 40, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					shadow.ShadowBlurRadius = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Blur Radius");
				}), 1, 0);

				var pro3 = PropertiesPanel.SliderInput("Shadow Depth", shadow.ShadowDepth, 0, 40, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					shadow.ShadowDepth = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Shadow Depth");
				}), 1, 0);

				var pro4 = PropertiesPanel.SliderInput("Shadow Direction", shadow.ShadowDirection, 0, 360, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					shadow.ShadowDirection = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Shadow Direction");
				}), 1, 0);

				var pro5 = PropertiesPanel.ColorInput("Shadow Color", shadow.ShadowColor,
					args => IPropertiesContainer.PropertyChangedHandler(container, () => {
						shadow.ShadowColor = args.NewValue;
					}, (oldP, newP) => {
						editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Shadow Color");
					}), () => {
						editHistory.InstantSealLastDelayedChange();
					}
				);

				var pro6 = PropertiesPanel.SliderInput("Shadow Opacity", shadow.ShadowOpacity, 0, 1, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					shadow.ShadowOpacity = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Shadow Opacity");
				}), 0.05, 2);

				panels.AddRange(new Panel[] { title, pro1, pro2, pro3, pro4, pro5, pro6 });
			}
			if(container is ITextShadow textShadow) {
				StackPanel title = PropertiesPanel.SectionTitle("Text Shadow");
				var pro1 = PropertiesPanel.DuoColumnIconsSelector("Enable Text Shadow", textShadow.EnableTextShadow, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					textShadow.EnableTextShadow = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyChanged(TargetType.Element, container, oldP, newP, "Enable Text Shadow");
				}));
				var pro2 = PropertiesPanel.SliderInput("Text Blur Radius", textShadow.TextShadowBlurRadius, 0, 40, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					textShadow.TextShadowBlurRadius = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Text Blur Radius");
				}), 1, 0);

				var pro3 = PropertiesPanel.SliderInput("Text Shadow Depth", textShadow.TextShadowDepth, 0, 40, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					textShadow.TextShadowDepth = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Text Shadow Depth");
				}), 1, 0);

				var pro4 = PropertiesPanel.SliderInput("Text Shadow Direction", textShadow.TextShadowDirection, 0, 360, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					textShadow.TextShadowDirection = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Text Shadow Direction");
				}), 1, 0);

				var pro5 = PropertiesPanel.ColorInput("Text Shadow Color", textShadow.TextShadowColor,
					args => IPropertiesContainer.PropertyChangedHandler(container, () => {
						textShadow.TextShadowColor = args.NewValue;
					}, (oldP, newP) => {
						editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Text Shadow Color");
					}), () => {
						editHistory.InstantSealLastDelayedChange();
					}
				);

				var pro6 = PropertiesPanel.SliderInput("Text Shadow Opacity", textShadow.TextShadowOpacity, 0, 1, args => IPropertiesContainer.PropertyChangedHandler(container, () => {
					textShadow.TextShadowOpacity = args.NewValue;
				}, (oldP, newP) => {
					editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Text Shadow Opacity");
				}), 0.05, 2);

				panels.AddRange(new Panel[] { title, pro1, pro2, pro3, pro4, pro5, pro6 });
			}
			return panels;
		}

		public abstract Panel CreateElementProperties();

		public abstract void SetFramework();

		protected abstract void UpdateStyle();

		public abstract void DoubleClick();
		public abstract void LeftClick(MouseButtonEventArgs e);
		public abstract void MiddleClick();
		public abstract void RightClick();
		public abstract void Deselect();

		public virtual void Delete(bool submitEditHistory = true) {
			if(parent == null) {
				throw BeyondLimitException;
			}
			parent.RemoveElement(this, submitEditHistory);
		}

		public virtual void AddToPreset() {
			if(parent == null) {
				throw BeyondLimitException;
			}
			parent.CallAddToPresetDialog(this);
		}

		protected static Exception BeyondLimitException => new Exception("Parent and canvas is null, it is not supposed to be used in such functions.");

	}
}
