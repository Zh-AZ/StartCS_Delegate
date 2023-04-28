﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using StartCS_Delegate.Infrastructure.Commands;
using StartCS_Delegate.Infrastructure.Commands.Base;
using StartCS_Delegate.ViewModels.Base;
using StartCS_Delegate.Views;
using System.IO.Packaging;
using System.Collections.ObjectModel;
using System.Windows.Data;
using StartCS_Delegate.Views.TransactionWindow;
using StartCS_Delegate.Views.ManagerWindow;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using StartCS_Delegate.Views.HistoryWindow;
using StartCS_Delegate.Views.MessageWindow;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Data;

namespace StartCS_Delegate.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        //public static TrulyObservableCollection<Client> Clients { get; set; }
        //public static FullyObservableCollection<Client> Clients { get; set; }
        public static ObservableCollection<Client> Clients { get; set; }
        private MainWindow MainWindow;
        static ManagerWindow ManagerWindow;
        static TransactionWindow TransactionWindow;
        static HistoryWindow HistoryWindow;
        static MessageWindow MessageWindow;
        
        private static Client _Selected;
        public Client Selected
        {
            get => _Selected;
            set => Set(ref _Selected, value);
        }

        public void OnViewInitialized(MainWindow mainWindow) { MainWindow = mainWindow; }

        #region Комманды

        /// <summary>
        /// Открытие окна для менеджера
        /// </summary>
        public ICommand OpenManagerWindowCommand { get; }
        private bool CanOpenManagerWindowCommandExecute(object p) => true;
        private void OnOpenManagerWindowCommandExecute(object p)
        {
            MainWindow.progressBar.ValueChanged += ProgressBar_ValueChanged;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainWindow.progressBar.Value == 100)
            {
                ManagerWindow = new ManagerWindow();
                ManagerWindow.Show();
                MainWindow.Close();
            }
        }

        /// <summary>
        /// Добавление новых клиентов
        /// </summary>
        public ICommand CreateNewClientCommand { get; }
        private bool CanCreateNewClientCommandExecute(object p) => true;
        private void OnCreateNewClientCommandExecute(object p)
        {
            var newClient = new Client(ManagerWindow.IDBox.Text, ManagerWindow.EmailBox.Text, ManagerWindow.NameBox.Text, ManagerWindow.SurnameBox.Text,
                ManagerWindow.PatronymicBox.Text, ManagerWindow.NumberPhoneBox.Text, ManagerWindow.AddressBox.Text, ManagerWindow.BillBox.Text, ManagerWindow.DepBillBox.Text);

            if (ManagerWindow.IDBox.Text == "" || ManagerWindow.EmailBox.Text == "" || ManagerWindow.NameBox.Text == "" ||
                ManagerWindow.SurnameBox.Text == "" || ManagerWindow.PatronymicBox.Text == "" || ManagerWindow.NumberPhoneBox.Text == "" || ManagerWindow.AddressBox.Text == "")
            {
                MessageBox.Show("Missing information!");
            }
            else
            {
                if (newClient.Bill == String.Empty && newClient.DepBill == String.Empty)
                {
                    newClient.Bill = "Закрытый";
                    newClient.DepBill = "Закрытый";
                    Clients.Add(newClient);

                    ManagerWindow.IDBox.Text = "";
                    ManagerWindow.EmailBox.Text = "";
                    ManagerWindow.NameBox.Text = "";
                    ManagerWindow.SurnameBox.Text = "";
                    ManagerWindow.PatronymicBox.Text = "";
                    ManagerWindow.NumberPhoneBox.Text = "";
                    ManagerWindow.AddressBox.Text = "";
                    ManagerWindow.BillBox.Text = "";
                    ManagerWindow.DepBillBox.Text = "";
                    XmlSerialize(Clients);
                }
                else
                {
                    if (Clients.Contains(newClient) == true)
                    {
                        Clients.Add(newClient);

                        ManagerWindow.IDBox.Text = "";
                        ManagerWindow.EmailBox.Text = "";
                        ManagerWindow.NameBox.Text = "";
                        ManagerWindow.SurnameBox.Text = "";
                        ManagerWindow.PatronymicBox.Text = "";
                        ManagerWindow.NumberPhoneBox.Text = "";
                        ManagerWindow.AddressBox.Text = "";
                        ManagerWindow.BillBox.Text = "";
                        ManagerWindow.DepBillBox.Text = "";
                        XmlSerialize(Clients);
                    }
                }
            }
        }

        /// <summary>
        /// Удаление клиента
        /// </summary>
        public ICommand DeleteClientCommand { get; }
        private bool CanDeleteClientCommandExecute(object p) => p is Client client && Clients.Contains(client);
        private void OnDeleteClientCommandExecute(object p)
        {
            if (!(p is Client client)) return;
            Clients.Remove(client);
            XmlSerialize(Clients);
        }

        /// <summary>
        /// Сохранение изменений клииента
        /// </summary>
        public ICommand ChangeClientCommand { get; }
        private bool CanChangeClientCommandExecute(object p) => p is Client client && Clients.Contains(client);
        async private void OnChangeClientCommandExecute(object p)
        {
            async Task SerializeAsync()
            {
                await Task.Run(() => { XmlSerialize(Clients); });
            }
            await SerializeAsync();
            //XmlSerialize(Clients);

            //HistoryWindow = new HistoryWindow();
            //HistoryWindow.Show();
        }

        /// <summary>
        /// Убрать фокус с выбранных элементов при нажатии на пустое пространство
        /// </summary>
        public ICommand ClearFocusCommand { get; }
        private bool CanClearFocusCommadExecute(object p) => true;
        private void OnClearFocusCommandExecute(object p)
        {
            if (ManagerWindow.myListView.SelectedItems.Count != 0)
            {
                ManagerWindow.myListView.SelectedItems[0] = false;
            }
        }

        /// <summary>
        /// Открыть окно перевода счетов
        /// </summary>
        public ICommand OpenTransactionWindowCommand { get; }
        private bool CanOpenTransactionrWindowCommandExecute(object p) => true;
        private void OnOpenTransactionWindowCommandExecute(object p)
        {
            TransactionWindow = new TransactionWindow();
            ManagerWindow.Close();
            TransactionWindow.Show();
        }

        /// <summary>
        /// Открыть окно истории
        /// </summary>
        public ICommand OpenHistoryWindowCommand { get; }
        private bool CanOpenHistoryWindowCommandExecute(object p) => true;
        private async void OnOpenHistoryWindowCommandExecute(object p)
        {
            HistoryWindow = new HistoryWindow();
            HistoryWindow.HistoryBlock.Text = await ReadToFileHistoryLog();
            ManagerWindow.Close();
            HistoryWindow.Show();
        }

        //string historyPath = @"..\Debug\HistoryLog.txt";
        //async Task<string> ReadToFileHistoryLog()
        //{
        //    using (StreamReader sr = new StreamReader(historyPath))
        //    {
        //        string text = await sr.ReadToEndAsync();
        //        return text;
        //    }
        //}

        /// <summary>
        /// Назад в окно менеджера
        /// </summary>
        public ICommand BackToManagerWindowCommand { get; }
        private bool CanBackToManagerWindowCommandExecute(object p) => true;
        private void OnBackToManagerWindowCommandExecute(object p)
        {
            ManagerWindow = new ManagerWindow();
            TransactionWindow.Close();
            ManagerWindow.Show();
        }

        /// <summary>
        /// Поиск клиента по ID 
        /// </summary>
        public ICommand SearchCommand { get; }
        private bool CanSearchCommandExecute(object p) => true;
        private void OnSearchCommandExecute(object p)
        {
            foreach (Client client in Clients)
            {
                if (TransactionWindow.SearchBox.Text == client.ID)
                {
                    TransactionWindow.FoundBalanceBlock.Text = client.Bill;
                    TransactionWindow.DepFoundBalanceBlock.Text = client.DepBill;
                    if (client.DepBill == "Закрытый")
                    {
                        TransactionWindow.OpenDepositButton.Content = "Открыть";
                    }
                    else { TransactionWindow.OpenDepositButton.Content = "Закрыть"; }
                    if (client.Bill == "Закрытый")
                    {
                        TransactionWindow.OpenNonDepositButton.Content = "Открыть";
                    }
                    else { TransactionWindow.OpenNonDepositButton.Content = "Закрыть"; }
                }
                //else { MessageBox.Show("Not Found!"); }
            }
        }

        /// <summary>
        /// Пополнение счёта найденного по ID клиента 
        /// </summary>
        public ICommand NonDepPlusCommand { get; }
        private bool CanNonDepPlusCommandExecute(object p) => true;
        private void OnNonDepPlusCommandExecute(object p)
        {
            MessageWindow = new MessageWindow();
            foreach (Client client in Clients)
            {
                if (TransactionWindow.NonDepAccountIDBlock.Text == client.ID && client.Bill != "Закрытый")
                {
                    if (TransactionWindow.NonDepAmountBlock.Text != string.Empty)
                    {
                        int sums = int.Parse(client.Bill) + int.Parse(TransactionWindow.NonDepAmountBlock.Text);
                        client.Bill = Convert.ToString(sums);
                        MessageWindow.MessageBlock.Text = $"счёт {client} пополнен";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                }
            }
            XmlSerialize(Clients);
        }

        //string historyLogPath = @"..\Debug\HistoryLog.txt";

        //async void WriteToFileHistoryLog(string propertyName)
        //{
        //    using (StreamWriter sw = new StreamWriter(historyLogPath, true))
        //    {
        //        await sw.WriteLineAsync($"Изменено {propertyName}");
        //    }
        //}

        /// <summary>
        /// Пополнение депозитного счёта найденного по ID клиента 
        /// </summary>
        public ICommand DepositPlusCommand { get; }
        private bool CanDepositPlusCommandExecute(object p) => true;
        private void OnDepositPlusCommandExecute(object p)
        {
            MessageWindow = new MessageWindow();
            foreach (Client client in Clients)
            {
                if (TransactionWindow.DepAccountIDBlock.Text == client.ID && client.DepBill != "Закрытый")
                {
                    if (TransactionWindow.DepAmountBlock.Text != string.Empty)
                    {
                        int sums = int.Parse(client.DepBill) + int.Parse(TransactionWindow.DepAmountBlock.Text);
                        client.DepBill = Convert.ToString(sums);
                        MessageWindow.MessageBlock.Text = $"депозитный счёт {client} пополнен";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                }
            }
            XmlSerialize(Clients);
        }

        private static int minusClientBalance;
        //private static bool ExistFromDepBill;
        private static bool ExistToBill = true;
        private static bool ExistToDepBill = true;
        private static Client dataNonDepositClient;
        private static Client dataDepositClient;

        /// <summary>
        /// Перевод счёта от найденного по ID клиента к другому
        /// </summary>
        public ICommand TransferCommand { get; }
        private bool CanTransferCommandExecute(object p) => true;
        private void OnTransferCommandExecute(object p)
        {
            MessageWindow = new MessageWindow();
            foreach (Client client in Clients)
            {
                //if (client.Bill == "Закрытый") { MessageBox.Show("FromСчёт закрытый"); }
                if (TransactionWindow.FromAccountTransaction.Text == client.ID && TransactionWindow.FromAccountTransaction.Text != String.Empty
                    && TransactionWindow.ToAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.TransactionAmountBlock.Text != String.Empty)
                    {
                        int amount = int.Parse(TransactionWindow.TransactionAmountBlock.Text);
                        int clientBalance = int.Parse(client.Bill) - amount;
                        client.Bill = Convert.ToString(clientBalance);
                        minusClientBalance = amount;

                        dataNonDepositClient = client;
                    }
                }
            }

            foreach (Client client in Clients)
            {
                if (client.Bill == "Закрытый") { ExistToBill = false; }
                //if (client.Bill == "Закрытый") { MessageBox.Show("ToСчёт закрытый"); }
                if (TransactionWindow.ToAccountTransaction.Text == client.ID && TransactionWindow.ToAccountTransaction.Text != String.Empty
                    && TransactionWindow.FromAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.TransactionAmountBlock.Text != String.Empty && client.Bill != "Закрытый")
                    {
                        int sums = int.Parse(client.Bill) + minusClientBalance;
                        client.Bill = Convert.ToString(sums);

                        MessageWindow.MessageBlock.Text = $"счёт переведен от {dataNonDepositClient} к {client} количесвто {minusClientBalance}";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                }
            }
            XmlSerialize(Clients);
        }

        /// <summary>
        /// Перевод депозитного счёта от найденного по ID клиента к другому
        /// </summary>
        public ICommand DepTransferCommand { get; }
        private bool CanDepTransferCommandExecute(object p) => true;
        private void OnDepTransferCommandExecute(object p)
        {
            MessageWindow = new MessageWindow();
            foreach (Client client in Clients)
            {
                if (TransactionWindow.FromAccountTransaction.Text == client.ID && TransactionWindow.FromAccountTransaction.Text != String.Empty
                    && TransactionWindow.ToAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.DepTransactionAmountBlock.Text != String.Empty)
                    {
                        int amount = int.Parse(TransactionWindow.DepTransactionAmountBlock.Text);
                        int clientBalance = int.Parse(client.DepBill) - amount;
                        client.DepBill = Convert.ToString(clientBalance);
                        minusClientBalance = amount;

                        dataDepositClient = client;
                    }
                }
            }

            foreach (Client client in Clients)
            { 
                if (client.DepBill == "Закрытый") { ExistToDepBill = false; }

                if (TransactionWindow.ToAccountTransaction.Text == client.ID && TransactionWindow.ToAccountTransaction.Text != String.Empty
                    && TransactionWindow.FromAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.DepTransactionAmountBlock.Text != String.Empty && client.DepBill != "Закрытый")
                    {
                        int sums = int.Parse(client.DepBill) + minusClientBalance;
                        client.DepBill = Convert.ToString(sums);
                        MessageWindow.MessageBlock.Text = $"депозитный счёт переведен от {dataDepositClient} к {client} количесвто {minusClientBalance}";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                    else { MessageBox.Show("Счёт клиента закрытый"); }
                }
            }
            XmlSerialize(Clients);
        }

        /// <summary>
        /// Открытие или закрытие депозитного счёта найденного по ID клиенту
        /// </summary>
        public ICommand OpenOrCloseDepositCommand { get; }
        private bool CanOpenDepositCommandExecute(object p) => true;
        private void OnOpenDepositCommandExecute(object p)
        {
            MessageWindow = new MessageWindow();
            foreach (Client client in Clients)
            {
                if (TransactionWindow.SearchBox.Text == client.ID)
                {
                    if (client.DepBill == "Закрытый")
                    {
                        client.DepBill = "0";
                        MessageWindow.MessageBlock.Text = $"депозитный счёт {client} открыт";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                    else 
                    { 
                        client.DepBill = "Закрытый";
                        MessageWindow.MessageBlock.Text = $"депозитный счёт {client} закрыт";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                }
            }
            XmlSerialize(Clients);
        }
         
        /// <summary>
        /// Открытие или закрытие счёта найденного по ID клиенту
        /// </summary>
        public ICommand OpenOrCloseNonDepositCommand { get; }
        private bool CanOpenNonDepositCommandExecute(object p) => true;
        private void OnOpenNonDepositCommandExecute(object p)
        {
            MessageWindow = new MessageWindow();
            foreach (Client client in Clients)
            {
                if (TransactionWindow.SearchBox.Text == client.ID)
                {
                    if (client.Bill == "Закрытый")
                    {
                        client.Bill = "0";
                        MessageWindow.MessageBlock.Text = $"счёт {client} открыт";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                    else 
                    {
                        client.Bill = "Закрытый";
                        MessageWindow.MessageBlock.Text = $"счёт {client} закрыт";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.ShowDialog();
                    }
                }
            }
            XmlSerialize(Clients);
        }

        #endregion

        string historyLogPath = @"..\Debug\HistoryLog.txt";

        async void WriteToFileHistoryLog(string propertyName)
        {
            using (StreamWriter sw = new StreamWriter(historyLogPath, true))
            {
                await sw.WriteLineAsync($"{ propertyName} ВРЕМЯ {DateTime.Now}");
            }
        }

        string historyPath = @"..\Debug\HistoryLog.txt";
        async Task<string> ReadToFileHistoryLog()
        {
            using (StreamReader sr = new StreamReader(historyPath))
            {
                string text = await sr.ReadToEndAsync();
                return text;
            }
        }

        string path = @"..\Debug\Client.xml"; //..\Publications\TravelBrochure.pdf

        public MainWindowViewModel()
        {
            OpenManagerWindowCommand = new LambdaCommand(OnOpenManagerWindowCommandExecute, CanOpenManagerWindowCommandExecute);
            OpenHistoryWindowCommand = new LambdaCommand(OnOpenHistoryWindowCommandExecute, CanOpenHistoryWindowCommandExecute);
            CreateNewClientCommand = new LambdaCommand(OnCreateNewClientCommandExecute, CanCreateNewClientCommandExecute);
            DeleteClientCommand = new LambdaCommand(OnDeleteClientCommandExecute, CanDeleteClientCommandExecute);
            ChangeClientCommand = new LambdaCommand(OnChangeClientCommandExecute, CanChangeClientCommandExecute);
            ClearFocusCommand = new LambdaCommand(OnClearFocusCommandExecute, CanClearFocusCommadExecute);
            OpenTransactionWindowCommand = new LambdaCommand(OnOpenTransactionWindowCommandExecute, CanOpenTransactionrWindowCommandExecute);
            BackToManagerWindowCommand = new LambdaCommand(OnBackToManagerWindowCommandExecute, CanBackToManagerWindowCommandExecute);
            NonDepPlusCommand = new LambdaCommand(OnNonDepPlusCommandExecute, CanNonDepPlusCommandExecute);
            DepositPlusCommand = new LambdaCommand(OnDepositPlusCommandExecute, CanDepositPlusCommandExecute);
            SearchCommand = new LambdaCommand(OnSearchCommandExecute, CanSearchCommandExecute);
            TransferCommand = new LambdaCommand(OnTransferCommandExecute, CanTransferCommandExecute);
            DepTransferCommand = new LambdaCommand(OnDepTransferCommandExecute, CanDepTransferCommandExecute);
            OpenOrCloseDepositCommand = new LambdaCommand(OnOpenDepositCommandExecute, CanOpenDepositCommandExecute);
            OpenOrCloseNonDepositCommand = new LambdaCommand(OnOpenNonDepositCommandExecute, CanOpenNonDepositCommandExecute);

            //Clients = new TrulyObservableCollection<Client>();
            //Clients = new FullyObservableCollection<Client>();
            Clients = new ObservableCollection<Client>();
            //Clients.CollectionChanged += ContentCollectionChanged;
            if (File.Exists(path)){ XmlDeserialize(Clients); }
            else { GenerationClient(); }
        }

        public void ContentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Client item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= MyClass_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Client item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += MyClass_PropertyChanged;
                }
            }
        }

        void MyClass_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ExecuteOnAnyChangeOfCollection();
        }

        private void ExecuteOnAnyChangeOfCollection()
        {
            MessageBox.Show("Collection has changed");
        }

        //void MyCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        foreach (Client item in e.NewItems) 
        //        {
        //            item.PropertyChanged += MyClass_PropertyChanged;
        //        }
        //    }

        //    if (e.OldItems != null)
        //    {
        //        foreach (Client item in e.OldItems)
        //        {
        //            item.PropertyChanged -= MyClass_PropertyChanged;
        //        }
        //    }
        //    ExecuteOnAnyChangeOfCollection();
        //}

        void GenerationClient()
        {
            Random random = new Random();

            bool billRandomBool;
            bool depBillRandomBool;
            int billRandValue;
            int depBillRandValue;
            int value = random.Next(1, 6);
            string depBill;
            string bill;

            for (int i = 0; i < value; i++)
            {
                billRandomBool = random.Next(2) == 1;
                depBillRandomBool = random.Next(2) == 1;

                billRandValue = random.Next(100, 1001);
                depBillRandValue = random.Next(100, 201);

                if (depBillRandomBool == true) { depBill = depBillRandValue.ToString(); }
                else { depBill = "Закрытый"; }

                if (billRandomBool == true) { bill = billRandValue.ToString(); }
                else { bill = "Закрытый"; }

                Clients.Add(new Client(i.ToString(), Faker.Internet.Email(), Faker.Name.First(), Faker.Name.Middle(),
                    Faker.Name.Last(), Faker.Phone.Number(), Faker.Address.StreetName(), bill, depBill));
            }
            XmlSerialize(Clients);
        }

        void XmlSerialize(ObservableCollection<Client> clients)
        {
            File.WriteAllText(path, String.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Client>));
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, clients);
            }
        }

        void XmlDeserialize(ObservableCollection<Client> clients)
        {
            if (File.Exists(path))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Client>));
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    ObservableCollection<Client> newClients = xmlSerializer.Deserialize(fs) as ObservableCollection<Client>;
                    if (newClients != null)
                    {
                        foreach (Client client in newClients)
                        {
                            Clients.Add(client);
                        }
                    }
                }
            }
        }
    }
}

