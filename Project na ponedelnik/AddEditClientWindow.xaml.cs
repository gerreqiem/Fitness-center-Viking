using FitnessCenter_Viking;
using System;
using System.Windows;
namespace FitnessCenter_VikingApp
{
    public partial class AddEditClientWindow : Window
    {
        private ClientDTO _client;
        public event EventHandler<ClientDTO> ClientUpdated;
        public event EventHandler<string> StatusChanged;
        public event EventHandler<ClientDTO> ClientStatusChanged;
        public AddEditClientWindow(ClientDTO client)
        {
            InitializeComponent();
            _client = client;
            DataContext = _client;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            ClientUpdated?.Invoke(this, _client);
            StatusChanged?.Invoke(this, _client.ClientStatus);
            ClientStatusChanged?.Invoke(this, _client); 
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            StatusSelectionWindow statusSelectionWindow = new StatusSelectionWindow(_client.ClientStatus);
            if (statusSelectionWindow.ShowDialog() == true)
            {
                _client.ClientStatus = statusSelectionWindow.SelectedStatus;
            }
        }
    }
}