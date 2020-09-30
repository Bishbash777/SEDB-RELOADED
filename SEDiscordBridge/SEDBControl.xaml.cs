using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VRage.Game;

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
    }
}
