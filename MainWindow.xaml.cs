using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ASCII_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient tcpClient;
        String filePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            
           tcpClient = new TcpClient();
           if(IPAddress.TryParse(serverIP.Text,out IPAddress ip) && int.TryParse(serverPort.Text, out int port))
           {
                try
                {
                    tcpClient.Connect(ip, port);
                    pickPicture.IsEnabled = true;
                    serverStatus.Content = "Connected";
                }
                catch (SocketException ex)
                {
                    serverStatus.Content = "Unable to connect";
                }
           }
           else
           {
                serverStatus.Content = "Unable to connect";
           }  
        }

        private void pickPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.ShowDialog();

            if(File.Exists(openFileDialog.FileName)) { 
                fileName.Content = "Path: " + openFileDialog.FileName;
                Uri picturePath = new Uri(openFileDialog.FileName);

                filePath = openFileDialog.FileName;

                picture.Source = new BitmapImage(picturePath);
                sendPicture.IsEnabled = true;
            }

        }

        private void sendPicture_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = File.ReadAllBytes(filePath);

            NetworkStream ns = tcpClient.GetStream();

            ns.Write(BitConverter.GetBytes(data.Length));
            ns.Write(data, 0, data.Length);

            data = new Byte[4];

            ns.Read(data);
            int size = BitConverter.ToInt32(data);
            data = new Byte[size];

            ns.Read(data);

            File.WriteAllBytes("Output.txt", data);

            ProcessStartInfo i = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = "Output.txt"
            };
            Process.Start(i);

            pickPicture.IsEnabled = false;
            sendPicture.IsEnabled = false;
            serverStatus.Content = "No connection";
            fileName.Content = "Path: ";
            picture.Source = null;
        }
    }
}