using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DDE_Broadcaster
{
        /// <summary>
        /// Beispiel für einen UDPClient. Ein Client sendet,
        /// der andere Client empfängt anschließend.
        /// </summary>
    class UDP_Client
    {
        //Quelle: https://stackoverflow.com/questions/7266101/receive-messages-continuously-using-udpclient
        internal void Recieve()
        {
            //Client uses as receive udp client
            UdpClient Client = new UdpClient(DDE_Client.UdpPort);

            try
            {
                Client.BeginReceive(new AsyncCallback(Recv), null);
            }
            catch (Exception e)
            {
                Console.WriteLine( e.ToString());
            }

            //CallBack
            void Recv(IAsyncResult res)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, DDE_Client.UdpPort);
                byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);

                //Weiter zuhören
                Client.BeginReceive(new AsyncCallback(Recv), null);

                //Process codes
                Console.WriteLine( Encoding.UTF8.GetString(received, 0,received.Length) );
                
            }

        }

//        //Client uses as receive udp client
//        UdpClient Client = new UdpClient(Port);

//try
//{
//     Client.BeginReceive(new AsyncCallback(recv), null);
//}
//catch(Exception e)
//{
//     MessageBox.Show(e.ToString());
//}

////CallBack
//private void recv(IAsyncResult res)
//{
//    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
//    byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);

//    //Process codes

//    MessageBox.Show(Encoding.UTF8.GetString(received));
//    Client.BeginReceive(new AsyncCallback(recv), null);
//}

        //internal void Send(string TagName, string TagValue)
        //{
        //    try
        //    {
        //        // Generiere einen EndPoint mit einer Loopback-Adresse
        //        IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Loopback, 4201);

        //        // Instanziere UdpClient
        //        UdpClient sender = new UdpClient();

        //        string payload = TagName + "=" + TagValue ;
        //        byte[] sendPacket = Encoding.ASCII.GetBytes(payload);

        //        sender.Send(sendPacket, sendPacket.Length, remoteIPEndPoint);
   
        //        sender.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}




    //public class UdpClientSample
    //    {
    //        public static void Main(string[] args)
    //        {
    //            try
    //            {
    //                // Generiere einen EndPoint mit einer Loopback-Adresse
    //                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Loopback, 4201);

    //                // Instanziere UdpClient
    //                UdpClient sender = new UdpClient();
    //                UdpClient receiver = new UdpClient(remoteIPEndPoint);

    //                sender.Connect(remoteIPEndPoint);   // Bindet UdpClient nur an den Port
    //                string welcome = "Hello, how are you?";
    //                byte[] sendPacket = Encoding.ASCII.GetBytes(welcome);

    //                sender.Send(sendPacket, sendPacket.Length);

    //                Console.WriteLine("Sent {0} bytes to the server...", sendPacket.Length);

    //                // Empfange den Echo string wieder
    //                byte[] rcvPacket = receiver.Receive(ref remoteIPEndPoint);

    //                Console.WriteLine("Received {0} bytes from {1}: {2}",
    //                                  rcvPacket.Length, remoteIPEndPoint,
    //                                  Encoding.ASCII.GetString(rcvPacket, 0, rcvPacket.Length));


    //                string input = DateTime.Now.Second.ToString(); //Console.ReadLine();

    //                sender.Send(Encoding.ASCII.GetBytes(input), input.Length);
    //                rcvPacket = receiver.Receive(ref remoteIPEndPoint);

    //                int counter = 0;

    //                // In Endlosschleife senden und empfangen
    //                while (true)
    //                {
    //                    counter++;
    //                    if (input == "0" || counter > 30)
    //                        break;


    //                    string data = Encoding.ASCII.GetString(rcvPacket, 0, rcvPacket.Length);
    //                    Console.WriteLine(data);
    //                    System.Threading.Thread.Sleep(1000);
    //                }

    //                Console.WriteLine("Stopping...");
    //                sender.Close();
    //                receiver.Close();
    //            }
    //            catch (Exception e)
    //            {
    //                Console.WriteLine(e.ToString());
    //            }
    //        }
    //    }

    }
}
