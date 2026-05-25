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
    public partial class MaterialEditWindow : Window
    {
        private readonly string _connString;
        private readonly int? _materialId;

        public MaterialEditWindow(int? id = null)
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            _materialId = id;
            if (id.HasValue)
                LoadMaterial();
        }

        private void LoadMaterial()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var cmd = new SqlCommand("SELECT Название, Категория, ЕдиницаИзмерения, ЦенаЗакупки, ЦенаПродажи, ТекущийОстаток, МинОстаток FROM Материалы WHERE ID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _materialId.Value);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader.GetString(0);
                        txtCategory.Text = reader.GetString(1);
                        txtUnit.Text = reader.GetString(2);
                        txtPurchasePrice.Text = reader.GetDecimal(3).ToString();
                        txtSellingPrice.Text = reader.GetDecimal(4).ToString();
                        txtCurrentStock.Text = reader.GetDecimal(5).ToString();
                        txtMinStock.Text = reader.GetDecimal(6).ToString();
                    }
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCategory.Text) || string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Заполните название, категорию и единицу измерения");
                return false;
            }
            if (!decimal.TryParse(txtPurchasePrice.Text, out decimal purchase) || purchase < 0)
            {
                MessageBox.Show("Цена закупки должна быть числом >=0");
                return false;
            }
            if (!decimal.TryParse(txtSellingPrice.Text, out decimal selling) || selling < 0)
            {
                MessageBox.Show("Цена продажи должна быть числом >=0");
                return false;
            }
            if (!decimal.TryParse(txtCurrentStock.Text, out decimal stock) || stock < 0)
            {
                MessageBox.Show("Текущий остаток должен быть числом >=0");
                return false;
            }
            if (!decimal.TryParse(txtMinStock.Text, out decimal min) || min < 0)
            {
                MessageBox.Show("Минимальный остаток должен быть числом >=0");
                return false;
            }
            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;
            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                if (_materialId.HasValue)
                {
                    var cmd = new SqlCommand(@"UPDATE Материалы SET 
                        Название = @name, Категория = @cat, ЕдиницаИзмерения = @unit,
                        ЦенаЗакупки = @purchase, ЦенаПродажи = @sell,
                        ТекущийОстаток = @stock, МинОстаток = @minStock
                        WHERE ID = @id", conn);
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@cat", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@unit", txtUnit.Text);
                    cmd.Parameters.AddWithValue("@purchase", decimal.Parse(txtPurchasePrice.Text));
                    cmd.Parameters.AddWithValue("@sell", decimal.Parse(txtSellingPrice.Text));
                    cmd.Parameters.AddWithValue("@stock", decimal.Parse(txtCurrentStock.Text));
                    cmd.Parameters.AddWithValue("@minStock", decimal.Parse(txtMinStock.Text));
                    cmd.Parameters.AddWithValue("@id", _materialId.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new SqlCommand(@"INSERT INTO Материалы 
                        (Название, Категория, ЕдиницаИзмерения, ЦенаЗакупки, ЦенаПродажи, ТекущийОстаток, МинОстаток)
                        VALUES (@name, @cat, @unit, @purchase, @sell, @stock, @minStock)", conn);
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@cat", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@unit", txtUnit.Text);
                    cmd.Parameters.AddWithValue("@purchase", decimal.Parse(txtPurchasePrice.Text));
                    cmd.Parameters.AddWithValue("@sell", decimal.Parse(txtSellingPrice.Text));
                    cmd.Parameters.AddWithValue("@stock", decimal.Parse(txtCurrentStock.Text));
                    cmd.Parameters.AddWithValue("@minStock", decimal.Parse(txtMinStock.Text));
                    cmd.ExecuteNonQuery();
                }
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