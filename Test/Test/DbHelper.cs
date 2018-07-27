﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal static class DbHelper
    {
        internal static int ExecuteNonQuery(OracleCommand command, string connectionString)
        {
            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<int>(
                    command,
                    cmd => cmd.ExecuteNonQuery()
                    );
            }
        }


        internal static DataTable FillDataTable(OracleCommand command, string connectionString)
        {
            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<DataTable>(
                    command,
                    cmd => {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {

                            //fix bug,必须要创建一个DataSet并把EnforceConstraints设置为False
                            //否则在select * 连接查询时,包含两个时间戳字段将导致默认推断主键.
                            //记录重复后会引发[System.Data.ConstraintException] = {"未能启用约束。一行或多行中包含违反非空、唯一或外键约束的值。"}异常

                            DataSet ds = new DataSet();
                            ds.EnforceConstraints = false;

                            DataTable table = new DataTable("_tb");
                            ds.Tables.Add(table);

                            table.Load(reader);
                            return table;
                        }
                    }
                    );
            }
        }

        internal static DataSet FillDataSet(OracleCommand command, string connectionString)
        {
            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<DataSet>(
                    command,
                    cmd => {
                        DataSet ds = new DataSet();
                        OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                        adapter.Fill(ds);
                        for (int i = 0; i < ds.Tables.Count; i++)
                        {
                            ds.Tables[i].TableName = "_tb" + i.ToString();
                        }
                        return ds;
                    }
                    );
            }
        }

        internal static T ExecuteScalar<T>(OracleCommand command, string connectionString)
        {
            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<T>(
                    command,
                    cmd => ConvertScalar<T>(cmd.ExecuteScalar())
                );
            }
        }

        internal static T ConvertScalar<T>(object obj)
        {
            if (obj == null || DBNull.Value.Equals(obj))
                return default(T);

            if (obj is T)
                return (T)obj;

            Type targetType = typeof(T);

            if (targetType == typeof(object))
                return (T)obj;

            return (T)Convert.ChangeType(obj, targetType);
        }

        internal static List<T> FillScalarList<T>(OracleCommand command, string connectionString)
        {
            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<List<T>>(
                    command,
                    cmd => {
                        List<T> list = new List<T>();
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(ConvertScalar<T>(reader[0]));
                            }
                            return list;
                        }
                    }
                    );
            }
        }

        internal static List<T> ToList<T>(OracleCommand cmd, string connectionString) where T : class, new()
        {
            Type type = typeof(T);

            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(type);

            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<List<T>>(cmd, p => {
                    using (OracleDataReader reader = p.ExecuteReader())
                    {
                        if (description.ExecuteFunc != null)
                        {
                            return description.ExecuteFunc(1, new object[] { reader }) as List<T>;
                        }
                        else if (type.IsSubclassOf(typeof(BaseEntity)))
                            throw BaseEntity.GetNonStandardExecption(type);
                        else
                            return ToList<T>(reader, description);
                    }
                });
            }
        }

        private static List<T> ToList<T>(OracleDataReader reader, TypeDescription description) where T : class, new()
        {
            Type type = typeof(T);

            Dictionary<string, DbMapInfo> dict = description.MemberDict;

            List<T> list = new List<T>();
            string[] names = GetColumnNames(reader);
            while (reader.Read())
            {
                T obj = Activator.CreateInstance(type) as T;
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];

                    DbMapInfo info;
                    if (dict.TryGetValue(name, out info))
                    {
                        object val = reader.GetValue(i);
                        if (val != null && DBNull.Value.Equals(val) == false && (info.AttrIgnore == null || info.AttrIgnore.SelectConvert))
                        {
                            if (info.AttrColumn != null && info.AttrColumn.TimeStamp)
                            {
                                info.PropertyInfo.FastSetValue(obj, val.ConvertToTimeStamp(info.PropertyInfo.PropertyType));
                            }
                            else
                            {
                                info.PropertyInfo.FastSetValue(obj, val.Convert(info.PropertyInfo.PropertyType));
                            }
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        internal static List<T> ToList<T>(DataTable table, TypeDescription description) where T : class, new()
        {
            Type type = typeof(T);

            Dictionary<string, DbMapInfo> dict = description.MemberDict;

            List<T> list = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T obj = Activator.CreateInstance(type) as T;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string name = table.Columns[i].ColumnName;
                    DbMapInfo info;
                    if (dict.TryGetValue(name, out info))
                    {
                        object val = row[i];

                        if (val != null && DBNull.Value.Equals(val) == false && info.AttrIgnore == null)
                        {
                            if (info.AttrColumn != null && info.AttrColumn.TimeStamp)
                            {
                                info.PropertyInfo.FastSetValue(obj, val.ConvertToTimeStamp(info.PropertyInfo.PropertyType));
                            }
                            else
                            {
                                info.PropertyInfo.FastSetValue(obj, val.Convert(info.PropertyInfo.PropertyType));
                            }
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }


        internal static T ToSingle<T>(OracleCommand cmd, string connectionString) where T : class, new()
        {

            Type type = typeof(T);

            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(type);

            using (ConnectionScope scope = new ConnectionScope(connectionString))
            {
                return scope.Current.ExecuteCommand<T>(cmd, p => {
                    using (OracleDataReader reader = p.ExecuteReader())
                    {
                        if (description.ExecuteFunc != null)
                        {
                            return description.ExecuteFunc(2, new object[] { reader }) as T;
                        }
                        else if (type.IsSubclassOf(typeof(BaseEntity)))
                            throw BaseEntity.GetNonStandardExecption(type);
                        else
                            return ToSingle<T>(reader, description);
                    }
                });
            }
        }

        private static T ToSingle<T>(OracleDataReader reader, TypeDescription description) where T : class, new()
        {
            Type type = typeof(T);

            Dictionary<string, DbMapInfo> dict = description.MemberDict;

            if (reader.Read())
            {
                string[] names = GetColumnNames(reader);

                T obj = Activator.CreateInstance(type) as T;
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];

                    DbMapInfo info;
                    if (dict.TryGetValue(name, out info))
                    {
                        object val = reader.GetValue(i);
                        if (val != null && DBNull.Value.Equals(val) == false && (info.AttrIgnore == null || info.AttrIgnore.SelectConvert))
                        {
                            if (info.AttrColumn != null && info.AttrColumn.TimeStamp)
                            {
                                info.PropertyInfo.FastSetValue(obj, val.ConvertToTimeStamp(info.PropertyInfo.PropertyType));
                            }
                            else
                            {
                                info.PropertyInfo.FastSetValue(obj, val.Convert(info.PropertyInfo.PropertyType));
                            }
                        }
                    }
                }
                return obj;
            }
            else
            {
                return default(T);
            }
        }

        internal static string[] GetColumnNames(OracleDataReader reader)
        {
            int count = reader.FieldCount;
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = reader.GetName(i);
            }
            return names;
        }
    }
}
