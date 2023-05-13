using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StartCS_Delegate.Views.MessageWindow
{
    /// <summary>
    /// Логика взаимодействия для MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
            //this.Initialized += MessageWindow_Initialized;
            this.Loaded += Window_Loaded;
        }

        //static int timerValue = 0;

        //private void MessageWindow_Initialized(object sender, EventArgs e)
        //{
        //    Timer timer = new Timer();
        //    timer.Interval = 1000;
        //    timer.Enabled = true;
        //    timer.Elapsed += new ElapsedEventHandler(TimerTick);
        //    timer.Start();
        //}

        //private void TimerTick(object sender, ElapsedEventArgs e)
        //{
        //    if (timerValue <= 3)
        //    {
        //        timerValue += 1;
        //        this.Background.Opacity = 1;
        //    }
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Timer t = new Timer();
            t.Interval = 3000;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();
            }), null);
        }
    }
}
