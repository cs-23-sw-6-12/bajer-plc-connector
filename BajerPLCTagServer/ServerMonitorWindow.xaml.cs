using BAjER;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;

namespace BajerPLCTagServer {
	/// <summary>
	/// Interaction logic for ServerMonitor.xaml
	/// </summary>
	/// 
	class LogMessage
    {
		public LogMessage(DateTime time, string message)
        {
			Time = time;
			Message = message;
        }
		public DateTime Time { get; set; }
		public string Message { get; set; }
	}

	enum MessageType {
		STEP,
		RESET,
		SETUP,
		OTHER
	}

	public partial class ServerMonitorWindow : Window {
		private IBAjERServer _server;
		private IPLCController _plcController;
		private ObservableCollection<LogMessage> _logMessages;

		public ServerMonitorWindow(IBAjERServer server, IPLCController plcController) {
			InitializeComponent();
			_server = server;
			_plcController = plcController;
			_logMessages = new ObservableCollection<LogMessage>();
			StatusText.Text = "Connecting to PLC...";

            LogGrid.ItemsSource = _logMessages;

			_server.StepHandler = StepHandler;
			_server.SetupHandler = SetupHandler;
			_server.ResetHandler = ResetHandler;
			_server.Connected = ConnectedHandler;
			_server.Disconnected = DisconnectedHandler;
			StatusText.Text = "Waiting for client";
		}

		private void DisconnectedHandler(string reason) {
			Dispatcher.BeginInvoke(() => {
				PrintToLog(MessageType.OTHER, $"Client disconnected: {reason}");
			});
		}

		private void ConnectedHandler() {
			Dispatcher.BeginInvoke(() => {
				PrintToLog(MessageType.OTHER, $"Client connected");
			});
		}

		private async Task ResetHandler() {
			_ = Dispatcher.BeginInvoke(async Task () => {
				//PrintToLog(MessageType.RESET, $"Reset");
			});

			await _plcController.Reset();
		}

		private async Task SetupHandler(byte inputs, byte outputs) {
			_ = Dispatcher.BeginInvoke(() => {
				PrintToLog(MessageType.SETUP, $"setup {inputs} {outputs}");
			});
			await _plcController.Setup(inputs, outputs);
		}

		private async Task<List<bool>> StepHandler(List<bool> inputs) {
			await _plcController.SetInput(inputs);

			var readBits = await _plcController.ReadOutput();
			_ = Dispatcher.BeginInvoke(() => {
				var message = "Step " + inputs.Select(input => input ? "1" : "0").Aggregate((prev, current) => prev + "," + current) +
					" - Sent " + readBits.Select(input => input ? "1" : "0").Aggregate((prev, current) => prev + "," + current);
				//PrintToLog(MessageType.STEP, message);
			});

			return readBits;
		}

		private void PrintToLog(MessageType messageType, string message) {
			var lm = new LogMessage(DateTime.Now, message);
			_logMessages.Add(lm);
			LogGrid.ScrollIntoView(lm);
		}

		private void Window_Closed(object sender, EventArgs e) {
			_server.Stop();
		}
    }
}
