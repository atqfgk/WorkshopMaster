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
using System.Windows.Shapes;

namespace WorkshopMaster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            cmbRole.SelectedIndex = 0;
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var selected = cmbRole.SelectedItem as ComboBoxItem;
            if (selected == null) return;
            string role = selected.Content.ToString();
            int userId = 1; // упрощённо; в реальном проекте – по логину/паролю из БД
            var main = new MainWindow(role, userId);
            main.Show();
            this.Close();
        }
    }
}
