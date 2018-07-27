using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Test
{
    /// <summary>
    /// DataTable扩展方法
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// 从DataTable获取一个实体列表
        /// </summary>
        /// <param name="table">DataTable实例</param>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>建议不要直接从DataTable返回实体,而是通过CPQuery或者StoreProcedure返回实体</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        ///		<para>下面的代码演示了从DataTable获取一个实体列表的用法</para>
        ///		<code>
        ///		<![CDATA[
        ///		//存储过程中包含两个SELECT语句,返回两个结果集
        ///		DataSet ds = StoreProcedure.Create("usp_GetTestDataType").FillDataSet();
        ///	
        ///		foreach( DataTable table in ds2.Tables ) {
        ///
        ///			//将DataTable转换为实体集合
        ///			List<TestDataType> list = table.ToList<TestDataType>();
        ///		
        ///		}
        ///		]]>
        ///		</code>
        /// </example>
        /// <typeparam name="T">实体类型</typeparam>
        /// <exception cref="ArgumentNullException">table参数为null</exception>
        /// <returns>实体列表</returns>
        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {

            if (table == null)
                throw new ArgumentNullException("table");

            Type type = typeof(T);

            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(type);

            if (description.ExecuteFunc != null)
                try
                {
                    return description.ExecuteFunc(10, new object[] { table }) as List<T>;
                }
                catch (System.Exception ex)
                {
                    //这里不希望调用者看到代码生成器产生的代码结构,于是在这里抛出捕获到的异常
                    throw ex;
                }
            else if (type.IsSubclassOf(typeof(BaseEntity)))
                throw new InvalidProgramException(
                        string.Format("类型 {0} 找不到ToList的操作方法，请确认已将实体类型定义在*.Entity.dll结尾的程序集中，且不是嵌套类，并已提供无参的构造函数。", type.FullName));
            else
                return DbHelper.ToList<T>(table, description);

        }


        /// <summary>
        /// 将泛类型集合List类转换成DataTable
        /// </summary>
        /// <param name="list">泛类型集合</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this List<T> entitys) where T : class, new()
        {
            //检查实体集合不能为空
            if (entitys == null)
            {
                throw new ArgumentNullException("需转换的集合为空");
            }
            //取出第一个实体的所有Propertie
            Type entityType = typeof(T);
            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(entityType);
            Dictionary<string, DbMapInfo> dict = description.MemberDict;


            //生成DataTable的structure
            //生产代码中，应将生成的DataTable结构Cache起来，此处略
            DataTable dt = new DataTable();

            foreach (string key in dict.Keys)
            {
                Type colType = dict[key].PropertyInfo.PropertyType;
                if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    colType = colType.GetGenericArguments()[0];
                }
                dt.Columns.Add(key, colType);
            }
            //将所有entity添加到DataTable中
            foreach (T entity in entitys)
            {
                object[] entityValues = new object[dict.Keys.Count];


                int i = 0;
                foreach (string key in dict.Keys)
                {
                    entityValues[i] = dict[key].PropertyInfo.FastGetValue(entity);
                    i++;
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }
    }
}
