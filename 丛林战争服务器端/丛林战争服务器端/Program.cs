using GameServer.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 丛林战争服务器端
{
    class Program
    {
        static void Main(string[] args)
        {
            //启动服务
            Server gameServer = new Server("127.0.0.1", 6688);
            gameServer.StartServer();

            Console.ReadKey();
        }
    }
}
