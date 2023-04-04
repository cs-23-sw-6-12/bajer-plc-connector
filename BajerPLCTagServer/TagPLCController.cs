using libplctag;
using libplctag.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BajerPLCTagServer
{
    class TagPLCController : IPLCController
    {
        public Tag<IntPlcMapper, short> _outputTag;
        public Tag<IntPlcMapper, short> _inputTag;
        public Tag<BoolPlcMapper, bool> _resetTag;
        private string _gateway;
        private byte _outputSize = 0;
        private byte _inputSize = 0;

        public TagPLCController(string gateway, byte resetPin)
        {
            _gateway = gateway;
            _resetTag = new Tag<BoolPlcMapper, bool>()
            {
                Name = $"B3:0/{resetPin}",
                Gateway = _gateway,
                PlcType = PlcType.MicroLogix,
                Protocol = Protocol.ab_eip,
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task Init()
        {
            await _resetTag.InitializeAsync();
        }

        public async Task<List<bool>> ReadOutput()
        {
            return DecodeOutputs(await _outputTag.ReadAsync());
        }

        public async Task Reset()
        {
            if (_inputTag != null)
                await _inputTag.WriteAsync(0);
            await _resetTag.WriteAsync(true);


            await Task.Delay(20);

            await _resetTag.WriteAsync(false);
        }

        public async Task SetInput(List<bool> inputs)
        {
             await _inputTag.WriteAsync(EncodeInputs(inputs));
        }

        public async Task Setup(byte inputSize, byte outputSize)
        {
            if (_inputSize != inputSize)
            {
                if (_inputTag != null)
                    _inputTag.Dispose();
                _inputTag = new Tag<IntPlcMapper, short>() {
                    Name = "B3:0",
                    Gateway = _gateway,
                    PlcType = PlcType.MicroLogix,
                    Protocol = Protocol.ab_eip,
                    Timeout = TimeSpan.FromSeconds(10)
                };
                await _inputTag.InitializeAsync();
                _inputSize = inputSize;
            }

            if (_outputSize != outputSize)
            {
                if (_outputTag != null)
                    _outputTag.Dispose();
                _outputTag = new Tag<IntPlcMapper, short>()
                {
                    Name = "O0:0",
                    Gateway = _gateway,
                    PlcType = PlcType.MicroLogix,
                    Protocol = Protocol.ab_eip,
                    Timeout = TimeSpan.FromSeconds(10),
                    ArrayDimensions = new int[] { outputSize }
                };
                await _outputTag.InitializeAsync();

                _outputSize = outputSize;
            }
        }

        public void Stop()
        {
            _inputTag.Dispose();
            _outputTag.Dispose();
            _resetTag.Dispose();
        }

        private short EncodeInputs(List<bool> inputs)
        {
            short rv = 0;
            for (var i = 0; i < inputs.Count; i++)
            {
                rv |= (short)((inputs[i] ? 1 : 0) << i);
            }
            return rv;
        }

        private List<bool> DecodeOutputs(short output)
        {
            var rv = new List<bool>();
            for (var i = 0; i < _outputSize; i++)
            {
                rv.Add((output & ((short)(1 << i))) != 0);
            }
            return rv;
        }
    }
}
