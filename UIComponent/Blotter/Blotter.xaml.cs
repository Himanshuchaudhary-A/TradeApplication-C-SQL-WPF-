using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace NirvanaTradingApp
{
    public partial class Blotter : Window
    {
        private DataRowView _selectedRowData;
        HelperFunctions _functions = new HelperFunctions();
        public Blotter()
        {
            InitializeComponent();
        }

        //Fetch OrderData
        public void BlotterDataGrid_Loaded(object sender, RoutedEventArgs e)
        {

            FetchBlotterDataFromDb("SelectAllTradedOrders", BlotterDataGridOrders);
        }

        //Fetch SubOrderData
        public void BlotterDataGridSubOrders_Loaded(object sender, RoutedEventArgs e)
        {
            FetchBlotterDataFromDb("SelectAllSubOrders", BlotterDataGridSubOrders);
        }

        //Creating new SubOrder
        private void CreateNewSubOrder_Click(object sender, RoutedEventArgs e)
        {
            CreateTradingTicketFromRow(_selectedRowData, "createSubOrder");

        }

        //Edit Exisitng Order
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            CreateTradingTicketFromRow(_selectedRowData, "edit");
        }

        //Delete order
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            object idValue = _functions.FetchIdValue(BlotterDataGridOrders);
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            string storedProcedure = "DeleteTradedOrder";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {  
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("Index", idValue);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
                BlotterDataGrid_Loaded(sender, e);
            }
        }

       //Helper Function creating Objects depeending upon the context provided.
        private void CreateTradingTicketFromRow(DataRowView rowView, string context)
        {

            DataGrid dataGrid = BlotterDataGridOrders;
            var currentCell = dataGrid.CurrentCell;

            if (currentCell != null)
            {
                if (currentCell.Item is DataRowView dataRowView)
                {
                    _selectedRowData = dataRowView;
                }
            }

            if (rowView != null)
            {
                
                string[] values = new string[5];
                for (int i = 0; i <= 4; i++)
                {
                   
                    if (i < rowView.Row.ItemArray.Length)
                    {
                        values[i] = rowView[i]?.ToString();
                    }
                    else
                    {
                        values[i] = "N/A"; 
                    }
                }
                object idValue = _functions.FetchIdValue(BlotterDataGridOrders);

                var newTradingTicketObj = new TradingTicket(
                    values[0],           
                    values[1],            
                    values[2],            
                    int.Parse(values[3]), 
                    int.Parse(values[4]), 
                    context  ,
                    idValue,
                    this
                );

                // Show the new TradingTicket object
                newTradingTicketObj.Show();
            }
            else
            {
                MessageBox.Show("No row data selected.");
            }
        }

        private void BlotterDataGridOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {          
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                //_selectedIndex = dataGrid.SelectedIndex;
               
            }        
        }

        ////Helper Function Fetching the data for both orders and sub_orders from the db.
        public void FetchBlotterDataFromDb(string storedProcedure, DataGrid gridName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand(storedProcedure, sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            try
            {
                sqlConnection.Open();
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dataTable = new DataTable();
                sqlAdapter.Fill(dataTable);
                gridName.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _functions.HideColumn("Id", gridName);
                sqlConnection.Close();
            }
        }
    }
}
