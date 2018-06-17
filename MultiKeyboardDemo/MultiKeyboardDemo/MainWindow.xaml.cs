using MultiKeyboardLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiKeyboardDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MultiKeyboardInput multiKeyboardInput;

        private ObservableCollection<RawKeyboard> _rawKeyboard = new ObservableCollection<RawKeyboard>();
        public ObservableCollection<RawKeyboard> Keyboards
        {
            get { return _rawKeyboard; }
        }

        
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (multiKeyboardInput == null)
            {
                multiKeyboardInput = new MultiKeyboardInput(HwndSource.FromHwnd(new WindowInteropHelper(this).Handle), OnRawInput);
            }
        }

        // TODO: You'd probably want to convert this into regular Key etc
        private void OnRawInput(IntPtr hDevice, Win32.VK key)
        {
            // TODO: You really want to use an ObservableDictionary instead of this fun
            var found = false;

            foreach (var item in Keyboards)
            {
                if (item.Device == hDevice.ToString())
                {
                    found = true;

                    item.Key = key.ToString();
                    break;
                }
            }

            if (!found)
            {
                Keyboards.Add(new RawKeyboard() { Device = hDevice.ToString(), Key = key.ToString() });
            }
        }
    }

    // TODO: You really want to use an ObservableDictionary instead of this fun
    public class RawKeyboard : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _device;
        public string Device
        {
            get { return _device; }
            set {
                _device = value;
                OnPropertyChanged();
            }
        }

        private string _key;
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
