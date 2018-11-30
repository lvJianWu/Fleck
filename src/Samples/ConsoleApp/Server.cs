using HW.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Trusit.Certs.Sign;
using Trusit.TableWrite;

namespace Fleck.Samples.ConsoleApp
{
    class Server
    {
        static void Main()
        {


            Console.Title = "Trusit.TableWrite"; //为控制台窗体指定一个标题，便于定位和区分
         
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("wss://0.0.0.0:8181");
            try
            {
                var cert = CertManager.GetCert("localhost", TimeSpan.FromDays(3650), "devpwd", AppDomain.CurrentDomain.BaseDirectory + "cert.dat");
                CertManager.ActivateCert(cert);
                server.Certificate = cert;
            }
            catch(Exception ex)
            {
                Console.Write("证书安装失败！");
            }
          
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

           #if !DEBUG
           Thread.Sleep(1000);
            windowUtilities.WindowHide("Trusit.TableWrite");
#endif

            //隐藏这个窗口
            windowUtilities.SetAutoStart();//设置开机自启动
            var input = Console.ReadLine();
         
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }

        }

        private static void StartSign(string message, IWebSocketConnection socket)
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
    }
}
