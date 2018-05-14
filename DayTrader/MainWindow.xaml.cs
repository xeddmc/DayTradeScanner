using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DayTradeScanner;

namespace DayTrader
{
	public class MainWindow : Window
	{
		private Scanner _scanner;
		private Button btnStart;
		private MenuItem menuItemSettings;
		private Thread _thread;
		private bool _running;

		public MainWindow()
		{
			DataContext = this;
			StartButton = "Start scanning";
			StatusText = "stopped...";
			Signals = new ObservableCollection<Signal>();
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoaderPortableXaml.Load(this);
			this.AttachDevTools();
			btnStart = this.Find<Button>("btnStart");
			menuItemSettings = this.Find<MenuItem>("menuItemSettings");
			btnStart.Click += menuItemSettings_Click;
			menuItemSettings.Click += menuItemSettings_Click;
		}


		private void menuItemSettings_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var dlg = new SettingsDialog();
			dlg.ShowDialog();
		}

		private void btnStart_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_thread != null)
			{
				_running = false;
				_thread.Join();
				_thread = null;
				StartButton = "Start scanning";
				StatusText = "stopped...";
				return;
			}
			else
			{
				StartButton = "Stop scanning";
				StatusText = "initializing...";
				_running = true;
				_thread = new Thread(new ThreadStart(DoScan));
				_thread.Start();
			}
		}

		private async void DoScan()
		{
			var exchange = "Bitfinex";
			var currency = "USD";
			var volume = 500000L;

			SetStatusText($"initializing {exchange} with 24hr volume of {volume} {currency}...");
			_scanner = new Scanner(exchange, currency, volume);
			while (_running)
			{
				int idx = 0;
				foreach (var symbol in _scanner.Symbols)
				{
					idx++;
					SetStatusText($"scanning {symbol} ({idx}/{_scanner.Symbols.Count})...");
					var signal = await _scanner.ScanAsync(symbol);
					if (signal != null)
					{
						await Dispatcher.UIThread.InvokeAsync(() =>
						{
							Signals.Insert(0, signal);
						});
					}
				}
				SetStatusText($"sleeping...");
				Thread.Sleep(5000);
			}
		}

		private void SetStatusText(string statusTxt)
		{
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				StatusText = statusTxt;
			});
		}

		public static readonly AvaloniaProperty<string> StartButtonProperty =
			AvaloniaProperty.Register<MainWindow, string>("StartButton", inherits: true);

		public string StartButton
		{
			get { return this.GetValue(StartButtonProperty); }
			set { this.SetValue(StartButtonProperty, value); }
		}

		public static readonly AvaloniaProperty<string> StatusProperty =
			AvaloniaProperty.Register<MainWindow, string>("StatusText", inherits: true);

		public string StatusText
		{
			get { return this.GetValue(StatusProperty); }
			set { this.SetValue(StatusProperty, value); }
		}

		public ObservableCollection<Signal> Signals { get; set; }
	}
}
