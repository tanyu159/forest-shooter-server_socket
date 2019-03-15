using Common;
using GameServer.DAO;
using GameServer.Model;
using GameServer.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Controller
{
    class UserController:BaseController
    {
        public UserController()
        {
            //设置RequestCode
            requestCode = RequestCode.User;
        }

        /// <summary>
        /// 执行登录-函数名与ActionCode相同，这里面的参数也就是反射机制那里调用时的参数
        /// </summary>
        /// <param name="data">客户端loginRequest传来的数据</param>
        /// <param name="client">Client的引用</param>
        /// <param name="server">Server的引用</param>
        public string Login(string data,Client client,Server server)
        {
            //拆分传来的data
            string[] strArr = data.Split('#');
            string username = strArr[0];
            string password = strArr[1];
            //调用UserDAO的验证用户方法
            User user= UserDAO.VerifyUser(client.MysqlConn,username,password);
            //回给客户端响应
            if (user == null)
            {
                //登陆失败
                //将ReturnCode.Fail转化为字符串返回
                return ((int)ReturnCode.Fail).ToString();
            }
            else {
                //登录成功-查询战绩
                Score score=ScoreDAO.GetScoreByUserId(client.MysqlConn,user.Id);
                //为Client中的User对象和Score对象初始化
                client.user = user;
                client.score = score;
                //将ReturnCode.Success转化为字符串,以及用户id，用户名，总场数，胜利数都转化为字符串返回.用#分割
                string info = ((int)ReturnCode.Success).ToString() + "#" + user.Id + "#" + user.Username + "#" + score.TotalCount + "#" + score.WinCount;
                return info;
            }
        }

        /// <summary>
        /// 执行注册-函数名与ActionCode相同，这里面的参数也就是反射机制那里调用时的参数
        /// </summary>
        /// <param name="data">客户端registerRequest传来的数据</param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Register(string data, Client client, Server server)
        {
            //拆分数据
            string[] strArr = data.Split('#');
            string username = strArr[0];
            string password = strArr[1];
            //:调用UserDAO中的注册用户方法
            bool isSucessful= UserDAO.RegisterUser(client.MysqlConn,username,password);
            if (isSucessful)
            {
                //响应客户端注册成功
                return ((int)ReturnCode.Success).ToString();
            }
            else {
                //响应客户端注册失败
                return ((int)ReturnCode.Fail).ToString();
            }

            

        }
    }
}
