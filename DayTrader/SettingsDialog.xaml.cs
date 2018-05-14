using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DayTrader
{
	public class SettingsDialog : Window
	{
		public SettingsDialog()
		{
			DataContext = this;
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoaderPortableXaml.Load(this);
			this.AttachDevTools();
		}
	}
}
