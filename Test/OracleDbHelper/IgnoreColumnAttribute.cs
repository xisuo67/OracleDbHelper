using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleDbHelper
{
    /// <summary>
    /// 表示忽略指定的数据列。
    /// </summary>
    /// <example>
    ///		<para>下面的代码演示了略指定的数据列的用法</para>
    ///		<code>
    ///		public class cbContract{
    ///			//这个属性将不会被数据访问层处理
    ///			[IgnoreColumn]
    ///			public string MyContractCode { get; set; }
    ///		}
    ///		</code>
    /// </example>
    [AttributeUsageAttribute(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreColumnAttribute : Attribute
    {
        /// <summary>
        /// 查询时是否对忽略字段进行实体转换
        /// </summary>
        public bool SelectConvert { get; set; }
    }
}
