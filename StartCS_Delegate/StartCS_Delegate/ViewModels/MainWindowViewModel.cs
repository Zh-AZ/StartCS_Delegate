using System;
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

namespace StartCS_Delegate.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        public static ObservableCollection<Client> Clients { get; set; }
        private MainWindow MainWindow;
        static ManagerWindow ManagerWindow;
        static TransactionWindow TransactionWindow;

        private static Client _Selected;
        public Client Selected
        {
            get => _Selected;
            set => Set(ref _Selected, value);
        }

        public void OnViewInitialized(MainWindow mainWindow) { MainWindow = mainWindow; }

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

        public ICommand DeleteClientCommand { get; }
        private bool CanDeleteClientCommandExecute(object p) => p is Client client && Clients.Contains(client);
        private void OnDeleteClientCommandExecute(object p)
        {
            if (!(p is Client client)) return;
            Clients.Remove(client);
            XmlSerialize(Clients);
        }

        public ICommand ChangeClientCommand { get; }
        private bool CanChangeClientCommandExecute(object p) => p is Client client && Clients.Contains(client);
        private void OnChangeClientCommandExecute(object p)
        {
            XmlSerialize(Clients);
        }

        public ICommand ClearFocusCommand { get; }
        private bool CanClearFocusCommadExecute(object p) => true;
        private void OnClearFocusCommandExecute(object p)
        {
            if (ManagerWindow.myListView.SelectedItems.Count != 0)
            {
                ManagerWindow.myListView.SelectedItems[0] = false;
            }
        }

        public ICommand OpenTransactionWindowCommand { get; }
        private bool CanOpenTransactionrWindowCommandExecute(object p) => true;
        private void OnOpenTransactionWindowCommandExecute(object p)
        {
            TransactionWindow = new TransactionWindow();
            ManagerWindow.Close();
            TransactionWindow.Show();
        }

        public ICommand BackToManagerWindowCommand { get; }
        private bool CanBackToManagerWindowCommandExecute(object p) => true;
        private void OnBackToManagerWindowCommandExecute(object p)
        {
            ManagerWindow = new ManagerWindow();
            TransactionWindow.Close();
            ManagerWindow.Show();
        }

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
                }
                //else { MessageBox.Show("Not Found!"); }
            }
        }

        public ICommand NonDepPlusCommand { get; }
        private bool CanNonDepPlusCommandExecute(object p) => true;
        private void OnNonDepPlusCommandExecute(object p)
        {
            foreach (Client client in Clients)
            {
                if (TransactionWindow.NonDepAccountIDBlock.Text == client.ID && client.Bill != "Закрытый")
                {
                    if (TransactionWindow.NonDepAmountBlock.Text != string.Empty)
                    {
                        int sums = int.Parse(client.Bill) + int.Parse(TransactionWindow.NonDepAmountBlock.Text);
                        client.Bill = Convert.ToString(sums);
                    }
                }
            }
            XmlSerialize(Clients);
        }

        public ICommand DepositPlusCommand { get; }
        private bool CanDepositPlusCommandExecute(object p) => true;
        private void OnDepositPlusCommandExecute(object p)
        {
            foreach (Client client in Clients)
            {
                if (TransactionWindow.DepAccountIDBlock.Text == client.ID && client.DepBill != "Закрытый")
                {
                    if (TransactionWindow.DepAmountBlock.Text != string.Empty)
                    {
                        int sums = int.Parse(client.DepBill) + int.Parse(TransactionWindow.DepAmountBlock.Text);
                        client.DepBill = Convert.ToString(sums);
                    }
                }
            }
            XmlSerialize(Clients);
        }

        private static int minusClientBalance;
        //private static bool ExistFromDepBill;
        private static bool ExistToDepBill;

        public ICommand TransferCommand { get; }
        private bool CanTransferCommandExecute(object p) => true;
        private void OnTransferCommandExecute(object p)
        {
            foreach (Client client in Clients)
            {
                //if (client.Bill == "Закрытый") { MessageBox.Show("FromСчёт закрытый"); }
                if (TransactionWindow.FromAccountTransaction.Text == client.ID && TransactionWindow.FromAccountTransaction.Text != String.Empty
                    && TransactionWindow.ToAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.TransactionAmountBlock.Text != String.Empty)
                    {
                        int minusSum = int.Parse(TransactionWindow.TransactionAmountBlock.Text);
                        int sumsClientBalance = int.Parse(client.Bill) - minusSum;
                        client.Bill = Convert.ToString(sumsClientBalance);
                        minusClientBalance = minusSum;
                    }
                }
            }

            foreach (Client client in Clients)
            {
                if (client.Bill == "Закрытый") { ExistToDepBill = false; }
                //if (client.Bill == "Закрытый") { MessageBox.Show("ToСчёт закрытый"); }
                if (TransactionWindow.ToAccountTransaction.Text == client.ID && TransactionWindow.ToAccountTransaction.Text != String.Empty
                    && TransactionWindow.FromAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.TransactionAmountBlock.Text != String.Empty && client.Bill != "Закрытый")
                    {
                        int sums = int.Parse(client.Bill) + minusClientBalance;
                        client.Bill = Convert.ToString(sums);
                    }
                }
            }
            XmlSerialize(Clients);
        }

        public MainWindowViewModel()
        {
            OpenManagerWindowCommand = new LambdaCommand(OnOpenManagerWindowCommandExecute, CanOpenManagerWindowCommandExecute);
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

            Clients = new ObservableCollection<Client>();
            //GenerationClient();
            XmlDeserialize(Clients);
        }

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
            XmlDeserialize(Clients);
        }

        string path = @"..\Debug\Client.xml"; //..\Publications\TravelBrochure.pdf

        void XmlSerialize(ObservableCollection<Client> clients)
        {
            if (File.Exists(path))
            {
                File.WriteAllText(path, String.Empty);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Client>));
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    xmlSerializer.Serialize(fs, clients);
                }
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

