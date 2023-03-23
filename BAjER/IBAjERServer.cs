using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAjER {
	public delegate Task Reset();
	public delegate Task Setup(byte inputs, byte outputs);
	public delegate Task<List<bool>> Step(List<bool> inputs);
	public delegate void Connected();
	public delegate void Disconnected(string reason);

	public interface IBAjERServer {
		Reset? ResetHandler { get; set; }
		Setup? SetupHandler { get; set; }
		Step? StepHandler { get; set; }
		Connected? Connected { get; set; }
		Disconnected? Disconnected { get; set; }

		void Stop();
	}
}
