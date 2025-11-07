using System;
using System.Text;
using System.Text.RegularExpressions;

namespace recycling.Common
{
    /// <summary>
    /// 字符串扩展方法类
    /// </summary>
    public static class StringExtensions
    {
        // 预编译的正则表达式，提高性能
        private static readonly Regex HtmlTagRegex = new Regex(@"<[^>]*>", RegexOptions.Compiled);
        private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex NumericRegex = new Regex(@"^-?\d+\.?\d*$", RegexOptions.Compiled);
        private static readonly Regex DigitsOnlyRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

        /// <summary>
        /// 判断字符串是否为空或null
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>是否为空</returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 判断字符串是否为空、null或只包含空白字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>是否为空白</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 截取字符串（超长部分用省略号代替）
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="suffix">后缀（默认为"..."）</param>
        /// <returns>截取后的字符串</returns>
        public static string Truncate(this string str, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;

            return str.Substring(0, maxLength) + suffix;
        }

        /// <summary>
        /// 移除HTML标签
        /// </summary>
        /// <param name="html">HTML字符串</param>
        /// <returns>纯文本</returns>
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            return HtmlTagRegex.Replace(html, string.Empty);
        }

        /// <summary>
        /// 转换为安全的HTML字符串（防止XSS）
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>编码后的HTML字符串</returns>
        public static string ToHtmlSafe(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return System.Web.HttpUtility.HtmlEncode(str);
        }

        /// <summary>
        /// 转换为首字母大写
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>首字母大写的字符串</returns>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length == 1)
                return str.ToUpper();

            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        /// <summary>
        /// 移除所有空白字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>移除空白后的字符串</returns>
        public static string RemoveWhitespace(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return WhitespaceRegex.Replace(str, string.Empty);
        }

        /// <summary>
        /// 转换为拼音首字母（简单实现，仅支持常用汉字）
        /// </summary>
        /// <param name="str">中文字符串</param>
        /// <returns>拼音首字母</returns>
        public static string ToPinyin(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            StringBuilder result = new StringBuilder();
            
            foreach (char c in str)
            {
                if (c >= 0x4e00 && c <= 0x9fa5) // 基本汉字范围
                {
                    result.Append(GetChineseInitial(c));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取汉字的拼音首字母（简化版，不完全准确）
        /// </summary>
        private static char GetChineseInitial(char c)
        {
            int charCode = (int)c;
            
            // 简化的拼音首字母映射（基于Unicode编码范围）
            if (charCode >= 45217 && charCode <= 45252) return 'A';
            if (charCode >= 45253 && charCode <= 45760) return 'B';
            if (charCode >= 45761 && charCode <= 46317) return 'C';
            if (charCode >= 46318 && charCode <= 46825) return 'D';
            if (charCode >= 46826 && charCode <= 47009) return 'E';
            if (charCode >= 47010 && charCode <= 47296) return 'F';
            if (charCode >= 47297 && charCode <= 47613) return 'G';
            if (charCode >= 47614 && charCode <= 48118) return 'H';
            if (charCode >= 48119 && charCode <= 49061) return 'J';
            if (charCode >= 49062 && charCode <= 49323) return 'K';
            if (charCode >= 49324 && charCode <= 49895) return 'L';
            if (charCode >= 49896 && charCode <= 50370) return 'M';
            if (charCode >= 50371 && charCode <= 50613) return 'N';
            if (charCode >= 50614 && charCode <= 50621) return 'O';
            if (charCode >= 50622 && charCode <= 50905) return 'P';
            if (charCode >= 50906 && charCode <= 51386) return 'Q';
            if (charCode >= 51387 && charCode <= 51445) return 'R';
            if (charCode >= 51446 && charCode <= 52217) return 'S';
            if (charCode >= 52218 && charCode <= 52697) return 'T';
            if (charCode >= 52698 && charCode <= 52979) return 'W';
            if (charCode >= 52980 && charCode <= 53688) return 'X';
            if (charCode >= 53689 && charCode <= 54480) return 'Y';
            if (charCode >= 54481 && charCode <= 55289) return 'Z';
            
            return c;
        }

        /// <summary>
        /// 判断字符串是否为数字
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>是否为数字</returns>
        public static bool IsNumeric(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            return NumericRegex.IsMatch(str);
        }

        /// <summary>
        /// 转换为Int32（失败返回默认值）
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>整数值</returns>
        public static int ToInt(this string str, int defaultValue = 0)
        {
            if (int.TryParse(str, out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// 转换为Decimal（失败返回默认值）
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>小数值</returns>
        public static decimal ToDecimal(this string str, decimal defaultValue = 0m)
        {
            if (decimal.TryParse(str, out decimal result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// 转换为DateTime（失败返回默认值）
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>日期时间</returns>
        public static DateTime ToDateTime(this string str, DateTime? defaultValue = null)
        {
            if (DateTime.TryParse(str, out DateTime result))
                return result;

            return defaultValue ?? DateTime.MinValue;
        }

        /// <summary>
        /// 手机号脱敏（显示前3位和后4位）
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>脱敏后的手机号</returns>
        public static string MaskPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 11)
                return phoneNumber;

            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(7);
        }

        /// <summary>
        /// 邮箱脱敏（显示前2位和@后内容）
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>脱敏后的邮箱</returns>
        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            string[] parts = email.Split('@');
            if (parts[0].Length <= 2)
                return email;

            return parts[0].Substring(0, 2) + "***@" + parts[1];
        }
    }
}
