using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
public enum RoomState
{
    WaitingJoin,//等待加入【未满员】
    WaitingStart,//等待开始【满员】
    Battle,//战斗中
    End//结束
}
namespace GameServer.Servers
{
    class Room
    {
        private List<Client> roomClientList = new List<Client>();//当前房间中的玩家client
        public RoomState roomState = RoomState.WaitingJoin;
        private Server currentServer;

        private const int MAX_HP = 100;//最大血量
        public List<Client> RoomClientList
        {
            get
            {
                return roomClientList;
            }

           
        }

        public Room(Server server)
        {
            currentServer = server;
        }
        /// <summary>
        /// 向房间中添加客户端
        /// </summary>
        /// <param name="client">client对象</param>
        public void AddClientToRoom(Client client)
        {
            //将该client加入到房间
            roomClientList.Add(client);
            //设置血量
            client.currentHp = MAX_HP;
            //设置这个Client的当前所在房间
            client.currentEnteredRoom = this;
            //每次加入后判断列表中数量，大于等于2后不可加入，更改房间状态为waitBattle
            if (roomClientList.Count >= 2)
            {
                roomState = RoomState.WaitingStart;
            }
        }
        /// <summary>
        /// 从房间中移除某个客户端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClientFromRoom(Client client)
        {
            client.currentEnteredRoom = null;
            roomClientList.Remove(client);
            if (roomClientList.Count < 2&&roomClientList.Count>=0)
            {
                roomState = RoomState.WaitingJoin;
            }
        }
        /// <summary>
        /// 得到房主的信息，格式为 房主名#总场数#胜利数
        /// </summary>
        /// <returns></returns>
        public string GetHouseOwnerData()
        {
            //房主信息就4个,其中房主userid拿来做房间的id，用#分割进行组拼
            string houseOwnerData = roomClientList[0].user.Id+"#"+roomClientList[0].user.Username + "#" +roomClientList[0].score.TotalCount + "#" + roomClientList[0].score.WinCount;
            return houseOwnerData;
        }
        /// <summary>
        /// 获得该房间所有玩家的信息【也就只有两个】
        /// 玩家之间用*分割，内部用#分割
        /// </summary>
        /// <returns></returns>
        public string GetInfoOfPlayerInRoom(bool isHaveP2)
        {
            if (isHaveP2)
            {
                //房间中有两个客户端，则两个的信息都发
                string playersInfo = GetHouseOwnerData() + "*";
                string p2Info = roomClientList[1].user.Id + "#" + roomClientList[1].user.Username + "#" + roomClientList[1].score.TotalCount + "#" + roomClientList[1].score.WinCount;
                playersInfo += p2Info;
                return playersInfo;
            }
            else {
                //只发房主
                return GetHouseOwnerData();
            }
        }
        /// <summary>
        /// 关闭房间-两种情况-这个主要用于处理非正常的退出房间【即直接关闭客户端】
        /// </summary>
        public void Close(Client client)
        {
            if (client == RoomClientList[0])
            {
                //调用这个的Client为列表中的第一个就说明是房主
                Close();
            }
            else {
                //非房主退出，就把这个玩家从这个房间的Client列表中移除
                RoomClientList.Remove(client);
            }
        }

        /// <summary>
        /// 向房间内除需排除的客户端之外的客户端广播消息
        /// </summary>
        /// <param name="excludeClient">需要排除的客户端</param>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        public void BroadcastMessage(Client excludeClient, ActionCode actionCode, string data)
        {
            foreach (Client client in roomClientList)
            {
                if (client!=excludeClient) {
                    currentServer.SendResponseToClient(client, actionCode, data);
                }
            }
        }
        /// <summary>
        /// 退出并销毁房间【房主退出时调用】-正常情况的退出
        /// </summary>
        public void Close()
        {
            foreach (Client temp in roomClientList)
            {
                temp.currentEnteredRoom = null;
            }
            currentServer.RoomList.Remove(this);
        }

        /// <summary>
        /// 计时器-开始计时
        /// </summary>
        public void StartTimer()
        {
            new Thread(RunTimer).Start();
        }

        private void RunTimer()
        {
            //先等待1s，保证所有客户端都响应了开始游戏StartGame
            Thread.Sleep(1000);
            for (int i = 3; i >= 1; i--)
            {
                //计时器-每过1s就把当前秒数广播给所有客户端
                BroadcastMessage(null, ActionCode.ShowTimer, i.ToString());
                Thread.Sleep(1000);
            }
            //计时结束-发送开始游玩响应
            BroadcastMessage(null, ActionCode.StartPlay, "null");

        }

        /// <summary>
        /// 扣血操作-其有返回值，如果返回真，代表游戏结束-胜负已定
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="excludeClient">排除的客户端</param>
        public bool DecreaseHP(int damage, Client excludeClient)
        {
            bool isOver = false;
            foreach (Client client in roomClientList)
            {
                if (client != excludeClient)
                {
                    client.currentHp -= damage;
                    if (client.currentHp <= 0)
                    {
                        client.currentHp = 0;
                        isOver = true;
                    }
                }
            }
            return isOver;
        }
        /// <summary>
        /// 得到当前房间内所有玩家的血量
        /// </summary>
        /// <returns></returns>
        public string GetAllPlayerHP()
        {
            string HPdata="";
            foreach (Client temp in roomClientList)
            {
                HPdata += temp.currentHp + "#";
            }
            HPdata= HPdata.Remove(HPdata.Length-1, 1);
            return HPdata;
        }
    }
}
