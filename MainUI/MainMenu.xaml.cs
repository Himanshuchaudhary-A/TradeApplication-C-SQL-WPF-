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
using NirvanaTradingApp;

namespace NirvanaTradingApp
{

    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void TT_Click(object sender, RoutedEventArgs e)
        {
               var tradingTicketObj = new TradingTicket();
                tradingTicketObj.Show();
        }

        private void Blotter_Click(object sender, RoutedEventArgs e)
        {
            var blotterObj = new Blotter();
            blotterObj.Show();
        }

        private void Allocation_Click(object sender, RoutedEventArgs e)
        {
            var allocationObj = new Allocation();
            allocationObj.Show();
        }
    }
}
