using GameServer.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DAO
{
    class ScoreDAO
    {
        /// <summary>
        /// 通过userid获得战绩-登录成功显示战绩时使用
        /// </summary>
        /// <param name="conn">mysql的连接</param>
        /// <param name="userid">用户id</param>
        /// <returns></returns>
        public static Score GetScoreByUserId(MySqlConnection conn, int userid)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM score WHERE userid=@userid", conn);
                cmd.Parameters.AddWithValue("userid", userid);             
                reader = cmd.ExecuteReader();
                //判断是否读到了记录，若有就将该记录的值提取后生成个User对象进行返回。否则返回null
                if (reader.Read())
                {
                    //将该记录的各属性值取出来构造Score对象返回
                    int id = reader.GetInt32("id");
                    int uid = reader.GetInt32("userid");
                    int totalCount = reader.GetInt32("totalcount");
                    int winCount = reader.GetInt32("wincount");
                    //关闭reader
                    reader.Close();
                    Score score = new Score(id, uid, totalCount, winCount);
                    return score;
                }
                else
                {
                    //【安全保障】未查询到说明在注册的时候建战绩表失败，但依然要返回记录，数据写0即可
                    //关闭reader
                    reader.Close();
                    Score noScore = new Score(-1, userid, 0, 0);
                    return noScore;
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
                Console.WriteLine("GetScoreByUserId根据用户id获取战绩时出现异常,异常信息为：" + e);
                return null;

            }
        }
        /// <summary>
        /// 通过userid创建战绩表的记录-注册成功时使用
        /// </summary>
        /// <param name="conn">mysql的连接</param>
        /// <param name="userid">用户id</param>
        public static void CreatUserScoreRecordByUserid(MySqlConnection conn, int userid)
        {
            try
            {
                string insertSqlStr = "INSERT INTO score(userid) VALUES(@userid)";
                MySqlCommand cmd = new MySqlCommand(insertSqlStr, conn);
                cmd.Parameters.AddWithValue("userid", userid);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("在创建用户战绩表时发生异常，异常信息为："+e);
            }
        }
        /// <summary>
        /// 通过userid更新战绩表-游戏结束时使用
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="userid"></param>
        /// <param name="totalCount"></param>
        /// <param name="winCount"></param>
        public static void UpdateUserScoreRecordByUserid(MySqlConnection conn, int userid,int totalCount,int winCount)
        {
            try
            {
                string updateSqlStr = "UPDATE score SET totalcount=@totalCount,wincount=@winCount WHERE userid=@userid";
                MySqlCommand cmd = new MySqlCommand(updateSqlStr, conn);
                cmd.Parameters.AddWithValue("userid", userid);
                cmd.Parameters.AddWithValue("totalCount", totalCount);
                cmd.Parameters.AddWithValue("winCount", winCount);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("更新玩家战绩时出现异常,异常信息为" + e);
            }
        }

    }
}
