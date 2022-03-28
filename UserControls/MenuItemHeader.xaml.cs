using System.Windows.Controls;

namespace MindMap.UserControls {
	public partial class MenuItemHeader: UserControl {
		private string header = "Default";
		private string footer = "";

		public string Header {
			get => header;
			set {
				header = value;
				HeaderText.Text = value;
			}
		}

		public string Footer {
			get => footer;
			set {
				footer = value;
				FooterText.Text = value;
			}
		}

		public MenuItemHeader() {
			InitializeComponent();
		}
	}
}
