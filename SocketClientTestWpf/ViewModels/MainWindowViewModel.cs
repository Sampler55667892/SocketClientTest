using System.Windows.Input;
using SocketClientTestWpf.Frame;
using SocketClientTestWpf.Socket;

namespace SocketClientTestWpf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        System.Windows.Threading.Dispatcher dispatcher; // UIスレッドのスレッドセーフ用
        SocketClient socketClient;

        #endregion // Fields

        #region Properties

        public ICommand ConnectCommand =>
            new RelayCommand(o => {
                // ソケット切断後の再初期化
                if (socketClient.Closed)
                    socketClient.Init(AppConfig.ServerHost, AppConfig.ServerPort);
                // 接続
                string error;
                if (!socketClient.Connect(out error)) {
                    if (!string.IsNullOrEmpty(error))
                        AddTextToConsole(error);
                    else
                        AddTextToConsole("Connection error has occurred.");
                } else
                    AddTextToConsole("Passed to connect.");
            });

        public ICommand DisconnectCommand =>
            new RelayCommand(o => {
                if (!socketClient.Disconnect())
                    AddTextToConsole("Failed to disconnect.");
                else
                    AddTextToConsole("Passed to disconnect.");
            });

        public ICommand StartReceivingCommand =>
            new RelayCommand(o => {
                if (socketClient.StartAsyncReceiving())
                    AddTextToConsole("Passed to start receiving.");
                else
                    AddTextToConsole("Failed to start receiving (connection failed).");
            });

        public ICommand SendCommand =>
            new RelayCommand(o => {
                // 送信
                socketClient.Send(SendText);
            });

        string serverHostText;
        public string ServerHostText
        {
            get => serverHostText;
            set {
                if (string.Compare(serverHostText, value) != 0) {
                    serverHostText = value;
                    base.RaisePropertyChanged("ServerHostText");
                }
            }
        }

        string serverPortText;
        public string ServerPortText
        {
            get => serverPortText;
            set {
                if (string.Compare(serverPortText, value) != 0) {
                    serverPortText = value;
                    base.RaisePropertyChanged("ServerPortText");
                }
            }
        }

        string sendText;
        public string SendText
        {
            get => sendText;
            set {
                if (string.Compare(sendText, value) != 0) {
                    sendText = value;
                    base.RaisePropertyChanged("SendText");
                }
            }
        }

        string consoleText;
        public string ConsoleText
        {
            get => consoleText;
            set {
                if (string.Compare(consoleText, value) != 0) {
                    consoleText = value;
                    base.RaisePropertyChanged("ConsoleText");
                }
            }
        }

        #endregion // Properties

        #region Methods

        public bool Init(System.Windows.Threading.Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            ServerHostText = AppConfig.ServerHost;
            ServerPortText = AppConfig.ServerPort.ToString();
            if (!InitSocket())
                return false;
            return true;
        }

        public void Close()
        {
            socketClient?.Disconnect();
        }

        bool InitSocket()
        {
            socketClient = new SocketClient();

            int port;
            if (!int.TryParse(ServerPortText, out port))
                return false;
            if (!socketClient.Init(ServerHostText, port))
                return false;
            socketClient.ReceiveTextEvent += SocketClient_ReceiveTextEvent;

            return true;
        }

        void SocketClient_ReceiveTextEvent(object sender, ReceiveTextEventArgs e)
        {
            AddTextToConsole($"Received: {e.Text}");
        }

        void AddTextToConsole(string text)
        {
            dispatcher.Invoke(() => {
                this.ConsoleText = $"{text}\r\n{this.ConsoleText}";
            });
        }

        #endregion // Methods
    }
}
