using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BajerPLCTagServer
{
    public interface IPLCController
    {
        Task Setup(byte inputSize, byte outputSize);

        Task SetInput(List<bool> inputs);

        Task<List<bool>> ReadOutput();

        Task Reset();

        void Stop();
    }
}
