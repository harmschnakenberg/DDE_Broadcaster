using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DDE_Broadcaster
{
    ///// <summary>
    ///// Interface für Klassen zur Verwaltung von Verbindungen auf Client-Seite
    ///// </summary>
    //public interface ITCP_ServerInstance
    //{
    //    /// <summary>
    //    /// Teilt dem Interface die verwendeten Verbindungsinformationen mit
    //    /// </summary>
    //    /// <param name="socket">Socket-Verbindung zum Client</param>
    //    void SetConnectionData(Socket socket);
    //    /// <summary>
    //    /// Automatisch registrierter Eventhandler
    //    /// </summary>
    //    /// <param name="Message"></param>
    //    void NewMessage(string Message);
    //    /// <summary>
    //    /// Beenden der Verbindung zum Client
    //    /// </summary>
    //    void CloseConnection();
    //    /// <summary>
    //    /// Wird aufgerufen, wenn Daten vom Client empfangen wurden
    //    /// </summary>
    //    /// <param name="data">Die Daten als Byte-Array</param>
    //    void Receive(byte[] data);
    //}

    //class TCP_ServerInstance
    //{

    //    /// <summary>
    //    /// Zeit in Millisekunden in Warteschleifen
    //    /// </summary>
    //    const int SleepTime = 200;
    //    public Thread serverThread;
    //    public Socket socket;

    //    /// <summary>
    //    /// Standard-Konstruktor, welcher u.a. den 
    //    /// Thread für diese Verbindung erzeugt.
    //    /// </summary>
    //    /// <param name="socket">Verbindungssocket</param>
    //    public TCP_ServerInstance instanceOfClass;
    //    public TCP_ServerInstance(Socket socket, TCP_ServerInstance instance)
    //    {
    //        this.socket = socket;
    //        this.instanceOfClass = instance;
    //        // Thread erzeugen
    //        serverThread = new Thread(new ThreadStart(Process));
    //        serverThread.Start();
    //    }

    //    public void Process()
    //    {
    //        try
    //        {
    //            socket.Close();
    //            serverThread.Abort();
    //        }
    //        catch
    //        {
    //            System.Console.WriteLine("Verbindung zum Client beendet");
    //        }
    //    }

    //}
}
