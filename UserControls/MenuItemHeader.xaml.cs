using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
