using HWPenSignLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HW.Helper
{
    class Program
    {
      
        static void Main(string[] args)
        {
            //HWPenSignClass hWPenSign = new HWPenSignClass();
            //hWPenSign.HWSetBkColor(0xE0F8E0);

            //hWPenSign.HWSwitchMonitor(1, 0);
     
            ////   hWPenSign.HWSetCtlFrame(2, 0x000000);
            //var res = hWPenSign.HWInitialize();
           
           // var base64 = hWPenSign.HWGetBase64Stream(0);

             DeviceChangeNotifier.Start();
            DeviceChangeNotifier.OKResultFunc=((a) =>
            {
                Console.WriteLine(a);
            });
            while (true)
            {
                var s = Console.ReadLine();
                switch (Convert.ToInt32(s))
                {
                    case 1:
                        DeviceChangeNotifier.hWPenSign.HWInitialize();
                        break;
                    case 2:
                        DeviceChangeNotifier.hWPenSign.HWFinalize();
                        break;
                    case 3:
                        Console.WriteLine(DeviceChangeNotifier.hWPenSign.HWGetBase64Stream(0));
                        break;
                    case 4:
                        DeviceChangeNotifier.hWPenSign.HWClearPenSign();
                        break;
                }
            }
            Console.ReadLine();
            


        }

        private static void HWPenSign_PenWidthChange(int penWidth)
        {
          
        }

        private static void HWPenSign_PenModeChange(int flag)
        {
        }
    }
}
