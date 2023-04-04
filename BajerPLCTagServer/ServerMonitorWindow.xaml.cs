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
	public partial class ServerMonitorWindow : Window {
		private IBAjERServer _server;
		private IPLCController _plcController;
		private ushort _stepDelay;
		private ObservableCollection<LogMessage> _logMessages;

		public ServerMonitorWindow(IBAjERServer server, IPLCController plcController) {
			InitializeComponent();
			_server = server;
			_plcController = plcController;
			_logMessages = new ObservableCollection<LogMessage>();
			StatusText.Text = "Connecting to PLC...";

            LogGrid.ItemsSource = _logMessages;

			_stepDelay = ushort.Parse(StepDelayInput.Text);

			_server.StepHandler = StepHandler;
			_server.SetupHandler = SetupHandler;
			_server.ResetHandler = ResetHandler;
			_server.Connected = ConnectedHandler;
			_server.Disconnected = DisconnectedHandler;
			StatusText.Text = "Waiting for client";
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
			_ = Dispatcher.BeginInvoke(async Task () => {
				PrintToLog($"Reset");
			});

			await _plcController.Reset();
		}

		private async Task SetupHandler(byte inputs, byte outputs) {
			_ = Dispatcher.BeginInvoke(() => {
				PrintToLog($"setup {inputs} {outputs}");
			});
			await _plcController.Setup(inputs, outputs);
		}

		private async Task<List<bool>> StepHandler(List<bool> inputs) {
			await _plcController.SetInput(inputs);

			await Task.Delay((int) _stepDelay);

			var readBits = await _plcController.ReadOutput();

			_ = Dispatcher.BeginInvoke(() => {
				var message = "Step " + inputs.Select(input => input ? "1" : "0").Aggregate((prev, current) => prev + "," + current) +
					" - Sent " + readBits.Select(input => input ? "1" : "0").Aggregate((prev, current) => prev + "," + current);
				PrintToLog(message);
			});

			return readBits;
		}

		private void PrintToLog(string message) {
			var lm = new LogMessage(DateTime.Now, message);
			_logMessages.Add(lm);
			LogGrid.ScrollIntoView(lm);
		}

		private void Window_Closed(object sender, EventArgs e) {
			_server.Stop();
		}

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
			try
			{
				_stepDelay = ushort.Parse(StepDelayInput.Text);
				PrintToLog($"Updated step delay to {_stepDelay}");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
        }
    }
}
