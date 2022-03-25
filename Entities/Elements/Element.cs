using MindMap.Entities.Elements.Interfaces;
using MindMap.Entities.Frames;
using MindMap.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;
using MindMap.Entities.Properties;
using MindMap.Entities.Connections;
using MindMap.Entities.Identifications;
using MindMap.Entities.Services;
using MindMap.Entities.Tags;
using MindMap.Entities.Interactions;
using System.Windows.Input;

namespace MindMap.Entities.Elements {
	public abstract class Element: IPropertiesContainer, IIdentityContainer, IInteractive {
		public abstract long TypeID { get; }
		public const long ID_Rectangle = 1;
		public const long ID_Ellipse = 2;
		public const long ID_Polygon = 3;
		public const long ID_Image = 4;

		public const double MIN_SIZE = 30;

		public abstract string ElementTypeName { get; }

		public Identity Identity { get; set; }

		protected MindMapPage parent;

		public ConnectionsFrame? ConnectionsFrame { get; protected set; }
		protected Canvas MainCanvas => parent.MainCanvas;

		public abstract FrameworkElement Target { get; }
		public abstract IProperty Properties { get; }

		//public bool Handled { get; set; } = false;

		public Element(MindMapPage parent, Identity? identity = null) {
			this.parent = parent;
			Identity = identity ?? new Identity(IntializeID(GetType()), InitializeDefaultName());
			Identity.OnNameChanged += (n, o) => parent.UpdateHistoryListView();
			Target.Tag = new ElementFrameworkTag(this);
			Debug();
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
			return $"{ElementTypeName} ({new Random().Next()})";
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

		public virtual Vector2 DefaultSize => new(150, 150);

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
			ConnectionsFrame = new ConnectionsFrame(this.parent, this, initialControls);
		}

		public List<ConnectionPath> GetRelatedPaths() {
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

		public void SetConnectionsFrameVisible(bool visible) => ConnectionsFrame?.SetVisible(visible);

		public List<ConnectionControl> GetAllConnectionDots() {
			return ConnectionsFrame == null ? new List<ConnectionControl>() : ConnectionsFrame.AllDots;
		}

		public virtual void CreateFlyoutMenu() {
			FlyoutMenu.CreateBase(Target, (s, e) => Delete());
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
					})
				);
				var pro2 = PropertiesPanel.ColorInput("Border Color", border.BorderColor,
					args => IPropertiesContainer.PropertyChangedHandler(container, () => {
						border.BorderColor = new SolidColorBrush(args.NewValue);
					}, (oldP, newP) => {
						editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, container, oldP, newP, "Border Color");
					})
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
			parent.RemoveElement(this, submitEditHistory);
		}
	}
}
