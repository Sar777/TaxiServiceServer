using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TaxiServiceServer.Common;
using TaxiServiceServer.Database;
using TaxiServiceServer.Managers;
using TaxiServiceServer.Networking;
using TaxiServiceServer.Parser;
using TaxiServiceServer.src.Managers;

namespace TaxiServiceServer
{
    class Program
    {
        private static void ServerUpdateLoop()
        {
            ///- Work server
            while (true)
            {
                Server.Server.Instance.Update();
                Thread.Sleep(Constants.SERVER_SLEEP_CONST);
            }
        }

        static void Main(string[] args)
        {
            if (!MySQL.Instance().Initialization())
                return;

            Console.WriteLine("Loading handlers...");
            Handler.LoadHandlers();

            Console.WriteLine("Loading cars...");
            CarsMgr.Instance.LoadFromDB();

            Console.WriteLine("Loading users...");
            UserMgr.Instance.LoadFromDB();

            Console.WriteLine("Loading orders...");
            OrderMgr.Instance.LoadFromDB();

            Task.Factory.StartNew(() => { new AsyncTcpServer(IPAddress.Parse("0.0.0.0"), 8085).Start(); });
            var taskServer = Task.Factory.StartNew(ServerUpdateLoop);
            Task.WaitAll(taskServer);
            AsyncTcpServer.Instanse.Stop();
        }
    }
}
