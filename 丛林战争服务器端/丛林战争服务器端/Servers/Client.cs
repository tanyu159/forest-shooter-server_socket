using Common;
using GameServer.DAO;
using GameServer.Model;
using GameServer.Tool;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Servers
{   
    /// <summary>
    /// 处理通信-该client并非真正的客户端，这只是与服务器建立的一条连接的对象，叫做client。而这条连接处理
    /// 对于服务器这一端来说的，收和发
    /// 处理的是来自
    /// </summary>
    class Client
    {

        private Socket _clientSocket;//客户端对象中的Socket对象引用
        private Server _currentServer;//服务器的引用，因为涉及到在关闭连接时，要把其从Server对象中的列表中移除
        private Message _msg = new Message();//定义并初始化一个Message对象，因为数据要保存到该对象中的byte数组中
        private MySqlConnection mysqlConn;//Mysql对象引用
        public User user;//当前玩家
        public Score score;//当前玩家的战绩信息
        public Room currentEnteredRoom;//当前进入的房间

        public int currentHp;//当前血量

        public bool isDie() {
            return currentHp <= 0;
        }
        public MySqlConnection MysqlConn
        {
            get
            {
                return mysqlConn;
            }

            
        }

        public Client() { }

        public Client(Socket _clientSocket,Server currentServer) {
            this._clientSocket = _clientSocket;
            _currentServer = currentServer;
            mysqlConn = ConnHelper.Connet();
        }

        public void StartClient()
        {
            _clientSocket.BeginReceive(_msg.Data,_msg.StartIdx,_msg.RemainSize,SocketFlags.None, ReceiveCallBack,null);
            //消息的具体处理放在Message类中进行处理
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                
                    //接收客户端的数据，并获取消息个数
                    int count = _clientSocket.EndReceive(ar);
                    //Console.WriteLine("count"+count);
                    if (count == 0)
                    {
                        CloseConnectionWithServer();
                    }
                    // 处理接收到的数据-指定委托函数
                    _msg.ReadMessage(count, OnProcessDataCallback);
                if (_clientSocket != null && _clientSocket.Connected)
                {
                    _clientSocket.BeginReceive(_msg.Data, _msg.StartIdx, _msg.RemainSize, SocketFlags.None, ReceiveCallBack, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("出现异常" + e);
                CloseConnectionWithServer();
            }
            finally
            {

            }

           
        }

        /// <summary>
        /// 处理解析之后的消息委托回调函数
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="actionCode"></param>
        /// <param name="dataStr"></param>
        public void OnProcessDataCallback(RequestCode requestCode,ActionCode actionCode,string dataStr)
        {
            //通过Server里面的HandleRequest函数来处理，然后Server又去找ControllerManager去处理
            _currentServer.HandleRequest(requestCode, actionCode, dataStr,this);
        }


        /// <summary>
        /// 断开与服务器的连接
        /// </summary>
        private void CloseConnectionWithServer()
        {
            //先关闭和数据库的连接
            ConnHelper.CloseConnetcion(MysqlConn);
            if (_clientSocket != null)
            {
                //关闭与服务器的连接，并从服务器连接客户端列表中移除
                _clientSocket.Close();
                _currentServer.RemoveClientFromClientList(this);
            }
            if (currentEnteredRoom != null)
            {
                //移除该Client的房间
                currentEnteredRoom.Close(this);
            }
        }

        /// <summary>
        /// 得到响应并发送-这相当于对数据进行包装-并发送至客户端
        /// </summary>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        public void GetResponse(ActionCode actionCode,string data)
        {
            try {
                //数据的包装用Message中的静态方法
                byte[] packge = Message.PackData(actionCode, data);
                //传输至客户端
                _clientSocket.Send(packge);
            } catch (Exception e) {
                Console.WriteLine("发回响应时出现问题,异常信息为" +e);
            }
        }
        /// <summary>
        /// 用于更新玩家的战绩-游戏结束时调用
        /// </summary>
        /// <param name="isWin">赢了吗</param>
        public void UpdateUserScore(bool isWin)
        {
            if (isWin)
            {
                //胜利
                score.TotalCount++;
                score.WinCount++;
                ScoreDAO.UpdateUserScoreRecordByUserid(mysqlConn, user.Id, score.TotalCount, score.WinCount);
               
            }
            else {
                //失败
                score.TotalCount++;
                ScoreDAO.UpdateUserScoreRecordByUserid(mysqlConn, user.Id, score.TotalCount, score.WinCount);
               
            }
            //告知客户端更新现在的显示【房间列表界面左侧的战绩信息】
            string data = score.TotalCount + "#" + score.WinCount;
            _currentServer.SendResponseToClient(this, ActionCode.UpdatePlayerInfo, data);
        }
    }
}
