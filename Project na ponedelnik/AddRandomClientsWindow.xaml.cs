using System.Windows;
namespace FitnessCenter_VikingApp
{
    public partial class AddRandomClientsWindow : Window
    {
        public int NumberOfClients { get; private set; }
        public AddRandomClientsWindow()
        {
            InitializeComponent();
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumberOfClientsTextBox.Text, out int result))
            {
                NumberOfClients = result;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Неверный ввод. Пожалуйста, введите действительный номер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
