/*----------------------------------------------------------------
 
    创建描述：DataPages操作
----------------------------------------------------------------*/

using System;
using System.Configuration;
using System.Runtime.Serialization.Formatters;
using DD373.Utils;

namespace MyDB.SQLServerUtil
{
    /// <summary>
    ///  
    /// </summary>
    public class PubConstant
    {
        /// <summary>
        /// 获取连接字符串（常用）
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return GetUnEncryptConnStr("webconnstring");
            }
        }

        /// <summary>
        /// 获得只读连接字符串方法，字符串启用加密时，解密处理
        /// </summary>
        /// <param name="appSettingKey">只读连接字符串配置名称</param>
        /// <returns>只读连接字符</returns>
        public static string DbConConnectionString(string appSettingKey)
        {
            return GetUnEncryptConnStr(appSettingKey);
        }

        /// <summary>
        /// 获得解密后连接字符串信息
        /// </summary>
        /// <param name="appSettingKey">待处理连接字符串名称</param>
        /// <returns>解密后连接字符串信息</returns>
        private static string GetUnEncryptConnStr(string appSettingKey)
        {
            string connectionString = ConfigurationManager.AppSettings[appSettingKey];
            if (string.IsNullOrEmpty(connectionString))
                return "";

            return SecurityUtil.DecryptStr(connectionString, "^(*(%((^1(J(J&%G&$$H^%$&OH4s5");
        }

        /// <summary>
        /// 获得解密的数据库连接字符串方法
        /// </summary>
        /// <param name="dbConnectionDecryptString">加密数据库连接字符串</param>
        /// <returns>解密后的数据库连接字符串</returns>
        public static string GetDbConConnectionString(string dbConnectionDecryptString)
        {
            return SecurityUtil.DecryptStr(dbConnectionDecryptString, "^(*(%((^1(J(J&%G&$$H^%$&OH4s5");
        }
    }
}
