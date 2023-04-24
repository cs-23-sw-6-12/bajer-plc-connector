using BajerPLCTagServer;

namespace TagTest {
	internal class Program {
		static void Main(string[] args) {
			AsyncMain().Wait();
		}

		static async Task AsyncMain() {
			TagPLCController pLCController = new TagPLCController("192.168.97.234", 15);
			await pLCController.Init();
			await pLCController.Setup(1, 6);
			await pLCController.SetInput(new List<bool> { true });
			await pLCController.SetInput(new List<bool> { false });
			await pLCController.SetInput(new List<bool> { true });
			await pLCController.SetInput(new List<bool> { false });
			try {
				Console.WriteLine("reading output");
				await pLCController.ReadOutput();
				Console.WriteLine("done");
			} catch (Exception) {
				Console.WriteLine("done");
				pLCController.Stop();
			}
		}
	}
}