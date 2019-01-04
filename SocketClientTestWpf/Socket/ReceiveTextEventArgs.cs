using System;

namespace SocketClientTestWpf.Socket
{
    public class ReceiveTextEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public ReceiveTextEventArgs(string text) => this.Text = text;
    }
}
