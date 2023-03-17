using BAjER;
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

namespace BajerDebugger {
	/// <summary>
	/// Interaction logic for ConnectWindow.xaml
	/// </summary>
	public partial class ConnectWindow : Window {
		public ConnectWindow() {
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Dispatcher.InvokeAsync(async Task () => {
				try {
					ConnectButton.IsEnabled = false;
					var bajer = new BAjERClient();
					await bajer.Connect(addressInput.Text, ushort.Parse(portInput.Text));
					var monitorWindow = new MonitorWindow(bajer);
					monitorWindow.Show();
					this.Close();
				} catch (Exception err) {
					MessageBox.Show(err.Message, "Error");
				}
				ConnectButton.IsEnabled = true;
			});
		}
	}
}
