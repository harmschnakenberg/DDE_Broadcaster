using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DDE_Broadcaster
{
    class TCP_Server2
    {
        // Tcp Server Quelle: https://stackoverflow.com/questions/19387086/how-to-set-up-tcplistener-to-always-listen-and-accept-multiple-connections

        // Events Quelle: https://stackoverflow.com/questions/1249517/super-simple-example-of-c-sharp-observer-observable-with-delegates

        public event EventHandler<IncomeingTcpMessageEventArgs> IncomeingTcpMessage;

        public class IncomeingTcpMessageEventArgs : EventArgs
        {
            public IncomeingTcpMessageEventArgs(string msg)
            { RecievedMessage = msg; }
            public string RecievedMessage { get; set; }
        }


        void StartTcpServer()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

                Console.WriteLine("Starting TCP listener...");

                TcpListener listener = new TcpListener(ipAddress, 500);

                listener.Start();

                while (true)
                {
                    Socket client = listener.AcceptSocket();
                    Console.WriteLine("Connection accepted.");

                    var childSocketThread = new Thread(() =>
                    {

                        byte[] data = new byte[1024];
                        int size = client.Receive(data);

                        Console.WriteLine("Recieved data: ");
                        string stringData = Encoding.UTF8.GetString(data, 0, data.Length);

                        Console.WriteLine(stringData);
              
                        IncomeingTcpMessage?.Invoke(this, new IncomeingTcpMessageEventArgs(stringData) );

                        client.Close();
                    });
                    childSocketThread.Start();
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
