/*----------------------------------------------------------------

    创建描述：Redis操作
----------------------------------------------------------------*/
using System;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace MyDB.RedisUtil
{
    /// <summary>
    /// ConnectionMultiplexer对象管理帮助类
    /// </summary>
    public static class RedisConnectionHelp
    {
        /// <summary>
        /// 系统自定义Key前缀
        /// </summary>
        public static string SysCustomKey;

        /// <summary>
        /// redis连接字符串
        /// </summary>
        public static string RedisConnectionString;

        /// <summary>
        /// 锁
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// 单例获取变量
        /// </summary>
        private static ConnectionMultiplexer _instance;

        /// <summary>
        /// 缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        #region 初始化Redis链接配置
        /// <summary>
        /// 初始化Redis链接配置
        /// </summary>
        /// <param name="redisConnectionString">Redis链接字符串</param>
        /// <param name="sysCustomKey">Key前缀</param>
        public static void CreateConfiguration(string redisConnectionString, string sysCustomKey)
        {
            RedisConnectionString = redisConnectionString;
            SysCustomKey = sysCustomKey;
        }
        #endregion

        /// <summary>
        /// 单例获取
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            _instance = GetManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 缓存获取
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnectionMultiplexer(string connectionString)
        {
            if (!ConnectionCache.ContainsKey(connectionString))
            {
                ConnectionCache[connectionString] = GetManager(connectionString);
            }
            return ConnectionCache[connectionString];
        }

        private static ConnectionMultiplexer GetManager(string connectionString = null)
        {
            connectionString = connectionString ?? RedisConnectionString;
            var connect = ConnectionMultiplexer.Connect(connectionString);
			connect.PreserveAsyncOrder = false;

            //注册如下事件
            connect.ConnectionFailed += MuxerConnectionFailed;
            connect.ErrorMessage += MuxerErrorMessage;
            connect.InternalError += MuxerInternalError;

            return connect;
        }

        #region 事件

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            _instance = null;

            throw new Exception(e.Message);
        }

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            _instance = null;

            throw e.Exception;
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            _instance = null;

            throw e.Exception;
        }

        #endregion 事件
    }
}
