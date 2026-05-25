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
    public partial class OrderDetailsWindow : Window
    {
        private readonly string _connString;
        private readonly int _orderId;
        private readonly string _role;
        private DataTable _stagesTable;
        private DataTable _employeesTable;
        private DataTable _materialsTable;
        private List<StageDetail> _stages = new List<StageDetail>();
        private List<MaterialDetail> _materials = new List<MaterialDetail>();

        public OrderDetailsWindow(int orderId, string role)
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            _orderId = orderId;
            _role = role;
            LoadOrderInfo();
            LoadStages();
            LoadMaterials();

            bool canEdit = (role == "Руководитель" || role == "Менеджер" || role == "Дизайнер");
            if (canEdit)
            {
                btnAddStage.Visibility = Visibility.Visible;
                btnDeleteStage.Visibility = Visibility.Visible;
                btnAddMaterial.Visibility = Visibility.Visible;
                btnDeleteMaterial.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
            }
            if (role == "Руководитель" || role == "Менеджер")
            {
                cmbStatus.IsEnabled = true;
                txtDescription.IsReadOnly = false;
                dpPlanDate.IsEnabled = true;
            }
            if (role == "Дизайнер")
            {
                cmbStatus.IsEnabled = false;
                txtDescription.IsReadOnly = true;
                dpPlanDate.IsEnabled = false;
            }
        }

        private void LoadOrderInfo()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var cmd = new SqlCommand("SELECT НомерЗаказа, IDКлиента, (SELECT ФИО FROM Клиенты WHERE ID = Заказы.IDКлиента) AS Клиент, ДатаПриема, ДатаИсполненияПлан, ОписаниеИзделия, Статус FROM Заказы WHERE ID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _orderId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblOrderNumber.Text = reader["НомерЗаказа"].ToString();
                        lblClient.Text = reader["Клиент"].ToString();
                        txtDescription.Text = reader["ОписаниеИзделия"].ToString();
                        dpPlanDate.SelectedDate = reader.GetDateTime(4);
                        string status = reader["Статус"].ToString();
                        cmbStatus.ItemsSource = new[] { "черновик", "утверждён", "в_работе", "выполнен", "закрыт" };
                        cmbStatus.SelectedItem = status;
                    }
                }
            }
        }

        private void LoadStages()
        {
            _stages.Clear();
            using (var conn = new SqlConnection(_connString))
            {
                var cmd = new SqlCommand(@"
                    SELECT пл.ID, пл.IDЭтапа, пл.IDСотрудника, пл.ДатаНачалаПлан, пл.ДатаОкончанияПлан, пл.ПлановыеЧасы,
                           ISNULL(ф.Статус, 'назначен') AS Статус, ф.ФактическиеЧасы
                    FROM ПлановыеЭтапыЗаказа пл
                    LEFT JOIN ФактическиеЭтапыЗаказа ф ON пл.ID = ф.IDПлановогоЭтапа
                    WHERE пл.IDЗаказа = @orderId", conn);
                cmd.Parameters.AddWithValue("@orderId", _orderId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _stages.Add(new StageDetail
                        {
                            ID = reader.GetInt32(0),
                            StageId = reader.GetInt32(1),
                            EmployeeId = reader.GetInt32(2),
                            PlanStart = reader.GetDateTime(3),
                            PlanEnd = reader.GetDateTime(4),
                            Hours = reader.GetDecimal(5),
                            Status = reader.GetString(6),
                            ActualHours = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7)
                        });
                    }
                }
            }
            LoadStagesGrid();
        }

        private void LoadStagesGrid()
        {
            var list = _stages.Select(s => new
            {
                StageName = GetStageName(s.StageId),
                EmployeeName = GetEmployeeName(s.EmployeeId),
                PlanStart = s.PlanStart.ToShortDateString(),
                PlanEnd = s.PlanEnd.ToShortDateString(),
                Hours = s.Hours,
                Status = s.Status,
                ActualHours = s.ActualHours?.ToString() ?? ""
            }).ToList();
            dgStages.ItemsSource = list;
        }

        private void LoadMaterials()
        {
            _materials.Clear();
            using (var conn = new SqlConnection(_connString))
            {
                var cmd = new SqlCommand(@"
                    SELECT sm.ID, sm.IDМатериала, sm.Количество, sm.ЦенаНаМоментСписания, м.Название, м.ЕдиницаИзмерения
                    FROM СписанияМатериалов sm
                    JOIN Материалы м ON sm.IDМатериала = м.ID
                    WHERE sm.IDЗаказа = @orderId", conn);
                cmd.Parameters.AddWithValue("@orderId", _orderId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _materials.Add(new MaterialDetail
                        {
                            ID = reader.GetInt32(0),
                            MaterialId = reader.GetInt32(1),
                            Quantity = reader.GetDecimal(2),
                            Price = reader.GetDecimal(3),
                            Name = reader.GetString(4),
                            Unit = reader.GetString(5)
                        });
                    }
                }
            }
            LoadMaterialsGrid();
        }

        private void LoadMaterialsGrid()
        {
            var list = _materials.Select(m => new
            {
                m.Name,
                m.Quantity,
                m.Unit,
                m.Price,
                Sum = m.Quantity * m.Price
            }).ToList();
            dgMaterials.ItemsSource = list;
        }

        private string GetStageName(int stageId)
        {
            if (_stagesTable == null)
            {
                using (var conn = new SqlConnection(_connString))
                {
                    var adapter = new SqlDataAdapter("SELECT ID, Название FROM ЭтапыРабот", conn);
                    _stagesTable = new DataTable();
                    adapter.Fill(_stagesTable);
                }
            }
            var row = _stagesTable.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == stageId);
            return row?.Field<string>("Название") ?? "";
        }

        private string GetEmployeeName(int empId)
        {
            if (_employeesTable == null)
            {
                using (var conn = new SqlConnection(_connString))
                {
                    var adapter = new SqlDataAdapter("SELECT ID, ФИО FROM Сотрудники", conn);
                    _employeesTable = new DataTable();
                    adapter.Fill(_employeesTable);
                }
            }
            var row = _employeesTable.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == empId);
            return row?.Field<string>("ФИО") ?? "";
        }

        private void AddStage_Click(object sender, RoutedEventArgs e)
        {
            // Открыть диалог добавления этапа (аналогично CreateOrderWindow)
            var dialog = new AddStageDialog(_orderId);
            if (dialog.ShowDialog() == true)
                LoadStages();
        }

        private void DeleteStage_Click(object sender, RoutedEventArgs e)
        {
            if (dgStages.SelectedItem == null) return;
            dynamic selected = dgStages.SelectedItem;
            var stage = _stages.FirstOrDefault(s => GetStageName(s.StageId) == selected.StageName && GetEmployeeName(s.EmployeeId) == selected.EmployeeName);
            if (stage != null)
            {
                if (MessageBox.Show("Удалить этап?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = new SqlConnection(_connString))
                    {
                        var cmd = new SqlCommand("DELETE FROM ПлановыеЭтапыЗаказа WHERE ID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", stage.ID);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    LoadStages();
                }
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            // Диалог добавления материала к заказу
            var dialog = new AddMaterialToOrderDialog(_orderId);
            if (dialog.ShowDialog() == true)
                LoadMaterials();
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterials.SelectedItem == null) return;
            dynamic selected = dgMaterials.SelectedItem;
            var material = _materials.FirstOrDefault(m => m.Name == selected.Name && m.Quantity == selected.Quantity);
            if (material != null)
            {
                if (MessageBox.Show("Удалить материал из заказа?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = new SqlConnection(_connString))
                    {
                        var cmd = new SqlCommand("DELETE FROM СписанияМатериалов WHERE ID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", material.ID);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    LoadMaterials();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Заказы SET ОписаниеИзделия = @desc, ДатаИсполненияПлан = @planDate, Статус = @status WHERE ID = @id", conn);
                cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
                cmd.Parameters.AddWithValue("@planDate", dpPlanDate.SelectedDate.Value);
                cmd.Parameters.AddWithValue("@status", cmbStatus.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@id", _orderId);
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Изменения сохранены");
            LoadOrderInfo();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }

    internal class StageDetail
    {
        public int ID { get; set; }
        public int StageId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime PlanStart { get; set; }
        public DateTime PlanEnd { get; set; }
        public decimal Hours { get; set; }
        public string Status { get; set; }
        public decimal? ActualHours { get; set; }
    }

    internal class MaterialDetail
    {
        public int ID { get; set; }
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
    }
}