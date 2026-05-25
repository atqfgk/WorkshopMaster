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
using Microsoft.VisualBasic;
using WorkshopMaster.Helpers;
using WorkshopMaster.WPF.Helpers;

namespace WorkshopMaster.Views
{
    public partial class MyTasksWindow : Window
    {
        private readonly string _connString;
        private readonly int _masterId;
        private DataTable _tasksTable;

        public MyTasksWindow(int masterId)
        {
            InitializeComponent();
            _connString = DatabaseHelper.GetConnectionString();
            _masterId = masterId;
            LoadTasks();
        }

        private void LoadTasks()
        {
            string sql = @"
                SELECT пл.ID AS PlanStageId, з.НомерЗаказа, э.Название AS Этап,
                       CONVERT(varchar, пл.ДатаНачалаПлан, 104) + ' - ' + CONVERT(varchar, пл.ДатаОкончанияПлан, 104) AS ПланДаты,
                       ISNULL(ф.Статус, 'назначен') AS Статус,
                       CASE WHEN ISNULL(ф.Статус, 'назначен') = 'назначен' THEN 'Начать'
                            WHEN ф.Статус = 'в_работе' THEN 'Завершить'
                            ELSE '—' END AS Кнопка
                FROM ПлановыеЭтапыЗаказа пл
                JOIN Заказы з ON пл.IDЗаказа = з.ID
                JOIN ЭтапыРабот э ON пл.IDЭтапа = э.ID
                LEFT JOIN ФактическиеЭтапыЗаказа ф ON пл.ID = ф.IDПлановогоЭтапа
                WHERE пл.IDСотрудника = @masterId
                ORDER BY пл.ДатаНачалаПлан";
            using (var conn = new SqlConnection(_connString))
            using (var adapter = new SqlDataAdapter(sql, conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@masterId", _masterId);
                _tasksTable = new DataTable();
                adapter.Fill(_tasksTable);
                dgTasks.ItemsSource = _tasksTable.DefaultView;
            }
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            int planStageId = (int)btn.Tag;
            var row = ((DataRowView)btn.DataContext).Row;
            string currentStatus = row["Статус"].ToString();
            if (currentStatus == "назначен")
            {
                using (var conn = new SqlConnection(_connString))
                {
                    var cmd = new SqlCommand(@"
                        IF NOT EXISTS (SELECT 1 FROM ФактическиеЭтапыЗаказа WHERE IDПлановогоЭтапа = @id)
                            INSERT INTO ФактическиеЭтапыЗаказа (IDПлановогоЭтапа, ДатаНачалаФакт, Статус)
                            VALUES (@id, GETDATE(), 'в_работе')
                        ELSE
                            UPDATE ФактическиеЭтапыЗаказа SET ДатаНачалаФакт = GETDATE(), Статус = 'в_работе'
                            WHERE IDПлановогоЭтапа = @id", conn);
                    cmd.Parameters.AddWithValue("@id", planStageId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                LoadTasks();
            }
            else if (currentStatus == "в_работе")
            {
                string hoursStr = SimpleInputDialog.Show("Введите фактические часы", "Завершение этапа", "0");
                if (decimal.TryParse(hoursStr, out decimal hours) && hours > 0)
                {
                    using (var conn = new SqlConnection(_connString))
                    {
                        var cmd = new SqlCommand(@"
                            UPDATE ФактическиеЭтапыЗаказа 
                            SET ДатаОкончанияФакт = GETDATE(), ФактическиеЧасы = @hours, Статус = 'выполнен'
                            WHERE IDПлановогоЭтапа = @id", conn);
                        cmd.Parameters.AddWithValue("@id", planStageId);
                        cmd.Parameters.AddWithValue("@hours", hours);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    LoadTasks();
                }
                else MessageBox.Show("Некорректное количество часов");
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadTasks();
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}