/*----------------------------------------------------------------

    创建描述：数据库操作异步封装类，轻量级Map ，代码参考Dapper和 meta两个项目
----------------------------------------------------------------*/
using MyDB.SQLServerUtil;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
namespace MyDB.DataHelper
{
    /// <summary>
    /// IDataRecord基础元数据解析方法扩展
    /// </summary>
    public static class DataRecordExtensions
    {
        public static async Task<List<T>> LoadToList<T>(this SqlDataReader dataReader, TableMap<T> tableMap) where T : new()
        {
            List<T> list = new List<T>();
            if (!dataReader.HasRows)
            {
                return list;
            }
            tableMap.CreateOrdinal(dataReader);
            while (await dataReader.ReadAsync().ConfigureAwait(false))
            {
                var item = tableMap.LoadItem(dataReader);
                list.Add(item);
            }
            return list;
        }
        public static bool? GetNullableBoolean(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (bool?)reader.GetBoolean(ordinal);
        }

        public static bool GetBoolean(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(bool) : reader.GetBoolean(ordinal);
        }

        public static byte? GetNullableByte(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (byte?)reader.GetByte(ordinal);
        }

        public static byte GetByte(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(byte) : reader.GetByte(ordinal);
        }

        public static short? GetNullableInt16(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (short?)reader.GetInt16(ordinal);
        }

        public static short GetInt16(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(short) : reader.GetInt16(ordinal);
        }

        public static int? GetNullableInt32(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (int?)reader.GetInt32(ordinal);
        }

        public static int GetInt32(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(int) : reader.GetInt32(ordinal);
        }

        public static long? GetNullableInt64(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (long?)reader.GetInt64(ordinal);
        }

        public static long GetInt64(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(long) : reader.GetInt64(ordinal);
        }

        public static char? GetNullableChar(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (char?)reader.GetChar(ordinal);
        }

        public static char GetChar(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(char) : reader.GetChar(ordinal);
        }

        //public static string GetNullableString(this IDataRecord reader, int ordinal)
        //{
        //    return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        //}

        public static string GetString(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static DateTime? GetNullableDateTime(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (DateTime?)reader.GetDateTime(ordinal);
        }

        public static DateTime GetDateTime(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(DateTime) : reader.GetDateTime(ordinal);
        }

        public static decimal? GetNullableDecimal(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (decimal?)reader.GetDecimal(ordinal);
        }

        public static decimal GetDecimal(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(decimal) : reader.GetDecimal(ordinal);
        }

        public static float? GetNullableFloat(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (float?)reader.GetFloat(ordinal);
        }

        public static float GetFloat(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(float) : reader.GetFloat(ordinal);
        }

        public static double? GetNullableDouble(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (double?)reader.GetDouble(ordinal);
        }

        public static double GetDouble(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(double) : reader.GetDouble(ordinal);
        }

        public static Guid? GetNullableGuid(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (Guid?)reader.GetGuid(ordinal);
        }

        public static Guid GetGuid(this IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? default(Guid) : reader.GetGuid(ordinal);
        }

        //public static DateTimeOffset? GetNullableDateTimeOffset(this ISqlDataRecord reader, int ordinal)
        //{
        //    return reader.IsDBNull(ordinal) ? null : (DateTimeOffset?)reader.GetDateTimeOffset(ordinal);
        //}

        //public static DateTimeOffset GetDateTimeOffset(ISqlDataRecord reader, int ordinal)
        //{
        //    return reader.GetDateTimeOffset(ordinal);
        //}
    }

    /// <summary>
    /// T和数据库关系映射
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    public class TableMap<T> where T : new()
    {
        public class ColumnDefination
        {
            public string ColumnName { get; set; }

            public int Ordinal { get; set; }
        }

        #region private 字段
        /// <summary>
        /// 延迟加载
        /// </summary>
        private static readonly Lazy<Dictionary<Type, MethodInfo>> DataRecordGetMethodsData =
            new Lazy<Dictionary<Type, MethodInfo>>(LoadDataRecordGetMethods);
        #endregion


        #region public  字段
        public static Dictionary<Type, MethodInfo> DataRecordGetMethods => DataRecordGetMethodsData.Value;

        /// <summary>
        /// T的属性和数据库列的关系映射
        /// </summary>
        public Dictionary<MemberInfo, ColumnDefination> FieldMapDefinitions { get; } =
         new Dictionary<MemberInfo, ColumnDefination>();
        #endregion


        public TableMap()
        {
            //
            CreateDefaultMap();
        }

        /// <summary>
        /// 创建默认的映射关系，属性名对应列名
        /// </summary>
        private void CreateDefaultMap()
        {
            var properties = typeof(T).GetProperties()
                                      .Where(property => IsValidType(property.PropertyType));
            foreach (var item in properties)
            {
                FieldMapDefinitions.Add(item, new ColumnDefination() { ColumnName = item.Name });
            }

        }

        /// <summary>
        /// 用户自定义关系映射
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="columnName">数据库中的列名</param>
        /// <returns>本条表达式解析后的this</returns>
        public TableMap<T> Map(Expression<Func<T, object>> expression, string columnName = null)
        {
            var destinationProperty = GetProperty(expression);

            if (!string.IsNullOrEmpty(columnName))
            {
                FieldMapDefinitions[destinationProperty].ColumnName = columnName;
            }

            return this;
        }

        /// <summary>
        /// 从IDataRecord中加载 Model
        /// </summary>
        /// <param name="dataRecord">数据库读取的IDataRecord</param>
        /// <returns>Model</returns>
        public T LoadItem(IDataRecord dataRecord)
        {
            T result = new T();
            foreach (var memberInfo in FieldMapDefinitions.Keys)
            {
                var columnDefination = FieldMapDefinitions[memberInfo];
                if (columnDefination.Ordinal == -1)
                {
                    continue;
                }

                if (memberInfo is PropertyInfo)
                {
                    var propertyInfo = (memberInfo as PropertyInfo);
                    var methodInfo = DataRecordGetMethods[propertyInfo.PropertyType];
                    var propertyValue = methodInfo.Invoke(dataRecord, new object[] { dataRecord, columnDefination.Ordinal });
                    propertyInfo.SetValue(result, propertyValue);
                }
            }

            return result;
        }

        /// <summary>
        /// 创建IDataRecord对应列的索引
        /// </summary>
        /// <param name="dataRecord">IDataRecord</param>
        public void CreateOrdinal(IDataRecord dataRecord)
        {
            foreach (var item in FieldMapDefinitions.Keys)
            {
                var value = FieldMapDefinitions[item];
                value.Ordinal = GetOrdinal(dataRecord, value.ColumnName);
            }
        }


        #region private static method
        /// <summary>
        /// 是否是合法的数据库类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否合法</returns>
        private static bool IsValidType(Type type)
        {
            return DataRecordGetMethods.ContainsKey(type);
        }

        /// <summary>
        /// 延迟加载注册的方法
        /// </summary>
        /// <returns>Dictionary[Type, MethodInfo]</returns>
        private static Dictionary<Type, MethodInfo> LoadDataRecordGetMethods()
        {
            var type = typeof(DataRecordExtensions);
            var methods = new Dictionary<Type, MethodInfo>
                              {
                                  { typeof(bool), type.GetMethod("GetBoolean") },
                                  { typeof(byte), type.GetMethod("GetByte") },
                                  { typeof(short), type.GetMethod("GetInt16") },
                                  { typeof(int), type.GetMethod("GetInt32") },
                                  { typeof(long), type.GetMethod("GetInt64") },
                                  { typeof(char), type.GetMethod("GetChar") },
                                  { typeof(string), type.GetMethod("GetString") },
                                  { typeof(DateTime), type.GetMethod("GetDateTime") },
                                  { typeof(decimal), type.GetMethod("GetDecimal") },
                                  { typeof(float), type.GetMethod("GetFloat") },
                                  { typeof(double), type.GetMethod("GetDouble") },
                                  { typeof(Guid), type.GetMethod("GetGuid") },
                                { typeof(bool?), type.GetMethod("GetNullableBoolean") },
                                  { typeof(byte?), type.GetMethod("GetNullableByte") },
                                  { typeof(short?), type.GetMethod("GetNullableInt16") },
                                  { typeof(int?), type.GetMethod("GetNullableInt32") },
                                  { typeof(long?), type.GetMethod("GetNullableInt64") },
                                  { typeof(char?), type.GetMethod("GetNullableChar") },
                                  { typeof(DateTime?), type.GetMethod("GetNullableDateTime") },
                                  { typeof(decimal?), type.GetMethod("GetNullableDecimal") },
                                  { typeof(float?), type.GetMethod("GetNullableFloat") },
                                  { typeof(double?), type.GetMethod("GetNullableDouble") },
                                  { typeof(Guid?), type.GetMethod("GetNullableGuid") }
                              };
            return methods;
        }

        /// <summary>
        /// 根据列名获取索引
        /// </summary>
        /// <param name="dataRecord">IDataRecord</param>
        /// <param name="columnName">列名</param>
        /// <returns>索引</returns>
        private static int GetOrdinal(IDataRecord dataRecord, string columnName)
        {

            try
            {
                return dataRecord.GetOrdinal(columnName);
            }
            catch (Exception ex)
            {

                return -1;
            }
        }

        /// <summary>
        /// 解析表达式树
        /// </summary>
        /// <typeparam name="TValue">泛型T</typeparam>
        /// <param name="selector">表达式树</param>
        /// <returns>解析出来的属性PropertyInfo</returns>
        private static PropertyInfo GetProperty<TValue>(Expression<Func<T, TValue>> selector)
        {
            var lambda = selector as LambdaExpression;
            var unaryExpression = lambda?.Body as UnaryExpression;
            var operand = unaryExpression?.Operand;

            var member = operand ?? lambda?.Body;
            if (member?.NodeType == ExpressionType.MemberAccess)
            {
                var memberInfo = ((MemberExpression)member).Member;
                if (memberInfo is PropertyInfo)
                {
                    return (PropertyInfo)memberInfo;
                }

            }

            throw new InvalidOperationException("无效的表达式,必须为属性，不能为字段");
        }
        #endregion
    }


    /// <summary>
    /// 数据库查询异步帮助类
    /// </summary>
    public class DbHelperSQLAsync : DbHelperSQL
    {
        /// <summary>
        /// 执行查询语句，返回List[T]
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="tableMap">table 映射关系</param>
        /// <param name="cmdParms">SqlParameter</param>
        /// <returns>List[T]</returns>
        public static async Task<List<T>> QueryAsync<T>(string SQLString, TableMap<T> tableMap = null, params SqlParameter[] cmdParms) where T : new()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    using (SqlDataReader dataReader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {

                        try
                        {
                            tableMap = tableMap ?? new TableMap<T>();
                            return await dataReader.LoadToList(tableMap).ConfigureAwait(false);
                        }
                        catch (System.Data.SqlClient.SqlException e)
                        {
                            Exception ex = new Exception(e.Message + "  " + SQLString, e);
                            throw ex;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="SQLString">SQLString</param>
        /// <param name="cmdParms">cmdParms</param>
        /// <returns>Task{object}</returns>
        public static async Task<object> ExecuteScalarAsync(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    try
                    {
                        return await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        Exception ex = new Exception(e.Message + "  " + SQLString, e);
                        throw ex;
                    }
                }
            }
        }


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
        /// <returns>Task{Tuple{List{T}, int}}}</returns>
        public static async Task<Tuple<List<T>, int>> ListPageAsync<T>(string tblName, string fldName, string strWhere, string orderKey, int pageSize, int pageIndex, OrderTypeDesc orderType, string fileds, TableMap<T> tableMap = null) where T : new()
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
            List<T> list = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = BuildQueryCommand(connection, "UP_GetRecordByPage", parameters))
                {
                    //using (SqlDataReader dataReader = command.ExecuteReader())
                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        try
                        {


                            tableMap = tableMap ?? new TableMap<T>();
                            list = await dataReader.LoadToList(tableMap).ConfigureAwait(false);

                            //return tuple;
                        }
                        catch (System.Data.SqlClient.SqlException e)
                        {
                            Exception ex = new Exception(e.Message + "UP_GetRecordByPage", e);
                            throw ex;
                        }
                    }
                }
            }

            int totalRecords = Convert.ToInt32(parameters[9].Value);
            Tuple<List<T>, int> tuple = new Tuple<List<T>, int>(list, totalRecords);

            return tuple;


        }

        /// <summary>
        /// 非查询语句执行
        /// </summary>
        /// <param name="SQLString">SQLString</param>
        /// <param name="cmdParms">cmdParms</param>
        /// <returns>Task{int}</returns>
        public static async Task<int> ExecuteNonQueryAsync(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    try
                    {
                        return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        Exception ex = new Exception(e.Message + "  " + SQLString, e);
                        throw ex;
                    }
                }
            }
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = DbHelperSQL.CheckSQL(cmdText); ;
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


        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
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
    }
}
