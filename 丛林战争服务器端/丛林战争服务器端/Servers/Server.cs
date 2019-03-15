using Common;
using GameServer.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Servers
{
    /// <summary>
    /// 建立连接
    /// </summary>
    class Server
    {
        
        private IPEndPoint _ipEndPoint;//IP地址和端口号
        private Socket _serverSocket;//ServerSocket对象引用
        private List<Client> _clientList=new List<Client>();//客户端列表，用于管理所有客户端
        private List<Room> _roomList = new List<Room>();//房间列表
        private ControllerManager _controllerManager ;//Controller管理器

        public List<Room> RoomList
        {
            get
            {
                return _roomList;
            }

            set
            {
                _roomList = value;
            }
        }
        #region 服务的开启与侦听客户端接入
        public Server()
        { }


        /// <summary>
        /// 构造Server对象
        /// </summary>
        /// <param name="IpStr">IP地址</param>
        /// <param name="port">端口号</param>
        public Server(string IpStr, int port)
        {
            _controllerManager = new ControllerManager(this);
            SetIpAndPort(IpStr, port);
        }

        /// <summary>
        /// 设置IP地址和端口号
        /// </summary>
        /// <param name="IpStr">IP地址</param>
        /// <param name="port">端口号</param>
        public void SetIpAndPort(string IpStr, int port)
        {
            _ipEndPoint = new IPEndPoint(IPAddress.Parse(IpStr), port);
        }

        /// <summary>
        /// 开启Server服务
        /// </summary>
        public void StartServer()
        {
            _serverSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(_ipEndPoint);
            _serverSocket.Listen(0);
            _serverSocket.BeginAccept(AcceptCallBack,null);
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            //处理对客户端的连接：后续处理单独在Client类中进行处理
            Socket clientSocket = _serverSocket.EndAccept(ar);
            Client client = new Client(clientSocket,this);//构造Client对象时，需要传入与客户端的socket连接，和当前Server对象
            client.StartClient();//客户端开始服务
            _clientList.Add(client);//每接入一个客户端就将其加入列表
            _serverSocket.BeginAccept(AcceptCallBack, null);//回调保证一直可接入客户端
        }
        #endregion
        /// <summary>
        /// 从客户端列表中移除指定客户端
        /// </summary>
        /// <param name="client">要移除的客户端</param>
        public void RemoveClientFromClientList(Client client)
        {
            lock (_clientList)//保证安全，避免多个客户端同时访问时造成的问题
            {
                _clientList.Remove(client);
            }
        }
        /// <summary>
        /// 从房间列表中移除指定房间
        /// </summary>
        /// <param name="room">要移除的房间</param>
        public void RemoveRoomFromRoomList(Room room)
        {
            if (_roomList != null && room != null)
            {
                _roomList.Remove(room);
            }
        }
        /// <summary>
        /// 通过房间id获得房间
        /// </summary>
        /// <param name="roomId">房间id</param>
        /// <returns></returns>
        public Room GetRoomByRoomId(int roomId)
        {
            foreach (Room temp in _roomList)
            {
                //判断房主id和传入的房间id相同
                if (temp.RoomClientList[0].user.Id == roomId)
                {
                    return temp;
                }
            }
            return null;
        }

        #region 处理请求与发送响应
        /// <summary>
        /// 发送响应到客户端
        /// </summary>
        /// <param name="receiverClient"></param>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        public void SendResponseToClient(Client receiverClient,ActionCode actionCode,string data)
        {
            //给客户端响应-具体是在client中进行
            receiverClient.GetResponse(actionCode, data);
        }


        public void HandleRequest(RequestCode requestCode, ActionCode actionCode, string data, Client runThisRequestClient)
        {
            _controllerManager.HandleRequest(requestCode, actionCode, data, runThisRequestClient);
        }
        #endregion
    }
}
