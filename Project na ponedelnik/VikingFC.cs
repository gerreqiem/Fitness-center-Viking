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