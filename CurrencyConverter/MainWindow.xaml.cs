using CurrencyConverter.exceptions;
using CurrencyConverter.model;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace CurrencyConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection;

        public MainWindow()
        {
            InitializeComponent();

            string connectionString = ConfigurationManager.ConnectionStrings["CurrencyConverter.Properties.Settings.luxinhoDBConnectionString1"].ConnectionString;
            sqlConnection = new SqlConnection(connectionString);

            ComboBoxFill(inputFrom);
            ComboBoxFill(inputTo);

            ShowDataInTable();

        }

        public void ComboBoxFill(ComboBox input)
        {
            try
            {
                string query = "SELECT * FROM Currency";
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlAdapter)
                {
                    DataTable dataTable = new DataTable();
                    sqlAdapter.Fill(dataTable);

                    input.SelectedValuePath = "id";
                    input.DisplayMemberPath = "name";
                    input.ItemsSource = dataTable.DefaultView;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when filling combo box: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowDataInTable()
        {
            try
            {
                string query = "SELECT * FROM Currency;";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                        dgTable.ItemsSource = dataTable.DefaultView;
                    else
                        dgTable.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when showing data from database: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void Convert_Click(object sender, RoutedEventArgs e)
        {
            bool isNum = Int32.TryParse(inputAmount.Text, out int number);
            try
            {
                CheckInput(isNum);

                if (inputFrom.SelectedItem == null || inputTo.SelectedItem == null)
                {
                    MessageBox.Show("Please fill currency type field.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    double amount = Convert.ToDouble(inputAmount.Text);

                    DataRowView dataFrom = (DataRowView)inputFrom.SelectedItem;
                    double valueFrom = Convert.ToDouble(dataFrom["value"]);

                    DataRowView dataTo = (DataRowView)inputTo.SelectedItem;
                    double valueTo = Convert.ToDouble(dataTo["value"]);

                    if (inputFrom.Text.Equals("EUR"))
                    {
                        lbCurrency.Content = $"{amount * valueTo} {dataTo["name"]}";
                    }
                    else
                    {
                        lbCurrency.Content = $"{amount / valueFrom * valueTo} {dataTo["name"]}";
                    }
                }
            }
            catch (WrongInputException ex)
            {
                MessageBox.Show(ex.Message + " \nPlease try again.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckInput(bool isNum)
        {
            if (Regex.IsMatch(inputAmount.Text, @"^\s+$") || inputAmount.Text == "")
            {
                inputAmount.Text = string.Empty;
                throw new WrongInputException("Wrong input! Input is empty.");
            }
            else if (!isNum)
            {
                inputAmount.Text = string.Empty;
                throw new WrongInputException("Wrong input! Input must be a number.");
            }
        }

        protected void Clear_Click(object sender, RoutedEventArgs e)
        {
            inputAmount.Text = string.Empty;
            lbCurrency.Content = string.Empty;
            inputFrom.SelectedItem = null;
            inputTo.SelectedItem = null;
        }

        public void CheckIfEmptyInput()
        {
            if (tbName.Text == "" || tbAmount.Text == "")
            {
                tbName.Text = string.Empty;
                tbAmount.Text = string.Empty;
                throw new WrongInputException("Wrong input! Amount or Name field can't be empty.");
            }
        }

        protected void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckIfEmptyInput();
                string query = "INSERT INTO Currency(name, value)" +
                "VALUES(@name,@value);";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@name", tbName.Text);
                sqlCommand.Parameters.AddWithValue("@value", tbAmount.Text.Replace(',', '.'));

                sqlCommand.ExecuteNonQuery();

                MessageBox.Show($"{tbName.Text} with value {tbAmount.Text} is successfully added to database.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (WrongInputException exception)
            {
                MessageBox.Show(exception.Message + " \nPlease try again.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when adding new data to database: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
                ShowDataInTable();
                ComboBoxFill(inputFrom);
                ComboBoxFill(inputTo);
                SetFieldsToNone();
            }
        }

        protected void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(MessageBox.Show("Are you sure you want to update?", "Currency converter", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    CheckIfEmptyInput();
                    DataRowView data = (DataRowView)dgTable.SelectedItem;
                    int id = Convert.ToInt32(data["id"]);

                    string query = "UPDATE Currency " +
                                    "SET name='" + tbName.Text + "', " +
                                    "value='" + tbAmount.Text.Replace(',', '.') + "' " +
                                    "WHERE id=" + id + ";";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlConnection.Open();

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show($"{tbName.Text} with value {tbAmount.Text} is successfully updated.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (WrongInputException exception)
            {
                MessageBox.Show(exception.Message + " \nPlease try again.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when updating currency data: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
                ShowDataInTable();
                ComboBoxFill(inputFrom);
                ComboBoxFill(inputTo);
                SetFieldsToNone();
            }
        }

        protected void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(MessageBox.Show("Are you sure you want to delete ALL data?\nThis action CAN NOT be undone!", "Currency converter", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    string query = "DELETE " +
                                    "FROM Currency;";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlConnection.Open();

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show($"Everything has been deleted!", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when deleting currency from database: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
                ShowDataInTable();
                ComboBoxFill(inputFrom);
                ComboBoxFill(inputTo);
                SetFieldsToNone();
            }
        }

        public void ShowSelectedData_EditClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView data = (DataRowView)dgTable.SelectedItem;

                if (dgTable.CurrentColumn.DisplayIndex == 0)
                {
                    tbName.Text = data["name"].ToString();
                    tbAmount.Text = data["value"].ToString();
                }
                else
                {
                    SetFieldsToNone();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when showing selected currency in TextBox: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeleteSelectedData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(MessageBox.Show("Are you sure you want to delete this data?", "Currency converter", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DataRowView data = (DataRowView)dgTable.SelectedItem;
                    int id = Convert.ToInt32(data["id"]);

                    string query = "DELETE " +
                                    "FROM Currency " +
                                    "WHERE id=" + id + ";";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlConnection.Open();

                    sqlCommand.ExecuteNonQuery();

                    MessageBox.Show($"{data["name"]} with value {data["value"]} is successfully deleted.", "Currency converter", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when deleting currency from database: " + ex.Message, "Currency converter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
                ShowDataInTable();
                ComboBoxFill(inputFrom);
                ComboBoxFill(inputTo);
            }
        }

        public void SetFieldsToNone()
        {
            tbName.Text = string.Empty;
            tbAmount.Text = string.Empty;
        }
    }
}
