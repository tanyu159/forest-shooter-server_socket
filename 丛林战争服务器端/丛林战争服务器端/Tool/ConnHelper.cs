using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace GameServer.Tool
{
    /// <summary>
    /// 用于连接MySql数据库
    /// </summary>
    class ConnHelper
    {
        public const string ConnetionStr = "Database=forestwar;Data Source=127.0.0.1;port=3306;User Id=root;Password=660317;SslMode=none;charSet=utf8";
        public static MySqlConnection Connet()
        {
            MySqlConnection conn = new MySqlConnection(ConnetionStr);
            try
            {
                conn.Open();
                return conn;
            }
            catch (Exception e)
            {
                Console.WriteLine("连接数据库失败-出现异常" + e);
                return null;
            }
           
        }

        public static void CloseConnetcion(MySqlConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
            else
            {
                Console.WriteLine("当前连接为空-无法关闭");
            }   
        }
    }
}
