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

/**
 * Everytime created a element, calculate the related connecton paths and store these in according element.
 * when deleled, store related paths into history
 * when redo, use these stored paths and reinitialized these.
 */
namespace MindMap.Entities.Elements {
	public abstract class Element: IPropertiesContainer, IIdentityContainer {
		public abstract long TypeID { get; }
		public const long ID_Rectangle = 1;
		public const long ID_Ellipse = 2;
		public const long ID_Polygon = 3;

		public abstract string ElementTypeName { get; }

		public Identity Identity { get; set; }

		protected MindMapPage parent;

		public ConnectionsFrame? ConnectionsFrame { get; protected set; }
		protected Canvas MainCanvas => parent.MainCanvas;

		public abstract FrameworkElement Target { get; }
		public abstract IProperty Properties { get; }

		public Element(MindMapPage parent, Identity? identity = null) {
			this.parent = parent;
			Identity = identity ?? new Identity(IntializeID(GetType()), InitializeDefaultName());
			Debug();
		}

		private async void Debug() {
			while(true) {
				ToolTipService.SetToolTip(Target, $"");
				await Task.Delay(100);
			}
		}

		private string IntializeID(Type type) => $"{type.Name}_({Methods.GetTick()})";
		private string InitializeDefaultName() => $"{ElementTypeName} ({parent.elements.Count + 1})";

		//protected string AssignID(string type) => $"{type.Trim()}_({Methods.GetTick()})";

		public Identity GetIdentity() => Identity;

		public Vector2 GetSize() => new(Target.Width, Target.Height);
		public void SetSize(Vector2 size) {
			Target.Width = size.X;
			Target.Height = size.Y;
		}
		public Vector2 GetPosition() => new(Canvas.GetLeft(Target), Canvas.GetTop(Target));
		public void SetPosition(Vector2 position) {
			Canvas.SetLeft(Target, position.X);
			Canvas.SetTop(Target, position.Y);
		}

		public virtual Vector2 DefaultSize => new(150, 150);

		public abstract void SetProperty(IProperty property);
		public abstract void SetProperty(string propertyJson);

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

		public List<ConnectionControl> GetAllConnectionDots() => ConnectionsFrame == null ? new List<ConnectionControl>() : ConnectionsFrame.AllDots;

		//public abstract FrameworkElement CreateFramework();

		public virtual void CreateFlyoutMenu() {
			FlyoutMenu.CreateBase(Target, (s, e) => Delete());
		}

		public List<Panel> CreatePropertiesList() {
			List<Panel> panels = new();
			panels.Add(CreateElementProperties());

			if(this is IBorderBasedStyle border) {
				panels.AddRange(new Panel[] {
					PropertiesPanel.SectionTitle("Border"),
					PropertiesPanel.ColorInput("Background Color", border.Background,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							border.Background = new SolidColorBrush(args.NewValue);
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Background Color");
						})
					),
					PropertiesPanel.ColorInput("Border Color", border.BorderColor,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							border.BorderColor = new SolidColorBrush(args.NewValue);
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Border Color");
						})
					),
					PropertiesPanel.SliderInput("Border Thickness", border.BorderThickness.Left, 0, 5,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							border.BorderThickness = new Thickness(args.NewValue);
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Border Thickness");
						})
					),
				});
			}
			if(this is ITextGrid text) {
				panels.AddRange(new Panel[] {
					PropertiesPanel.SectionTitle("Text"),
					PropertiesPanel.FontSelector("Font Family", text.FontFamily,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							if(args.NewValue == null){
								return;
							}
							text.FontFamily = args.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyChanged(TargetType.Element, this, oldP, newP, "Font Family");
						}), FontsList.AvailableFonts),
					PropertiesPanel.ComboSelector("Font Weight", text.FontWeight,
						value => IPropertiesContainer.PropertyChangedHandler(this, ()=>{
							text.FontWeight = value.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyChanged(TargetType.Element, this, oldP, newP, "Font Weight");
						})
					, FontsList.AllFontWeights),
					PropertiesPanel.SliderInput("Font Size", text.FontSize, 5, 42,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							text.FontSize = args.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Font Size");
						}), 1, 0),
					PropertiesPanel.ColorInput("Font Color", text.FontColor,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							text.FontColor = args.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(TargetType.Element, this, oldP, newP, "Font Color");
						}), ()=>{
							parent.editHistory.InstantSealLastDelayedChange();
						}
					),
				});
			}
			return panels;
		}



		public void SubmitPropertyChangedEditHistory(IProperty property) {
			//parent.editHistory.SubmitByElementPropertyChanged(this, (IProperty)property.Clone());
		}

		public abstract Panel CreateElementProperties();

		public abstract void SetFramework();

		protected abstract void UpdateStyle();

		public abstract void DoubleClick();
		public abstract void LeftClick();
		public abstract void MiddleClick();
		public abstract void RightClick();
		public abstract void Deselect();

		public virtual void Delete(bool submitEditHistory = true) {
			parent.RemoveElement(this, submitEditHistory);
		}
	}
}
