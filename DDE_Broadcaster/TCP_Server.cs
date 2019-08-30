using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Threading;

//Quelle: https://www.mycsharp.de/wbb2/thread.php?threadid=19670

namespace DDE_Broadcaster.TCP_Server
{
    /// <summary>
    /// Delegat für einen Eventhandler zur Verwaltung interner Meldungen
    /// </summary>
    public delegate void MessageEvent(string Message);

    /// <summary>
    /// Verwaltet Informationen zum aktuellen Status des Servers
    /// </summary>
    public class ServerStatus
    {
        /// <summary>
        /// Anzahl aktueller Verbindungen
        /// </summary>
        public int CurrentConnections = 0;
        /// <summary>
        /// Aktuelle Eventhandler für interne Meldungen
        /// </summary>
        public event MessageEvent MessageEvents;
        /// <summary>
        /// Alle registrierten Event-Handler über 
        /// eine neue Mitteilung informieren
        /// </summary>
        /// <param name="Message"></param>
        public void NewMessage(string Message)
        {
            MessageEvents?.Invoke(Message);
        }
    }

    /// <summary>
    /// Interface für Klassen zur Verwaltung von Verbindungen auf Client-Seite
    /// </summary>
    public interface IServerInterface
    {
        /// <summary>
        /// Teilt dem Interface die verwendeten Verbindungsinformationen mit
        /// </summary>
        /// <param name="socket">Socket-Verbindung zum Client</param>
        void SetConnectionData(Socket socket);
        /// <summary>
        /// Automatisch registrierter Eventhandler
        /// </summary>
        /// <param name="Message"></param>
        void NewMessage(string Message);
        /// <summary>
        /// Beenden der Verbindung zum Client
        /// </summary>
        void CloseConnection();
        /// <summary>
        /// Wird aufgerufen, wenn Daten vom Client empfangen wurden
        /// </summary>
        /// <param name="data">Die Daten als Byte-Array</param>
        void Receive(byte[] data);
    }

    /// <summary>
    /// Serverseitige Verbindungsinstanz
    /// </summary>
    class ServerInstance
    {
        /// <summary>
        /// Zeit in Millisekunden
        /// </summary>
        const int SleepTime = 200;
        public IServerInterface instanceOfClass;
        public Thread serverThread;
        public Socket socket;
        const int BufferSize = 10240;
        /// <summary>
        /// Standard-Konstruktor, welcher u.a. den 
        /// Thread für diese Verbindung erzeugt.
        /// </summary>
        /// <param name="socket">Verbindungssocket</param>
        public ServerInstance(Socket socket, IServerInterface instance)
        {
            this.socket = socket;
            this.instanceOfClass = instance;
            // Thread erzeugen
            serverThread = new Thread(new ThreadStart(Process));
            serverThread.Start();
        }

        public void Process()
        {
            MemoryStream mem = new MemoryStream();// Empfangspuffer
            byte[] buffer = new byte[BufferSize];
            int TimeOut = 0;
            instanceOfClass.NewMessage("Server gestartet");
            while (TimeOut < (10 * 1000 / SleepTime))
            {
                mem.Seek(0, SeekOrigin.Begin);
                mem.SetLength(0);
                while (socket.Available > 0)
                {
                    //Byte[] buffer = new byte[bytesAvailable];
                    int bytesRead = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                    if (bytesRead <= 0) continue;
                    mem.Write(buffer, 0, bytesRead);
                    //instanceOfClass.Receive(buffer);
                    // Alles zurücksetzen
                }
                if (mem.Length > 0)
                {
                    if (mem.Length == 4)
                        if (System.Text.Encoding.ASCII.GetString(mem.ToArray(), 0, 4) == "quit")
                        {
                            instanceOfClass.CloseConnection();
                            break;
                        }
                    //Console.WriteLine("send {0} bytes",mem.Length);
                    instanceOfClass.Receive(mem.ToArray());
                    mem.Seek(0, SeekOrigin.Begin);
                    mem.SetLength(0);
                    TimeOut = 0;
                }
                else
                {
                    TimeOut++;
                    Thread.Sleep(SleepTime);
                }
            }
            instanceOfClass.CloseConnection();
            socket.Close();
            socket = null;
            serverThread.Abort();
        }

        ~ServerInstance()
        {
            if (serverThread != null)
            {
                serverThread.Abort();
            }
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    }

    /// <summary>
    /// Zusammenfassung für den Server
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Aktueller Server-Status
        /// </summary>
        public ServerStatus status = new ServerStatus();
        /// <summary>
        /// Port, auf dem der Server auf Clientverbindungen wartet
        /// </summary>
        public const int serverListenPort = 10000;
        /// <summary>
        /// Anzahl der maximal möglichen Clients pro Server
        /// </summary>
        public const int maxServerConnections = 100;
        /// <summary>
        /// Array für die möglichen Verbindungen
        /// </summary>
        private System.Collections.ArrayList Clients = new ArrayList(maxServerConnections);
        /// <summary>
        /// Anzahl Millisekunden in Warteschleifen
        /// </summary>
        public const int sleepTime = 200;
        /// <summary>
        /// Anzahl Millisekunden bis zu einer TimeOut-Meldung
        /// </summary>
        public const int TimeOut = 2000;
        /// <summary>
        /// IP-Adresse, an der auf Verbindungen gewartet werden soll.
        /// Standardmässig wird auf allen Schnittstellen gewartet.
        /// </summary>
        public IPAddress ipAddress = IPAddress.Any;//Dns.Resolve("127.0.0.1").AddressList[0];
                                                   /// <summary>
                                                   /// Der Haupt-Thread
                                                   /// </summary>
        private Thread mainThread;
        /// <summary>
        /// Zugriff auf Ereignisanzeige
        /// </summary>
        private EventLog Log = new EventLog("", ".", "TcpServer");
        /// <summary>
        /// Klasse, welche bei einer neuen Verbindung für die serverseitige
        /// Verwaltung erstellt werden soll.
        /// </summary>
        private Type serverClass;

        /// <summary>
        /// Der Standardkonstruktor richtet den Thread ein, welcher
        /// anschliessend auf Client-Verbindungen wartet.
        /// </summary>
        public Server(Type serverClass)
        {
            Type it = typeof(IServerInterface);
            // Zentralen Eventhandler registrieren
            status.MessageEvents += new MessageEvent(Server_MessageEvents);
            // Klasse für serverseitige Verwaltung setzen
            Type[] ifs = serverClass.GetInterfaces();
            bool found = false;
            foreach (Type aType in ifs)
                if (it.ToString() == aType.ToString()) found = true;
            if (found)
                this.serverClass = serverClass;
            // Hauptthread wird instanziiert ...
            mainThread = new Thread(new ThreadStart(this.MainListener));
            // ... und gestartet
            mainThread.Start();
        }

        public void Dispose()
        {
            // Haupt-Thread beenden
            if (mainThread != null)
            {
                mainThread.Abort();
            }
            // Client-Threads beenden
            if (Clients.Count > 0)
            {
                for (int cnt = Clients.Count - 1; cnt >= 0; cnt--)
                {
                    ServerInstance srv = (ServerInstance)Clients[cnt];
                    srv.serverThread.Abort();
                    Clients.RemoveAt(cnt);
                }
            }
            // Event-Handler abmelden
            //status.messageEvents=null;
        }

        ~Server()
        {
            Dispose();
        }

        /// <summary>
        /// Interner Event-Handler für alle Server-Meldungen, welche in die 
        /// Ereignisüberwachung des aktuellen Systems geschrieben werden.
        /// </summary>
        /// <param name="Message">Die Mitteilung</param>
        private void Server_MessageEvents(string Message)
        {
            Log.WriteEntry(Message);
        }

        /// <summary>
        /// Haupt-Thread, wartet auf neue Client-Verbindungen
        /// </summary>
        private void MainListener()
        {
            // Alle Netzwerk-Schnittstellen abhören
            TcpListener listener = new TcpListener(ipAddress, serverListenPort);
            status.NewMessage("Listening on port " + serverListenPort + "...");
            try
            {
                listener.Start();
                // Solange Clients akzeptieren, bis das 
                // angegebene Maximum erreicht ist
                while (status.CurrentConnections <= maxServerConnections)
                {
                    while (!listener.Pending()) { Thread.Sleep(sleepTime); }
                    Socket newSocket = listener.AcceptSocket();
                    if (newSocket != null)
                    {
                        status.CurrentConnections++;
                        // Mitteilung bzgl. neuer Clientverbindung
                        status.NewMessage("Neue Client-Verbindung (" +
                            "IP: " + newSocket.RemoteEndPoint + ", " +
                            "Port " + ((IPEndPoint)newSocket.LocalEndPoint).Port.ToString() + ")");
                        // Instanz der serverseitigen Verwaltungsklasse erzeugen
                        IServerInterface x = (IServerInterface)Activator.CreateInstance(serverClass);
                        x.SetConnectionData(newSocket);
                        ServerInstance newConnection = new ServerInstance(newSocket, x);
                        Clients.Add(newConnection);
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                status.NewMessage("Server wird beendet.\r\n"+ ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler bei Verbindungserkennung", ex);
            }
        }
    }
}