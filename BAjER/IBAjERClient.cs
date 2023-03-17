using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAjER {
	public interface IBAjERClient {
		Task<List<bool>> Step(List<bool> bits);
		Task Setup(byte inBits, byte outBits);
		Task Reset();
	}
}
