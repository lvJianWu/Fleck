using Fleck;
using Fleck.Samples.ConsoleApp;
using HW.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Trusit.Certs.Sign;

namespace Trusit.Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
 
             ServiceHelper.CreateProcess("Fleck.Samples.ConsoleApp.exe", @"C:\Users\lvjw\source\repos\Fleck\src\Samples\ConsoleApp\bin\Debug\");
            //  Main();
        }
        List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        void Main()
        {
            FleckLog.Level = LogLevel.Debug;
         
            var server = new WebSocketServer("wss://0.0.0.0:8181");
            var cert = CertManager.GetCert("localhost", TimeSpan.FromDays(3650), "devpwd", AppDomain.CurrentDomain.BaseDirectory + "cert.dat");
            CertManager.ActivateCert(cert);
            server.Certificate = cert;
            server.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12;
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {

                    try
                    {
                        var rq = SocketRequest.DeserializeObject(message);
                        switch (rq.cmd.ToLower())
                        {
                            case "start":
                                StartSign(message, socket);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        SocketReponse socketReponse = new SocketReponse();
                        socketReponse.Code = 1;
                        socketReponse.Message = ex.Message;
                        socket.Send(socketReponse.ToString());
                    }

                    Console.WriteLine(message);

                };
            });


        

        }

        private static void StartSign(string message, IWebSocketConnection socket)
        {
            try
            {

          
            if (!DeviceChangeNotifier.isStart())
            {
                DeviceChangeNotifier.Start();
            }
            else
            {
                DeviceChangeNotifier.Init();
              
             
            }
            DeviceChangeNotifier.OKResultFunc = ((a) =>
            {
                SocketReponse socketReponse = new SocketReponse();
                socketReponse.Data = a;
                DeviceChangeNotifier.Finalize();
                socket.Send(socketReponse.ToString());
            });
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        protected override void OnStop()
        {
            allSockets.ForEach(m => m.Close());
        }
    }
}
