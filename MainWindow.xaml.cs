using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System;

namespace NirvanaTradingApp
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void handleLogin(object sender, RoutedEventArgs e)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            string query = "SELECT * FROM T_CompanyUser WHERE Username = @UserName AND Password = @Password";

            SqlConnection sqlcon = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(query, sqlcon);
            {
                try
                {
                    command.Parameters.AddWithValue("@Username", usernameTextBox.Text);
                    command.Parameters.AddWithValue("@Password", passwordTextBox.Password);
                    // Open the connection
                    sqlcon.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var mainUI = new MainMenu();
                        mainUI.Show();
                        this.Close();

                    }
                    else
                    {
                        MessageBox.Show("Invalid Username or Password!", "Login Failure", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    sqlcon.Close();
                }
            }
        }
    }
}
