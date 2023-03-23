using BAjER;
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

namespace BajerDebugger {
	/// <summary>
	/// Interaction logic for MonitorWindow.xaml
	/// </summary>
	public partial class MonitorWindow : Window {
		private IBAjERClient _bajer;
		private SortedList<DateTime, BajerState> _bajerStates;
		public MonitorWindow(IBAjERClient bajer) {
			InitializeComponent();
			_bajer = bajer;
			_bajerStates = new SortedList<DateTime, BajerState>(new InvertedComparer());
			BajerStateList.ItemsSource = _bajerStates;
		}

		private void Step_Click(object sender, RoutedEventArgs e) {
			Dispatcher.InvokeAsync(async Task () => {
				DisableInputs();
				var inputBits = inputStringInput.Text.Select(c => c != '0').ToList();
				var newState = await _bajer.Step(inputBits);
				var state = new BajerState {
					DateTime = DateTime.Now,
					InputString = new string(inputBits.Select(b => b ? '1' : '0').ToArray()),
					OutputString = new string(newState.Select(b => b ? '1' : '0').ToArray())
				};
				_bajerStates.Add(state.DateTime, state);
				BajerStateList.Items.Refresh();
				EnableInputs();
			});
		}

		private void Setup_Click(object sender, RoutedEventArgs e) {
			Dispatcher.InvokeAsync(async Task () => {
				DisableInputs();
				await _bajer.Setup(byte.Parse(setupInputBits.Text), byte.Parse(setupOutputBits.Text));
				EnableInputs();
			});
		}

		private void Reset_Click(object sender, RoutedEventArgs e) {
			Dispatcher.InvokeAsync(async Task () => {
				DisableInputs();
				await _bajer.Reset();
				EnableInputs();
				_bajerStates.Clear();
				BajerStateList.Items.Refresh();
			});
		}

		private void DisableInputs() {
			StepButton.IsEnabled = false;
			SetupButton.IsEnabled = false;
			ResetButton.IsEnabled = false;
		}

		private void EnableInputs() {
			StepButton.IsEnabled = true;
			SetupButton.IsEnabled = true;
			ResetButton.IsEnabled = true;
		}
	}
}
