﻿using FitnessCenter_Viking;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
namespace FitnessCenter_VikingApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ClientDTO> _clients;
        private SQLiteConnection _connection;
        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            _clients = new ObservableCollection<ClientDTO>();
            DataContext = this;
            LoadClientsFromDatabase();
            Closing += Window_Closing;
            AddHandlerToChildWindows();
        }
        private void AddHandlerToChildWindows()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is AddEditClientWindow addEditClientWindow)
                {
                    addEditClientWindow.ClientStatusChanged += AddEditClientWindow_ClientStatusChanged;
                }
            }
        }
        private void InitializeDatabase()
        {
            _connection = new SQLiteConnection("Data Source=fitness_center.db;Version=3;");
            _connection.Open();
            using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Clients (Id INTEGER PRIMARY KEY, LastName TEXT, FirstName TEXT, MiddleName TEXT, DateOfBirth DATE, PhoneNumber TEXT, ClientStatus TEXT, SubscriptionExpiration DATE)", _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public ObservableCollection<ClientDTO> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ShowClients_Click(object sender, RoutedEventArgs e)
        {
            LoadClientsFromDatabase();
        }
        private void LoadClientsFromDatabase()
        {
            try
            {
                Clients.Clear();
                using (var cmd = new SQLiteCommand("SELECT * FROM Clients", _connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var client = new ClientDTO
                            {
                                Id = reader.GetInt32(0),
                                LastName = reader.GetString(1),
                                FirstName = reader.GetString(2),
                                MiddleName = reader.GetString(3),
                                DateOfBirth = reader.GetDateTime(4),
                                PhoneNumber = reader.GetString(5),
                                ClientStatus = reader.GetString(6),
                                SubscriptionExpiration = reader.GetDateTime(7)
                            };
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Clients.Add(client);
                            });
                        }
                    }
                }
            }
            catch (Exception ex){}
        }
        private void SaveClientsToDatabase()
        {
            using (var transaction = _connection.BeginTransaction())
            {
                foreach (var client in Clients)
                {
                    string insertQuery = @"INSERT INTO Clients (Id, LastName, FirstName, MiddleName, DateOfBirth, PhoneNumber, ClientStatus, SubscriptionExpiration) 
                                           VALUES (@Id, @LastName, @FirstName, @MiddleName, @DateOfBirth, @PhoneNumber, @ClientStatus, @SubscriptionExpiration)";
                    using (var cmd = new SQLiteCommand(insertQuery, _connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", client.Id);
                        cmd.Parameters.AddWithValue("@LastName", client.LastName);
                        cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
                        cmd.Parameters.AddWithValue("@MiddleName", client.MiddleName);
                        cmd.Parameters.AddWithValue("@DateOfBirth", client.DateOfBirth);
                        cmd.Parameters.AddWithValue("@PhoneNumber", client.PhoneNumber);
                        cmd.Parameters.AddWithValue("@ClientStatus", client.ClientStatus);
                        cmd.Parameters.AddWithValue("@SubscriptionExpiration", client.SubscriptionExpiration);
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            ClientDTO newClient = new ClientDTO();
            AddEditClientWindow addEditClientWindow = new AddEditClientWindow(newClient);
            addEditClientWindow.ClientUpdated += AddEditClientWindow_ClientUpdated;
            addEditClientWindow.StatusChanged += AddEditClientWindow_StatusChanged;
            if (addEditClientWindow.ShowDialog() == true)
            {
                if (!Clients.Contains(newClient))
                {
                    using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Clients WHERE PhoneNumber = @PhoneNumber", _connection))
                    {
                        cmd.Parameters.AddWithValue("@PhoneNumber", newClient.PhoneNumber);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show($"Номер телефона уже принадлежит клиенту.");
                            return;
                        }
                    }
                    newClient.Id = GenerateUniqueId();
                    using (var cmd = new SQLiteCommand("INSERT INTO Clients (Id, LastName, FirstName, MiddleName, DateOfBirth, PhoneNumber, ClientStatus, SubscriptionExpiration) VALUES (@Id, @LastName, @FirstName, @MiddleName, @DateOfBirth, @PhoneNumber, @ClientStatus, @SubscriptionExpiration)", _connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", newClient.Id);
                        cmd.Parameters.AddWithValue("@LastName", newClient.LastName);
                        cmd.Parameters.AddWithValue("@FirstName", newClient.FirstName);
                        cmd.Parameters.AddWithValue("@MiddleName", newClient.MiddleName);
                        cmd.Parameters.AddWithValue("@DateOfBirth", newClient.DateOfBirth);
                        cmd.Parameters.AddWithValue("@PhoneNumber", newClient.PhoneNumber);
                        cmd.Parameters.AddWithValue("@ClientStatus", newClient.ClientStatus);
                        cmd.Parameters.AddWithValue("@SubscriptionExpiration", newClient.SubscriptionExpiration);
                    }
                    Clients.Add(newClient);
                }
            }
        }
        private void AddEditClientWindow_ClientUpdated(object sender, ClientDTO e)
        {
            var existingClient = Clients.FirstOrDefault(c => c.Id == e.Id);
            if (existingClient != null)
            {
                existingClient.LastName = e.LastName;
                existingClient.FirstName = e.FirstName;
                existingClient.MiddleName = e.MiddleName;
                existingClient.DateOfBirth = e.DateOfBirth;
                existingClient.PhoneNumber = e.PhoneNumber;
                existingClient.ClientStatus = e.ClientStatus;
                existingClient.SubscriptionExpiration = e.SubscriptionExpiration;
            }
            else
            {
                Clients.Add(e);
            }
        }
        private void AddEditClientWindow_StatusChanged(object sender, string e)
        {
            var updatedStatus = e;
            foreach (var client in Clients)
            {
                if (client == (sender as AddEditClientWindow)?.DataContext)
                {
                    client.ClientStatus = updatedStatus;
                    break;
                }
            }
        }
        private void AddEditClientWindow_ClientStatusChanged(object sender, ClientDTO e)
        {
            var existingClient = Clients.FirstOrDefault(c => c.Id == e.Id);
            if (existingClient != null)
            {
                existingClient.ClientStatus = e.ClientStatus;
            }
        }
        private int GenerateUniqueId()
        {
            return Interlocked.Increment(ref clientIdCounter);
        }
        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            ClientDTO selectedClient = ClientsListBox.SelectedItem as ClientDTO;
            if (selectedClient != null)
            {
                AddEditClientWindow addEditClientWindow = new AddEditClientWindow(selectedClient);
                addEditClientWindow.ClientUpdated += (s, updatedClient) =>
                {
                    int index = Clients.IndexOf(selectedClient);
                    if (index != -1)
                    {
                        Clients[index] = updatedClient;
                    }
                };
                addEditClientWindow.ClientStatusChanged += (s, updatedClient) =>
                {
                    int index = Clients.IndexOf(selectedClient);
                    if (index != -1)
                    {
                        Clients[index].ClientStatus = updatedClient.ClientStatus; 
                    }
                };
                addEditClientWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.");
            }
        }
        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            ClientDTO selectedClient = ClientsListBox.SelectedItem as ClientDTO;
            if (selectedClient != null)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтвердить удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var cmd = new SQLiteCommand("DELETE FROM Clients WHERE Id = @Id", _connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", selectedClient.Id);
                        cmd.ExecuteNonQuery();
                    }
                    Clients.Remove(selectedClient);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.");
            }
        }
        private void AddRandomClients_Click(object sender, RoutedEventArgs e)
        {
            AddRandomClientsWindow addRandomClientsWindow = new AddRandomClientsWindow();
            if (addRandomClientsWindow.ShowDialog() == true)
            {
                int count = addRandomClientsWindow.NumberOfClients;
                ResetClientIdCounter();
                for (int i = 0; i < count; i++)
                {
                    ClientDTO newRandomClient = GenerateRandomClient();
                    using (var cmd = new SQLiteCommand("INSERT INTO Clients (Id, LastName, FirstName, MiddleName, DateOfBirth, PhoneNumber, ClientStatus, SubscriptionExpiration) VALUES (@Id, @LastName, @FirstName, @MiddleName, @DateOfBirth, @PhoneNumber, @ClientStatus, @SubscriptionExpiration)", _connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", newRandomClient.Id);
                        cmd.Parameters.AddWithValue("@LastName", newRandomClient.LastName);
                        cmd.Parameters.AddWithValue("@FirstName", newRandomClient.FirstName);
                        cmd.Parameters.AddWithValue("@MiddleName", newRandomClient.MiddleName);
                        cmd.Parameters.AddWithValue("@DateOfBirth", newRandomClient.DateOfBirth);
                        cmd.Parameters.AddWithValue("@PhoneNumber", newRandomClient.PhoneNumber);
                        cmd.Parameters.AddWithValue("@ClientStatus", newRandomClient.ClientStatus);
                        cmd.Parameters.AddWithValue("@SubscriptionExpiration", newRandomClient.SubscriptionExpiration);
                    }
                    Clients.Add(newRandomClient);
                }
            }
        }
        private static int clientIdCounter = 0;
        private ClientDTO GenerateRandomClient()
        {
            Random rnd = new Random();
            string[] maleLastNames = { "Иванов", "Смирнов", "Кузнецов", "Попов", "Васильев", "Петров", "Соколов", "Михайлов", "Новиков", "Фёдоров", "Морозов", "Волков", "Алексеев", "Лебедев", "Семёнов", "Егоров", "Павлов", "Козлов", "Степанов", "Николаев", "Орлов", "Андреев", "Макаров", "Никитин", "Захаров", "Зайцев", "Соловьев", "Борисов", "Яковлев", "Григорьев", "Романов", "Воробьев", "Сергеев", "Кузьмин", "Фролов", "Александров", "Дмитриев", "Королев", "Гусев", "Киселев", "Ильин", "Максимов", "Поляков", "Сорокин", "Виноградов", "Ковалев", "Белов", "Медведев", "Антонов", "Тарасов", "Жуков", "Баранов", "Филиппов", "Комаров", "Давыдов", "Беляев", "Герасимов", "Богданов", "Осипов", "Сидоров", "Матвеев", "Титов", "Марков", "Миронов", "Крылов", "Куликов", "Карпов", "Власов", "Мельников", "Денисов", "Гаврилов", "Тихонов", "Казаков", "Афанасьев", "Данилов", "Савельев", "Тимофеев", "Фомин", "Чернов", "Абрамов", "Мартынов", "Ефимов", "Федотов", "Щербаков", "Назаров", "Калинин", "Исаев", "Чернышев", "Быков", "Маслов", "Родионов", "Коновалов", "Лазарев", "Воронин", "Климов", "Филатов", "Пономарев", "Голубев", "Кудрявцев", "Прохоров", "Наумов", "Потапов", "Журавлев", "Овчинников", "Трофимов", "Леонов", "Соболев", "Ермаков", "Колесников", "Гончаров", "Емельянов", "Никифоров", "Грачев", "Котов", "Гришин", "Ефремов", "Архипов", "Громов", "Кириллов", "Малышев", "Панов", "Моисеев", "Румянцев", "Акимов", "Кондратьев", "Бирюков", "Горбунов", "Анисимов", "Еремин", "Тихомиров", "Галкин", "Лукьянов", "Михеев", "Скворцов", "Юдин", "Белоусов", "Нестеров", "Симонов", "Прокофьев", "Харитонов", "Князев", "Цветков", "Левин", "Митрофанов", "Воронов", "Аксенов", "Софронов", "Мальцев", "Логинов", "Горшков", "Савин", "Краснов", "Майоров", "Демидов", "Елисеев", "Рыбаков", "Сафонов", "Плотников", "Демин", "Хохлов", "Фадеев", "Молчанов", "Игнатов", "Литвинов", "Ершов", "Ушаков", "Дементьев", "Рябов", "Мухин", "Калашников", "Леонтьев", "Лобанов", "Кузин", "Корнеев", "Евдокимов", "Бородин", "Платонов", "Некрасов", "Балашов", "Бобров", "Жданов", "Блинов", "Игнатьев", "Коротков", "Муравьев", "Крюков", "Беляков", "Богомолов", "Дроздов", "Лавров", "Зуев", "Петухов", "Ларин", "Никулин", "Серов", "Терентьев", "Зотов", "Устинов", "Фокин", "Самойлов", "Константинов", "Сахаров", "Шишкин", "Самсонов", "Черкасов", "Чистяков", "Носов", "Спиридонов", "Карасев", "Авдеев", "Воронцов", "Зверев", "Владимиров", "Селезнев", "Нечаев", "Кудряшов", "Седов", "Фирсов", "Андрианов", "Панин", "Головин", "Терехов", "Ульянов", "Шестаков", "Агеев", "Никонов", "Селиванов", "Баженов", "Гордеев", "Кожевников", "Пахомов", "Зимин", "Костин", "Широков", "Филимонов", "Ларионов", "Овсянников", "Сазонов", "Суворов", "Нефедов", "Корнилов", "Любимов", "Львов", "Горбачев", "Копылов", "Лукин", "Токарев", "Кулешов", "Шилов", "Большаков", "Панкратов", "Родин", "Шаповалов", "Покровский", "Бочаров", "Никольский", "Маркин", "Горелов", "Агафонов", "Березин", "Ермолаев", "Зубков", "Куприянов", "Трифонов", "Масленников", "Круглов", "Третьяков", "Колосов", "Рожков", "Артамонов", "Шмелев", "Лаптев", "Лапшин", "Федосеев", "Зиновьев", "Зорин", "Уткин", "Столяров", "Зубов", "Ткачев", "Дорофеев", "Антипов", "Завьялов", "Свиридов", "Золотарев", "Кулаков", "Мещеряков", "Макеев", "Дьяконов", "Гуляев", "Петровский", "Бондарев", "Поздняков", "Панфилов", "Кочетков", "Суханов", "Рыжов", "Старостин", "Калмыков", "Колесов", "Золотов", "Кравцов", "Субботин", "Шубин", "Щукин", "Лосев", "Винокуров", "Лапин", "Парфенов", "Исаков", "Голованов", "Коровин", "Розанов", "Артемов", "Козырев", "Русаков", "Алешин", "Крючков", "Булгаков", "Кошелев", "Сычев", "Синицын", "Черных", "Рогов", "Кононов", "Лаврентьев", "Евсеев", "Пименов", "Пантелеев", "Горячев", "Аникин", "Лопатин", "Рудаков", "Одинцов", "Серебряков", "Панков", "Дегтярев", "Орехов", "Царев", "Шувалов", "Кондрашов", "Горюнов", "Дубровин", "Голиков", "Курочкин", "Латышев", "Севастьянов", "Вавилов", "Ерофеев", "Сальников", "Клюев", "Носков", "Озеров", "Кольцов", "Комиссаров", "Меркулов", "Киреев", "Хомяков", "Булатов", "Ананьев", "Буров", "Шапошников", "Дружинин", "Островский", "Шевелев", "Долгов", "Суслов", "Шевцов", "Пастухов", "Рубцов", "Бычков", "Глебов", "Ильинский", "Успенский", "Дьяков", "Кочетов", "Вишневский", "Высоцкий", "Глухов", "Дубов", "Бессонов", "Ситников", "Астафьев", "Мешков", "Шаров", "Яшин", "Козловский", "Туманов", "Басов", "Корчагин", "Болдырев", "Олейников", "Чумаков", "Фомичев", "Губанов", "Дубинин", "Шульгин", "Касаткин", "Пирогов", "Семин", "Трошин", "Горохов", "Стариков", "Щеглов", "Фетисов", "Колпаков", "Чесноков", "Зыков", "Верещагин", "Минаев", "Руднев", "Троицкий", "Окулов", "Ширяев", "Малинин", "Черепанов", "Измайлов", "Алехин", "Зеленин", "Касьянов", "Пугачев", "Павловский", "Чижов", "Кондратов", "Воронков", "Капустин", "Сотников", "Демьянов", "Косарев", "Беликов", "Сухарев", "Белкин", "Беспалов", "Кулагин", "Савицкий", "Жаров", "Хромов", "Еремеев", "Карташов", "Астахов", "Русанов", "Сухов", "Вешняков", "Волошин", "Козин", "Жилин", "Малахов", "Сизов", "Ежов", "Толкачев", "Анохин", "Вдовин", "Бабушкин", "Усов", "Лыков", "Горлов", "Коршунов", "Маркелов", "Постников", "Черный", "Дорохов", "Свешников", "Гущин", "Калугин", "Блохин", "Сурков", "Кочергин", "Греков", "Казанцев", "Швецов", "Ермилов", "Парамонов", "Агапов", "Минин", "Корнев", "Черняев", "Гуров", "Ермолов", "Сомов", "Добрынин", "Барсуков", "Глушков", "Чеботарев", "Москвин", "Уваров", "Безруков", "Муратов", "Раков", "Снегирев", "Гладков", "Злобин", "Моргунов", "Поликарпов", "Рябинин", "Судаков", "Кукушкин", "Калачев", "Грибов", "Елизаров", "Звягинцев", "Корольков", "Федосов" };
            string[] femaleLastNames = { "Иванова", "Смирнова", "Кузнецова", "Попова", "Васильева", "Петрова", "Соколова", "Михайлова", "Новикова", "Фёдорова", "Морозова", "Волкова", "Алексеева", "Лебедева", "Семёнова", "Егорова", "Павлова", "Козлова", "Степанова", "Николаева", "Орлова", "Андреева", "Макарова", "Никитина", "Захарова", "Зайцева", "Соловьева", "Борисова", "Яковлева", "Григорьева", "Романова", "Воробьева", "Сергеева", "Кузьмина", "Фролова", "Александрова", "Дмитриева", "Королева", "Гусева", "Киселева", "Ильина", "Максимова", "Полякова", "Сорокина", "Виноградова", "Ковалева", "Белова", "Медведева", "Антонова", "Тарасова", "Жукова", "Баранова", "Филиппова", "Комарова", "Давыдова", "Беляева", "Герасимова", "Богданова", "Осипова", "Сидорова", "Матвеева", "Титова", "Маркова", "Миронова", "Крылова", "Куликова", "Карпова", "Власова", "Мельникова", "Денисова", "Гаврилова", "Тихонова", "Казакова", "Афанасьева", "Данилова", "Савельева", "Тимофеева", "Фомина", "Чернова", "Абрамова", "Мартынова", "Ефимова", "Федотова", "Щербакова", "Назарова", "Калинина", "Исаева", "Чернышева", "Быкова", "Маслова", "Родионова", "Коновалова", "Лазарева", "Воронина", "Климова", "Филатова", "Пономарева", "Голубева", "Кудрявцева", "Прохорова", "Наумова", "Потапова", "Журавлева", "Овчинникова", "Трофимова", "Леонова", "Соболева", "Ермакова", "Колесникова", "Гончарова", "Емельянова", "Никифорова", "Грачева", "Котова", "Гришина", "Ефремова", "Архипова", "Громова", "Кириллова", "Малышева", "Панова", "Моисеева", "Румянцева", "Акимова", "Кондратьева", "Бирюкова", "Горбунова", "Анисимова", "Еремина", "Тихомирова", "Галкина", "Лукьянова", "Михеева", "Скворцова", "Юдина", "Белоусова", "Нестерова", "Симонова", "Прокофьева", "Харитонова", "Князева", "Цветкова", "Левина", "Митрофанова", "Воронова", "Аксенова", "Софронова", "Мальцева", "Логинова", "Горшкова", "Савина", "Краснова", "Майорова", "Демидова", "Елисеева", "Рыбакова", "Сафонова", "Плотникова", "Демина", "Хохлова", "Фадеева", "Молчанова", "Игнатова", "Литвинова", "Ершова", "Ушакова", "Дементьева", "Рябова", "Мухина", "Калашникова", "Леонтьева", "Лобанова", "Кузина", "Корнеева", "Евдокимова", "Бородина", "Платонова", "Некрасова", "Балашова", "Боброва", "Жданова", "Блинова", "Игнатьева", "Короткова", "Муравьева", "Крюкова", "Белякова", "Богомолова", "Дроздова", "Лаврова", "Зуева", "Петухова", "Ларина", "Никулина", "Серова", "Терентьева", "Зотова", "Устинова", "Фокина", "Самойлова", "Константинова", "Сахарова", "Шишкина", "Самсонова", "Черкасова", "Чистякова", "Носова", "Спиридонова", "Карасева", "Авдеева", "Воронцова", "Зверева", "Владимирова", "Селезнева", "Нечаева", "Кудряшова", "Седова", "Фирсова", "Андрианова", "Панина", "Головина", "Терехова", "Ульянова", "Шестакова", "Агеева", "Никонова", "Селиванова", "Баженова", "Гордеева", "Кожевникова", "Пахомова", "Зимина", "Костина", "Широкова", "Филимонова", "Ларионова", "Овсянникова", "Сазонова", "Суворова", "Нефедова", "Корнилова", "Любимова", "Львова", "Горбачева", "Копылова", "Лукина", "Токарева", "Кулешова", "Шилова", "Большакова", "Панкратова", "Родина", "Шаповалова", "Покровская", "Бочарова", "Никольская", "Маркина", "Горелова", "Агафонова", "Березина", "Ермолаева", "Зубкова", "Куприянова", "Трифонова", "Масленникова", "Круглова", "Третьякова", "Колосова", "Рожкова", "Артамонова", "Шмелева", "Лаптева", "Лапшина", "Федосеева", "Зиновьева", "Зорина", "Уткина", "Столярова", "Зубова", "Ткачева", "Дорофеева", "Антипова", "Завьялова", "Свиридова", "Золотарева", "Кулакова", "Мещерякова", "Макеева", "Дьяконова", "Гуляева", "Петровская", "Бондарева", "Позднякова", "Панфилова", "Кочеткова", "Суханова", "Рыжова", "Старостина", "Калмыкова", "Колесова", "Золотова", "Кравцова", "Субботина", "Шубина", "Щукина", "Лосева", "Винокурова", "Лапина", "Парфенова", "Исакова", "Голованова", "Коровина", "Розанова", "Артемова", "Козырева", "Русакова", "Алешина", "Крючкова", "Булгакова", "Кошелева", "Сычева", "Синицына", "Черных", "Рогова", "Кононова", "Лаврентьева", "Евсеева", "Пименова", "Пантелеева", "Горячева", "Аникина", "Лопатина", "Рудакова", "Одинцова", "Серебрякова", "Панкова", "Дегтярева", "Орехова", "Царева", "Шувалова", "Кондрашова", "Горюнова", "Дубровина", "Голикова", "Курочкина", "Латышева", "Севастьянова", "Вавилова", "Ерофеева", "Сальникова", "Клюева", "Носкова", "Озерова", "Кольцова", "Комиссарова", "Меркулова", "Киреева", "Хомякова", "Булатова", "Ананьева", "Бурова", "Шапошникова", "Дружинина", "Островская", "Шевелева", "Долгова", "Суслова", "Шевцова", "Пастухова", "Рубцова", "Бычкова", "Глебова", "Ильинская", "Успенская", "Дьякова", "Кочетова", "Вишневская", "Высоцкая", "Глухова", "Дубова", "Бессонова", "Ситникова", "Астафьева", "Мешкова", "Шарова", "Яшина", "Козловская", "Туманова", "Басова", "Корчагина", "Болдырева", "Олейникова", "Чумакова", "Фомичева", "Губанова", "Дубинина", "Шульгина", "Касаткина", "Пирогова", "Семина", "Трошина", "Горохова", "Старикова", "Щеглова", "Фетисова", "Колпакова", "Чеснокова", "Зыкова", "Верещагина", "Минаева", "Руднева", "Троицкая", "Окулова", "Ширяева", "Малинина", "Черепанова", "Измайлова", "Алехина", "Зеленина", "Касьянова", "Пугачева", "Павловская", "Чижова", "Кондратова", "Воронкова", "Капустина", "Сотникова", "Демьянова", "Косарева", "Беликова", "Сухарева", "Белкина", "Беспалова", "Кулагина", "Савицкая", "Жарова", "Хромова", "Еремеева", "Карташова", "Астахова", "Русанова", "Сухова", "Вешнякова", "Волошина", "Козина", "Жилина", "Малахова", "Сизова", "Ежова", "Толкачева", "Анохина", "Вдовина", "Бабушкина", "Усова", "Лыкова", "Горлова", "Коршунова", "Маркелова", "Постникова", "Черный", "Дорохова", "Свешникова", "Гущина", "Калугина", "Блохина", "Суркова", "Кочергина", "Грекова", "Казанцева", "Швецова", "Ермилова", "Парамонова", "Агапова", "Минина", "Корнева", "Черняева", "Гурова", "Ермолова", "Сомова", "Добрынина", "Барсукова", "Глушкова", "Чеботарева", "Москвина", "Уварова", "Безрукова", "Муратова", "Ракова", "Снегирева", "Гладкова", "Злобина", "Моргунова", "Поликарпова", "Рябинина", "Судакова", "Кукушкина", "Калачева", "Грибова", "Елизарова", "Звягинцева", "Королькова", "Федосова" };
            string[] maleMiddleNames = { "Александрович", "Михайлович", "Кириллович", "Алексеевич", "Даниилович", "Матвеевич", "Денисович", "Дмитриевич", "Иванович", "Артемович", "Егорович", "Степанович", "Ярославович", "Арсеньевич", "Ильич", "Григорьевич", "Викторович", "Дамирович", "Миронович", "Львович", "Сергеевич", "Максимович", "Романович", "Евгеньевич", "Никитович", "Тимофеевич", "Андреевич", "Васильевич", "Платонович", "Владимирович", "Захарович", "Николаевич", "Богданович", "Тимурович", "Эрикович", "Федорович", "Назарович", "Геннадиевич", "Владиславович", "Константинович", "Антонович", "Брониславович", "Мстиславович", "Леонидович", "Робертович", "Елизарович", "Георгиевич", "Глебович", "Савелиевич", "Олегович" };
            string[] femaleMiddleNames = {
    "Григорьевна", "Даниловна", "Денисовна", "Дмитриевна", "Евгеньевна",
    "Егоровна", "Ефимовна", "Ивановна", "Игоревна", "Ильинична",
    "Иосифовна", "Кирилловна", "Константиновна", "Леонидовна", "Львовна",
    "Максимовна", "Матвеевна", "Михайловна", "Николаевна", "Олеговна",
    "Павловна", "Петровна", "Платоновна", "Робертовна", "Романовна",
    "Семеновна", "Сергеевна", "Станиславовна", "Степановна", "Тарасовна",
    "Тимофеевна", "Федоровна", "Феликсовна", "Филипповна", "Эдуардовна",
    "Юрьевна", "Яковлевна", "Ярославовна"
};
            string[] maleFirstNames = { "Александр", "Михаил", "Кирилл", "Алексей", "Даниил", "Матвей", "Денис", "Дмитрий", "Иван", "Артем", "Егор", "Степан", "Ярослав", "Арсений", "Илья", "Григорий", "Виктор", "Дамир", "Мирон", "Лев", "Сергей", "Максим", "Роман", "Евгений", "Никита", "Тимофей", "Андрей", "Василий", "Платон", "Владимир", "Захар", "Николай", "Богдан", "Тимур", "Эрик", "Федор", "Назар", "Геннадий", "Владислав", "Константин", "Антон", "Бронислав", "Мстислав", "Леонид", "Роберт", "Елизар", "Георгий", "Глеб", "Савелий", "Олег" };
            string[] femaleFirstNames = {
    "Анна", "Софья", "Алиса", "Мария", "Василиса", "Анастасия", "Вероника",
    "Виктория", "Маргарита", "Кира", "Ксения", "Валерия", "Елена", "Таисия",
    "Екатерина", "Дарья", "Александра", "Варвара", "Арина", "Любовь", "Агата",
    "Полина", "Олеся", "Милана", "Ника", "Вера", "Татьяна", "Амалия", "Алина",
    "Наталья", "Евгения", "Злата", "Камилла", "Мирослава", "Эвелина", "Ева",
    "Ярослава", "Диана", "Кристина", "Регина", "Марта", "Ольга", "Надежда",
    "Виолетта", "Богдана", "Элеонора", "Зоя", "Ефросинья", "Ангелина", "Агния"
};
            string[] clientStatuses = { "клиент", "бизнес-клиент", "вип-клиент" };
            string lastName;
            string middleName;
            string firstName;

            if (rnd.Next(2) == 0) 
            {
                lastName = maleLastNames[rnd.Next(maleLastNames.Length)];
                middleName = maleMiddleNames[rnd.Next(maleMiddleNames.Length)];
                firstName = maleFirstNames[rnd.Next(maleFirstNames.Length)];
            }
            else
            {
                lastName = femaleLastNames[rnd.Next(femaleLastNames.Length)];
                middleName = femaleMiddleNames[rnd.Next(femaleMiddleNames.Length)];
                firstName = femaleFirstNames[rnd.Next(femaleFirstNames.Length)];
            }

            string clientStatus = clientStatuses[rnd.Next(clientStatuses.Length)];
            DateTime dateOfBirth = DateTime.Now.AddYears(-rnd.Next(20, 60));
            string phoneNumber = GenerateRandomPhoneNumber();
            DateTime subscriptionExpiration = DateTime.Now.AddMonths(rnd.Next(1, 12));
            int id = Interlocked.Increment(ref clientIdCounter);
            ClientDTO randomClient = new ClientDTO
            {
                Id = id,
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                DateOfBirth = dateOfBirth,
                PhoneNumber = phoneNumber,
                ClientStatus = clientStatus,
                SubscriptionExpiration = subscriptionExpiration
            };
            return randomClient;
        }
        private string GenerateRandomPhoneNumber()
        {
            Random rnd = new Random();
            StringBuilder phoneNumber = new StringBuilder();
            phoneNumber.Append("+");
            phoneNumber.Append(rnd.Next(1, 10)); 
            for (int i = 0; i < 10; i++)
            {
                phoneNumber.Append(rnd.Next(0, 10)); 
            }
            return phoneNumber.ToString();
        }
        private void ResetClientIdCounter()
        {
            Interlocked.Exchange(ref clientIdCounter, 0);
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveClientsToDatabase();
        }
    }
    public class AgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateOfBirth)
            {
                int age = DateTime.Today.Year - dateOfBirth.Year;
                if (DateTime.Today < dateOfBirth.AddYears(age))
                {
                    age--;
                }
                return age;
            }
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SubscriptionExpiredConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime expirationDate)
            {
                if (expirationDate < DateTime.Today)
                {
                    return "Истёк";
                }
                else
                {
                    return expirationDate.ToString("yyyy-MM-dd");
                }
            }
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
