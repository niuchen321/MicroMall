/*----------------------------------------------------------------
  
    创建描述：DbHelperSQL操作
----------------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using DD373.Utils;
using DD373.Utils.ReturnValueUtil;
using Newtonsoft.Json;

namespace MyDB.SQLServerUtil
{
    /// <summary>
    /// 数据访问抽象基础类
    /// </summary>
    public class DbHelperSQL
    {
        ////数据库连接字符串		
        public static string connectionString;

        public DbHelperSQL()
        {
            #region 兼容旧版没初始化配置调用
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = PubConstant.ConnectionString;
            }
            #endregion
        }

        static DbHelperSQL()
        {
            #region 兼容旧版没初始化配置调用
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = PubConstant.ConnectionString;
            }
            #endregion
        }

        #region 初始化数据库链接配置
        /// <summary>
        /// 初始化数据库链接配置
        /// </summary>
        /// <param name="connection">数据库链接字符串</param>
        public static void CreateConfiguration(string connection)
        {
            connectionString = SecurityUtil.DecryptStr(connection, "^(*(%((^1(J(J&%G&$$H^%$&OH4s5");
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 判断是否存在某表的某个字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列名称</param>
        /// <returns>是否存在</returns>
        public static bool ColumnExists(string tableName, string columnName)
        {
            string sql = "select count(1) from syscolumns where [id]=object_id('" + tableName + "') and [name]='" + columnName + "'";
            object res = GetSingle(sql);
            if (res == null)
            {
                return false;
            }
            return Convert.ToInt32(res) > 0;
        }
        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }
        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static bool TabExists(string TableName)
        {
            string strsql = "select count(*) from sysobjects where id = object_id(N'[" + TableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
            //string strsql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + TableName + "]') AND type in (N'U')";
            object obj = GetSingle(strsql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();

                        Exception ex = new Exception(e.Message + "  " + SQLString, e);
                        throw ex;
                    }
                }
            }
        }

        public static int ExecuteSqlByTime(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();

                        Exception ex = new Exception(e.Message + "  " + SQLString, e);
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    Exception ex = new Exception(e.Message + "  " + SQLString + " parameter:" + content, e);
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static object ExecuteSqlGet(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(SQLString, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    Exception ex = new Exception(e.Message + "  " + SQLString + " parameter:" + content, e);
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(strSQL, connection);
                System.Data.SqlClient.SqlParameter myParameter = new System.Data.SqlClient.SqlParameter("@fs", SqlDbType.Image);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    Exception ex = new Exception(e.Message + "  " + strSQL, e);
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(CheckSQL(SQLString), connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();

                        Exception ex = new Exception(e.Message + "  " + SQLString, e);
                        throw ex;
                    }
                }
            }
        }

        public static object GetSingle(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(CheckSQL(SQLString), connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();

                        Exception ex = new Exception(e.Message + "  " + SQLString, e);
                        throw ex;
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string strSQL)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(CheckSQL(strSQL), connection);
            try
            {
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                Exception ex = new Exception(e.Message + "  " + strSQL, e);
                throw ex;
            }

        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(CheckSQL(SQLString), connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    Exception ex = new Exception(e.Message + "  " + SQLString, e);
                    throw ex;
                }
                return ds;
            }
        }

        public static DataSet Query(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                    command.SelectCommand.CommandTimeout = Times;
                    command.Fill(ds, "ds");
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    Exception ex = new Exception(e.Message + "  " + SQLString, e);
                    throw ex;
                }
                return ds;
            }
        }



        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        Exception ex = new Exception(e.Message + "  " + SQLString + " parameter:" + SerializeSqlParameters(cmdParms), e);
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static void ExecuteSqlTranIfBack(List<CommandInfo> cmdList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (CommandInfo cmdInfo in cmdList)
                        {
                            string cmdText = cmdInfo.CommandText.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])cmdInfo.Parameters;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            if (val <= 0)
                            {
                                throw new Exception("更新出错");
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        try
                        {
                            trans.Rollback();
                        }
                        catch (SqlException)
                        {

                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static bool ExecuteSqlTranIfBackBool(List<CommandInfo> cmdList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (CommandInfo cmdInfo in cmdList)
                        {
                            string cmdText = cmdInfo.CommandText.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])cmdInfo.Parameters;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            if (val <= 0)
                            {
                                throw new Exception("更新出错:" + JsonConvert.SerializeObject(cmdInfo) + ",参数:" + SerializeSqlParameters(cmdParms));
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        try
                        {
                            trans.Rollback();
                        }
                        catch (SqlException)
                        {

                        }
                        throw;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        Exception ex = new Exception(e.Message + "  " + SQLString + " parameter:" + SerializeSqlParameters(cmdParms), e);
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                Exception ex = new Exception(e.Message + "  " + SQLString + " parameter:" + SerializeSqlParameters(cmdParms), e);
                throw ex;
            }
            //			finally
            //			{
            //				cmd.Dispose();
            //				connection.Close();
            //			}	

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        Exception ex = new Exception(e.Message + "  " + SQLString + " parameter:" + SerializeSqlParameters(cmdParms), e);
                        throw ex;
                    }
                    return ds;
                }
            }
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = CheckSQL(cmdText);
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )  
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataReader returnReader;
            connection.Open();
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return returnReader;

        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }

        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.SelectCommand.CommandTimeout = Times;
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }


        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (SqlParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// 执行存储过程，返回影响的行数		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                //Connection.Close();
                return result;
            }
        }

        /// <summary>
        /// 创建 SqlCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        #endregion

        #region 获得分页通用方法

        /// <summary>
        /// 通用分页方法
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="fldName">主键</param>
        /// <param name="strWhere">查询条件</param>
        /// <param name="orderKey">排序字段</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="orderType">排序方式</param>
        /// <param name="fileds">查询字段</param>
        /// <param name="totalRecords">返回总数</param>
        /// <returns></returns>
        public static DataSet ListPage(string tblName, string fldName, string strWhere, string orderKey, int pageSize, int pageIndex, OrderTypeDesc orderType, string fileds, out int totalRecords)
        {
            IDataParameter[] parameters = {
                new SqlParameter("@tblName", SqlDbType.VarChar, 255){Value = tblName},
                new SqlParameter("@fldName", SqlDbType.VarChar, 255){Value = fldName},
                new SqlParameter("@PageSize", SqlDbType.Int){Value = pageSize},
                new SqlParameter("@PageIndex", SqlDbType.Int){Value = pageIndex},
                new SqlParameter("@IsReCount", SqlDbType.Bit){Value = 0},
                new SqlParameter("@OrderType", SqlDbType.Bit){Value = orderType == OrderTypeDesc.升序 ? 0 : 1},
                new SqlParameter("@strWhere", SqlDbType.VarChar,4000){Value = DbConHelperSQL.CheckSQL(strWhere)},
                new SqlParameter("@ordNames", SqlDbType.VarChar, 255){Value = orderKey},
                new SqlParameter("@fldNames", SqlDbType.VarChar, 4000){Value = fileds},
                new SqlParameter("@TotalRecords",SqlDbType.Int){Direction = ParameterDirection.Output}
            };

            var ds = DbHelperSQL.RunProcedure("UP_GetRecordByPage", parameters, "ds");
            totalRecords = Convert.ToInt32(parameters[9].Value);

            return ds;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="parameters">参数</param>
        /// <param name="orderKey">排序</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="orderType">排序</param>
        /// <param name="fileds">查询字段</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static PagesModel<T> GetPageRecord<T>(string strWhere, SqlParameter[] parameters, string orderKey, int pageSize, int pageIndex, DbHelperSQL.OrderTypeDesc orderType, string fileds, string tableName) where T : class, new()
        {
           
            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 20;
            }

            int totalRecords = 0;
            PagesModel<T> pagesModelList = new PagesModel<T>();
            DataTable dt = GetList(strWhere, parameters, orderKey, pageSize, pageIndex, orderType, fileds, tableName, out totalRecords);

            pagesModelList.PageIndex = pageIndex;
            pagesModelList.PageSize = pageSize;
            pagesModelList.TotalRecord = totalRecords;

            pagesModelList.PageResult = dt != null ? dt.ToModelList<T>() : new List<T>();

            return pagesModelList;
        }

        /// <summary>
        /// 查询分页方法
        /// </summary>
        /// <param name="strWhere">查询条件</param>
        /// <param name="parameters">查询条件参数</param>
        /// <param name="orderKey">排序字段</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="orderType">排序方式</param>
        /// <param name="fileds">查询字段</param>
        /// <param name="tableName">表名</param>
        /// <param name="totalRecords">返回总数</param>
        /// <returns></returns>
        private static DataTable GetList(string strWhere, SqlParameter[] parameters, string orderKey, int pageSize, int pageIndex, DbHelperSQL.OrderTypeDesc orderType, string fileds, string tableName, out int totalRecords)
        {
            StringBuilder strSql = new StringBuilder();
            //获取总记录数
            strSql.AppendFormat("select count(1) as TotalCount from {0} with(nolock) ", tableName);

            if (!string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.AppendFormat(" where {0} ", strWhere);
            }

            //获取分页数据
            strSql.AppendFormat(
                ";select {0} from {1} with(nolock) ", fileds, tableName);
            if (!string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.AppendFormat(" where {0} ", strWhere);
            }
            strSql.AppendFormat(
                " order by {0} {1} offset {2} rows fetch next {3} rows only", orderKey,
                orderType == DbHelperSQL.OrderTypeDesc.升序 ? "asc" : "desc", (pageIndex - 1) * pageSize, pageSize);

            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);

            totalRecords = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalCount"]);

            return ds.Tables[1];
        }
        #endregion

        #region 防注入

        /// <summary>
        /// 防注入字符串-单词
        /// </summary>
        public static string SQL_String = "insert|delete|select|update|like|where|drop|and|exec|count|chr|mid|master|truncate|declare|case|varchar|nvarchar|char|nchar|ntext|text|int|fetch|deallocate|convert|trim|set|0x";

        public static string CheckSQL(string SQLString)
        {

            string[] str = SQL_String.Split('|');

            string isOK = SQLString;

            try
            {
                Regex _Regex = new Regex("'[^']+?'");
                MatchCollection _MatchCollection = _Regex.Matches(SQLString);

                foreach (Match math in _MatchCollection)
                {
                    string val = math.Value;

                    foreach (string s in str)
                    {
                        if (val.ToLower().IndexOf(s.ToLower()) >= 0)
                            isOK = isOK.Replace(val, val.Replace(s, "%"));
                    }
                }
            }
            catch { }

            return isOK;
        }

        #endregion

        /// <summary>
        /// 排序
        /// </summary>
        public enum OrderTypeDesc
        {
            /// <summary>
            /// 降序
            /// </summary>
            降序 = 1,

            /// <summary>
            /// 升序
            /// </summary>
            升序 = 0
        }

        #region 参数字典对转SqlParameter
        /// <summary>
        /// 参数字典对转SqlParameter
        /// </summary>
        /// <param name="dicParameters">参数字典对</param>
        /// <returns>SqlParameter数组</returns>
        public static SqlParameter[] GetSqlParameter(Dictionary<string, object> dicParameters)
        {
            SqlParameter[] parameters = null;
            if (dicParameters != null)
            {
                parameters = new SqlParameter[dicParameters.Count];
                int i = 0;
                foreach (var para in dicParameters)
                {
                    parameters[i] = new SqlParameter(para.Key, para.Value);

                    i++;
                }
            }
            return parameters;
        }

        /// <summary>
        /// 序列化sql参数值
        /// </summary>
        /// <param name="parameters">sql参数数组</param>
        /// <returns></returns>
        public static string SerializeSqlParameters(SqlParameter[] parameters)
        {
            if (parameters == null || parameters.Length <= 0)
            {
                return "";
            }

            List<object> objList = new List<object>();

            foreach (SqlParameter parameter in parameters)
            {
                object obj = new { parameter.ParameterName, parameter.Value };
                objList.Add(obj);
            }

            return JsonConvert.SerializeObject(objList);
        }
        #endregion
    }
}
