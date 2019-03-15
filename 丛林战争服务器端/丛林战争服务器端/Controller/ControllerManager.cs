using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Reflection;
using GameServer.Servers;

namespace GameServer.Controller
{
    class ControllerManager
    {
        /// <summary>
        /// 存储所有Controller的字典，一个RequestCode对应一个Controller
        /// </summary>
        private Dictionary<RequestCode, BaseController> _allControllerDic = new Dictionary<RequestCode, BaseController>();
        private Server server;


        public ControllerManager(Server server) {
            this.server = server;
            InitController();
        }

        /// <summary>
        ///  实例化所有Controller，并存在字典中。到时候需要用到时直接拿访问字典即可
        /// </summary>
        void InitController()
        {
            //生成Controller对象，再将其加入字典
            _allControllerDic.Add(RequestCode.None,new DefaultController());
            _allControllerDic.Add(RequestCode.User, new UserController());
            _allControllerDic.Add(RequestCode.Room, new RoomController());
            _allControllerDic.Add(RequestCode.Game, new GameController());
        }
        /// <summary>
        /// 处理请求-通过RequestCode找到Controller，ActionCode找到具体执行的方法
        /// </summary>
        /// <param name="requestCode">请求枚举</param>
        /// <param name="actionCode">方法枚举</param>
        /// <param name="data">数据</param>
        /// <param name="runThisRequestClient">发起这个请求的客户端</param>
        public void HandleRequest(RequestCode requestCode,ActionCode actionCode,string data,Client runThisRequestClient)
        {
            BaseController controller;
            bool isSuccessful=  _allControllerDic.TryGetValue(requestCode,out controller);
            if (!isSuccessful)
            {
                //报错
                Console.WriteLine("未从allControllerDic字典中找到RequestCode：" + requestCode.ToString() + "对应的Controller");
                return;
            }
            else {
                //找到了就调用Controller中ActionCode对应的方法
            }
            //将ActionCode枚举类型转化为字符串-不用ToString，因为这种方式效率不高
            string methodName = Enum.GetName(typeof(ActionCode), actionCode);
            //利用反射机制-ActionCode就是Controller中方法的名字，通过方法名字来访问方法
            //得到函数信息
            MethodInfo methodInfo=  controller.GetType().GetMethod(methodName);
            if (methodInfo == null)
            {
                Console.WriteLine("未找到类" + controller.ToString() + "对应的方法" + methodName);
                return;
            }
            else {
                //执行这个函数-要指定哪个对象，要参数可以传参数[为一个Object
                object[] parameters = new object[] { data,runThisRequestClient,server};
                object responseObj=  methodInfo.Invoke(controller, parameters);
                //Console.WriteLine("");

                //判断obj，看需不需要返回客户端数据
                if (responseObj != null && (responseObj as string) != "")
                {
                    //不为空且不为空串时-返回给客户端数据[响应客户端]
                    server.SendResponseToClient(runThisRequestClient,actionCode, responseObj as string);
                }
            }
        }
    }
}
