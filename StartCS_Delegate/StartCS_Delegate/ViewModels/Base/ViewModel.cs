using StartCS_Delegate.Views.MessageWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StartCS_Delegate.ViewModels.Base
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string path = @"..\Debug\HistoryLog.txt"; //string path = @"..\Debug\Client.xml";
        async void WriteToFileHistoryLog(string propertyName, string ID, string Email, string Name, string Surname, string Patronymic, string NumberPhone, string Address)
        {
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                await sw.WriteLineAsync($"Изменено {propertyName} У клиента {ID} {Email} {Name} {Surname} {Patronymic} {NumberPhone} {Address}");
            }
        }

        /// <summary>
        /// Обновление значения свойства
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
