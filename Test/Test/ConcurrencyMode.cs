﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// 并发检测模式
    /// </summary>
    public enum ConcurrencyMode
    {
        /// <summary>
        /// 根据时间戳字段来检测
        /// </summary>
        TimeStamp,

        /// <summary>
        /// 根据提供原始值方式来检测
        /// </summary>
        OriginalValue
    }
}
