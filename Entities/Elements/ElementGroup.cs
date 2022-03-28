using MindMap.Entities.Identifications;
using MindMap.Entities.Properties;
using MindMap.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMap.Entities.Elements {
	public class ElementGroup: Element {

		public override long TypeID => throw new NotImplementedException();

		public override string ElementTypeName => throw new NotImplementedException();
		public override (string icon, string fontFamily) Icon => throw new NotImplementedException();

		public override FrameworkElement Target => throw new NotImplementedException();

		public override IProperty Properties => throw new NotImplementedException();


		public ElementGroup(MindMapPage parent, Identity? identity = null) : base(parent, identity) {
		}

		public override Panel CreateElementProperties() {
			throw new NotImplementedException();
		}

		public override void Deselect() {
			throw new NotImplementedException();
		}

		public override void DoubleClick() {
			throw new NotImplementedException();
		}

		public override void LeftClick(MouseButtonEventArgs e) {
			throw new NotImplementedException();
		}

		public override void MiddleClick() {
			throw new NotImplementedException();
		}

		public override void RightClick() {
			throw new NotImplementedException();
		}

		public override void SetFramework() {
			throw new NotImplementedException();
		}

		public override void SetProperty(IProperty property) {
			throw new NotImplementedException();
		}

		public override void SetProperty(string propertyJson) {
			throw new NotImplementedException();
		}

		protected override void UpdateStyle() {
			throw new NotImplementedException();
		}
	}
}
