using Common;
using GameServer.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Controller
{
    class RoomController:BaseController
    {
        public RoomController() {
            requestCode = RequestCode.Room;
        }
        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="data"></param>
        /// <param name="p1client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string CreatRoom(string data, Client p1client, Server server)
        {
            Room room = new Room(server);//创建房间对象
            room.AddClientToRoom(p1client);//将发起创建房间的client对象【即房主】加入房间中的client列表
            server.RoomList.Add(room);//将该room对象加入到server下的
            return ((int)ReturnCode.Success).ToString()+"#"+((int)RoleType.Blue).ToString();//房主分配蓝色
            //后期扩展：比如增加用户封号功能，就不能创建房间

        }
        /// <summary>
        /// 列出房间列表-只列出可加入的房间
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string ListRoom(string data, Client client, Server server)
        {
            //要将每个可加入的房间的房主信息进行组拼
            //格式每个房间之间用*隔开，房间内信息用#号隔开
            string roomListInfo = "";
            foreach (Room temp in server.RoomList)
            {
                //只显示出处于等待加入状态的房间
                if (temp.roomState == RoomState.WaitingJoin)
                {
                    roomListInfo += temp.GetHouseOwnerData() + "*";
                }
            }
            //在有值的情况下会多写1个*号最后，将其去除
            if (roomListInfo.Length > 0)
            {
                Console.WriteLine("roomListInfo.Length" + roomListInfo.Length);
                roomListInfo= roomListInfo.Remove(roomListInfo.Length-1 , 1);//该函数是返回，并非直接作用在其本身【调用这个函数的字符串上】
                Console.WriteLine("房间列表信息" + roomListInfo);
                return roomListInfo;
            }
            else {
                //这种情况说明没有房间-但依然要返回字符串
                roomListInfo = "EMPTY";
                Console.WriteLine("响应信息：没有房间");
                return roomListInfo;
            }
            
        }
        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string JoinRoom(string data, Client p2client, Server server)
        {
            //解析房间id
            int roomId = int.Parse(data);
            //通过房间id找到房间
            Room room = server.GetRoomByRoomId(roomId);
            if (room == null)
            {
                //:不可加入-因为房间不存在
                return ((int)ReturnCode.Fail).ToString();
            }
            else {
                //判断房间状态
                if (room.roomState == RoomState.WaitingJoin)
                {
                    #region 完成P2【加入者】加入房间的响应
                    //可加入-将P2添加进去
                    room.AddClientToRoom(p2client);
                    string playersInfo = room.GetInfoOfPlayerInRoom(true);
                    string responseData = ((int)ReturnCode.Success).ToString() +"-"+ ((int)RoleType.Red).ToString() + "-" + playersInfo;
                    //广播消息-但除开这个客户端【避免重复发送数据】
                    room.BroadcastMessage(p2client, ActionCode.UpdateRoom, playersInfo);//这个数据不包含ReturnCode;
                    //涉及3个分割符，'-'号将ReturnCode与RoleType与数据部分分开，'*'号将每个玩家之间分开，'#'号将玩家的每个数据分开
                    Console.WriteLine("进入房间请求处理了且成功，返回的响应信息为：" + responseData);
                    return responseData;
                    #endregion
                }
                else {
                    //:不可加入-房间存在但满员
                    return ((int)ReturnCode.Fail).ToString();
                }
            }
        }

        public string QuitRoom(string data, Client quitClient, Server server)
        {
            //判断该Client是不是房主，拿到该Client的Room中的Client列表第一个对象进行比较
            //若相同-则是房主
            if (quitClient == quitClient.currentEnteredRoom.RoomClientList[0])
            {
                //是房主TODO
                //向房间内除房主之外的其他客户端广播 退出房间响应
                quitClient.currentEnteredRoom.BroadcastMessage(quitClient, ActionCode.QuitRoom, ((int)ReturnCode.Success).ToString());
                //删除这个房间
                quitClient.currentEnteredRoom.Close();
                return ((int)ReturnCode.Success).ToString();
            }
            else {
                //不是房主
                //向其他客户端广播有客户端已经退出
                quitClient.currentEnteredRoom.BroadcastMessage(quitClient, ActionCode.UpdateRoom, quitClient.currentEnteredRoom.GetInfoOfPlayerInRoom(false));
                //从房间中移除这个客户端
                quitClient.currentEnteredRoom.RemoveClientFromRoom(quitClient);
                //向当前客户端【非房主退出的】发送退出成功的响应
                return ((int)ReturnCode.Success).ToString() ;
            }



            //return null;
        }
    }
}
