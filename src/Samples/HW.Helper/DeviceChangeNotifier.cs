/***************************************************************************
 * Copyright (c) 2018 All Rights Reserved.
 * CLR版本： 4.0.30319.42000
 *机器名称：LVJW-PC
 *公司名称：北京华誉维诚技术服务有限公司
 *命名空间：HW.Helper
 *文件名：  DeviceChangeNotifier
 *唯一标识：9de046e5-9ce7-4681-ab4e-5c1fff4ec366
 *当前的用户域：CORP
 *创建人：  lvjw
 *创建时间：2018-11-28 13:40:29
 *描述：
 *
 *=====================================================================*/
using AxHWPenSignLib;
using HWPenSignLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW.Helper
{
  public  class DeviceChangeNotifier : Form
    {
       public static Action<string> OKResultFunc;
       public static AxHWPenSign hWPenSign =null
            ;
        static int complete_msg = 0x7ffe;
        static int cancel_msg = 0x7ffd;
        private AxHWPenSign axHWPenSign1;

        public delegate void DeviceNotifyDelegate(Message msg);
        public static event DeviceNotifyDelegate DeviceNotify;
        private static DeviceChangeNotifier mInstance;

        public static bool isStart()
        {
            return hWPenSign != null;
        }
        public static void Clear()
        {
            hWPenSign.HWClearPenSign();
        }
        public static void Init()
        {
        var res=    hWPenSign.HWInitialize();
        }
        public static void Finalize()
        {
            hWPenSign.HWFinalize();
        }
        public DeviceChangeNotifier()
        {
            InitializeComponent();
            hWPenSign = this.axHWPenSign1;
            hWPenSign.HWSetExtWndHandle(this.Handle.ToInt32());
            var res = hWPenSign.HWInitialize();
            mInstance = this;
        }
     
            public static void Start()
        {
           
            Thread t = new Thread(runForm);
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
          
        }
        public static void Stop()
        {
            if (mInstance == null) return;
            DeviceNotify = null;
            hWPenSign = null;
            mInstance.Invoke(new MethodInvoker(mInstance.endForm));
        }
        private static void runForm()
        {
            Application.Run(new DeviceChangeNotifier());
        }

        private void endForm()
        {
            this.Close();
        }
      
        protected override void SetVisibleCore(bool value)
        {
            // Prevent window getting visible
            if (mInstance == null) CreateHandle();
                mInstance = this;
            value = false;



            


            base.SetVisibleCore(value);
          

        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == complete_msg)
            {
              
                if (OKResultFunc != null)
                {
                  var a=  hWPenSign.HWGetBase64Stream(1);
                    OKResultFunc(a);
                    hWPenSign.HWClearPenSign();
                }
            }
            else if (m.Msg == cancel_msg)
            {
               
            }

            base.WndProc(ref m);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceChangeNotifier));
            this.axHWPenSign1 = new AxHWPenSignLib.AxHWPenSign();
            ((System.ComponentModel.ISupportInitialize)(this.axHWPenSign1)).BeginInit();
            this.SuspendLayout();
            // 
            // axHWPenSign1
            // 
            this.axHWPenSign1.Enabled = true;
            this.axHWPenSign1.Location = new System.Drawing.Point(12, 12);
            this.axHWPenSign1.Name = "axHWPenSign1";
            this.axHWPenSign1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axHWPenSign1.OcxState")));
            this.axHWPenSign1.Size = new System.Drawing.Size(600, 300);
            this.axHWPenSign1.TabIndex = 0;
            // 
            // DeviceChangeNotifier
            // 
            this.ClientSize = new System.Drawing.Size(625, 335);
            this.Controls.Add(this.axHWPenSign1);
            this.Name = "DeviceChangeNotifier";
            ((System.ComponentModel.ISupportInitialize)(this.axHWPenSign1)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
