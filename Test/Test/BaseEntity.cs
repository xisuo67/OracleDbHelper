using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// 数据实体的基类
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>本类是所有数据实体的基类,数据实体需要继承自本类,才能在初始化时自动编译</description></item>
    /// <item><description>初始化时自动编译实体信息请参见:<see cref="Initializer"/>类</description></item>
    /// <item><description>实体类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para>下面的代码演示了从BaseEntity继承的实体</para>
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Linq;
    /// using System.Text;
    /// using Citms.Data.Extensions.DAL; //引入数据访问层命名空间
    ///
    /// namespace SmokingTest.CS.Entity
    /// {
    ///     //实体cbContract继承自BaseEntity
    ///     public class cbContract : BaseEntity
    ///     {
    ///         public string ContractName { get; set; }
    ///         public decimal HtAmount { get; set; }
    /// 		//....其他属性
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public abstract class BaseEntity
    {
        private List<string> _changedProperties = null;
        private Func<CPQuery, bool> _funcBefore;

        internal Func<CPQuery, bool> FuncBefore
        {
            get { return _funcBefore; }
        }

        internal static System.Exception GetNonStandardExecption(Type entryType)
        {
            return new NonStandardExecption(
                        string.Format("类型 {0} 不能执行指定的操作，因为它的定义不符合规范。请确认已将此类型定义在 {1} 结尾的程序集中，且不是嵌套类，并已提供无参的构造函数。",
                        entryType.FullName, BuildManager.EntityAssembly));
        }

        internal CPQuery GetCPQuery(int flag, params object[] parameters)
        {
            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(GetType());
            if (description.ExecuteFunc == null)
            {
                throw GetNonStandardExecption(this.GetType());
            }

            try
            {
                return description.ExecuteFunc(flag, parameters) as CPQuery;
            }
            catch (System.Exception ex)
            {
                //这里不希望调用者看到代码生成器产生的代码结构,于是在这里抛出捕获到的异常
                throw ex;
            }
        }

        /// <summary>
        /// 表示数据库操作执行前的切入函数
        /// </summary>
        /// <param name="func">表示执行前可以由外部指定的行为,返回true则继续执行,返回false则不会继续执行数据库操作</param>
        public void HookExecute(Func<CPQuery, bool> func)
        {
            if (_funcBefore == null)
            {
                _funcBefore = func;
            }
            else
            {
                throw new InvalidOperationException("在一个对象实例中,BeforeExecute方法只能调用一次。");
            }
        }

        /// <summary>
        /// 将数据实体插到对应的数据库表中。
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了Insert()方法的用法</para>
        /// <code>
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// Contract contract = new Contract();
        /// 
        /// contract.ContractGUID = Guid.NewGuid();
        /// contract.ContractName = "...";
        /// //...其他字段
        /// 
        /// int count = contract.Insert();
        /// //插入成功后,count等于1
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">1.如果没有对实体类的任何一个字段赋值,就进行Insert()操作,则会抛出此异常2.类没有定义主键,即没有任何一个属性被标记为PrimaryKey=true,则抛出此异常</exception>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public virtual int Insert(string connectionString = null)
        {
            CPQuery query = GetCPQuery(3, new object[] { this, connectionString });
            if (query == null)
                throw new InvalidOperationException("传入对象不能生成有效的SQL语句。");

            if (_funcBefore != null)
            {
                if (_funcBefore(query) == false)
                {
                    return -1;
                }
            }

            return query.ExecuteNonQuery();
        }

        /// <summary>
        /// 获取Insert对应SQL
        /// </summary>
        /// <returns>Insert对应SQL</returns>
        public virtual CPQuery GetInsertQuery()
        {
            CPQuery query = GetCPQuery(3, new object[] { this, null });
            if (query == null)
                throw new InvalidOperationException("传入对象不能生成有效的SQL语句。");
            query.GetCommand();
            return query;
        }

        public virtual object CloneMe()
        {

            object obj = GetCPQuery(13, new object[] { this });
            if (obj == null)
                throw new InvalidOperationException("传入对象不能克隆");

            return obj;
        }


        /// <summary>
        /// 获取查询SQL
        /// </summary>
        /// <returns>查询SQL</returns>
        public virtual string GetSelectSQL()
        {
            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(GetType());
            if (description.ExecuteFunc == null)
            {
                throw GetNonStandardExecption(this.GetType());
            }
            try
            {
                return description.ExecuteFunc(15, null) as string;
            }
            catch (System.Exception ex)
            {
                //这里不希望调用者看到代码生成器产生的代码结构,于是在这里抛出捕获到的异常
                throw ex;
            }
        }

        /// <summary>
        /// 将数据实体对应的记录从数据库表中删除。
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了Delete()方法的用法</para>
        /// <code>
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// Contract contract = new Contract();
        /// 
        /// //对主键字段进行赋值
        /// contract.ContractGUID = Request.QueryString["ContractGUID"];
        /// 
        /// int count = contract.Delete();
        /// //删除成功后,count等于1
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">类没有定义主键,即没有任何一个属性被标记为PrimaryKey=true,则抛出此异常</exception>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public virtual int Delete(string connectionString = null)
        {
            CPQuery query = GetCPQuery(4, new object[] { this, connectionString });

            if (_funcBefore != null)
            {
                if (_funcBefore(query) == false)
                {
                    return -1;
                }
            }

            return query.ExecuteNonQuery();
        }

        /// <summary>
        /// 将数据实体对应的记录从数据库表中虚拟删除,将删除标记标识为1。
        /// 实体
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了VirtualDelete()方法的用法</para>
        /// <code>
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// Contract contract = new Contract();
        /// 
        /// //对主键字段进行赋值
        /// contract.ContractGUID = Request.QueryString["ContractGUID"];
        /// 
        /// int count = contract.VirtualDelete();
        /// 虚拟删除成功后,count等于1，VirtualDelete字段更新为1
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">类没有定义主键或者虚拟删除字段,即没有任何一个属性被标记为PrimaryKey=true,VirtualDelete=true则抛出此异常</exception>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public virtual int VirtualDelete(string connectionString = null)
        {
            CPQuery query = GetCPQuery(16, new object[] { this, connectionString });

            if (_funcBefore != null)
            {
                if (_funcBefore(query) == false)
                {
                    return -1;
                }
            }

            return query.ExecuteNonQuery();
        }

        /// <summary>
        /// 用并发检测的方式，将数据实体对应的记录从数据库表中删除。
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了并发检测模式下,Delete()方法的用法</para>
        /// <code>
        /// <![CDATA[
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// 
        /// public void Load(){
        /// 
        ///     //在页面加载时,查询数据库信息,时间戳字段需要通过CAST转换为长整型,绑定到界面中
        ///     Contract contract = CPQuery.From("SELECT ContractGUID, CAST(ContractVersion AS BigInt) ContractVersion  .... FROM cb_Contract WHERE ...").ToSingle<Contract>();
        ///     
        ///     //其他数据绑定代码
        ///     //...
        /// }
        /// 
        /// //删除通道,前端需要传递合同GUID,时间戳字段
        /// public void Delete(Guid contractGUID, long contractVersion){
        /// 
        ///		//删除动作,需要构建一个实体对象
        ///		Contract contract = new Contract();
        ///		contract.ContractGUID = contractGUID; //主键必须赋值,这是删除语句的首要条件
        ///		contract.ContractVersion = contractVersion.Int64ToTimeStamp(); //界面中长整型的时间戳字段可以通过Int64ToTimeStamp扩展方法转换为byte[]数组
        ///		
        ///		try{
        ///			//根据时间戳字段,进行并发检测
        ///			int count = contract.Delete(ConcurrencyMode.TimeStamp);
        ///			//如果删除成功,则count为1		
        /// 
        ///			//根据原始值,进行并发检测
        ///			//count = contract.Delete(oldContract, ConcurrencyMode.OriginalValue);
        ///		}
        ///		catch(OptimisticConcurrencyException ex){
        ///			//并发检测失败,将会抛出OptimisticConcurrencyException异常
        ///		}
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <exception cref="InvalidOperationException">类没有定义主键,即没有任何一个属性被标记为PrimaryKey=true,则抛出此异常</exception>
        /// <exception cref="Citms.Data.Extensions.Exception.OptimisticConcurrencyException">并发检测失败时,则会抛出此异常</exception>
        /// <param name="concurrencyMode">并发检测模式</param>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public virtual int Delete(ConcurrencyMode concurrencyMode, string connectionString = null)
        {
            int flag = concurrencyMode == ConcurrencyMode.TimeStamp ? 5 : 6;
            CPQuery query = GetCPQuery(flag, new object[] { this, connectionString });

            if (_funcBefore != null)
            {
                if (_funcBefore(query) == false)
                {
                    return -1;
                }
            }

            int effectRows = query.ExecuteNonQuery();

            if (effectRows == 0)
                throw new OptimisticConcurrencyException(
                            "并发操作失败，本次操作没有删除任何记录，请确认当前数据行没有被其他用户更新或删除。");

            return effectRows;
        }

        /// <summary>
        /// 更新数据实体对应的记录。
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了Update()方法的用法</para>
        /// <code>
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// Contract contract = new Contract();
        /// 
        /// //为类的主键赋值
        /// contract.ContractGUID = Reuqest.QueryString["ContractGUID"];
        /// 
        /// //为类的其他字段赋值
        /// //contract.ContractName = Request.Form["ContractName"];
        /// //...
        ///
        /// int count = contract.Update();
        /// //更新成功后,count等于1
        /// </code>
        /// </example>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <exception cref="InvalidOperationException">类没有定义主键,即没有任何一个属性被标记为PrimaryKey=true,则抛出此异常</exception>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public virtual int Update(string connectionString = null)
        {
            CPQuery query = GetCPQuery(7, new object[] { this, bakObject, connectionString });

            if (query == null)
                return 0;

            if (_funcBefore != null)
            {
                if (_funcBefore(query) == false)
                {
                    return -1;
                }
            }

            return query.ExecuteNonQuery();
        }
        /// <summary>
        /// 用并发检测的方式，更新数据实体对应的记录。
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了并发检测模式下,Update()方法的用法</para>
        /// <code>
        /// <![CDATA[
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// 
        /// public void Load(){
        /// 
        ///     //在页面加载时,查询数据库信息,时间戳字段需要通过CAST转换为长整型,绑定到界面中
        ///     Contract contract = CPQuery.From("SELECT ContractGUID, CAST(ContractVersion AS BigInt) ContractVersion  .... FROM cb_Contract WHERE ...").ToSingle<Contract>();
        ///     
        ///     //其他数据绑定代码
        ///     //...
        /// }
        /// 
        /// public void Update(string dataXML, long contractVersion){
        ///		//将AppFrom的xml直接转换为实体对象
        ///		Contract contract = XmlDataEntity.ConvertXmlToSingle<CbContract>(dataXML)
        ///		
        ///		//构造用于并发检测的原对象
        ///		Contract origContract = new Contract();
        ///		origContract.ContractGUID = contract.ContractGUID; //并发检测时,原对象的主键是必须提供的
        ///		contract.ContractVersion = contractVersion.Int64ToTimeStamp(); //界面中长整型的时间戳字段可以通过Int64ToTimeStamp扩展方法转换为byte[]数组
        ///		
        ///		try{
        ///			//根据时间戳字段,进行并发检测
        ///			int count = contract.Update(origContract, ConcurrencyMode.TimeStamp);
        ///			//如果更新成功,则count为1		
        /// 
        ///			//根据原始值,进行并发检测
        ///			//count = contract.Update(origContract, ConcurrencyMode.OriginalValue);
        ///		}
        ///		catch(OptimisticConcurrencyException ex){
        ///			//并发检测失败,将会抛出OptimisticConcurrencyException异常
        ///		}
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="original">用于并发检测的原始对象</param>
        /// <param name="concurrencyMode">并发检测模式</param>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <exception cref="InvalidOperationException">类没有定义主键,即没有任何一个属性被标记为PrimaryKey=true,则抛出此异常</exception>
        /// <exception cref="ArgumentException">用于并发检测的原始对象不能是当前对象</exception>
        /// <exception cref="Citms.Data.Extensions.Exception.OptimisticConcurrencyException">并发检测失败时,则会抛出此异常</exception>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public virtual int Update(BaseEntity original, ConcurrencyMode concurrencyMode, string connectionString = null)
        {
            if (original == null)
                throw new ArgumentNullException("original");


            if (concurrencyMode == ConcurrencyMode.OriginalValue && object.ReferenceEquals(this, original))
                throw new ArgumentException("用于并发检测的原始对象不能是当前对象。");

            int flag = concurrencyMode == ConcurrencyMode.TimeStamp ? 8 : 9;
            CPQuery query = GetCPQuery(flag, new object[] { this, original, bakObject, connectionString });

            if (query == null)
                return 0;

            if (_funcBefore != null)
            {
                if (_funcBefore(query) == false)
                {
                    return -1;
                }
            }

            int effectRows = query.ExecuteNonQuery();

            if (effectRows == 0)
                throw new OptimisticConcurrencyException(
                            "并发操作失败，本次操作没有更新任何记录，请确认当前数据行没有被其他用户更新或删除。");

            return effectRows;
        }


        /// <summary>
        /// 将指定的【值类型】属性对应的字段值设置为零值。
        /// </summary>
        /// <remarks>说明：属性的零值由 .net framework来定义，数字类型为 0，布尔类型为 false，等等。
        /// </remarks>
        /// <exception cref="ArgumentNullException">属性名为空</exception>
        /// <exception cref="ArgumentOutOfRangeException">属性名不能匹配任何属性</exception>
        /// <exception cref="InvalidOperationException">指定的属性是一个引用类型</exception>
        /// <exception cref="InvalidOperationException">指定的属性是一个可空类型</exception>
        /// <param name="propertyName">属性名</param>
        public void SetPropertyDefaultValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            PropertyInfo propInfo = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            if (propInfo == null)
                throw new ArgumentOutOfRangeException(string.Format("指定的属性名 {0} 不能匹配任何属性。", propertyName));

            if (propInfo.PropertyType.IsValueType == false)
                throw new InvalidOperationException(string.Format("指定的属性名 {0} 不是值类型，不需要这个调用。", propertyName));

            Type memberType = propInfo.PropertyType;
            object defaultValue = Activator.CreateInstance(memberType);

            SetValue(propertyName, defaultValue);
        }

        private static T GetDefaultValue<T>()
        {
            return default(T);
        }

        /// <summary>
        /// 此API不宜在项目代码中调用，仅供内部使用。
        /// </summary>
        /// <returns>....</returns>
        public string[] ChangedProperties()
        {
            if (_changedProperties == null)
                return new string[0];

            return _changedProperties.ToArray();
        }

        /// <summary>
        /// 此API不宜在项目代码中调用，仅供内部使用。
        /// </summary>
        public void TrackChange()
        {
            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(GetType());
            if (description.ExecuteFunc == null)
                throw GetNonStandardExecption(this.GetType());

            bakObject = description.ExecuteFunc(13, new object[] { this }) as BaseEntity;
        }

        /// <summary>
        /// 根据属性名称,设置实体的属性值。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值</param>
        public void SetValue(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            Type instanceType = GetType();
            PropertyInfo prop = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
            {
                throw new InvalidOperationException(string.Format("指定的属性名 {0} 不能匹配任何属性。", propertyName));
            }

            if (prop.CanWrite == false)
            {
                throw new InvalidOperationException(string.Format("指定的属性名 {0} 不可写。", propertyName));
            }

            if (_changedProperties == null)
            {
                _changedProperties = new List<string>();
            }

            _changedProperties.Add(propertyName);

            prop.SetValue(this, value, null);
        }

        internal BaseEntity bakObject;
    }

    /// <summary>
    /// 数据实体的基类
    /// </summary>
    /// <typeparam name="TEntity">实体的类型</typeparam>
    public class BaseEntity<TEntity> : BaseEntity where TEntity : BaseEntity
    {
        /// <summary>
        /// 根据表达式,设置实体的属性值。
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="value">属性值</param>
        public void SetValue(Expression<Func<TEntity, object>> expression, object value)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Expression part = null;

            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                UnaryExpression unaryExpress = expression.Body as UnaryExpression;
                part = unaryExpress.Operand;
            }
            else
            {
                part = expression.Body;
            }

            if (part.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExpress = part as MemberExpression;
                string propName = memberExpress.Member.Name;
                SetValue(propName, value);
            }
            else
            {
                throw new InvalidExpressionException("输入的表达式不正确。");
            }
        }

        /// <summary>
        /// 通过过滤条件查询数据
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="args">参数化查询</param>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>List<TEntity></returns>
        public virtual List<TEntity> Select(string where = "", Hashtable args = null, string connectionString = null)
        {
            return Select(where, args, connectionString, -1, -1);
        }

        /// <summary>
        /// 通过过滤条件分页查询数据
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="PageIndex">页下标</param>
        /// <param name="PageSize">页大小</param>
        /// <returns>List<TEntity></returns>
        public virtual List<TEntity> Select(string where, int PageIndex, int PageSize)
        {
            return Select(where, null, null, PageIndex, PageSize);
        }

        /// <summary>
        /// 直接分页查询数据
        /// </summary>
        /// <param name="PageIndex">页下标</param>
        /// <param name="PageSize">页大小</param>
        /// <returns>List<TEntity></returns>
        public virtual List<TEntity> Select(int PageIndex, int PageSize)
        {
            return Select("", null, null, PageIndex, PageSize);
        }

        /// <summary>
        /// 通过过滤条件分页查询数据
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="args">参数化查询</param>
        /// <param name="PageIndex">页下标</param>
        /// <param name="PageSize">页大小</param>
        /// <returns>List<TEntity></returns>
        public virtual List<TEntity> Select(string where, Hashtable args, int PageIndex, int PageSize)
        {
            return Select(where, args, null, PageIndex, PageSize);
        }

        /// <summary>
        /// 通过过滤条件分页查询数据
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="args">参数化查询</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="PageIndex">页下标</param>
        /// <param name="PageSize">页大小</param>
        /// <returns>List<TEntity></returns>
        public virtual List<TEntity> Select(string where, Hashtable args, string connectionString, int PageIndex, int PageSize)
        {
            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(GetType());
            if (description.ExecuteFunc == null)
            {
                throw GetNonStandardExecption(this.GetType());
            }
            try
            {
                return description.ExecuteFunc(14, new object[] { where, args, connectionString, PageIndex, PageSize }) as List<TEntity>;
            }
            catch (System.Exception ex)
            {
                //这里不希望调用者看到代码生成器产生的代码结构,于是在这里抛出捕获到的异常
                throw ex;
            }
        }

        /// <summary>
        /// 根据主键查询数据
        /// </summary>
        /// <example>
        /// <para>下面的代码演示了FindByID()方法的用法</para>
        /// <code>
        /// //Contract类需要继承自BaseEntity,并且需要放在以*.Entity.dll结尾的程序集中
        /// Contract.FindByID("主键值","查询的字段默认为*")
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">类没有定义主键或者参数ID为空则抛出此异常</exception>
        /// <exception cref="InvalidProgramException">如果数据实体类型的定义不符合规范，就会抛出此异常</exception>
        /// <param name="connectionString">数据库连接字符串,默认值为null 即表示使用默认连接字符串</param>
        /// <returns>返回ADO.NET的原始结果</returns>
        public static TEntity FindByID(string id, string Field = "*", string connectionString = null)
        {
            TypeDescription description = TypeDescriptionCache.GetTypeDiscription(typeof(TEntity));
            if (description.ExecuteFunc == null)
            {
                throw GetNonStandardExecption(typeof(TEntity));
            }
            try
            {
                return description.ExecuteFunc(17, new object[] { id, Field, connectionString }) as TEntity;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}
