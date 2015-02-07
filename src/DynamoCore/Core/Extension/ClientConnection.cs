using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Dynamo.Core.Extension
{
    class ClientConnection
    {
        private readonly byte[] buffer;
        private readonly Socket socket;
        private readonly DynamoListener listener;
        private readonly MemoryStream stream;

        internal ClientConnection(DynamoListener listener, Socket socket)
        {
            this.socket = socket;
            this.listener = listener;

            buffer = new byte[4096];
            stream = new MemoryStream();
            BeginReceiveData();
        }

        internal void SendMessage(byte[] data)
        {
            var e = new SocketAsyncEventArgs
            {
                UserToken = this
            };

            bool completedAsynchronously = false;

            try
            {
                e.SetBuffer(data, 0, data.Length);
                e.Completed += OnSendMessageCompleted;
                completedAsynchronously = socket.SendAsync(e);
            }
            catch (Exception)
            {
            }

            if (!completedAsynchronously)
                OnSendMessageCompleted(this, e);
        }

        private void OnSendMessageCompleted(object sender, SocketAsyncEventArgs e)
        {
        }

        private void BeginReceiveData()
        {
            socket.BeginReceive(buffer, 0, buffer.Length,
                SocketFlags.None, ReceiveCallback, this);
        }

        private void RemoveSelfFromClientList()
        {
            listener.RemoveFromClientList(this);
        }

        private void FlushToStream(int bytesToFlush)
        {
            if (bytesToFlush <= 0)
            {
                var data = Encoding.UTF8.GetString(stream.GetBuffer());
                stream.Position = 0; // Reset write position.
            }
            else
            {
                stream.Write(buffer, ((int)stream.Position), bytesToFlush);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var clientConnection = ((ClientConnection)ar.AsyncState);

            try
            {
                var socket = clientConnection.socket;
                clientConnection.FlushToStream(socket.EndReceive(ar));
                clientConnection.BeginReceiveData();
            }
            catch (SocketException exception)
            {
                // Client disconnected, remove from client list.
                clientConnection.RemoveSelfFromClientList();
            }
        }
    }
}
