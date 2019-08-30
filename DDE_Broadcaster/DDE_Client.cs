using NDde.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DDE_Broadcaster
{
    class DDE_Client
    {

        public static string DdeServerName { get; } = "view";
        public static string DdeTopic { get; } = "tagname";
        public static int DdeLoopTime { get; set; } = 1000;
        public static readonly int UdpPort = 4201;

        /// <summary>
        /// Liste der TagNames, die gelsesen udn übertragen werden sollen.
        /// </summary>
        public List<string> TagNames = new List<string>();

        public void OnIncomeingTcpMessage(object sender, TCP_Server2.IncomeingTcpMessageEventArgs args)
        {
            Console.WriteLine("Something happened to " + sender);
            Console.WriteLine("Empfangen: " + args.RecievedMessage);

            // Erwarteter Aufbau dem empfangenen strings:
            // +; für Advise
            // -; für Remove
            // z.B. +;A01_DB10_DBW6,$Hour,A01_DB22_DBX100

            //ToDo:
            //Bearbeite empfangenen String und erzeuge eine Liste.
            //[...]

            string recieved = args.RecievedMessage;

            bool newTag = false; //true: Advise, false: Remove
            if (recieved[0] == '+') newTag = true;

            recieved = recieved.Remove(0, 2);

            List<string> newTagNames = recieved.Split(',').ToList();
            
            //ToDo:
            //Prüfen, ob alle Einträge gültige TagNames sind.

            if (newTagNames.Count > 0)
            {
                foreach (string newTagName in newTagNames)
                {
                    if (newTag)
                    {
                        Advise(newTagName);
                    }
                    else
                    {
                        Remove(newTagName);
                    }
                }
            }
        }


        public void Advise(string TagName)
        {
            if (TagNames.Contains(TagName)) return;

            //ToDo: hier Prüfen, ob TagName gültig ist.
            TagNames.Add(TagName);

            try
            {
                // Create a client that connects to 'myapp|mytopic'. 
                using (DdeClient dde_client = new DdeClient(DdeServerName, DdeTopic))
                {
                    // Subscribe to the Disconnected event.  This event will notify the application when a conversation has been terminated.
                    dde_client.Disconnected += OnDisconnected;

                    // Connect to the server.  It must be running or an exception will be thrown.
                    dde_client.Connect();

                    //Lese den Wert initial von DDE Server:
                    string TagValue = dde_client.Request(TagName, 60000);                    
                    UdpSend(TagName, TagValue);

                    //Lese den Wert von DDE Server neu ein bei Wertänderung.
                    // Advise Loop.
                    dde_client.StartAdvise(TagName, 1, true, 60000);

                    dde_client.Advise += OnAdvise2;

                    while (TagNames.Contains(TagName))
                    {
                        System.Threading.Thread.Sleep(DdeLoopTime);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().ToString() + "\r\n" + e.Message + "\r\n" + e.StackTrace);
            }

        }

        public void Remove(string TagName)
        {
            TagNames.Remove(TagName);
        }

        #region Events

        private static void OnExecuteComplete(IAsyncResult ar)
        {
            try
            {
                DdeClient client = (DdeClient)ar.AsyncState;
                client.EndExecute(ar);
                Console.WriteLine("OnExecuteComplete");
            }
            catch (Exception e)
            {
                Console.WriteLine("OnExecuteComplete: " + e.Message);
            }
        }

        private static void OnPokeComplete(IAsyncResult ar)
        {
            try
            {
                DdeClient client = (DdeClient)ar.AsyncState;
                client.EndPoke(ar);
                Console.WriteLine("OnPokeComplete");
            }
            catch (Exception e)
            {
                Console.WriteLine("OnPokeComplete: " + e.Message);
            }
        }

        internal static void OnRequestComplete(IAsyncResult ar)
        {
            try
            {
                DdeClient client = (DdeClient)ar.AsyncState;
                byte[] data = client.EndRequest(ar);

                Console.WriteLine("OnRequestComplete: " + Encoding.ASCII.GetString(data));         
            }
            catch (Exception e)
            {
                Console.WriteLine("OnRequestComplete: " + e.Message);
            }
        }

        private static void OnStartAdviseComplete(IAsyncResult ar)
        {
            try
            {
                DdeClient client = (DdeClient)ar.AsyncState;
                client.EndStartAdvise(ar);
                Console.WriteLine("OnStartAdviseComplete");
            }
            catch (Exception e)
            {
                Console.WriteLine("OnStartAdviseComplete: " + e.Message);
            }
        }

        private static void OnStopAdviseComplete(IAsyncResult ar)
        {
            try
            {
                DdeClient client = (DdeClient)ar.AsyncState;
                client.EndStopAdvise(ar);
                Console.WriteLine("OnStopAdviseComplete");
            }
            catch (Exception e)
            {
                Console.WriteLine("OnStopAdviseComplete: " + e.Message);
            }
        }

        private void OnAdvise2(object sender, DdeAdviseEventArgs args)
        {
            Console.WriteLine("OnAdvise2: " + args.Item + " - " + args.Text);

            UdpSend(args.Item, args.Text);
        }

        private static void OnDisconnected(object sender, DdeDisconnectedEventArgs args)
        {
            Console.WriteLine(
                "OnDisconnected: " +
                "IsServerInitiated=" + args.IsServerInitiated.ToString() + " " +
                "IsDisposed=" + args.IsDisposed.ToString());
            //Console.ReadKey();
        }

        #endregion


        internal void UdpSend(string TagName, string TagValue)
        {
            try
            {
                // Generiere einen EndPoint mit einer Loopback-Adresse
                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Loopback, UdpPort);

                // Instanziere UdpClient
                UdpClient sender = new UdpClient();

                string payload = TagName + "=" + TagValue;
                byte[] sendPacket = Encoding.UTF8.GetBytes(payload);

                Console.WriteLine("Sende per UDP: " + payload);
                sender.Send(sendPacket, sendPacket.Length, remoteIPEndPoint);

                sender.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
