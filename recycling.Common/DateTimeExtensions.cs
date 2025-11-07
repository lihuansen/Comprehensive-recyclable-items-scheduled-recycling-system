using System;

namespace recycling.Common
{
    /// <summary>
    /// 日期时间扩展方法类
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 转换为中文日期格式（例如：2024年1月1日）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>中文日期字符串</returns>
        public static string ToChineseDateString(this DateTime dateTime)
        {
            return $"{dateTime:yyyy年M月d日}";
        }

        /// <summary>
        /// 转换为中文日期时间格式（例如：2024年1月1日 10:30）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>中文日期时间字符串</returns>
        public static string ToChineseDateTimeString(this DateTime dateTime)
        {
            return $"{dateTime:yyyy年M月d日 HH:mm}";
        }

        /// <summary>
        /// 转换为友好的时间描述（例如：刚刚、5分钟前、1小时前、昨天、2天前）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>友好的时间描述</returns>
        public static string ToFriendlyString(this DateTime dateTime)
        {
            TimeSpan span = DateTime.Now - dateTime;

            if (span.TotalSeconds < 60)
                return "刚刚";
            
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes}分钟前";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours}小时前";

            if (span.TotalDays < 2)
                return "昨天";

            if (span.TotalDays < 7)
                return $"{(int)span.TotalDays}天前";

            if (span.TotalDays < 30)
            {
                int weeks = (int)(span.TotalDays / 7);
                return $"{weeks}周前";
            }

            if (span.TotalDays < 365)
            {
                int months = (int)(span.TotalDays / 30);
                return $"{months}个月前";
            }

            int years = (int)(span.TotalDays / 365);
            return $"{years}年前";
        }

        /// <summary>
        /// 获取一天的开始时间（00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天开始时间</returns>
        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// 获取一天的结束时间（23:59:59.999）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>当天结束时间</returns>
        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddMilliseconds(-1);
        }

        /// <summary>
        /// 获取一周的开始时间（周一00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>本周开始时间</returns>
        public static DateTime StartOfWeek(this DateTime dateTime)
        {
            int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dateTime.AddDays(-diff).Date;
        }

        /// <summary>
        /// 获取一周的结束时间（周日23:59:59.999）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>本周结束时间</returns>
        public static DateTime EndOfWeek(this DateTime dateTime)
        {
            return dateTime.StartOfWeek().AddDays(7).AddMilliseconds(-1);
        }

        /// <summary>
        /// 获取一个月的开始时间（1号00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>本月开始时间</returns>
        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// 获取一个月的结束时间（最后一天23:59:59.999）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>本月结束时间</returns>
        public static DateTime EndOfMonth(this DateTime dateTime)
        {
            return dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);
        }

        /// <summary>
        /// 获取一年的开始时间（1月1日00:00:00）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>本年开始时间</returns>
        public static DateTime StartOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1);
        }

        /// <summary>
        /// 获取一年的结束时间（12月31日23:59:59.999）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>本年结束时间</returns>
        public static DateTime EndOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
        }

        /// <summary>
        /// 判断是否为工作日（周一至周五）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>是否为工作日</returns>
        public static bool IsWeekday(this DateTime dateTime)
        {
            return dateTime.DayOfWeek != DayOfWeek.Saturday && 
                   dateTime.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// 判断是否为周末（周六或周日）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>是否为周末</returns>
        public static bool IsWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || 
                   dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// 判断是否为今天
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>是否为今天</returns>
        public static bool IsToday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today;
        }

        /// <summary>
        /// 判断是否为昨天
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>是否为昨天</returns>
        public static bool IsYesterday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// 判断是否为明天
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>是否为明天</returns>
        public static bool IsTomorrow(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// 判断是否在指定日期范围内
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>是否在范围内</returns>
        public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
        {
            return dateTime >= startDate && dateTime <= endDate;
        }

        /// <summary>
        /// 获取年龄（根据出生日期）
        /// </summary>
        /// <param name="birthDate">出生日期</param>
        /// <returns>年龄</returns>
        public static int GetAge(this DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            
            if (birthDate.Date > today.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// 添加工作日（跳过周末）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <param name="workdays">工作日数量</param>
        /// <returns>添加工作日后的日期</returns>
        public static DateTime AddWorkdays(this DateTime dateTime, int workdays)
        {
            int direction = workdays < 0 ? -1 : 1;
            DateTime result = dateTime;
            int addedDays = 0;

            while (addedDays < Math.Abs(workdays))
            {
                result = result.AddDays(direction);
                if (result.IsWeekday())
                    addedDays++;
            }

            return result;
        }

        /// <summary>
        /// 转换为Unix时间戳（秒）
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>Unix时间戳</returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// 从Unix时间戳转换为DateTime
        /// </summary>
        /// <param name="timestamp">Unix时间戳（秒）</param>
        /// <returns>日期时间</returns>
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
        }
    }
}
