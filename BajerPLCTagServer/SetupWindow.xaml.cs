using BAjER;
using libplctag.DataTypes;
using libplctag;
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
using System.Globalization;
using System.Net;

namespace BajerPLCTagServer {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SetupWindow : Window {
		public SetupWindow() {
			InitializeComponent();
		}

		private void StartServerButton_Click(object sender, RoutedEventArgs e) {
			Dispatcher.InvokeAsync(async Task () => {
				StartServerButton.IsEnabled = false;
				try {
					var plcController = new TagPLCController(PLCAddressInput.Text, byte.Parse(PLCResetPin.Text));
					await plcController.Init();

					var bajerServer = new BAjERServer();
					bajerServer.Start(IPAddress.Parse(BajerAddressInput.Text), ushort.Parse(BajerPortInput.Text));

					var serverMonitorWindow = new ServerMonitorWindow(bajerServer, plcController);
					
					serverMonitorWindow.Show();
					this.Close();
				} catch (Exception err) {
					LibPlcTag.Shutdown();
					MessageBox.Show(err.ToString());
				}
				StartServerButton.IsEnabled = true;
			});
		}
	}
}
