using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BAjER {
	public class BAjERServer : IBAjERServer {
		TcpListener? _listener;
		Thread? _listenAndServiceThread;
		byte _inputBytes;
		byte _outputBytes;
		
		public Reset? ResetHandler { get; set; }
		public Setup? SetupHandler { get; set; }
		public Step? StepHandler { get; set; }
		public Connected? Connected { get; set; }
		public Disconnected? Disconnected { get; set; }

		public BAjERServer() {
			_inputBytes = 0;
			_outputBytes = 0;
		}

		public void Start(IPAddress address, int port) {
			_listener = new TcpListener(address, port);
			_listener.Start();
			_listenAndServiceThread = new Thread(() => { ListenAndService().Wait(); });
			_listenAndServiceThread.Start();
		}

		private async Task ListenAndService() {
			if (_listener == null) {
				throw new Exception("ListenAndService was invoked but listener was null");
			}
			while (true) {
				var client = _listener.AcceptTcpClient();
				var clientStream = client.GetStream();

				Connected?.Invoke();

				try {
					await ServeClient(clientStream);
					Disconnected?.Invoke("Unknown");
				} catch (Exception ex) {
					Disconnected?.Invoke(ex.Message);
				}
			}
		}

		private async Task ServeClient(NetworkStream clientStream) {
			while (true) {
				var buffer = new byte[1];
				var bytesRead = await clientStream.ReadAsync(buffer, 0, 1);
				if (bytesRead != 1) {
					throw new Exception("Could not read any more bytes");
				}

				switch (buffer[0]) {
					case 0x00:
						await HandleStepCommand(clientStream);
						break;

					case 0x01:
						await HandleResetCommand(clientStream);
						break;
					case 0x02:
						await HandleSetupCommand(clientStream);
						break;
					default:
						Console.Error.WriteLine($"Received unknown command {buffer[0]}");
						break;
				}
			}
		}

		private async Task HandleStepCommand(NetworkStream clientStream) {
			var inputs = new byte[_inputBytes];
			var bytesRead = await clientStream.ReadAsync(inputs.AsMemory(0, _inputBytes));
			if (bytesRead != _inputBytes)
				throw new Exception($"Could not read {bytesRead} bytes from client");
			
			if (StepHandler != null) {
				var outputStates = await StepHandler.Invoke(inputs.Select(b => b != 0).ToList());
				if (outputStates.Count != _outputBytes) {
					throw new Exception("StepHandler returned incorrect number of bytes");
				}
				var bytes = (new byte[] { 0 }).Concat(outputStates.Select(b => b ? (byte)1 : (byte)0)).ToArray();
				await clientStream.WriteAsync(bytes);
				await clientStream.FlushAsync();
			} else {
				throw new Exception("Step command needs to have a handler");
			}
		}

		private async Task HandleResetCommand(NetworkStream clientStream) {
			if (ResetHandler != null)
				await ResetHandler.Invoke();

			await clientStream.WriteAsync(new byte[] { 0 });
			await clientStream.FlushAsync();
		}

		private async Task HandleSetupCommand(NetworkStream clientStream) {
			var buffer = new byte[2];
			var bytesRead = await clientStream.ReadAsync(buffer.AsMemory(0, 2));
			if (bytesRead != 2)
				throw new Exception("Could not read 2 bytes from client");
	
			_inputBytes = buffer[0];
			_outputBytes = buffer[1];
			if (SetupHandler != null)
				await SetupHandler.Invoke(buffer[0], buffer[1]);
			await clientStream.WriteAsync(new byte[] { 0 });
			await clientStream.FlushAsync();
		}

		public void Stop() {
			_listenAndServiceThread?.Interrupt();
		}
	}
}
