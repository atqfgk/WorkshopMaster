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
    public partial class ReportsWindow : Window
    {
        private readonly string _connString;
        public ReportsWindow()
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            LoadReport();
        }

        private void LoadReport()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var adapter = new SqlDataAdapter("SELECT * FROM vw_СтатусЗаказов ORDER BY ДатаПриема DESC", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgReport.ItemsSource = dt.DefaultView;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadReport();
    }
}