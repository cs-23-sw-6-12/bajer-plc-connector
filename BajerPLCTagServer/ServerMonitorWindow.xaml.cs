using BAjER;
using libplctag.DataTypes;
using libplctag;
using libplctag.NativeImport;
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
using System.Windows.Shapes;
using System.Threading;
using System.DirectoryServices.ActiveDirectory;

namespace BajerPLCTagServer {
	/// <summary>
	/// Interaction logic for ServerMonitor.xaml
	/// </summary>
	public partial class ServerMonitorWindow : Window {
		private IBAjERServer _server;
		private string _plcGateway;
		private Tag<IntPlcMapper, short> _inputTag;
		private Tag<IntPlcMapper, short> _outputTag;
		private byte _inputCount;
		private byte _outputCount;
		
		public ServerMonitorWindow(IBAjERServer server, string plcGateway) {
			InitializeComponent();
			_server = server;
			_plcGateway = plcGateway;
			StatusText.Text = "Connecting to PLC...";

			_inputCount = 0;
			_outputCount = 0;

			_inputTag = new Tag<IntPlcMapper, short>() {
				Name = "B3:0",
				Gateway = plcGateway,
				PlcType = PlcType.MicroLogix,
				Protocol = Protocol.ab_eip,
				Timeout = TimeSpan.FromSeconds(10)
			};

			_outputTag = new Tag<IntPlcMapper, short>() {
				Name = "O0:0",
				Gateway = plcGateway,
				PlcType = PlcType.MicroLogix,
				Protocol = Protocol.ab_eip,
				Timeout = TimeSpan.FromSeconds(10)
			};

			_server.StepHandler = StepHandler;
			_server.SetupHandler = SetupHandler;
			_server.ResetHandler = ResetHandler;
			_server.Connected = ConnectedHandler;
			_server.Disconnected = DisconnectedHandler;
			Dispatcher.BeginInvoke(async Task () => {
				try {
					await _inputTag.InitializeAsync();
					await _outputTag.InitializeAsync();
					await Task.WhenAll(_inputTag.InitializeAsync(), _outputTag.InitializeAsync());
				} catch (Exception ex) {
					PrintToLog($"Could not connect to PLC: {ex.Message}");
					StatusText.Text = "Failed to connect to PLC";
					return;
				}
				PrintToLog("Connected to PLC");
				StatusText.Text = "Waiting for client";
			});
		}

		private void DisconnectedHandler(string reason) {
			Dispatcher.BeginInvoke(() => {
				PrintToLog($"Client disconnected: {reason}");
			});
		}

		private void ConnectedHandler() {
			Dispatcher.BeginInvoke(() => {
				PrintToLog($"Client connected");
			});
		}

		private async Task ResetHandler() {
			_ = Dispatcher.BeginInvoke(() => {
				PrintToLog($"Reset");
			});
		}

		private async Task SetupHandler(byte inputs, byte outputs) {
			_ = Dispatcher.BeginInvoke(() => {
				PrintToLog($"setup {inputs} {outputs}");
			});

			_inputCount = inputs;
			_outputCount = outputs;
		}

		private async Task<List<bool>> StepHandler(List<bool> inputs) {
			_ = Dispatcher.BeginInvoke(() => {
				PrintToLog("Step " + inputs.Select(input => input ? "1" : "0").Aggregate((prev, current) => prev + "," + current));
			});


			await _inputTag.WriteAsync(EncodeInputs(inputs));

			await Task.Delay(500);

			var readBits = DecodeOutputs(await _inputTag.ReadAsync());

			_ = Dispatcher.BeginInvoke(() => {
				PrintToLog("Sent " + readBits.Select(input => input ? "1" : "0").Aggregate((prev, current) => prev + "," + current));
			});

			return readBits;
		}

		private void PrintToLog(string message) {
			LogTextBox.Text += message + "\n";
		} 

		private short EncodeInputs(List<bool> inputs) {
			short rv = 0;
			for (var i = 0; i < inputs.Count; i++) {
				rv |= (short)((inputs[i] ? 1 : 0) << i);
			}
			return rv;
		}

		private List<bool> DecodeOutputs(short output) {
			var rv = new List<bool>();
			for (var i = 0; i < _outputCount; i++) {
				rv.Add((output & ((short)(1 << i))) != 0);
			}
			return rv;
		}

		private void Window_Closed(object sender, EventArgs e) {
			_server.Stop();
		}
	}
}
