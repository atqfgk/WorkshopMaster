using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkshopMaster.Views;

namespace WorkshopMaster
{
    public partial class MainWindow : Window
    {
        private string _role;
        private int _userId;

        public MainWindow(string role, int userId)
        {
            InitializeComponent();
            _role = role;
            _userId = userId;
            ConfigureButtons();
        }

        private void ConfigureButtons()
        {
            if (_role == "Руководитель")
            {
                btnClients.Visibility = Visibility.Visible;
                btnOrders.Visibility = Visibility.Visible;
                btnMaterials.Visibility = Visibility.Visible;
                btnReports.Visibility = Visibility.Visible;
            }
            else if (_role == "Менеджер")
            {
                btnClients.Visibility = Visibility.Visible;
                btnOrders.Visibility = Visibility.Visible;
            }
            else if (_role == "Мастер")
            {
                btnMyTasks.Visibility = Visibility.Visible;
            }
            else if (_role == "Кладовщик")
            {
                btnMaterials.Visibility = Visibility.Visible;
            }
            else if (_role == "Дизайнер")
            {
                btnOrders.Visibility = Visibility.Visible;
            }
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            new ClientsWindow().ShowDialog();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrdersWindow(_role).ShowDialog();
        }

        private void BtnMyTasks_Click(object sender, RoutedEventArgs e)
        {
            new MyTasksWindow(_userId).ShowDialog();
        }

        private void BtnMaterials_Click(object sender, RoutedEventArgs e)
        {
            new MaterialsWindow(_role).ShowDialog();
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            new ReportsWindow().ShowDialog();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
