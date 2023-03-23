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
					var bajerServer = new BAjERServer();
					bajerServer.Start(IPAddress.Parse(BajerAddressInput.Text), ushort.Parse(BajerPortInput.Text));

					await TestPlcConnectionAsync();

					var serverMonitorWindow = new ServerMonitorWindow(bajerServer, PLCAddressInput.Text, short.Parse(PLCResetPin.Text));
					
					serverMonitorWindow.Show();
					this.Close();
				} catch (Exception err) {
					MessageBox.Show(err.ToString());
				}
				StartServerButton.IsEnabled = true;
			});
		}

		private async Task TestPlcConnectionAsync() {
			var myTag = new Tag<IntPlcMapper, short>() {
				Name = "O0:0",
				Gateway = PLCAddressInput.Text,
				PlcType = PlcType.MicroLogix,
				Protocol = Protocol.ab_eip,
				Timeout = TimeSpan.FromSeconds(10)
			};

			await myTag.InitializeAsync();

			myTag.Dispose();

			LibPlcTag.Shutdown();
		}
	}
}
