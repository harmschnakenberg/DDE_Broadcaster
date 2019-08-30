using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDE_Broadcaster
{
    /// <summary>
    /// Über eingehende TCP-Verbindung (Port?) sollen TagNames beim DDE-Server abboniert/abbestellt werden.
    /// Die per DDE erhaltenen Werte sollen per UDP weitergeleitet werden. (Port ?)
    /// UDP-Auswertung und TCP-Client Anfragen kommen später über eine NET Core Anwendung, die auch ein FrontEnd für Browser bereithält. 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            TCP_Server2 tcp_server = new TCP_Server2();
            DDE_Client dde_client = new DDE_Client();

            dde_client.Advise("$Second");

            tcp_server.IncomeingTcpMessage += dde_client.OnIncomeingTcpMessage;

           

        }

    }
}
