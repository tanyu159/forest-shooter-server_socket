using Common;
using GameServer.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Controller
{
    class GameController:BaseController
    {
        public GameController()
        {
            requestCode = RequestCode.Game;
        }
        /// <summary>
        /// 处理开始游戏请求
        /// </summary>
        /// <param name="data"></param>
        /// <param name="roomOwnerClient"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string StartGame(string data, Client roomOwnerClient, Server server)
        {
            //由于已经对Start按钮进行了处理
            //执行该函数的client必定是房主

            //解析这个data，来看有没有P2
            if (data.Equals("EMPTY"))
            {
                //说明没有P2玩家，不能开始游戏，返回 失败
                return ((int)ReturnCode.Fail).ToString();
            }
            else {
                //有P2玩家，可以开始游戏
                //广播给该房主的房间的其他客户端
                roomOwnerClient.currentEnteredRoom.BroadcastMessage(roomOwnerClient, ActionCode.StartGame, ((int)ReturnCode.Success).ToString());
                //开启房间计时器
                roomOwnerClient.currentEnteredRoom.StartTimer();
                return ((int)ReturnCode.Success).ToString();
            }
        }
        /// <summary>
        /// 处理同步位置动画信息请求
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Move(string data, Client client, Server server)
        {
            //将该数据返回给房间中的另一个客户端
            if(client.currentEnteredRoom!=null)
            client.currentEnteredRoom.BroadcastMessage(client, ActionCode.Move, data);
            return null;//当前客户端不用返回数据
        }
        /// <summary>
        /// 处理同步箭的请求
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string SyncArrow(string data, Client client, Server server)
        {
            //和同步位置一样，将这个数据返回给房间中的另一个客户端
            if (client.currentEnteredRoom != null)
                client.currentEnteredRoom.BroadcastMessage(client, ActionCode.SyncArrow, data);
            return null;//当前客户端不用返回数据
        }
        /// <summary>
        /// 处理动画同步请求
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string SyncAnimation(string data, Client client, Server server)
        {
            if (client.currentEnteredRoom != null)
                client.currentEnteredRoom.BroadcastMessage(client, ActionCode.SyncAnimation, "null");

            return null;
        }
        /// <summary>
        /// 执行扣血操作
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string TakeDamage(string data, Client client, Server server)
        {
            if (client.currentEnteredRoom == null)
            {
                return null; 
            }
            //解析扣除血量
            int damage = int.Parse(data);
            //执行扣血并返回结果
            bool isOver = client.currentEnteredRoom.DecreaseHP(damage, client);
            if (isOver)
            {
                //:游戏结束
                foreach (Client temp in client.currentEnteredRoom.RoomClientList)
                {
                    if (temp.isDie())
                    {
                        //说明该玩家死亡:向客户端返回游戏失败
                        string res = ((int)ReturnCode.Fail).ToString();
                        server.SendResponseToClient(temp, ActionCode.GameOver, res);
                        //：更新数据库
                        temp.UpdateUserScore(false);

                    }
                    else
                    {
                        //说明该玩家没有死亡:向客户端返回游戏胜利
                        string res = ((int)ReturnCode.Success).ToString();
                        server.SendResponseToClient(temp, ActionCode.GameOver, res);
                        //：更新数据库
                        temp.UpdateUserScore(true);
                    }
                }
                //销毁房间
                client.currentEnteredRoom.Close();
                
            }
            //广播一次消息-同步所有人血量
            if(client.currentEnteredRoom!=null)
            client.currentEnteredRoom.BroadcastMessage(null,ActionCode.SyncHP,client.currentEnteredRoom.GetAllPlayerHP());


            return null;//暂时返回空【后期血条做扩展】
        }


        public string QuitInBattle(string data, Client client, Server server)
       {
            //广播退出游戏响应给所有在房间中的客户端
            client.currentEnteredRoom.BroadcastMessage(null, ActionCode.QuitInBattle, "null");
            //直接销毁这个房间即可
            client.currentEnteredRoom.Close();
            return null;
       }
    }
    
   
}
