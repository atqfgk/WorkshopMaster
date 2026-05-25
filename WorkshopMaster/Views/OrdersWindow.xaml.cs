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
    public partial class OrdersWindow : Window
    {
        private readonly string _connString;
        private readonly string _role;
        private DataTable _ordersTable;

        public OrdersWindow(string role)
        {
            InitializeComponent();
            _role = role;
            _connString = DatabaseHelper.GetConnectionString();
            btnCreateOrder.Visibility = (role == "Руководитель" || role == "Менеджер") ? Visibility.Visible : Visibility.Collapsed;
            cmbStatus.SelectedIndex = 0;
            LoadOrders();
        }

        private void LoadOrders()
        {
            string status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
            string sql = @"
                SELECT z.ID, z.НомерЗаказа, к.ФИО AS Клиент, z.ДатаПриема, z.ДатаИсполненияПлан, z.Статус
                FROM Заказы z
                JOIN Клиенты к ON z.IDКлиента = к.ID";
            if (status != "Все")
                sql += " WHERE z.Статус = @status";
            sql += " ORDER BY z.ДатаПриема DESC";

            using (var conn = new SqlConnection(_connString))
            using (var adapter = new SqlDataAdapter(sql, conn))
            {
                if (status != "Все")
                    adapter.SelectCommand.Parameters.AddWithValue("@status", status);
                _ordersTable = new DataTable();
                adapter.Fill(_ordersTable);
                dgOrders.ItemsSource = _ordersTable.DefaultView;
            }
        }

        private void CmbStatus_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => LoadOrders();

        private void BtnCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreateOrderWindow();
            if (win.ShowDialog() == true)
                LoadOrders();
        }

        private void BtnOpenOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem == null) return;
            var row = ((DataRowView)dgOrders.SelectedItem).Row;
            int orderId = Convert.ToInt32(row["ID"]);
            var win = new OrderDetailsWindow(orderId, _role);
            win.ShowDialog();
            LoadOrders();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
