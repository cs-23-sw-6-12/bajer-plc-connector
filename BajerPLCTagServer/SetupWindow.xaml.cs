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
					var myTag = new Tag<BoolPlcMapper, bool>() {
						Name = "I1:0/0",
						Gateway = "192.168.73.234",
						PlcType = PlcType.MicroLogix,
						Protocol = Protocol.ab_eip,
						Timeout = TimeSpan.FromSeconds(5),
						DebugLevel = DebugLevel.Warn
					};

					await myTag.InitializeAsync();
					Console.WriteLine(myTag.GetStatus());
					//Read value from PLC
					//Value will also be accessible at myTag.Value
					bool myDint = await myTag.ReadAsync();
					
					//Write to console
					Console.WriteLine(myDint);
					var bajerServer = new BAjERServer();
					bajerServer.Start(System.Net.IPAddress.Parse(BajerAddressInput.Text), ushort.Parse(BajerPortInput.Text));

					var serverMonitorWindow = new ServerMonitorWindow(bajerServer);
					
					serverMonitorWindow.Show();
					this.Close();
				} catch (Exception err) {
					MessageBox.Show(err.ToString());
				}
				StartServerButton.IsEnabled = true;
			});
		}
	}
}
