using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WorkshopMaster.Helpers
{
    public static class SimpleInputDialog
    {
        public static string Show(string prompt, string title = "Ввод данных", string defaultValue = "")
        {
            var window = new Window
            {
                Title = title,
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight
            };

            var textBlock = new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(10, 10, 10, 5)
            };

            var textBox = new TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(10, 5, 10, 10),
                MinWidth = 250
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(5),
                IsDefault = true
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 75,
                Margin = new Thickness(5),
                IsCancel = true
            };

            string result = null;
            okButton.Click += (s, e) => { result = textBox.Text; window.Close(); };
            cancelButton.Click += (s, e) => { result = null; window.Close(); };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            var mainPanel = new StackPanel();
            mainPanel.Children.Add(textBlock);
            mainPanel.Children.Add(textBox);
            mainPanel.Children.Add(buttonPanel);

            window.Content = mainPanel;
            window.ShowDialog();

            return result;
        }
    }
}