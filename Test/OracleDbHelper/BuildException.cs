using System.Collections.Generic;
namespace OracleDbHelper
{
    /// <summary>
    /// 表示代码生成异常
    /// </summary>
    public sealed class BuildException : System.Exception
    {
        /// <summary>
        /// 初始化 BuildException 的新实例。
        /// </summary>
        public BuildException()
        {
        }

        /// <summary>
        /// 初始化 BuildException 的新实例。
        /// </summary>
        /// <param name="message">异常的信息</param>
        /// <param name="innerException">异常的内部异常</param>
        public BuildException(string message, System.Exception innerException) : base(message, innerException)
        {

        }

        /// <summary>
        /// 包含多个编译异常或其他异常实例
        /// </summary>
        public List<System.Exception> BuildExceptions { get; internal set; }
    }
}
