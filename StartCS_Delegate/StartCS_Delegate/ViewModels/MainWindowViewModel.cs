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
using StartCS_Delegate.Views.HistoryWindow;
using StartCS_Delegate.Views.MessageWindow;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Data;
using Faker;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;

namespace StartCS_Delegate.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
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
            MainWindow.progressBar.Visibility = Visibility.Visible;
            MainWindow.progressBar.ValueChanged += ProgressBar_ValueChanged;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainWindow.progressBar.Value == 100)
            {
                ManagerWindow = new ManagerWindow();
                ManagerWindow.ChangeWorkerComboBox.SelectedIndex = 0;
                ManagerWindow.WorkerImage.Source = new BitmapImage(new Uri("/Images/Manager.png", UriKind.Relative));
                ManagerWindow.MainGrid.Background = new SolidColorBrush(Colors.MediumTurquoise);
                ManagerWindow.Show();
                MainWindow.Close();
            }
        }

        /// <summary>
        /// Открытие окна для консультанта
        /// </summary>
        public ICommand OpenConsultantWindowCommand { get; }
        private bool CanOpenConsultantWindowCommandExecute(object p) => true;
        private void OnOpenConsultantWindowCommandExecute(object p)
        {
            MainWindow.progressBar.Visibility = Visibility.Visible;
            MainWindow.progressBar.ValueChanged += ProgressBar_ValueChangedForConsultant;
        }

        private void ProgressBar_ValueChangedForConsultant(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainWindow.progressBar.Value == 100)
            {
                ManagerWindow = new ManagerWindow();
                ManagerWindow.ChangeWorkerComboBox.SelectedIndex = 1;
                ManagerWindow.WorkerImage.Source = new BitmapImage(new Uri("/Images/Consultant.png", UriKind.Relative));
                ManagerWindow.MainGrid.Background = new SolidColorBrush(Colors.LightSeaGreen);
                MatterManagerWindowElements();
                ManagerWindow.Show();
                MainWindow.Close();
            }
        }

        void MatterManagerWindowElements()
        {
            ManagerWindow.IDBox.IsReadOnly = true;
            ManagerWindow.EmailBox.IsReadOnly = true;
            ManagerWindow.SurnameBox.IsReadOnly = true;
            ManagerWindow.NameBox.IsReadOnly = true;
            ManagerWindow.PatronymicBox.IsReadOnly = true;
            ManagerWindow.AddressBox.IsReadOnly = true;
            ManagerWindow.OpenTransferWindowButton.IsEnabled = false;
            ManagerWindow.AddClientButton.IsEnabled = false;
            ManagerWindow.DeleteClientButton.IsEnabled = false;
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
                MessageBox.Show("Неполная информация!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (newClient.Bill == String.Empty && newClient.DepBill == String.Empty)
                {
                    newClient.Bill = "Закрытый";
                    newClient.DepBill = "Закрытый";
                    Clients.Add(newClient);

                    WriteToFileHistoryLog($"\nДобавлен Менеджером {newClient.ToString().ToUpper()}");
                    EmptyStringValue();
                    XmlSerialize(Clients);
                }
                else
                {
                    if (Clients.Contains(newClient) == true)
                    {
                        Clients.Add(newClient);

                        WriteToFileHistoryLog($"\nДобавлен Менеджером {newClient.ToString().ToUpper()}");
                        EmptyStringValue();
                        XmlSerialize(Clients);
                    }
                }
            }
        }

        void EmptyStringValue()
        {
            ManagerWindow.IDBox.Text = "";
            ManagerWindow.EmailBox.Text = "";
            ManagerWindow.NameBox.Text = "";
            ManagerWindow.SurnameBox.Text = "";
            ManagerWindow.PatronymicBox.Text = "";
            ManagerWindow.NumberPhoneBox.Text = "";
            ManagerWindow.AddressBox.Text = "";
            ManagerWindow.BillBox.Text = "";
            ManagerWindow.DepBillBox.Text = "";
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
            WriteToFileHistoryLog($"\nУдалён Менеджером {client.ToString().ToUpper()}");
            XmlSerialize(Clients);
        }

        /// <summary>
        /// Сохранение изменений клииента
        /// </summary>
        public ICommand ChangeClientCommand { get; }
        private bool CanChangeClientCommandExecute(object p) => p is Client client && Clients.Contains(client);
        private void OnChangeClientCommandExecute(object p)
        {
            XmlSerialize(Clients);
            MessageWindow = new MessageWindow();
            MessageWindow.Owner = ManagerWindow;
            MessageWindow.MessageBlock.Text = "Изменения сохранены, подробности в журнале историй";
            MessageWindow.Show();
        }

        /// <summary>
        /// Убрать фокус с выбранных элементов при нажатии на пустое пространство
        /// </summary>
        public ICommand ClearFocusCommand { get; }
        private bool CanClearFocusCommadExecute(object p) => true;
        private void OnClearFocusCommandExecute(object p)
        {
            if (ManagerWindow != null)
            {
                if (ManagerWindow.myListView.SelectedItems.Count != 0)
                {
                    ManagerWindow.myListView.SelectedItems[0] = false;
                }
            }

            if (TransactionWindow != null)
            {
                TransactionWindow.FoundBalanceBlock.Background = new SolidColorBrush(Colors.White);
                TransactionWindow.DepFoundBalanceBlock.Background = new SolidColorBrush(Colors.White);
                TransactionWindow.OpenNonDepositButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                TransactionWindow.OpenNonDepositButton.Content = "";
                TransactionWindow.OpenDepositButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                TransactionWindow.OpenDepositButton.Content = "";
                TransactionWindow.DepButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                TransactionWindow.NonDepButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                TransactionWindow.FromIDNonDepositBox.Background = new SolidColorBrush(Colors.White);
                TransactionWindow.FromIDDepositBox.Background = new SolidColorBrush(Colors.White);
                TransactionWindow.ToIDNonDepositBox.Background = new SolidColorBrush(Colors.White);
                TransactionWindow.ToIDDepositBox.Background = new SolidColorBrush(Colors.White);

                TransactionWindow.SearchBox.Text = "";
                TransactionWindow.FoundBalanceBlock.Text = "";
                TransactionWindow.DepFoundBalanceBlock.Text = "";
                TransactionWindow.NonDepAccountIDBlock.Text = "";
                TransactionWindow.NonDepAmountBlock.Text = "";
                TransactionWindow.DepAccountIDBlock.Text = "";
                TransactionWindow.DepAmountBlock.Text = "";
                TransactionWindow.FromAccountTransaction.Text = "";
                TransactionWindow.ToAccountTransaction.Text = "";
                TransactionWindow.FromIDNonDepositBox.Text = "";
                TransactionWindow.FromIDDepositBox.Text = "";
                TransactionWindow.ToIDNonDepositBox.Text = "";
                TransactionWindow.ToIDDepositBox.Text = "";
                TransactionWindow.TransactionAmountBlock.Text = "";
                TransactionWindow.DepTransactionAmountBlock.Text = "";
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
        private void OnOpenHistoryWindowCommandExecute(object p)
        {
            HistoryWindow = new HistoryWindow();

            ReadFileReverse();

            ManagerWindow.Close();
            HistoryWindow.Show();
        }

        /// <summary>
        /// Очистка истории
        /// </summary>
        public ICommand ClearHistoryCommand { get; }
        private bool CanClearHistoryCommandExecute(object p) => true;
        async private void OnClearHistoryCommandExecute(object p)
        {
            System.IO.File.WriteAllText(historyLogPath, string.Empty);
           
            HistoryWindow.HistoryControl.Items.Add(await ReadToFileHistoryLog());
            HistoryWindow.HistoryControl.Items.Clear();

            ReadFileReverse();
        }

        /// <summary>
        /// Назад в окно менеджера
        /// </summary>
        public ICommand BackToManagerWindowCommand { get; }
        private bool CanBackToManagerWindowCommandExecute(object p) => true;
        private void OnBackToManagerWindowCommandExecute(object p)
        {
            ManagerWindow = new ManagerWindow();
            ManagerWindow.ChangeWorkerComboBox.SelectedIndex = 1;
            ManagerWindow.WorkerImage.Source = new BitmapImage(new Uri("/Images/Consultant.png", UriKind.Relative));
            ManagerWindow.MainGrid.Background = new SolidColorBrush(Colors.LightSeaGreen);
            MatterManagerWindowElements();
            TransactionWindow?.Close();
            HistoryWindow?.Close();
            ManagerWindow.Show();
        }

        /// <summary>
        /// Поиск клиента по ID 
        /// </summary>
        public ICommand SearchCommand { get; }
        private bool CanSearchCommandExecute(object p) => true;
        private void OnSearchCommandExecute(object p)
        {
            SearchCommandMethod();
        }

        void SearchCommandMethod()
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
                        TransactionWindow.OpenDepositButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;                       
                        TransactionWindow.DepButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorCloseStyle") as Style;
                        TransactionWindow.DepFoundBalanceBlock.Background = new SolidColorBrush(Colors.DarkRed);
                    }
                    else
                    { 
                        TransactionWindow.OpenDepositButton.Content = "Закрыть";
                        TransactionWindow.OpenDepositButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorCloseStyle") as Style;
                        TransactionWindow.DepButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                        TransactionWindow.DepFoundBalanceBlock.Background = new SolidColorBrush(Colors.White);
                    }

                    if (client.Bill == "Закрытый")
                    {
                        TransactionWindow.OpenNonDepositButton.Content = "Открыть";
                        TransactionWindow.OpenNonDepositButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                        TransactionWindow.NonDepButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorCloseStyle") as Style;
                        TransactionWindow.FoundBalanceBlock.Background = new SolidColorBrush(Colors.DarkRed);
                    }
                    else 
                    { 
                        TransactionWindow.OpenNonDepositButton.Content = "Закрыть";
                        TransactionWindow.OpenNonDepositButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorCloseStyle") as Style;                      
                        TransactionWindow.NonDepButton.Style = TransactionWindow.OpenDepositButton.TryFindResource("ButtonColorOpenStyle") as Style;
                        TransactionWindow.FoundBalanceBlock.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }

            foreach (Client client in Clients)
            {
                if (TransactionWindow.FromAccountTransaction.Text == client.ID && TransactionWindow.FromAccountTransaction.Text != String.Empty
                    && TransactionWindow.ToAccountTransaction.Text != string.Empty)
                {
                    TransactionWindow.FromIDNonDepositBox.Text = client.Bill;
                    TransactionWindow.FromIDDepositBox.Text = client.DepBill;
                }
            }

            foreach (Client client in Clients)
            {
                if (TransactionWindow.ToAccountTransaction.Text == client.ID && TransactionWindow.ToAccountTransaction.Text != String.Empty
                   && TransactionWindow.FromAccountTransaction.Text != String.Empty)
                {
                    TransactionWindow.ToIDDepositBox.Text = client.DepBill;
                    TransactionWindow.ToIDNonDepositBox.Text = client.Bill;
                }
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
                        MessageWindow.MessageBlock.Text = $"\nСчёт {client.ToString().ToUpper()} пополнен";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.Show();
                    }
                }
            }
            SearchCommandMethod();
            XmlSerialize(Clients);
        }

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
                        MessageWindow.MessageBlock.Text = $"\nДепозитный счёт {client.ToString().ToUpper()} пополнен";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.Show();
                    }
                }
            }
            SearchCommandMethod();
            XmlSerialize(Clients);
        }

        /// <summary>
        /// Поиск клиентов по ID от отправителя к получателю
        /// </summary>
        public ICommand SearchIDFromToCommand { get; }
        private bool CanSearchIDFromToCommandExecute(object p) => true;
        private void OnSearchIDFromToCommandExecute(object p)
        {
            foreach (Client client in Clients)
            {
                if (TransactionWindow.FromAccountTransaction.Text == client.ID && TransactionWindow.FromAccountTransaction.Text != String.Empty
                    && TransactionWindow.ToAccountTransaction.Text != string.Empty)
                {
                    TransactionWindow.FromIDNonDepositBox.Text = client.Bill;
                    TransactionWindow.FromIDDepositBox.Text = client.DepBill;

                    if (TransactionWindow.FromIDNonDepositBox.Text == "Закрытый")
                    {
                        TransactionWindow.FromIDNonDepositBox.Background = new SolidColorBrush(Colors.DarkRed);
                    }
                    else { TransactionWindow.FromIDNonDepositBox.Background = new SolidColorBrush(Colors.White); }

                    if (TransactionWindow.FromIDDepositBox.Text == "Закрытый")
                    {
                        TransactionWindow.FromIDDepositBox.Background = new SolidColorBrush(Colors.DarkRed);
                    }
                    else { TransactionWindow.FromIDDepositBox.Background = new SolidColorBrush(Colors.White); }

                }
                else continue;
            }

            foreach (Client client in Clients)
            {
                if (TransactionWindow.ToAccountTransaction.Text == client.ID && TransactionWindow.ToAccountTransaction.Text != String.Empty
                   && TransactionWindow.FromAccountTransaction.Text != String.Empty)
                {
                    TransactionWindow.ToIDDepositBox.Text = client.DepBill;
                    TransactionWindow.ToIDNonDepositBox.Text = client.Bill;

                    if (TransactionWindow.ToIDDepositBox.Text == "Закрытый") { TransactionWindow.ToIDDepositBox.Background = new SolidColorBrush(Colors.DarkRed); }
                    else { TransactionWindow.ToIDDepositBox.Background = new SolidColorBrush(Colors.White); }

                    if (TransactionWindow.ToIDNonDepositBox.Text == "Закрытый") { TransactionWindow.ToIDNonDepositBox.Background = new SolidColorBrush(Colors.DarkRed); }
                    else { TransactionWindow.ToIDNonDepositBox.Background = new SolidColorBrush(Colors.White); }
                }
                else continue;
            }
        }

        private static int minusClientBalance;
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
                if (TransactionWindow.ToAccountTransaction.Text == client.ID && TransactionWindow.ToAccountTransaction.Text != String.Empty
                    && TransactionWindow.FromAccountTransaction.Text != String.Empty)
                {
                    if (TransactionWindow.TransactionAmountBlock.Text != String.Empty && client.Bill != "Закрытый")
                    {
                        int sums = int.Parse(client.Bill) + minusClientBalance;
                        client.Bill = Convert.ToString(sums);

                        MessageWindow.MessageBlock.Text = $"\nСчёт переведен от ||{dataNonDepositClient}|| к ||{client}|| сумма перевода ({minusClientBalance})";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.Show();
                    }
                    else { MessageBox.Show("Счёт клиента закрытый"); }
                }
            }
            SearchCommandMethod();
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
                    if (TransactionWindow.DepTransactionAmountBlock.Text != String.Empty && client.Bill != "Закрытый")
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
                        MessageWindow.MessageBlock.Text = $"\nДепозитный счёт переведен от ||{dataDepositClient}|| к ||{client}|| сумма перевода ({minusClientBalance})";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.Show();
                    }
                    else { MessageBox.Show("Депозитный счёт клиента закрытый"); }
                }
            }
            SearchCommandMethod();
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
                        MessageWindow.MessageBlock.Text = $"\nДепозитный счёт {client} открыт";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.Show();
                    }
                    else
                    {
                        if (MessageBox.Show("Внимание весь депозитный счёт будет обнулён! Вы уверены что хотите продолжить?", "Внимание",
                           MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            client.DepBill = "Закрытый";
                            MessageWindow.MessageBlock.Text = $"\nДепозитный счёт {client} закрыт";
                            WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                            MessageWindow.Show();
                        }
                    }
                }
            }
            SearchCommandMethod();
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
                        MessageWindow.MessageBlock.Text = $"\nСчёт {client} открыт";
                        WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                        MessageWindow.Show();
                    }
                    else
                    {
                        if (MessageBox.Show("Внимание весь счёт будет обнулён! Вы уверены что хотите продолжить?", "Внимание", 
                            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            client.Bill = "Закрытый";
                            MessageWindow.MessageBlock.Text = $"\nСчёт {client} закрыт";
                            WriteToFileHistoryLog(MessageWindow.MessageBlock.Text.ToString());
                            MessageWindow.Show();
                        }
                    }
                }
            }
            SearchCommandMethod();
            XmlSerialize(Clients);
        }

        /// <summary>
        /// Выбор консультанта в комбобокс
        /// </summary>
        public ICommand ChooseConsultantCommand { get; }
        private bool CanChooseConsultantCommandExecute(object p) => true;
        private void OnChooseConsultantCommandExecute(object p)
        {
            ManagerWindow.ChangeWorkerComboBox.SelectedIndex = 1;
            ManagerWindow.MainGrid.Background = new SolidColorBrush(Colors.LightSeaGreen);
            ManagerWindow.WorkerImage.Source = new BitmapImage(new Uri("/Images/Consultant.png", UriKind.Relative));
            MatterManagerWindowElements();
        }

        /// <summary>
        /// Выбор менеджера в комбобокс
        /// </summary>
        public ICommand ChooseManagerCommand { get; }
        private bool CanChooseManagerCommandExecute(object p) => true;
        private void OnChooseManagerCommandExecute(object p)
        {
            ManagerWindow.ChangeWorkerComboBox.SelectedIndex = 0;
            ManagerWindow.MainGrid.Background = new SolidColorBrush(Colors.MediumTurquoise);
            ManagerWindow.WorkerImage.Source = new BitmapImage(new Uri("/Images/Manager.png", UriKind.Relative));
            ManagerWindow.IDBox.IsReadOnly = false;
            ManagerWindow.EmailBox.IsReadOnly = false;
            ManagerWindow.SurnameBox.IsReadOnly = false;
            ManagerWindow.NameBox.IsReadOnly = false;
            ManagerWindow.PatronymicBox.IsReadOnly = false;
            ManagerWindow.AddressBox.IsReadOnly = false;
            ManagerWindow.OpenTransferWindowButton.IsEnabled = true;
            ManagerWindow.AddClientButton.IsEnabled = true;
            ManagerWindow.DeleteClientButton.IsEnabled = true;
        }

        /// <summary>
        /// Выбор цвета в зависимоти от состояния счёта
        /// </summary>
        public ICommand ChooseColorBillBoxCommand { get; }
        private bool CanChooseColorBillBoxCommandExecute(object p) => true;
        private void OnChooseColorBillBoxCommandExecute(object p)
        {
            if (ManagerWindow.BillBox.Text == "Закрытый" && ManagerWindow.BillBox.Text != String.Empty) { ManagerWindow.BillBox.Background = new SolidColorBrush(Colors.Red); }
            else ManagerWindow.BillBox.Background = new SolidColorBrush(Colors.White);

            if (ManagerWindow.DepBillBox.Text == "Закрытый" && ManagerWindow.BillBox.Text != String.Empty) { ManagerWindow.DepBillBox.Background = new SolidColorBrush(Colors.Red); }
            else ManagerWindow.DepBillBox.Background = new SolidColorBrush(Colors.White);
        }

        public void TextChangedBillBox(object sender, TextChangedEventArgs args)
        {
            if (ManagerWindow.BillBox.Text == "Закрытый" && ManagerWindow.BillBox.Text != String.Empty) { ManagerWindow.BillBox.Background = new SolidColorBrush(Colors.Red); }
            else ManagerWindow.BillBox.Background = new SolidColorBrush(Colors.White);

            if (ManagerWindow.DepBillBox.Text == "Закрытый" && ManagerWindow.BillBox.Text != String.Empty) { ManagerWindow.DepBillBox.Background = new SolidColorBrush(Colors.Red); }
            else ManagerWindow.DepBillBox.Background = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Выход из приложения
        /// </summary>
        public ICommand CloseApplicationCommand { get; }
        private bool CanCloseApplicationCommandExecute(object p) => true;
        private void OnCloseApplicationCommandExecute(object p)
        {
            ManagerWindow.Close();
        }

        #endregion

        string historyLogPath = @"..\Debug\HistoryLog.txt";
        
        async void WriteToFileHistoryLog(string propertyName)
        {
            using (StreamWriter sw = new StreamWriter(historyLogPath, true))
            {
                await sw.WriteLineAsync($"{ propertyName} || ВРЕМЯ {DateTime.Now}");
            }
        }
        
        async Task<string> ReadToFileHistoryLog()
        {
            using (StreamReader sr = new StreamReader(historyLogPath))
            {
                string text = await sr.ReadToEndAsync();
                return text;
            }
        }

        void ReadFileReverse()
        {
            var lines = File.ReadAllLines(historyLogPath).Reverse();

            foreach (string line in lines)
            {
                HistoryWindow.HistoryControl.Items.Add(line);
                HistoryWindow.HistoryControl.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        string path = @"..\Debug\Client.xml"; 

        public MainWindowViewModel()
        {
            OpenManagerWindowCommand = new LambdaCommand(OnOpenManagerWindowCommandExecute, CanOpenManagerWindowCommandExecute);
            OpenConsultantWindowCommand = new LambdaCommand(OnOpenConsultantWindowCommandExecute, CanOpenConsultantWindowCommandExecute);
            OpenHistoryWindowCommand = new LambdaCommand(OnOpenHistoryWindowCommandExecute, CanOpenHistoryWindowCommandExecute);
            ClearHistoryCommand = new LambdaCommand(OnClearHistoryCommandExecute, CanClearHistoryCommandExecute);
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
            ChooseConsultantCommand = new LambdaCommand(OnChooseConsultantCommandExecute, CanChooseConsultantCommandExecute);
            ChooseManagerCommand = new LambdaCommand(OnChooseManagerCommandExecute, CanChooseManagerCommandExecute);
            SearchIDFromToCommand = new LambdaCommand(OnSearchIDFromToCommandExecute, CanSearchIDFromToCommandExecute);
            ChooseColorBillBoxCommand = new LambdaCommand(OnChooseColorBillBoxCommandExecute, CanChooseColorBillBoxCommandExecute);
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecute, CanCloseApplicationCommandExecute);

            Clients = new ObservableCollection<Client>();
            Clients.CollectionChanged += ContentCollectionChanged;
            if (File.Exists(path)){ XmlDeserialize(Clients); }
            else { GenerationClient(); }
        }

        public void ContentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Client item in e.OldItems)
                {
                    item.PropertyChanged -= MyClass_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Client item in e.NewItems)
                {
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
            XmlSerialize(Clients);

            if (Convert.ToInt32(ManagerWindow.ChangeWorkerComboBox.SelectedIndex) == 0)
            { WriteToFileHistoryLog("\nИзменения внесены Менеджером"); }
            else WriteToFileHistoryLog("\nИзменения внесены Консультантом");
        }

        void MyCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Client item in e.NewItems)
                {
                    item.PropertyChanged += MyClass_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Client item in e.OldItems)
                {
                    item.PropertyChanged -= MyClass_PropertyChanged;
                }
            }
            ExecuteOnAnyChangeOfCollection();
        }

        void GenerationClient()
        {
            Random random = new Random();

            bool billRandomBool;
            bool depBillRandomBool;
            int billRandValue;
            int depBillRandValue;
            int value = random.Next(5, 11);
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

        public void XmlSerialize(ObservableCollection<Client> clients)
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

