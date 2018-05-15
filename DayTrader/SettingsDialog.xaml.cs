using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DayTradeScanner;
using System.Collections.ObjectModel;

namespace DayTrader
{
    public class SettingsDialog : Window
    {
        private DropDown _dropDown;
        public ObservableCollection<string> Exchanges { get; set; }

        public SettingsDialog()
        {
            DataContext = this;
            Exchanges = new ObservableCollection<string>();
            Exchanges.Add("Bitfinex");
            Exchanges.Add("Bittrex");
            Exchanges.Add("Binance");
            Exchanges.Add("GDax");
            Exchanges.Add("HitBTC");
            Exchanges.Add("Kraken");

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);
            this.AttachDevTools();
            _dropDown = this.Find<DropDown>("dropExchange");

            var btnSave = this.Find<Button>("btnSave");
            btnSave.Click += BtnSave_Click;

            var settings = SettingsStore.Load();
            CurrencyUSD = settings.USD;
            CurrencyETH = settings.ETH;
            CurrencyEUR = settings.EUR;
            CurrencyBNB = settings.BNB;
            CurrencyBTC = settings.BTC;
            AllowShorts = settings.AllowShorts;

            Volume = settings.Min24HrVolume.ToString();
            _dropDown.SelectedIndex = Exchanges.IndexOf(settings.Exchange);
        }

        private void BtnSave_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var settings = SettingsStore.Load();
            settings.USD = CurrencyUSD;
            settings.ETH = CurrencyETH;
            settings.EUR = CurrencyEUR;
            settings.BNB = CurrencyBNB;
            settings.BTC = CurrencyBTC;
            settings.AllowShorts = AllowShorts;

            long volume;
            if (long.TryParse(Volume, out volume))
            {
                settings.Min24HrVolume = volume;
            }
            settings.Exchange = Exchanges[_dropDown.SelectedIndex];
            SettingsStore.Save(settings);
            this.Close();
        }

        public static readonly AvaloniaProperty<bool> CurrencyUSDProperty = AvaloniaProperty.Register<SettingsDialog, bool>("CurrencyUSD", inherits: true);

        public bool CurrencyUSD
        {
            get { return this.GetValue(CurrencyUSDProperty); }
            set { this.SetValue(CurrencyUSDProperty, value); }
        }

        public static readonly AvaloniaProperty<bool> CurrencyEURProperty = AvaloniaProperty.Register<SettingsDialog, bool>("CurrencyEUR", inherits: true);

        public bool CurrencyEUR
        {
            get { return this.GetValue(CurrencyEURProperty); }
            set { this.SetValue(CurrencyEURProperty, value); }
        }

        public static readonly AvaloniaProperty<bool> CurrencyETHProperty = AvaloniaProperty.Register<SettingsDialog, bool>("CurrencyETH", inherits: true);

        public bool CurrencyETH
        {
            get { return this.GetValue(CurrencyETHProperty); }
            set { this.SetValue(CurrencyETHProperty, value); }
        }

        public static readonly AvaloniaProperty<bool> CurrencyBNBProperty = AvaloniaProperty.Register<SettingsDialog, bool>("CurrencyBNB", inherits: true);

        public bool CurrencyBNB
        {
            get { return this.GetValue(CurrencyBNBProperty); }
            set { this.SetValue(CurrencyBNBProperty, value); }
        }

        public static readonly AvaloniaProperty<bool> CurrencyBTCProperty = AvaloniaProperty.Register<SettingsDialog, bool>("CurrencyBTC", inherits: true);

        public bool CurrencyBTC
        {
            get { return this.GetValue(CurrencyBTCProperty); }
            set { this.SetValue(CurrencyBTCProperty, value); }
        }

        public static readonly AvaloniaProperty<bool> AllowShortsProperty = AvaloniaProperty.Register<SettingsDialog, bool>("AllowShorts", inherits: true);

        public bool AllowShorts
        {
            get { return this.GetValue(AllowShortsProperty); }
            set { this.SetValue(AllowShortsProperty, value); }
        }

        public static readonly AvaloniaProperty<string> VolumeProperty = AvaloniaProperty.Register<SettingsDialog, string>("Volume", inherits: true);

        public string Volume
        {
            get { return this.GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }
    }
}