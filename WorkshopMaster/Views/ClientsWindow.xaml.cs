using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using WorkshopMaster.WPF.Helpers;

namespace WorkshopMaster.Views
{
    public partial class ClientsWindow : Window
    {
        private readonly string _connString;
        private DataTable _clientsTable;

        public ClientsWindow()
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            LoadClients();
        }

        private void LoadClients(string filter = "")
        {
            string sql = "SELECT ID, ФИО, Телефон, Email, АдресДоставки FROM Клиенты";
            if (!string.IsNullOrWhiteSpace(filter))
                sql += " WHERE ФИО LIKE @filter OR Телефон LIKE @filter";
            using (var conn = new SqlConnection(_connString))
            using (var adapter = new SqlDataAdapter(sql, conn))
            {
                if (!string.IsNullOrWhiteSpace(filter))
                    adapter.SelectCommand.Parameters.AddWithValue("@filter", $"%{filter}%");
                _clientsTable = new DataTable();
                adapter.Fill(_clientsTable);
                dgClients.ItemsSource = _clientsTable.DefaultView;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadClients(txtSearch.Text);

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var win = new ClientEditWindow();
            if (win.ShowDialog() == true)
                LoadClients(txtSearch.Text);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgClients.SelectedItem == null) return;
            var row = ((DataRowView)dgClients.SelectedItem).Row;
            int id = Convert.ToInt32(row["ID"]);
            var win = new ClientEditWindow(id);
            if (win.ShowDialog() == true)
                LoadClients(txtSearch.Text);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgClients.SelectedItem == null) return;
            var row = ((DataRowView)dgClients.SelectedItem).Row;
            int id = Convert.ToInt32(row["ID"]);
            if (MessageBox.Show("Удалить клиента?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var conn = new SqlConnection(_connString))
                {
                    var cmd = new SqlCommand("DELETE FROM Клиенты WHERE ID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadClients(txtSearch.Text);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
