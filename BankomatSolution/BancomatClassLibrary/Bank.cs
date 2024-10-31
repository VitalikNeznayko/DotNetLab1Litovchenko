using System;
using System.Collections.Generic;
using System.Linq;

namespace BancomatClassLibrary
{
    public class Bank
    {
        private static readonly Random Random = new Random();
        public string BankName { get; }
        public List<AutomatedTellerMachine> AtmList { get; }
        public List<Account> Accounts { get; }

        public event EventHandler<MessageEventArgs> Message;

        public Bank(string name)
        {
            BankName = name ?? throw new ArgumentNullException(nameof(name), "Назва банку не може бути пустою");
            AtmList = new List<AutomatedTellerMachine>();
            Accounts = new List<Account>();
        }

        public void AddAtm(int bankId, string address, double balance)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                Message?.Invoke(this, new MessageEventArgs("Адреса банкомата не може бути пустою"));
                return;
            }

            var atm = new AutomatedTellerMachine(bankId, address, balance);
            atm.Message += Message;
            AtmList.Add(atm);
        }

        public bool CreateAccount(string name, string pinCode)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Message?.Invoke(this, new MessageEventArgs("Ім'я не може бути пустим"));
                return false;
            }
            if (pinCode.Length != 4 || !pinCode.All(char.IsDigit))
            {
                Message?.Invoke(this, new MessageEventArgs("Пін-код повинен містити 4 цифри"));
                return false;
            }

            var account = new Account(name, GenerateCardNumber(), pinCode);
            account.Message += Message;
            Accounts.Add(account);
            Message?.Invoke(this, new MessageEventArgs($"Аккаунт створено для {name}. Номер рахунку: {account.CardNumber}"));
            return true;
        }

        public bool Authenticate(string cardNumber, string pinCode)
        {
            var account = Accounts.FirstOrDefault(a => a.CardNumber == cardNumber);
            if (account == null)
            {
                Message?.Invoke(this, new MessageEventArgs("Картка з таким номером відсутня"));
                return false;
            }
            if (pinCode != account.PinCode)
            {
                Message?.Invoke(this, new MessageEventArgs("Невірний пін-код"));
                return false;
            }
            Message?.Invoke(this, new MessageEventArgs("Аутентифікація успішна"));
            return true;
        }

        public void TransferFunds(string fromCardNumber, string toCardNumber, double amount)
        {
            var fromAccount = Accounts.FirstOrDefault(a => a.CardNumber == fromCardNumber);
            var toAccount = Accounts.FirstOrDefault(a => a.CardNumber == toCardNumber);

            if (fromAccount == null || toAccount == null)
            {
                Message?.Invoke(this, new MessageEventArgs("Один з рахунків не знайдено"));
                return;
            }
            if (fromAccount == toAccount)
            {
                Message?.Invoke(this, new MessageEventArgs("Неможливо здійснити переказ на власний рахунок"));
                return;
            }

            if (fromAccount.Withdraw(amount))
            {
                toAccount.AddToBalance(amount);
                Message?.Invoke(this, new MessageEventArgs($"Переказ {amount} грн від {fromAccount.Name} до {toAccount.Name} здійснено успішно"));
            }
            else
            {
                Message?.Invoke(this, new MessageEventArgs("Недостатньо коштів для переказу"));
            }
        }

        public Account FindAccount(string cardNumber)
        {
            var account = Accounts.FirstOrDefault(a => a.CardNumber == cardNumber);
            if (account == null)
            {
                Message?.Invoke(this, new MessageEventArgs("Картку з таким номером не знайдено"));
            }
            return account;
        }

        private static string GenerateCardNumber()
        {
            return string.Concat(Enumerable.Range(0, 6).Select(_ => Random.Next(0, 10).ToString()));
        }

        public string GetAtmAddress(int index)
        {
            if (index < 0 || index >= AtmList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Неправильний індекс банкомата");
            }

            return "вул." + AtmList[index].GetBankomatAddress();
        }
    }
}
