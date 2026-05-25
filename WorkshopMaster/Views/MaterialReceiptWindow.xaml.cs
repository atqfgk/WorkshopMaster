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
    public partial class MaterialReceiptWindow : Window
    {
        private readonly string _connString;
        private DataTable _materialsTable;
        private List<ReceiptItem> _items = new List<ReceiptItem>();

        public MaterialReceiptWindow()
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var adapter = new SqlDataAdapter("SELECT ID, Название FROM Материалы ORDER BY Название", conn);
                _materialsTable = new DataTable();
                adapter.Fill(_materialsTable);
                cmbMaterial.ItemsSource = _materialsTable.DefaultView;
                cmbMaterial.DisplayMemberPath = "Название";
                cmbMaterial.SelectedValuePath = "ID";
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaterial.SelectedValue == null)
            {
                MessageBox.Show("Выберите материал");
                return;
            }
            if (!decimal.TryParse(txtQuantity.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Количество должно быть положительным числом");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом");
                return;
            }
            int materialId = (int)cmbMaterial.SelectedValue;
            string materialName = cmbMaterial.Text;
            _items.Add(new ReceiptItem
            {
                MaterialId = materialId,
                MaterialName = materialName,
                Quantity = qty,
                Price = price,
                Sum = qty * price
            });
            RefreshGrid();
            txtQuantity.Text = "0";
            txtPrice.Text = "0";
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn?.Tag as ReceiptItem;
            if (item != null)
            {
                _items.Remove(item);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            dgItems.ItemsSource = null;
            dgItems.ItemsSource = _items;
        }

        private void SaveReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
            {
                MessageBox.Show("Введите номер накладной");
                return;
            }
            if (_items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну позицию");
                return;
            }
            try
            {
                using (var conn = new SqlConnection(_connString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        decimal totalSum = _items.Sum(i => i.Sum);
                        var cmdHeader = new SqlCommand(@"
                            INSERT INTO ПоступленияМатериалов (IDПоставщика, Дата, НомерНакладной, Сумма)
                            VALUES (@supplierId, @date, @invoice, @sum);
                            SELECT SCOPE_IDENTITY();", conn, transaction);
                        cmdHeader.Parameters.AddWithValue("@supplierId", GetOrCreateSupplier(conn, transaction));
                        cmdHeader.Parameters.AddWithValue("@date", dpReceiptDate.SelectedDate ?? DateTime.Now);
                        cmdHeader.Parameters.AddWithValue("@invoice", txtInvoiceNumber.Text);
                        cmdHeader.Parameters.AddWithValue("@sum", totalSum);
                        int receiptId = Convert.ToInt32(cmdHeader.ExecuteScalar());

                        foreach (var item in _items)
                        {
                            var cmdItem = new SqlCommand(@"
                                INSERT INTO ПозицииПоступления (IDПоступления, IDМатериала, Количество, ЦенаЗакупки)
                                VALUES (@receiptId, @materialId, @qty, @price)", conn, transaction);
                            cmdItem.Parameters.AddWithValue("@receiptId", receiptId);
                            cmdItem.Parameters.AddWithValue("@materialId", item.MaterialId);
                            cmdItem.Parameters.AddWithValue("@qty", item.Quantity);
                            cmdItem.Parameters.AddWithValue("@price", item.Price);
                            cmdItem.ExecuteNonQuery();

                            // Обновляем остаток и закупочную цену материала (средневзвешенная)
                            var cmdUpdate = new SqlCommand(@"
                                UPDATE Материалы 
                                SET ТекущийОстаток = ТекущийОстаток + @qty,
                                    ЦенаЗакупки = (ЦенаЗакупки * (ТекущийОстаток - @qty) + @qty * @price) / ТекущийОстаток
                                WHERE ID = @materialId", conn, transaction);
                            cmdUpdate.Parameters.AddWithValue("@qty", item.Quantity);
                            cmdUpdate.Parameters.AddWithValue("@price", item.Price);
                            cmdUpdate.Parameters.AddWithValue("@materialId", item.MaterialId);
                            cmdUpdate.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                MessageBox.Show("Приход сохранён");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private int GetOrCreateSupplier(SqlConnection conn, SqlTransaction transaction)
        {
            const string supplierName = "Основной поставщик";
            var cmdSelect = new SqlCommand("SELECT ID FROM Поставщики WHERE Название = @name", conn, transaction);
            cmdSelect.Parameters.AddWithValue("@name", supplierName);
            var result = cmdSelect.ExecuteScalar();
            if (result != null)
                return (int)result;
            var cmdInsert = new SqlCommand("INSERT INTO Поставщики (Название) VALUES (@name); SELECT SCOPE_IDENTITY();", conn, transaction);
            cmdInsert.Parameters.AddWithValue("@name", supplierName);
            return Convert.ToInt32(cmdInsert.ExecuteScalar());
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ReceiptItem
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get; set; }
    }
}