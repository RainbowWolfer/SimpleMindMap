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

namespace MindMap.Entities.Elements {
	public abstract class Element: IPropertiesContainer {
		public abstract long TypeID { get; }
		public const long ID_Rectangle = 1;
		public const long ID_Ellipse = 2;
		public const long ID_Polygon = 3;

		public abstract string ID { get; protected set; }

		protected MindMapPage parent;

		protected ConnectionsFrame? connectionsFrame;
		protected Canvas MainCanvas => parent.MainCanvas;

		public abstract FrameworkElement Target { get; }
		public abstract IProperty Properties { get; }

		public Element(MindMapPage parent) {
			this.parent = parent;
		}

		protected string AssignID(Type type) => $"{type.Name} ({parent.elements.Count + 1})";
		protected string AssignID(string type) => $"{type.Trim()} ({parent.elements.Count + 1})";

		public string GetID() => ID;

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

		public void CreateConnectionsFrame() {
			connectionsFrame = new ConnectionsFrame(this.parent, this);
		}

		public ConnectionControl? GetConnectionControlByID(string id) {
			return connectionsFrame?.GetControlByID(id);
		}

		public void UpdateConnectionsFrame() {//also includes connected dots
			connectionsFrame?.UpdateConnections();
		}

		public void SetConnectionsFrameVisible(bool visible) => connectionsFrame?.SetVisible(visible);

		public List<ConnectionControl> GetAllConnectionDots() => connectionsFrame == null ? new List<ConnectionControl>() : connectionsFrame.AllDots;

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
							parent.editHistory.SubmitByElementPropertyDelayedChanged(this, oldP, newP, "Background Color");
						})
					),
					PropertiesPanel.ColorInput("Border Color", border.BorderColor,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							border.BorderColor = new SolidColorBrush(args.NewValue);
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(this, oldP, newP, "Border Color");
						})
					),
					PropertiesPanel.SliderInput("Border Thickness", border.BorderThickness.Left, 0, 5,
						value => border.BorderThickness = new Thickness(value.NewValue)
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
							parent.editHistory.SubmitByElementPropertyChanged(this, oldP, newP, "Font Family");
						}), FontsList.AvailableFonts),
					PropertiesPanel.ComboSelector("Font Weight", text.FontWeight,
						value => IPropertiesContainer.PropertyChangedHandler(this, ()=>{
							text.FontWeight = value.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyChanged(this, oldP, newP, "Font Weight");
						})
					, FontsList.AllFontWeights),
					PropertiesPanel.SliderInput("Font Size", text.FontSize, 5, 42,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							text.FontSize = args.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(this, oldP, newP, "Font Size");
						}), 1, 0),
					PropertiesPanel.ColorInput("Font Color", text.FontColor,
						args => IPropertiesContainer.PropertyChangedHandler(this, () => {
							text.FontColor = args.NewValue;
						}, (oldP, newP) => {
							parent.editHistory.SubmitByElementPropertyDelayedChanged(this, oldP, newP, "Font Color");
						})
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
			parent.RemoveElement(this);
			connectionsFrame?.ClearConnections();
			parent.UpdateCount();
			if(submitEditHistory) {
				parent.editHistory.SubmitByElementDeleted(this);
			}
		}
	}
}
