using StartCS_Delegate.Views.HistoryWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using StartCS_Delegate.Views.MessageWindow;
using StartCS_Delegate.Views.ManagerWindow;
using System.Windows.Input;
using StartCS_Delegate.Infrastructure.Commands;
using System.IO;

namespace StartCS_Delegate
{
    public class Client : INotifyPropertyChanged
    {
        public delegate void Notify<in T>(T arg);
        public event Notify<MessageBoxResult> NotifyMessage;
        //static HistoryWindow HistoryWindow;

        //public ICommand OpenHistoryWindowCommand { get; set; }
        //private bool CanOpenHistoryWindowCommandExecute(object p) => true;
        //private void OnOpenHistoryWindowCommandExecute(object p)
        //{
        //    HistoryWindow = new HistoryWindow();
        //    ManagerWindow.Close();
        //    HistoryWindow.Show();
        //}
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) 
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);

                if (args.PropertyName != "ItemSource")
                {
                    //await Task.Run(() => MessageBox.Show($"Изменено {propertyName}"));
                    //NotifyMessage?.Invoke(MessageBox.Show($"Изменено {propertyName}"));
                    //MessageBox.Show($"Изменено {propertyName}");
                    WriteToFileHistoryLog(propertyName);
                    MessageWindow messageWindow = new MessageWindow();
                    messageWindow.MessageBlock.Text = $"Изменено {propertyName}";
                    messageWindow.ShowDialog();
                    //HistoryWindow historyWindow = new HistoryWindow();
                    //historyWindow.HistoryBlock.Text += $"Изменено {propertyName}";
                    //Thread.Sleep(1000);
                    //historyWindow.Close();
                }
            }
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //NotifyMessage?.Invoke(MessageBox.Show($"Изменено {propertyName}"));
            //MessageBox.Show($"Изменено {propertyName}");
        }

        string path = @"..\Debug\HistoryLog.txt"; //string path = @"..\Debug\Client.xml";
        async void WriteToFileHistoryLog(string propertyName)
        {
            using (StreamWriter  sw = new StreamWriter(path, true)) 
            {
                await sw.WriteLineAsync($"Изменено {propertyName}");
            }
        }

        private string _ID;
        private string _Email;
        private string _Name;
        private string _Surname;
        private string _Patronymic;
        private string _NumberPhone;
        private string _Address;
        private string _Bill;
        private string _DepBill;
        
        public string ID
        {
            get => _ID;
            set
            {
                if (Equals(_ID, value)) return;
                _ID = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _Email;
            set
            {
                if (Equals(_Email, value)) return;
                _Email = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _Name;
            set
            {
                if (Equals(_Name, value)) return;
                _Name = value;
                OnPropertyChanged();
            }
        }

        public string Surname
        {
            get => _Surname;
            set
            {
                if (Equals(_Surname, value)) return;
                _Surname = value;
                OnPropertyChanged();
            }
        }

        public string Patronymic
        {
            get => _Patronymic;
            set
            {
                if (Equals(_Patronymic, value)) return;
                _Patronymic = value;
                OnPropertyChanged();
            }
        }

        public string NumberPhone
        {
            get => _NumberPhone;
            set
            {
                if (Equals(_NumberPhone, value)) return;
                _NumberPhone = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get => _Address;
            set
            {
                if (Equals(_Address, value)) return;
                _Address = value;
                OnPropertyChanged();
            }
        }

        public string Bill
        {
            get => _Bill;
            set
            {
                if (Equals(_Bill, value)) return;
                _Bill = value;
                OnPropertyChanged();
            }
        }

        public string DepBill
        {
            get => _DepBill;
            set
            {
                if (Equals(_DepBill, value)) return;
                _DepBill = value;
                OnPropertyChanged();
            }
        }

        public Client() { }

        public Client(string id, string email, string name, string surname, string patronymic,
            string numberPhone, string address, string bill, string depBill)
        {
            ID = id;
            Email = email;
            Name = name;
            Surname = surname;
            Patronymic = patronymic;
            NumberPhone = numberPhone;
            Address = address;
            Bill = bill;
            DepBill = depBill;

            //OpenHistoryWindowCommand = new LambdaCommand(OnOpenHistoryWindowCommandExecute, CanOpenHistoryWindowCommandExecute);
        }

        public override string ToString() 
        {
            return String.Concat($"{ID} {Email} {Name} {Surname} {Patronymic} {NumberPhone} {Address}");
        }
    }
}


