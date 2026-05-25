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
    public partial class MaterialsWindow : Window
    {
        private readonly string _connString;
        private readonly string _role;
        private DataTable _materialsTable;

        public MaterialsWindow(string role)
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            _role = role;
            LoadMaterials();
            if (role == "Руководитель" || role == "Кладовщик")
            {
                btnAdd.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
                btnReceipt.Visibility = Visibility.Visible;
            }
        }

        private void LoadMaterials()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var adapter = new SqlDataAdapter("SELECT ID, Название, Категория, ЕдиницаИзмерения, ЦенаПродажи, ТекущийОстаток, МинОстаток FROM Материалы ORDER BY Название", conn);
                _materialsTable = new DataTable();
                adapter.Fill(_materialsTable);
                dgMaterials.ItemsSource = _materialsTable.DefaultView;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MaterialEditWindow();
            if (dialog.ShowDialog() == true)
                LoadMaterials();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterials.SelectedItem == null) return;
            var row = ((DataRowView)dgMaterials.SelectedItem).Row;
            int id = Convert.ToInt32(row["ID"]);
            var dialog = new MaterialEditWindow(id);
            if (dialog.ShowDialog() == true)
                LoadMaterials();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterials.SelectedItem == null) return;
            var row = ((DataRowView)dgMaterials.SelectedItem).Row;
            int id = Convert.ToInt32(row["ID"]);
            if (MessageBox.Show("Удалить материал?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var conn = new SqlConnection(_connString))
                {
                    var cmd = new SqlCommand("DELETE FROM Материалы WHERE ID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadMaterials();
            }
        }

        private void Receipt_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MaterialReceiptWindow();
            dialog.ShowDialog();
            LoadMaterials();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
