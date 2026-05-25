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
    public partial class AddMaterialToOrderDialog : Window
    {
        private readonly int _orderId;
        private DataTable _materialsTable;
        public AddMaterialToOrderDialog(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            using (var conn = new SqlConnection(DatabaseHelper.GetConnectionString()))
            {
                var da = new SqlDataAdapter("SELECT ID, Название, ЦенаПродажи FROM Материалы", conn);
                _materialsTable = new DataTable();
                da.Fill(_materialsTable);
                cmbMaterial.ItemsSource = _materialsTable.DefaultView;
                cmbMaterial.DisplayMemberPath = "Название";
                cmbMaterial.SelectedValuePath = "ID";
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaterial.SelectedValue == null || !decimal.TryParse(txtQuantity.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Выберите материал и корректное количество");
                return;
            }
            int materialId = (int)cmbMaterial.SelectedValue;
            decimal price = 0;
            foreach (DataRowView row in cmbMaterial.ItemsSource)
            {
                if ((int)row["ID"] == materialId)
                {
                    price = Convert.ToDecimal(row["ЦенаПродажи"]);
                    break;
                }
            }
            using (var conn = new SqlConnection(DatabaseHelper.GetConnectionString()))
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO СписанияМатериалов (IDЗаказа, IDМатериала, ДатаСписания, Количество, ЦенаНаМоментСписания)
                    VALUES (@orderId, @materialId, GETDATE(), @qty, @price)", conn);
                cmd.Parameters.AddWithValue("@orderId", _orderId);
                cmd.Parameters.AddWithValue("@materialId", materialId);
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@price", price);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}