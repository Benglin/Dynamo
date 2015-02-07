using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Dynamo.Core.Extension
{
    class DynamoListener
    {
        private enum EventIndex
        {
            ReadyForConnection, Shutdown, Count
        }

        private readonly Thread workerThread;
        private readonly TcpListener listener;
        private readonly List<ClientConnection> clients;
        private readonly ManualResetEvent[] events;

        #region Public Class Operational Methods

        internal DynamoListener()
        {
            events = new[]
            {
                new ManualResetEvent(false),
                new ManualResetEvent(false)
            };

            clients = new List<ClientConnection>();
            workerThread = new Thread(ListenerThreadProc)
            {
                IsBackground = true
            };

            IPAddress localHostAddress = null;
            IPAddress.TryParse("127.0.0.1", out localHostAddress);
            listener = new TcpListener(localHostAddress, 3002);
        }

        internal void StartListening()
        {
            listener.Start();
            workerThread.Start(this);
        }

        internal void StopListening()
        {
            events[(int)EventIndex.Shutdown].Set();
            workerThread.Join();

            // TODO: Clean-up internal.
        }

        internal void AddToClientList(ClientConnection client)
        {
            lock (clients)
            {
                clients.Add(client);
            }
        }

        internal void RemoveFromClientList(ClientConnection client)
        {
            lock (clients)
            {
                clients.Remove(client);
            }
        }

        internal void Broadcast(byte[] data)
        {
            lock (clients)
            {
                foreach (var clientConnection in clients)
                {
                    clientConnection.SendMessage(data);
                }
            }
        }

        #endregion

        #region Private Class Helper Methods

        private static void ListenerThreadProc(object state)
        {
            var dynamoListener = ((DynamoListener)state);
            var events = dynamoListener.events;
            var listener = dynamoListener.listener;

            while (true)
            {
                events[(int)EventIndex.ReadyForConnection].Reset();
                listener.BeginAcceptSocket(AcceptSocketCallback, dynamoListener);
                var handleIndex = WaitHandle.WaitAny(events);
                if (handleIndex == ((int)EventIndex.Shutdown))
                    break;
            }
        }

        private static void AcceptSocketCallback(IAsyncResult ar)
        {
            var dynamoListener = ((DynamoListener)ar.AsyncState);
            var socket = dynamoListener.listener.EndAcceptSocket(ar);

            // Allow DynamoListener to wait for next connection.
            dynamoListener.events[(int)EventIndex.ReadyForConnection].Set();
            dynamoListener.AddToClientList(new ClientConnection(dynamoListener, socket));
        }

        #endregion
    }
}
