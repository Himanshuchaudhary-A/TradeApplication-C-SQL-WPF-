using System;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace NirvanaTradingApp
{
    /// <summary>
    /// Interaction logic for Allocation.xaml
    /// </summary>
    public partial class Allocation : Window
    {
        private object _selectedRowData;
        HelperFunctions _helperFunctions = new HelperFunctions();
        public Allocation()
        {
            InitializeComponent();
            FetchCompanyFunds();
        }

        private void UnallocatedOrdersDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            FetchDataFromDBForAllocation("unallocated", "SelectAllUnallocatedOrders");
        }

        private void AllocatedOrdersDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            FetchDataFromDBForAllocation("allocated", "SelectAccountHoldingOrders");
        }


        private void FetchDataFromDBForAllocation(string context, string storedProcedure)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(storedProcedure, sqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                sqlConnection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if(context == "unallocated")
                {
                    UnallocatedOrdersDataGrid.ItemsSource = dataTable.DefaultView;
                }else
                {
                    AllocatedOrdersDataGrid.ItemsSource = dataTable.DefaultView;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if(context == "unallocated") {
                    _helperFunctions.HideColumn("Id", UnallocatedOrdersDataGrid);
                }else
                {
                    _helperFunctions.HideColumn("Id", AllocatedOrdersDataGrid);
                }
              
                sqlConnection.Close();
            }
        }

        private void FetchCompanyFunds()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            string query = "Select * From T_CompanyFunds";
            SqlCommand cmd = new SqlCommand(query, sqlConnection);

            try
            {
                sqlConnection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {  
                    OptionsComboBox.Items.Add(reader["Accounts"].ToString());
                }

            }catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally { sqlConnection.Close(); }
        }
        
        //When any row is selected its id is being stored in the variable.
        private void UnallocatedOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRowData = _helperFunctions.FetchIdValue(UnallocatedOrdersDataGrid);
            OptionsComboBox.Text = "";

        }

        //When any row is selected its id is being stored in the variable.
        private void AllocatedOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRowData = _helperFunctions.FetchIdValue(AllocatedOrdersDataGrid);
            OptionsComboBox.Text = "";
        }

        private void AllocateButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            string storedProcedure = "UpdateAccountInGroup";
            SqlCommand cmd = new SqlCommand(storedProcedure, sqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            System.Diagnostics.Debug.WriteLine(_selectedRowData, OptionsComboBox.Text);
            try
            {
                sqlConnection.Open();
                cmd.Parameters.AddWithValue("Index", _selectedRowData);
                cmd.Parameters.AddWithValue("UpdatedAccount", OptionsComboBox.Text);
                cmd.ExecuteNonQuery();
            }catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                sqlConnection.Close();
                UnallocatedOrdersDataGrid_Loaded(sender, e);
                AllocatedOrdersDataGrid_Loaded(sender, e);
            }
        }


    }
}
