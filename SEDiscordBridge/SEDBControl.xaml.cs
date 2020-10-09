using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SEDiscordBridge
{
    /// <summary>
    /// Interação lógica para SEDBControl.xaml
    /// </summary>
    public partial class SEDBControl : UserControl
    {
        private SEDiscordBridgePlugin Plugin { get; }

        public SEDBControl()
        {
            InitializeComponent();
        }

        public SEDBControl(SEDiscordBridgePlugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
            GridMessages.ItemsSource = Plugin.Config.GridDeathMessages;
            PlayerMessages.ItemsSource = Plugin.Config.PlayerDeathMessages;
            ChannelRoutes.ItemsSource = Plugin.Config.DeathRoutes;
            UpdateFacDataGrid();
            UpdatePermsDataGrid();
        }

        private void UpdateFacDataGrid()
        {
            var factions = from f in Plugin.Config.FactionChannels select new { Faction = f.Split(':')[0], Channel = f.Split(':')[1] };
            dgFacList.ItemsSource = factions;
        }

        private void UpdatePermsDataGrid()
        {
            var perms = from f in Plugin.Config.CommandPerms select new { Player = f.Split(':')[0], Permission = f.Split(':')[1] };
            dgPermList.ItemsSource = perms;
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
            Plugin.DDBridge?.SendStatus(null);

            if (Plugin.Config.Enabled)
            {
                if (Plugin.Torch.CurrentSession == null && !Plugin.Config.PreLoad)
                {
                    Plugin.UnloadSEDB();

                }
                else
                {
                    Plugin.LoadSEDB();
                }
            }
            else
            {
                Plugin.UnloadSEDB();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelRoutes.SelectedItem != null && ChannelRoutes.SelectedItem is DeathRoutes routes) Plugin.Config.DeathRoutes.Remove(routes);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (GridMessages.SelectedItem is DamageTexts pair) Plugin.Config.GridDeathMessages.Remove(pair);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (PlayerMessages.SelectedItem is DamageTexts pair) Plugin.Config.PlayerDeathMessages.Remove(pair);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Plugin.Config.GridDeathMessages.Add(new DamageTexts(DamageType.Asphyxia, new ObservableCollection<StringInvalid>()));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Plugin.Config.PlayerDeathMessages.Add(new DamageTexts(DamageType.Asphyxia, new ObservableCollection<StringInvalid>()));
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (GridMessages.SelectedItem is DamageTexts damageTexts) new Window1(damageTexts).ShowDialog();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (PlayerMessages.SelectedItem is DamageTexts damageTexts) new Window1(damageTexts).ShowDialog();
        }
    }
}
