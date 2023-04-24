	using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BAjER {
	public class BAjERClient : IBAjERClient {
		private int _inBits;
		private int _outBits;
		private TcpClient _client;
		public BAjERClient() {
			_inBits = 0;
			_outBits = 0;
			_client = new TcpClient();
		}

		public async Task Connect(string address, ushort port) {
			await _client.ConnectAsync(address, port);
		}

		public async Task<List<bool>> Step(List<bool> bits) {
			if (bits.Count != _inBits) {
				throw new ArgumentException("Incorrect amount of bits");
			}

			await _client.GetStream().WriteAsync((new byte[] { 0 }).Concat(bits.Select(x => x == true ? ((byte)1) : ((byte)0))).ToArray());
			await _client.GetStream().FlushAsync();

			var buffer = new byte[_outBits + 1];

			var bytesRead = await _client.GetStream().ReadAsync(buffer);
			if (bytesRead != buffer.Length || buffer[0] != 0) {
				throw new Exception("Unexpected output");
			}

			return buffer
				.Skip(1)
				.Select(b => b != 0)
				.ToList();
		}

		public async Task Setup(byte inBits, byte outBits) {
			await _client.GetStream().WriteAsync(new byte[] { 2, inBits, outBits });
			await _client.GetStream().FlushAsync();
			var buffer = new byte[] { 0 };
			var bytesRead = await _client.GetStream().ReadAsync(buffer);
			if (bytesRead != 1 || buffer[0] != 0) {
				throw new Exception("Unexpected output");
			}
			_inBits = inBits;
			_outBits = outBits;
		}

		public async Task Reset() {
			await _client.GetStream().WriteAsync(new byte[] { 1 });
			await _client.GetStream().FlushAsync();
			var buffer = new byte[] { 0 };
			var bytesRead = await _client.GetStream().ReadAsync(buffer);
			if (bytesRead != 1 || buffer[0] != 0) {
				throw new Exception("Unexpected output");
			}

		}
	}
}
