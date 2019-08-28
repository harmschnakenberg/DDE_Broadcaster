using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDE_Broadcaster
{
    class DDE_Client
    {

        public static string DdeServerName { get; } = "view";
        public static string DdeTopic { get; } = "tagname";
        public static int DdeLoopTime { get; set; } = 1000;

        //public static string SyncRequest(string TagName)
        //{
        //    if (TagName.Length < 3)
        //    {
        //        return "TagName zu kurz.";
        //    }

        //    try
        //    { 
        //        using (DdeClient client = new DdeClient(DdeServerName, DdeTopic))
        //        {

        //            client.Disconnected += OnDisconnected;

        //            client.Connect();

        //            // Syncronous Request Operation
        //            return client.Request(TagName, 60000);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.GetType().ToString() + "\r\n" + e.Message + "\r\n" + e.StackTrace + "\r\nTagName: " + TagName + "\r\n");
        //        //Console.WriteLine("Press ENTER to quit...");
        //        //Console.ReadLine();
        //        return "FEHLER SyncRequest";
        //    }
        //}

        //public static void DdeRequest(string TagName)
        //{

        //    try
        //    {
        //        // Create a client that connects to 'myapp|mytopic'. 
        //        using (DdeClient client = new DdeClient(DdeServerName, DdeTopic))
        //        {
        //            // Subscribe to the Disconnected event.  This event will notify the application when a conversation has been terminated.
        //            client.Disconnected += OnDisconnected;

        //            // Connect to the server.  It must be running or an exception will be thrown.
        //            client.Connect();

        //            // Syncronous Request Operation
        //            // string result = client.Request(TagName, 60000);


        //            // Asynchronous Request Operation
        //            client.BeginRequest(TagName, 1, OnRequestComplete, client);

        //            //string result = "=>AsyncTest";

        //            // return result;

        //            // Wait for the user to press ENTER before proceding.
        //            //Console.WriteLine("Press ENTER to quit...");
        //            //Console.ReadLine();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.GetType().ToString() + "\r\n" + e.Message + "\r\n" + e.StackTrace);
        //        //Console.WriteLine("Press ENTER to quit...");
        //        //Console.ReadLine();
        //        //return null;
        //    }

        //}

        public void DdeAdvise(string TagName)
        {

            try
            {
                DdeQueue.AddTag(TagName);

                // Create a client that connects to 'myapp|mytopic'. 
                using (DdeClient client = new DdeClient(DdeServerName, DdeTopic))
                {
                    // Subscribe to the Disconnected event.  This event will notify the application when a conversation has been terminated.
                    client.Disconnected += OnDisconnected;

                    // Connect to the server.  It must be running or an exception will be thrown.
                    client.Connect();

                    //Lese den Wert initial von DDE Server:
                    // Asynchronous Request Operation. 
                    //client.BeginRequest(TagName, 1, OnRequestComplete, client);
                    DdeQueue.DdeDict[TagName] = client.Request(TagName, 60000);

                    //Lese den Wert von DDE Server neu ein bei Wertänderung.
                    // Advise Loop.
                    client.StartAdvise(TagName, 1, true, 60000);
                    /// client.Advise += OnAdvise;
                    client.Advise += OnAdvise2;

                    while (DdeQueue.DdeDict.ContainsKey(TagName))
                    {
                        System.Threading.Thread.Sleep(DdeLoopTime);
                    }
                    Console.WriteLine("Advise beendet für " + TagName);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().ToString() + "\r\n" + e.Message + "\r\n" + e.StackTrace);
                //Console.WriteLine("Press ENTER to quit...");
                //Console.ReadLine();
                //return null;
            }

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
                GrapeVine.ResultTest = Encoding.ASCII.GetString(data);
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

        //private static void OnAdvise(object sender, DdeAdviseEventArgs args)
        //{
        //    Console.WriteLine("OnAdvise: "+ args.Item + " - "  + args.Text);
        //    DdeQueue.DdeDict[args.Item] = args.Text;
        //}

        private void OnAdvise2(object sender, DdeAdviseEventArgs args)
        {
            Console.WriteLine("OnAdvise: " + args.Item + " - " + args.Text);
            DdeQueue.DdeDict[args.Item] = args.Text;
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



    }
}
