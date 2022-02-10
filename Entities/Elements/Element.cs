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
	public abstract class Element {
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
						color => {
							//previewing = true;
							border.Background = new SolidColorBrush(color);
						},
						valueBefore => {
							//previewing = false;
							var scb = (SolidColorBrush)border.Background;
							if(scb.Color != valueBefore) {
								var p =Properties.Clone();
								SubmitPropertyChangedEditHistory(Properties);
							}
						}
					),
					PropertiesPanel.ColorInput("Border Color", border.BorderColor,
						color => border.BorderColor = new SolidColorBrush(color),
						valueBefore => {

						}
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
						value => text.FontFamily = value
					, FontsList.AvailableFonts),
					PropertiesPanel.ComboSelector("Font Weight", text.FontWeight,
						value => text.FontWeight = value
					, FontsList.AllFontWeights),
					PropertiesPanel.SliderInput("Font Size", text.FontSize, 5, 42,
						args => {
							IProperty oldProperty = IProperty.MakeClone(Properties);
							text.FontSize = args.NewValue;
							IProperty newProperty = IProperty.MakeClone(Properties);
							parent.editHistory.SubmitByElementPropertyDelayedChanged(this, oldProperty, newProperty);
						}, 1, 0),
					PropertiesPanel.ColorInput("Font Color", text.FontColor,
						color => text.FontColor = color
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
