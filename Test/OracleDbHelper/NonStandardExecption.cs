using System.Runtime.Serialization;

namespace OracleDbHelper
{
    /// <summary>
    /// 不标准异常。表示代码的定义不符合事先约定的说明。
    /// </summary>
    public sealed class NonStandardExecption : System.Exception
    {
        /// <summary>
        /// 初始化 NonStandardExecption 的新实例。
        /// </summary>
        public NonStandardExecption()
        {
        }

        /// <summary>
        /// 使用指定的错误消息初始化 NonStandardExecption 的新实例。
        /// </summary>
        /// <param name="message">错误消息</param>
        public NonStandardExecption(string message)
            : base(message)
        {
        }

        private NonStandardExecption(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
