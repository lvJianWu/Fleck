/***************************************************************************
 * Copyright (c) 2018 All Rights Reserved.
 * CLR版本： 4.0.30319.42000
 *机器名称：LVJW-PC
 *公司名称：北京华誉维诚技术服务有限公司
 *命名空间：Fleck.Samples.ConsoleApp
 *文件名：  RequestAndResponse
 *唯一标识：1146eb38-99fd-4d3f-a008-c7c1e5ead86f
 *当前的用户域：CORP
 *创建人：  lvjw
 *创建时间：2018-11-29 9:49:25
 *描述：
 *
 *=====================================================================*/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fleck.Samples.ConsoleApp
{
    public class SocketRequest
    {
        public string cmd { get; set; }
        public string data { get; set; }

        public static SocketRequest DeserializeObject(string rq)
        {
            return JsonConvert.DeserializeObject< SocketRequest>(rq);
        }


    }
    public class SocketReponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
