using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;

namespace NirvanaTradingApp
{
    public partial class TradingTicket : Window
    {
        private string _context= "";
        private object _id= "";
        private Blotter _parentBlotter;
        public TradingTicket()
        {
            InitializeComponent();
        }

        public TradingTicket(string symbol, string sideText, string account, int quantity, double price, string context, object index, Blotter blotter)
        {
            InitializeComponent();
            SetUIComponents(symbol, sideText, account,  quantity, price, context, index);
            _parentBlotter = blotter;
        }

        private void SetUIComponents( string symbol, string sideText, string account, int quantity, double price, string context, object index)
        {
            SymbolTextBox.Text = symbol;
            SideComboBox.Text = sideText;
            SymbolTextBox.IsEnabled = false;
            SideComboBox.IsEnabled = false;
            if(context == "createSubOrder")
            {
                AccountComboBox.IsEnabled = false;
                CreateOrderButton.Visibility = Visibility.Hidden;
            }
            else
            {
                AccountComboBox.IsEnabled = true;
                CreateOrderButton.Visibility = Visibility.Hidden;
                DoneAwayButton.Visibility = Visibility.Hidden;
                SaveButton.Visibility = Visibility.Visible;
            }
           
            AccountComboBox.Text = account;
            QuantityTextBox.Text = quantity.ToString();
            PriceTextBox.Text = price.ToString();
            _context = context;
            _id = index;
            
        }

        private void Qauntity_Textbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Price_Textbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            SendDataToDB("createOrder");
        }

        private void SendDataToDB(string context = "createOrder")
        {
            int quantity = int.Parse(QuantityTextBox.Text);
            decimal price = decimal.Parse(PriceTextBox.Text);

            // Validate if all the required fields are filled
            if (string.IsNullOrWhiteSpace(SymbolTextBox.Text) ||
                SideComboBox.SelectedIndex == -1 ||
                AccountComboBox.SelectedIndex == -1 ||
                quantity == 0 ||
                price == 0.00m)
            {
                MessageBox.Show("Please fill data properly!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Order successfully created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            string storedProcedure, orderStatus;

            if (context == "createOrder")
            {
                storedProcedure = "InsertTradedOrder";
                orderStatus = "New";
            }
            else if(context == "doneAway")
            {
                storedProcedure = "InsertTradedOrderInAllocation";
                orderStatus = "Filled";
            }
            else  if(context == "createSubOrder")
            {
                storedProcedure = "InsertSubOrders";
                orderStatus= "Filled";

            } else
            {
                storedProcedure = "UpdateTradedOrder";
                orderStatus = "New";
            }

            try
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Symbol", SymbolTextBox.Text);
                sqlCommand.Parameters.AddWithValue("@Side", SideComboBox.Text);
                sqlCommand.Parameters.AddWithValue("@Account", AccountComboBox.Text);
                sqlCommand.Parameters.AddWithValue("@Quantity", QuantityTextBox.Text);
                sqlCommand.Parameters.AddWithValue("@AvgPrice", PriceTextBox.Text);
                sqlCommand.Parameters.AddWithValue("@RemQuantity", 12);
                sqlCommand.Parameters.AddWithValue("@OrderStatus", orderStatus);
                sqlCommand.Parameters.AddWithValue("@CLOrderId", "null");
                sqlCommand.Parameters.AddWithValue("@ParentCLOrderId", "null");
                sqlCommand.Parameters.AddWithValue("@StagedOrderId", "null");
                sqlCommand.Parameters.AddWithValue("@OriginalCLOrderId", "null");

                if(context == "createSubOrder")
                {
                    sqlCommand.Parameters.AddWithValue("@Index", _id);

                }
                

                sqlCommand.ExecuteNonQuery();
              

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                
                sqlConnection.Close();
            }
        }

        private void DoneAway_Click(object sender, RoutedEventArgs e)
        {  
            if(_context == "createSubOrder")
           {
                SendDataToDB("createSubOrder");
                _parentBlotter.BlotterDataGridSubOrders_Loaded(sender, e);
                _parentBlotter.BlotterDataGrid_Loaded(sender, e);
            }
           else
           {
               SendDataToDB("doneAway");
           }

            Blotter blotter = new Blotter();
            blotter.BlotterDataGrid_Loaded(sender, e);
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("UpdateTradedOrder", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", _id);
                sqlCommand.Parameters.AddWithValue("@NewAccount", AccountComboBox.Text);
                sqlCommand.Parameters.AddWithValue("@NewQuantity", QuantityTextBox.Text);
                sqlCommand.Parameters.AddWithValue("@NewAvgPrice", PriceTextBox.Text);

                sqlCommand.ExecuteNonQuery();
                _parentBlotter.BlotterDataGridSubOrders_Loaded(sender, e);
                _parentBlotter.BlotterDataGrid_Loaded(sender, e);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                sqlConnection.Close();
            }
        }

        private void RefreshDataGrid()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            string query = "SELECT * FROM T_TradedOrders"; // Adjust the query as needed

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, sqlConnection);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    Blotter blotter = new Blotter();    
                    blotter.BlotterDataGridOrders.ItemsSource = dataTable.DefaultView; // Bind DataTable to DataGrid
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

    }
}
