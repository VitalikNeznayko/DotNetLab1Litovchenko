using BancomatClassLibrary;
using System;
using System.Collections.Generic;

namespace BancomatConsoleApp
{
    internal class Program
    {
        static Bank[] banks = {
            new Bank("Моно банк"),
            new Bank("Приват банк"),
        };

        static Bank selectedBank;
        static AutomatedTellerMachine activeBankomat;
        static Account currentAccount;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            foreach (var bank in banks)
            {
                bank.Message += BankMessageHandler;
            }

            banks[0].AddAtm(1, "Велика Бердичiвська 52", 20000);
            banks[0].AddAtm(1, "Вітрука 43", 15000);
            banks[1].AddAtm(2, "Київська 87", 25000);

            MenuChooseBank();
        }

        private static int MenuLoop(List<string> menuItems)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Оберіть пункт меню:");

                for (int i = 0; i < menuItems.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {menuItems[i]}");
                }

                Console.Write("Ваш вибір: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int index) && index >= 1 && index <= menuItems.Count)
                {
                    return index - 1;
                }
                else
                {
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    Console.ReadKey();
                }
            }
        }

        private static void MenuChooseBank()
        {
            Console.Clear();
            List<string> menuItems = new List<string>();
            foreach (var bank in banks)
            {
                menuItems.Add(bank.BankName);
            }
            menuItems.Add("Вихід");

            int selectedIndex = MenuLoop(menuItems);

            if (selectedIndex == menuItems.Count - 1)
            {
                return;
            }

            selectedBank = banks[selectedIndex];
            MenuChooseBankomat();
        }

        private static void MenuChooseBankomat()
        {
            Console.Clear();
            List<string> menuItems = new List<string>();
            for (int i = 0; i < selectedBank.AtmList.Count; i++)
            {
                menuItems.Add(selectedBank.GetAtmAddress(i));
            }
            menuItems.Add("Повернутися назад");

            int selectedIndex = MenuLoop(menuItems);

            if (selectedIndex == menuItems.Count - 1)
            {
                MenuChooseBank();
            }
            else
            {
                activeBankomat = selectedBank.AtmList[selectedIndex];
                MenuChooseAuth();
            }
        }

        private static void MenuChooseAuth()
        {
            List<string> menuItems = new List<string> { "Авторизуватися", "Зареєструватися", "Повернутися назад" };
            while (true)
            {
                Console.Clear();
                int selectedIndex = MenuLoop(menuItems);
                switch (selectedIndex)
                {
                    case 0:
                        if (Authenticate())
                            AccountMenu();
                        break;
                    case 1:
                        CreateNewAccount();
                        break;
                    case 2:
                        MenuChooseBankomat();
                        break;
                }
            }
        }

        static void AccountMenu()
        {
            List<string> menuItems = new List<string>() { "Переглянути баланс", "Зняти кошти",
                "Поповнити рахунок", "Перерахувати кошти", "Повернутися назад" };
            while (true)
            {
                Console.Clear();
                int selectedIndex = MenuLoop(menuItems);

                switch (selectedIndex)
                {
                    case 0:
                        Console.WriteLine($"Ваш баланс: {currentAccount.GetBalance()} грн");
                        Console.ReadKey();
                        break;
                    case 1:
                        Console.WriteLine("Введіть суму для зняття:");
                        if (int.TryParse(Console.ReadLine(), out int amountWithdraw))
                        {
                            activeBankomat.WithDrawMoney(currentAccount, amountWithdraw);
                        }
                        else
                        {
                            Console.WriteLine("Невірний формат суми.");
                        }
                        Console.ReadKey();
                        break;
                    case 2:
                        Console.WriteLine("Введіть суму для поповнення:");
                        if (int.TryParse(Console.ReadLine(), out int amountDeposit))
                        {
                            activeBankomat.PutMoney(currentAccount, amountDeposit);
                        }
                        else
                        {
                            Console.WriteLine("Невірний формат суми.");
                        }
                        break;
                    case 3:
                        Console.WriteLine("Введіть номер рахунку отримувача:");
                        string receiverAccountNumber = Console.ReadLine();
                        Console.WriteLine("Введіть суму для перерахування:");
                        if (int.TryParse(Console.ReadLine(), out int amountTransfer))
                        {
                            selectedBank.TransferFunds(currentAccount.CardNumber, receiverAccountNumber, amountTransfer);
                        }
                        else
                        {
                            Console.WriteLine("Невірний формат суми.");
                        }
                        Console.ReadKey();
                        break;
                    case 4:
                        return;
                }
            }
        }

        static void CreateNewAccount()
        {
            while (true)
            {
                Console.WriteLine("Введіть своє ім'я:");
                string name = Console.ReadLine();
                Console.WriteLine("Введіть пін-код (4 цифри):");
                string pinCode = Console.ReadLine();

                if (selectedBank.CreateAccount(name, pinCode))
                {
                    Console.WriteLine("Акаунт створено успішно.");
                    break;
                }
                else
                {
                    Console.WriteLine("Помилка при створенні акаунту.");
                }
            }
        }

        static bool Authenticate()
        {
            Console.WriteLine("Введіть номер картки:");
            string accountNumber = Console.ReadLine();
            Console.WriteLine("Введіть пін-код:");
            string pinCode = Console.ReadLine();

            if (selectedBank.Authenticate(accountNumber, pinCode))
            {
                currentAccount = selectedBank.FindAccount(accountNumber);
                return true;
            }
            else
            {
                Console.WriteLine("Аутентифікація не вдалася.");
                return false;
            }
        }

        static void BankMessageHandler(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message);
            Console.ReadKey();
        }
    }
}
