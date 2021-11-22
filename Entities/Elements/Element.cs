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
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class Element {
		public abstract long TypeID { get; }
		public const long ID_Rectangle = 1;
		public const long ID_Ellipse = 2;

		protected MindMapPage parent;

		protected ConnectionsFrame? connectionsFrame;
		protected Canvas MainCanvas => parent.MainCanvas;

		public abstract FrameworkElement Target { get; }
		public abstract IProperty Properties { get; }

		public Element(MindMapPage parent) {
			this.parent = parent;
		}

		public Vector2 GetSize => new(Target.Width, Target.Height);
		public void SetSize(Vector2 size) {
			Target.Width = size.X;
			Target.Height = size.Y;
		}
		public Vector2 GetPosition() => new(Canvas.GetLeft(Target), Canvas.GetTop(Target));
		public void SetPosition(Vector2 position) {
			Canvas.SetLeft(Target, position.X);
			Canvas.SetTop(Target, position.Y);
		}
		public void CreateConnectionsFrame() {
			connectionsFrame = new ConnectionsFrame(this.parent, this);
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
				//grid.FontFamily = null;
				panels.AddRange(new Panel[] {
					PropertiesPanel.SectionTitle("Border"),
					PropertiesPanel.ColorInput("Background Color", border.Background,
						color => border.Background = new SolidColorBrush(color)
					),
					PropertiesPanel.ColorInput("Border Color", border.BorderColor,
						color => border.BorderColor = new SolidColorBrush(color)
					),
					PropertiesPanel.SliderInput("Border Thickness", border.BorderThickness.Left, 0, 5,
						value => border.BorderThickness = new Thickness(value)
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
						value => text.FontWeight=value
					, FontsList.AllFontWeights),
					PropertiesPanel.SliderInput("Font Size", text.FontSize, 5, 42,
						value => text.FontSize = value
					, 1, 0),
					PropertiesPanel.ColorInput("Font Color", text.FontColor,
						color => text.FontColor = color
					),
				});
			}
			return panels;
		}
		public abstract Panel CreateElementProperties();

		public abstract void UpdateStyle();

		public abstract void DoubleClick();
		public abstract void LeftClick();
		public abstract void MiddleClick();
		public abstract void RightClick();
		public abstract void Deselect();

		public virtual void Delete() {
			parent.RemoveElement(this);
			connectionsFrame?.ClearConnections();
		}
	}
}
