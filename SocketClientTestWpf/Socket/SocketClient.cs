using System;
using System.Net.Sockets;

namespace SocketClientTestWpf.Socket
{
    public class SocketClient
    {
        public delegate void ReceiveTextEventHandler(object sender, ReceiveTextEventArgs e);
        public event ReceiveTextEventHandler ReceiveTextEvent;

        #region Fields

        string host;
        int port;
        System.Net.Sockets.Socket socket;
        System.IO.MemoryStream receivedBinaryStream;
        System.Text.Encoding utf8;

        #endregion // Fields

        #region Properties

        public bool Closed => socket == null;

        #endregion // Properties

        #region Methods

        public bool Init(string host, int port)
        {
            if (socket != null)
                return false;

            this.host = host;
            this.port = port;

            socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            utf8 = System.Text.Encoding.UTF8;

            return true;
        }

        #region Connect/Disconnect

        public bool Connect(out string error)
        {
            error = string.Empty;

            if (socket == null)
                return false;

            //var ip = Dns.GetHostEntry(IPAddress.Any);
            //var ipEndPoint = new IPEndPoint(ip.AddressList[0], port);
            //socket.Connect(ipEndPoint);

            try {
                socket.Connect(host, port);
            } catch (Exception e) {
                error = e.ToString();
                return false;
            }

            return true;
        }

        public bool Disconnect()
        {
            // socket オブジェクトの排他
            lock (this) {
                if (socket == null)
                    return false;
                if (socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;

                ClearBinaryStream();
            }

            return true;
        }

        #endregion // Connect/Disconnect

        #region Send

        public bool Send(string text)
        {
            if (socket == null)
                return false;

            var bytes = utf8.GetBytes(text);

            lock (this) {
                socket.Send(bytes);
            }

            return true;
        }

        #endregion // Send

        #region Receive

        public bool StartAsyncReceiving()
        {
            if (!socket.Connected)
                return false;

            var receiveBuffer = new byte[4096];
            receivedBinaryStream = new System.IO.MemoryStream();
            int offset = 0;

            socket.BeginReceive(receiveBuffer, offset, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), receiveBuffer);

            return true;
        }

        void ReceiveDataCallback(IAsyncResult asyncResult)
        {
            int receivedTextLength = -1;
            try {
                lock (this) {
                    receivedTextLength = socket.EndReceive(asyncResult);
                }
            } catch {
            }
            if (receivedTextLength <= 0) {
                Disconnect();
                return;
            }

            var receivedBuffer = (byte[])asyncResult.AsyncState;

            receivedBinaryStream.Write(receivedBuffer, 0, receivedTextLength);

            //// 末尾が '\0' かどうかチェック
            //receivedBinaryStream.Seek(0, System.IO.SeekOrigin.End);
            //if (receivedBinaryStream.ReadByte() == (int)'\0') {}

            // バイナリ -> テキスト変換
            var text = utf8.GetString(receivedBinaryStream.ToArray());
            receivedBinaryStream.Close();
            receivedBinaryStream = null;
            this.ReceiveTextEvent?.Invoke(this, new ReceiveTextEventArgs(text));
            receivedBinaryStream = new System.IO.MemoryStream();

            lock(this) {
                int offset = 0;
                // Async Recursive
                socket.BeginReceive(receivedBuffer, offset, receivedBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), receivedBuffer);
            }
        }

        #endregion // Receive

        void ClearBinaryStream()
        {
            if (receivedBinaryStream != null) {
                receivedBinaryStream.Close();
                receivedBinaryStream = null;
            }
        }

        #endregion // Methods
    }
}
