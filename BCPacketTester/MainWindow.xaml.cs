using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BCPacketTester
{
    /// <summary>
    /// Interaction logic for PacketTester.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        public static byte[] ToBytes(string hexString)
        {
            hexString = hexString.Replace("0x", "");

            if (!string.IsNullOrEmpty(hexString) && hexString.Length % 2 != 0)
            {
                throw new FormatException("Hexadecimal string must not be empty and must contain an even number of digits to be valid.");
            }

            hexString = hexString.ToUpperInvariant();
            byte[] data = new byte[hexString.Length / 2];

            for (int index = 0; index < hexString.Length; index += 2)
            {
                int highDigitValue = hexString[index] <= '9' ? hexString[index] - '0' : hexString[index] - 'A' + 10;
                int lowDigitValue = hexString[index + 1] <= '9' ? hexString[index + 1] - '0' : hexString[index + 1] - 'A' + 10;

                if (highDigitValue < 0 || lowDigitValue < 0 || highDigitValue > 15 || lowDigitValue > 15)
                {
                    throw new FormatException("An invalid digit was encountered. Valid hexadecimal digits are 0-9 and A-F.");
                }
                else
                {
                    byte value = (byte)((highDigitValue << 4) | (lowDigitValue & 0x0F));
                    data[index / 2] = value;
                }
            }

            return data;
        }

        private void SendPacket()
        {
            bool PacketError = false;
            bool ServerError = false;
            Packet TestPacket = new Packet();
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.PacketID = Convert.ToUInt32(PacketIDBox.Text);
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PacketIDBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }));
                PacketError = true;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.RequestType = (byte)Convert.ToUInt32(RequestTypeBox.Text);
                }));
            }
            catch (Exception)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        TestPacket.RequestType = ToBytes(RequestTypeBox.Text)[0];
                    }));
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        RequestTypeBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    }));
                    PacketError = true;
                }
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.RequestBody = Encoding.ASCII.GetBytes($"{RequestBodyBox.Text}\x0");
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    RequestBodyBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }));
                PacketError = true;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.ChannelID = Convert.ToUInt32(ChannelIDBox.Text);
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ChannelIDBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }));
                PacketError = true;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.AuthKey = Encoding.ASCII.GetBytes($"{AuthKeyBox.Text}\x0");
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AuthKeyBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }));
                PacketError = true;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.ServerIP = ServerIpBox.Text;
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ServerIpBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }));
                ServerError = true;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TestPacket.ServerPort = Convert.ToInt32(ServerPortBox.Text);
                }));
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ServerPortBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }));
                ServerError = true;
            }

            if (PacketError && ServerError)
            {
                return;
            }
            else if (ServerError)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SettingsList.SelectedIndex = 1;
                }));
                return;
            }
            else if (PacketError)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    SettingsList.SelectedIndex = 0;
                }));
                return;
            }

            TestPacket.GetPacketSize();

            try
            {
                TestPacket.SendPacket();
            }
            catch (Exception e)
            {
                CreateCard($"[Sending Error]\nMessage: {e.Message}");
                return;
            }

            Response res = null;

            try
            {
                res = TestPacket.RecvResponse();
            }
            catch (Exception e)
            {
                CreateCard($"[Response Error]\nMessage: {e.Message}");
                return;
            }

            CreateCard(res);
        }

        private void CreateCard(string info)
        {
            ListViewItem LVI = null;
            StackPanel panel = null;
            TextBlock titleBlock = null;
            TextBox tb = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LVI = new ListViewItem();
                panel = new StackPanel();
                titleBlock = new TextBlock();
                tb = new TextBox();
            }));

            // Define Button
            DateTime localDate = DateTime.Now;
            var culture = new CultureInfo("en-US");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                titleBlock.Text = localDate.ToString(culture);
                titleBlock.Foreground = new SolidColorBrush(Colors.Red);
                titleBlock.Background = new SolidColorBrush(Colors.Transparent);
                titleBlock.FontSize = 14;
            }));

            // Define TextBox
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                tb.AcceptsReturn = true;
                tb.BorderThickness = new Thickness(0, 0, 0, 0);
                tb.FontSize = 12;
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Margin = new Thickness(5, 5, 5, 5);
                tb.IsReadOnly = true;
                tb.Width = 150;
                tb.Background = new SolidColorBrush(Colors.Transparent);
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Text = info;
                tb.Name = "OutputBox";
                tb.Visibility = Visibility.Collapsed;
            }));

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                panel.Children.Add(titleBlock);
                panel.Children.Add(tb);
                LVI.Content = panel;
                ResponseList.Items.Insert(0, LVI);
                LVI.MouseUp += new MouseButtonEventHandler(ListViewItem_MouseUp);
            }));
        }

        private void CreateCard(Response res)
        {
            ListViewItem LVI = null;
            StackPanel panel = null;
            TextBlock titleBlock = null;
            TextBox tb = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LVI = new ListViewItem();
                panel = new StackPanel();
                titleBlock = new TextBlock();
                tb = new TextBox();
            }));

            // Define Button
            DateTime localDate = DateTime.Now;
            var culture = new CultureInfo("en-US");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                titleBlock.Text = localDate.ToString(culture);
                titleBlock.Foreground = new SolidColorBrush(Colors.White);
                titleBlock.Background = new SolidColorBrush(Colors.Transparent);
                titleBlock.FontSize = 14;
            }));

            // Define TextBox
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                tb.AcceptsReturn = true;
                tb.BorderThickness = new Thickness(0, 0, 0, 0);
                tb.FontSize = 12;
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Margin = new Thickness(5, 5, 5, 5);
                tb.IsReadOnly = true;
                tb.Width = 150;
                tb.Background = new SolidColorBrush(Colors.Transparent);
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Text = $"ID: {res.PacketID}\nSize: {res.PacketSize}\nStatus: {res.ResponseStatus}\nBody: {res.ResponseBody}\nBody(Hex): {res.ResponseBodyHex}";
                tb.Name = "OutputBox";
                tb.Visibility = Visibility.Collapsed;
            }));

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                panel.Children.Add(titleBlock);
                panel.Children.Add(tb);
                LVI.Content = panel;
                LVI.MouseUp += new MouseButtonEventHandler(ListViewItem_MouseUp);
                ResponseList.Items.Insert(0, LVI);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(ResponseList, 0);
                scrollViewer.ScrollToBottom();
            }));
        }

        private void SettingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                switch (SettingsList.SelectedIndex)
                {
                    case 0:
                        PacketStack.Visibility = Visibility.Visible;
                        ServerStack.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        PacketStack.Visibility = Visibility.Collapsed;
                        ServerStack.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendButton.Visibility = Visibility.Collapsed;
            SendingProgress.Visibility = Visibility.Visible;
            await Task.Run(() =>
            {
                SendPacket();
            });
            ClearButton.Visibility = Visibility.Visible;
            SendButton.Visibility = Visibility.Visible;
            SendingProgress.Visibility = Visibility.Collapsed;
        }

        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }

        private void ResponseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResponseList.SelectedIndex = -1;
        }

        private void ListViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var cp = FindVisualChild<ContentPresenter>(sender as ListViewItem);

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            var dt = cp.Content as StackPanel;
            var tb = (TextBox)dt.Children[1];

            if (tb.Visibility == Visibility.Visible)
            {
                tb.Visibility = Visibility.Collapsed;
            }
            else
            {
                tb.Visibility = Visibility.Visible;
            }
        }

        private void AuthKeyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AuthKeyBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void PacketIDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PacketIDBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void ChannelIDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChannelIDBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void RequestTypeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RequestTypeBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void RequestBodyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RequestBodyBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void ServerIpBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerIpBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void ServerPortBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerPortBox.BorderBrush = new SolidColorBrush(Colors.White);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseList.Items.Clear();
            ClearButton.Visibility = Visibility.Collapsed;
        }
    }
}