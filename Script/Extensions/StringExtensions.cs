using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExShrinkSidebar.Script.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 截取字符串，超过指定长度时添加省略号
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="ellipsis">省略号，默认为"..."</param>
        /// <returns>处理后的字符串</returns>
        public static string TruncateWithEllipsis(this string input, int maxLength = 8, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            {
                return input;
            }

            return input.Substring(0, maxLength) + ellipsis;
        }

    }
}
