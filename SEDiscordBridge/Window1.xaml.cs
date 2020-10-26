using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SEDiscordBridge
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private DamageTexts _texts;
        public Window1()
        {
            InitializeComponent();
        }

        public Window1(DamageTexts texts) : this()
        {
            Title = texts.DamageType.ToString();
            DataGrid.ItemsSource = texts.Strings;
            _texts = texts;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _texts.Strings.Add(new StringInvalid("Some Text"));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is StringInvalid invalid)
                _texts.Strings.Remove(invalid);
        }
    }
}
