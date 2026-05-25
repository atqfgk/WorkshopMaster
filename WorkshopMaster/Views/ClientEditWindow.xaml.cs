using System;
using System.Collections.Generic;
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
    public partial class ClientEditWindow : Window
    {
        private readonly string _connString;
        private readonly int? _clientId;

        public ClientEditWindow(int? id = null)
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            _clientId = id;
            if (id.HasValue)
                LoadClient();
        }

        private void LoadClient()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var cmd = new SqlCommand("SELECT ФИО, Телефон, Email, АдресДоставки FROM Клиенты WHERE ID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _clientId.Value);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtFIO.Text = reader.GetString(0);
                        txtPhone.Text = reader.GetString(1);
                        txtEmail.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        txtAddress.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFIO.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("ФИО и телефон обязательны", "Ошибка");
                return;
            }
            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                if (_clientId.HasValue)
                {
                    var cmd = new SqlCommand("UPDATE Клиенты SET ФИО=@fio, Телефон=@phone, Email=@email, АдресДоставки=@address WHERE ID=@id", conn);
                    cmd.Parameters.AddWithValue("@fio", txtFIO.Text);
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);
                    cmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(txtAddress.Text) ? (object)DBNull.Value : txtAddress.Text);
                    cmd.Parameters.AddWithValue("@id", _clientId.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new SqlCommand("INSERT INTO Клиенты (ФИО, Телефон, Email, АдресДоставки) VALUES (@fio, @phone, @email, @address)", conn);
                    cmd.Parameters.AddWithValue("@fio", txtFIO.Text);
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text);
                    cmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(txtAddress.Text) ? (object)DBNull.Value : txtAddress.Text);
                    cmd.ExecuteNonQuery();
                }
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
