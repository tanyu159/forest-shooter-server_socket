using GameServer.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DAO
{
    /// <summary>
    /// User表和UserController所对应的数据库操作
    /// </summary>
    class UserDAO
    {
        /// <summary>
        /// 验证用户
        /// </summary>
        public static User VerifyUser(MySqlConnection mysqlConn, string username, string password)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM user WHERE username=@username AND password=@password", mysqlConn);
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);
                reader = cmd.ExecuteReader();
                //判断是否读到了记录，若有就将该记录的值提取后生成个User对象进行返回。否则返回null
                if (reader.Read())
                {
                    //将该记录的各属性值取出来构造User对象返回
                    int id = reader.GetInt32("id");
                    string usernameStr = reader.GetString("username");
                    string passwordStr = reader.GetString("password");
                    //关闭reader
                    reader.Close();
                    User user = new User(id, usernameStr, passwordStr);
                    return user;
                }
                else
                {
                    //关闭reader
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e)
            {
                //关闭reader
                if (reader != null)
                {
                    reader.Close();
                }
                //显示异常信息
                Console.WriteLine("VeriftyUser验证用户时发生异常,异常信息为："+e);
                return null;

            }
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="mysqlConn"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool RegisterUser(MySqlConnection mysqlConn, string username, string password)
        {
            try
            {
                //先查有无该记录，无该记录再进行插入
                string selectSqlStr = "SELECT * FROM user WHERE username=@username";
                MySqlCommand cmd = new MySqlCommand(selectSqlStr, mysqlConn);
                cmd.Parameters.AddWithValue("username", username);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    return false;//代表查到了该username对应的记录，不能注册，直接返回法拉瑟
                }
                //否则没有该记录-可以插入
                reader.Close();
                string insertSqlStr = "INSERT INTO user(username,password) VALUES(@username,@password)";
                cmd = new MySqlCommand(insertSqlStr, mysqlConn);
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);
                cmd.ExecuteNonQuery();
                //建立用户的数据-创建该用户的战绩表
                //先按这个用户名再查一次，得到用户的id
                cmd = new MySqlCommand(selectSqlStr, mysqlConn);
                cmd.Parameters.AddWithValue("username", username);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int userid= reader.GetInt32("id");
                    reader.Close();
                    //建立用户战绩表
                    ScoreDAO.CreatUserScoreRecordByUserid(mysqlConn, userid);
                    
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("在执行RegisterUser时出现异常。异常信息为" + e);
                return false;
            }
       }
    }
}
