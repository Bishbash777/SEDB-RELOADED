using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VRageMath;

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
            dgFacList.ItemsSource = Plugin.Config.FactionChannels;
            dgPermList.ItemsSource = Plugin.Config.CommandPerms;
            FacColorBox.ItemsSource = utils.Colors;
            GlobalChatColorBox.ItemsSource = utils.Colors;
            FacColorBox.SelectedItem = utils.GetFromColor(Plugin.Config.FacColor);
            GlobalChatColorBox.SelectedItem = utils.GetFromColor(Plugin.Config.GlobalColor);
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();

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

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Plugin.Config.FactionChannels.Add(new FactionChannel()
            {
                Faction = "ABC",
                Channel = 1234,
            });
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (dgFacList.SelectedItem is FactionChannel factionChannel) Plugin.Config.FactionChannels.Remove(factionChannel);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            Plugin.Config.CommandPerms.Add(new CommandPermission()
            {
                Player = 1234,
                Permission = "*",
            });
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (dgPermList.SelectedItem is CommandPermission commandPermission) Plugin.Config.CommandPerms.Remove(commandPermission);
        }

        private void FacColorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FacColorBox.SelectedValue is Color color) Plugin.Config.FacColor = color; 
        }

        private void GlobalChatColorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalChatColorBox.SelectedValue is Color color) Plugin.Config.GlobalColor = color;
        }
    }
}
