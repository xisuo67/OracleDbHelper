using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class TypeDescription
    {
        public Dictionary<string, DbMapInfo> MemberDict { get; set; }

        public Func<int, object[], object> ExecuteFunc { get; set; }
    }


    internal class TypeDescriptionCache
    {

        private static Hashtable s_typeInfoDict = Hashtable.Synchronized(new Hashtable(2048));

        private static BindingFlags s_flag = BindingFlags.Instance | BindingFlags.Public;

        public static TypeDescription GetTypeDiscription(Type type)
        {
            TypeDescription description = s_typeInfoDict[type.FullName] as TypeDescription;
            if (description == null)
            {

                PropertyInfo[] properties = type.GetProperties(s_flag);
                int length = properties.Length;
                Dictionary<string, DbMapInfo> dict = new Dictionary<string, DbMapInfo>(length, StringComparer.OrdinalIgnoreCase);

                foreach (PropertyInfo prop in properties)
                {

                    DbMapInfo info = null;

                    DataColumnAttribute attrColumn = prop.GetMyAttribute<DataColumnAttribute>();
                    IgnoreColumnAttribute attrIgnore = prop.GetMyAttribute<IgnoreColumnAttribute>();

                    if (attrColumn != null)
                    {
                        info = new DbMapInfo(string.IsNullOrEmpty(attrColumn.Alias) ? prop.Name : attrColumn.Alias, prop.Name, attrColumn, attrIgnore, prop);
                    }
                    else
                    {
                        info = new DbMapInfo(prop.Name, prop.Name, null, attrIgnore, prop);
                    }

                    dict[info.DbName] = info;
                }

                description = new TypeDescription { MemberDict = dict };

                // 添加到缓存字典
                s_typeInfoDict[type.FullName] = description;
            }

            return description;
        }

        public static void SaveComplieResult(Type type, TypeDescription description)
        {
            s_typeInfoDict[type.FullName] = description;
        }
    }
}
