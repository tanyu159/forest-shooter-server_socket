using Common;
using GameServer.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Controller
{
    /// <summary>
    /// 该类为抽象类，所有Controller继承至该该类
    /// </summary>
    abstract class BaseController
    {
       /// <summary>
       /// 该Controller对应的RequestCode
       /// </summary>
       public RequestCode requestCode = RequestCode.None;

        /// <summary>
        /// 处理默认请求-未指定ActionCode时调用【即ActionCode.None】
        /// 返回类型为String，因为有时要返回客户端数据
        /// </summary>
        /// <param name="data">客户端传来的数据-【已经是解析好的数据了】</param>
        /// <param name="runThisHandleClient">调用这个handle的客户端</param>
        /// <param name="server">服务器的引用</param>
        public virtual string DefaultHandle(string data,Client runThisHandleClient,Server server) {
            return null;
        }
       
    }
}
