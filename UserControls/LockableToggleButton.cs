using System.Windows;
using System.Windows.Controls.Primitives;

namespace MindMap.UserControls {
	public class LockableToggleButton: ToggleButton {
		protected override void OnToggle() {
			if(!LockToggle) {
				base.OnToggle();
			}
		}

		public bool LockToggle {
			get => (bool)GetValue(LockToggleProperty);
			set {
				SetValue(LockToggleProperty, value);
			}
		}

		public static readonly DependencyProperty LockToggleProperty =
			DependencyProperty.Register("LockToggle", typeof(bool), typeof(LockableToggleButton), new PropertyMetadata(false));
	}
}
