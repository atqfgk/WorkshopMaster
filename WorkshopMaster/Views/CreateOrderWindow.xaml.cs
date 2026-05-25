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
using WorkshopMaster.WPF.Models;

namespace WorkshopMaster.Views
{
    public partial class CreateOrderWindow : Window
    {
        private readonly string _connString;
        private DataTable _clientsTable;
        private DataTable _stagesTable;
        private DataTable _employeesTable;
        private DataTable _materialsTable;
        private List<PlanStage> _stages = new List<PlanStage>();
        private List<dynamic> _materialItems = new List<dynamic>();

        public CreateOrderWindow()
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            LoadClients();
            LoadStagesAndEmployees();
            LoadMaterials();
        }

        private void LoadClients()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var adapter = new SqlDataAdapter("SELECT ID, ФИО FROM Клиенты ORDER BY ФИО", conn);
                _clientsTable = new DataTable();
                adapter.Fill(_clientsTable);
                cmbClient.ItemsSource = _clientsTable.DefaultView;
                cmbClient.DisplayMemberPath = "ФИО";
                cmbClient.SelectedValuePath = "ID";
            }
        }

        private void LoadStagesAndEmployees()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var adapterStages = new SqlDataAdapter("SELECT ID, Название FROM ЭтапыРабот", conn);
                _stagesTable = new DataTable();
                adapterStages.Fill(_stagesTable);
                cmbStage.ItemsSource = _stagesTable.DefaultView;
                cmbStage.DisplayMemberPath = "Название";
                cmbStage.SelectedValuePath = "ID";

                var adapterEmps = new SqlDataAdapter("SELECT ID, ФИО FROM Сотрудники WHERE Должность = 'мастер'", conn);
                _employeesTable = new DataTable();
                adapterEmps.Fill(_employeesTable);
                cmbEmployee.ItemsSource = _employeesTable.DefaultView;
                cmbEmployee.DisplayMemberPath = "ФИО";
                cmbEmployee.SelectedValuePath = "ID";
            }
        }

        private void LoadMaterials()
        {
            using (var conn = new SqlConnection(_connString))
            {
                var adapter = new SqlDataAdapter("SELECT ID, Название, ЕдиницаИзмерения, ЦенаПродажи FROM Материалы ORDER BY Название", conn);
                _materialsTable = new DataTable();
                adapter.Fill(_materialsTable);
                cmbMaterial.ItemsSource = _materialsTable.DefaultView;
                cmbMaterial.DisplayMemberPath = "Название";
                cmbMaterial.SelectedValuePath = "ID";
            }
        }

        private void AddStage_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStage.SelectedValue == null || cmbEmployee.SelectedValue == null) return;
            int stageId = (int)cmbStage.SelectedValue;
            int empId = (int)cmbEmployee.SelectedValue;
            DateTime planStart = dpPlanStart.SelectedDate ?? DateTime.Now;
            DateTime planEnd = dpPlanEnd.SelectedDate ?? DateTime.Now.AddDays(1);
            if (!decimal.TryParse(txtHours.Text, out decimal hours) || hours <= 0)
            {
                MessageBox.Show("Введите корректное количество часов (>0)");
                return;
            }
            if (planStart > planEnd)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания");
                return;
            }

            string stageName = _stagesTable.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == stageId)?.Field<string>("Название");
            string empName = _employeesTable.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == empId)?.Field<string>("ФИО");
            _stages.Add(new PlanStage
            {
                IDЭтапа = stageId,
                IDСотрудника = empId,
                ДатаНачалаПлан = planStart,
                ДатаОкончанияПлан = planEnd,
                ПлановыеЧасы = hours
            });
            dgStages.ItemsSource = null;
            dgStages.ItemsSource = _stages.Select(s => new
            {
                StageName = _stagesTable.AsEnumerable().First(r => r.Field<int>("ID") == s.IDЭтапа).Field<string>("Название"),
                EmployeeName = _employeesTable.AsEnumerable().First(r => r.Field<int>("ID") == s.IDСотрудника).Field<string>("ФИО"),
                PlanStart = s.ДатаНачалаПлан.ToShortDateString(),
                PlanEnd = s.ДатаОкончанияПлан.ToShortDateString(),
                Hours = s.ПлановыеЧасы
            }).ToList();
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaterial.SelectedValue == null) return;
            int materialId = (int)cmbMaterial.SelectedValue;
            if (!decimal.TryParse(txtMaterialQty.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество (>0)");
                return;
            }
            var row = _materialsTable.AsEnumerable().First(r => r.Field<int>("ID") == materialId);
            string name = row.Field<string>("Название");
            string unit = row.Field<string>("ЕдиницаИзмерения");
            decimal price = row.Field<decimal>("ЦенаПродажи");
            _materialItems.Add(new { Name = name, Quantity = qty, Unit = unit, Price = price, Sum = qty * price });
            dgMaterials.ItemsSource = null;
            dgMaterials.ItemsSource = _materialItems;
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Введите описание изделия");
                return;
            }
            if (_stages.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один этап");
                return;
            }

            string orderNumber = $"З-{DateTime.Now:yyyy}-{new Random().Next(100, 999)}";
            int clientId = (int)cmbClient.SelectedValue;
            DateTime receiveDate = DateTime.Now;
            DateTime dueDate = dpDueDate.SelectedDate ?? DateTime.Now.AddDays(14);
            string description = txtDescription.Text;

            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var cmdOrder = new SqlCommand(@"
                            INSERT INTO Заказы (НомерЗаказа, IDКлиента, ДатаПриема, ДатаИсполненияПлан, ОписаниеИзделия, Статус, ПредварительнаяСтоимость)
                            VALUES (@num, @clientId, @receiveDate, @dueDate, @desc, 'черновик', @prelim);
                            SELECT SCOPE_IDENTITY();", conn, transaction);
                        decimal prelim = _materialItems.Sum(m => (decimal)m.Sum) + _stages.Sum(s => s.ПлановыеЧасы * 400); // ставка 400 руб/час упрощённо
                        cmdOrder.Parameters.AddWithValue("@num", orderNumber);
                        cmdOrder.Parameters.AddWithValue("@clientId", clientId);
                        cmdOrder.Parameters.AddWithValue("@receiveDate", receiveDate);
                        cmdOrder.Parameters.AddWithValue("@dueDate", dueDate);
                        cmdOrder.Parameters.AddWithValue("@desc", description);
                        cmdOrder.Parameters.AddWithValue("@prelim", prelim);
                        int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                        foreach (var stage in _stages)
                        {
                            var cmdStage = new SqlCommand(@"
                                INSERT INTO ПлановыеЭтапыЗаказа (IDЗаказа, IDЭтапа, IDСотрудника, ДатаНачалаПлан, ДатаОкончанияПлан, ПлановыеЧасы)
                                VALUES (@orderId, @stageId, @empId, @start, @end, @hours)", conn, transaction);
                            cmdStage.Parameters.AddWithValue("@orderId", orderId);
                            cmdStage.Parameters.AddWithValue("@stageId", stage.IDЭтапа);
                            cmdStage.Parameters.AddWithValue("@empId", stage.IDСотрудника);
                            cmdStage.Parameters.AddWithValue("@start", stage.ДатаНачалаПлан);
                            cmdStage.Parameters.AddWithValue("@end", stage.ДатаОкончанияПлан);
                            cmdStage.Parameters.AddWithValue("@hours", stage.ПлановыеЧасы);
                            cmdStage.ExecuteNonQuery();
                        }

                        foreach (var mat in _materialItems)
                        {
                            var cmdMat = new SqlCommand(@"
                                INSERT INTO СписанияМатериалов (IDЗаказа, IDПлановогоЭтапа, IDМатериала, ДатаСписания, Количество, ЦенаНаМоментСписания)
                                VALUES (@orderId, NULL, @materialId, @date, @qty, @price)", conn, transaction);
                            cmdMat.Parameters.AddWithValue("@orderId", orderId);
                            cmdMat.Parameters.AddWithValue("@materialId", _materialsTable.AsEnumerable().First(r => r.Field<string>("Название") == mat.Name).Field<int>("ID"));
                            cmdMat.Parameters.AddWithValue("@date", DateTime.Now);
                            cmdMat.Parameters.AddWithValue("@qty", mat.Quantity);
                            cmdMat.Parameters.AddWithValue("@price", mat.Price);
                            cmdMat.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        MessageBox.Show($"Заказ {orderNumber} создан");
                        DialogResult = true;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}