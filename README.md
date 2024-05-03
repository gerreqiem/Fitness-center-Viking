Описание проекта

Мы разработали программу, которая содержит в себе сведения о клиентах фитнес центре. С помощью этой программы можно удобно хранить информацию о том, у какого клиента какой статус в фитнес центре. Программа может хранить ФИО, дату рождения, номер телефона клиента. Так же там отображается срок годности абонемента. Пользователь может удалять, добавлять, изменять статус клиента (всего три статуса: клиент, бизнес-клиент, вип-клиент). Пользователь может поменять статус клиента на любой другой.

Интерфейс проекта

Открывая приложение, вы находитесь в главном меню. Тут хранятся записи о клиентах фитнес центра и показывается информация о каждом клиенте. Так же тут расположены четыре кнопки, о которых будет рассказано чуть ниже.  

![Рисунок1](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/f3ac8366-b855-49d7-97aa-a89eb7e87d2c)

-Главное меню приложения-


При нажатии на кнопку “Добавить клиента” открывается окно, где заносятся данные о клиенте, такие как: ФИО, дата рождения, номер телефона и статус пропуска.

![Рисунок2](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/ada0994c-3f1c-4813-8d4d-ed4c6d0f54e9)
 
-Окно добавления клиента-


Для изменения клиента мы добавили кнопку “Редактирование клиента”, она предназначена для редактирования уже существующего клиента.

![Рисунок3](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/cb67ff6a-a49e-4b66-abf9-dfb2c056732b)
 
-Окно изменения клиента- 


Если клиент перестает ходить в фитнес центр, мы будем использовать кнопку “Удалить клиента”, за два простых клика он исчезает. 

![Рисунок4](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/1687246c-27ec-4397-843a-79784e8504f3)

![Рисунок5](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/5774ee86-c395-43c3-922e-90d83746b3f1)

-Кнопка “Удалить клиента”-


![Рисунок6](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/5df8744b-6106-4956-8d0f-3a1153afd889)

![Рисунок7](https://github.com/gerreqiem/Fitness-center-Viking/assets/167652940/96094145-4110-4172-bdff-a6967a7d03d4)

-Добавить случайных клиентов-

Она может понадобиться, если, например, нужно сделать вид что у вас есть клиенты. 

Объяснение некоторых участков кода

Этот участок кода представляет собой часть класса StatusSelectionWindow, который является окном выбора статуса.

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


Этот код представляет часть класса AddEditClientWindow, который представляет окно для добавления или редактирования информации о клиенте в приложении для фитнес-центра.

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


Этот участок кода представляет класс AddRandomClientsWindow, который, представляет окно для добавления случайного количества клиентов в приложении для фитнес-центра.

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


Также в этом проекте реализована dll библиотека. SQLiteClientServiceAdapter выступает в роли адаптера, который преобразует вызовы методов интерфейса IClientService в вызовы методов класса SQLiteClientRepository и наоборот.

using System;

using System.Collections.Generic;

using System.Linq;

namespace FitnessCenter_Viking

{

    public interface IClientService
    
    {
    
        void AddNewClient(ClientDTO client);
        
        void UpdateClientInfo(ClientDTO client);
        
        void RemoveClient(int clientId);
        
        void SaveChanges();
        
        ClientDTO GetClientById(int clientId);
        
        IEnumerable<ClientDTO> GetAllClients();
        
    }
    
    public class SQLiteClientRepository
    
    {
    
        private string connectionString;
        
        public SQLiteClientRepository(string connectionString)
        
        {
        
            this.connectionString = connectionString;
            
        }
        
        public void AddClient(Client client) { }
        
        public void UpdateClient(Client client) { }
        
        public void DeleteClient(int clientId) { }
        
        public void SaveChanges() { }
        
        public Client GetClientById(int clientId) { return null; }
        
        public IEnumerable<Client> GetAllClients() { return null; }
        
    }
    
    public class SQLiteClientServiceAdapter : IClientService
    
    {
    
        private SQLiteClientRepository clientRepository;

        public SQLiteClientServiceAdapter(string connectionString)
        
        {
        
            this.clientRepository = new SQLiteClientRepository(connectionString);
            
        }
        
        public void SaveChanges()
        
        {
        
            clientRepository.SaveChanges();
            
        }
        
        public void AddNewClient(ClientDTO client)
        
        {
        
            Client clientToAdd = ConvertToClient(client);
            
            clientRepository.AddClient(clientToAdd);
            
        }
        
        public void UpdateClientInfo(ClientDTO client)
        
        {
        
            Client clientToUpdate = ConvertToClient(client);
            
            clientRepository.UpdateClient(clientToUpdate);
            
        }
        
        public void RemoveClient(int clientId)
        
        {
        
            clientRepository.DeleteClient(clientId);
            
        }
        
        public ClientDTO GetClientById(int clientId)
        
        {
        
            Client client = clientRepository.GetClientById(clientId);
            
            return ConvertToClientDTO(client);
            
        }
        
        public IEnumerable<ClientDTO> GetAllClients()
        
        {
        
            IEnumerable<Client> clients = clientRepository.GetAllClients();
            
            return clients.Select(ConvertToClientDTO);
            
        }
        
        private Client ConvertToClient(ClientDTO clientDTO)
        
        {
        
            return new Client
            
            {
            
                Id = clientDTO.Id,
                
                LastName = clientDTO.LastName,
                
                FirstName = clientDTO.FirstName,
                
                MiddleName = clientDTO.MiddleName,
                
                DateOfBirth = clientDTO.DateOfBirth,
                
                PhoneNumber = clientDTO.PhoneNumber,
                
                ClientStatus = clientDTO.ClientStatus,
                
                SubscriptionExpiration = clientDTO.SubscriptionExpiration
                
            };
            
        }
        
        private ClientDTO ConvertToClientDTO(Client client)
        
        {
        
            return new ClientDTO
            
            {
            
                Id = client.Id,
                
                LastName = client.LastName,
                
                FirstName = client.FirstName,
                
                MiddleName = client.MiddleName,
                
                DateOfBirth = client.DateOfBirth,
                
                PhoneNumber = client.PhoneNumber,
                
                ClientStatus = client.ClientStatus,
                
                SubscriptionExpiration = client.SubscriptionExpiration
                
            };
            
        }
        
    }
    
    public class Client
    
    {
    
        public int Id { get; set; }
        
        public string LastName { get; set; }
        
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string ClientStatus { get; set; }
        
        public DateTime SubscriptionExpiration { get; set; }
        
    }
    
    public class ClientDTO
    
    {
    
        public int Id { get; set; }
        
        public string LastName { get; set; }
        
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string ClientStatus { get; set; }
        
        public DateTime SubscriptionExpiration { get; set; }
        
    }
    
}

Этот проект был разработан и также будет дорабатываться студентами группы ИСП-8: Карабеков Дияр и Некрасов Максим.

