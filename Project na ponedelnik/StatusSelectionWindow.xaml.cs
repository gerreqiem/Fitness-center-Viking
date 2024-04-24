using System;
using System.Windows;
using System.Windows.Controls;
namespace FitnessCenter_VikingApp
{
    public partial class StatusSelectionWindow : Window
    {
        public event EventHandler<string> StatusSelected;
        public string SelectedStatus { get; set; }
        public StatusSelectionWindow(string currentStatus)
        {
            InitializeComponent();
            CurrentStatusTextBlock.Text = $"Текущий статус: {currentStatus}";
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem != null)
            {
                SelectedStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                StatusSelected?.Invoke(this, SelectedStatus);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите статус.");
            }
        }
    }
}