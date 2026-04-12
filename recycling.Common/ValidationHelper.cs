using System;
using System.Text.RegularExpressions;

namespace recycling.Common
{
    /// 验证辅助类 - 提供常用的数据验证方法
    public static class ValidationHelper
    {
        // 预编译的正则表达式，提高性能
        private static readonly Regex PhoneNumberRegex = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);
        private static readonly Regex UsernameRegex = new Regex(@"^[\u4e00-\u9fa5a-zA-Z0-9_]+$", RegexOptions.Compiled);
        private static readonly Regex NumericRegex = new Regex(@"^-?\d+\.?\d*$", RegexOptions.Compiled);
        private static readonly Regex DigitsOnlyRegex = new Regex(@"^\d+$", RegexOptions.Compiled);
        private static readonly Regex UpperCaseRegex = new Regex(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex LowerCaseRegex = new Regex(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex DigitRegex = new Regex(@"\d", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new Regex(@"[!@#$%^&*(),.?""':{}|<>]", RegexOptions.Compiled);

        /// 验证手机号格式
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // 中国大陆手机号：1开头，第二位3-9，总共11位数字
            return PhoneNumberRegex.IsMatch(phoneNumber);
        }

        /// 验证邮箱格式
        /// <param name="email">邮箱地址</param>
        /// <returns>是否有效</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // 基本的邮箱格式验证
            return EmailRegex.IsMatch(email);
        }

        /// 验证用户名格式
        /// <param name="username">用户名</param>
        /// <param name="minLength">最小长度（默认3）</param>
        /// <param name="maxLength">最大长度（默认50）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidUsername(string username, int minLength = 3, int maxLength = 50)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < minLength || username.Length > maxLength)
                return false;

            // 允许字母、数字、中文、下划线，不允许特殊字符
            return UsernameRegex.IsMatch(username);
        }

        /// 验证密码强度
        /// <param name="password">密码</param>
        /// <param name="minLength">最小长度（默认6）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidPassword(string password, int minLength = 6)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return password.Length >= minLength;
        }

        /// 验证密码强度（强密码要求）
        /// <param name="password">密码</param>
        /// <returns>是否有效</returns>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // 至少8位，包含大小写字母、数字和特殊字符
            if (password.Length < 8)
                return false;

            bool hasUpper = UpperCaseRegex.IsMatch(password);
            bool hasLower = LowerCaseRegex.IsMatch(password);
            bool hasDigit = DigitRegex.IsMatch(password);
            bool hasSpecial = SpecialCharRegex.IsMatch(password);

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// 验证ID是否有效
        /// <param name="id">ID值</param>
        /// <returns>是否有效（大于0）</returns>
        public static bool IsValidId(int id)
        {
            return id > 0;
        }

        /// 验证日期是否在有效范围内
        /// <param name="date">日期</param>
        /// <param name="minDate">最小日期（可选）</param>
        /// <param name="maxDate">最大日期（可选）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidDate(DateTime date, DateTime? minDate = null, DateTime? maxDate = null)
        {
            if (minDate.HasValue && date < minDate.Value)
                return false;

            if (maxDate.HasValue && date > maxDate.Value)
                return false;

            return true;
        }

        /// 验证预约日期（必须是未来日期）
        /// <param name="appointmentDate">预约日期</param>
        /// <returns>是否有效</returns>
        public static bool IsValidAppointmentDate(DateTime appointmentDate)
        {
            // 预约日期必须大于今天
            return appointmentDate.Date > DateTime.Today;
        }

        /// 验证重量值
        /// <param name="weight">重量（公斤）</param>
        /// <param name="minWeight">最小重量（默认0.1kg）</param>
        /// <param name="maxWeight">最大重量（默认1000kg）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidWeight(decimal weight, decimal minWeight = 0.1m, decimal maxWeight = 1000m)
        {
            return weight >= minWeight && weight <= maxWeight;
        }

        /// 验证价格值
        /// <param name="price">价格</param>
        /// <param name="minPrice">最小价格（默认0）</param>
        /// <param name="maxPrice">最大价格（默认100000）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidPrice(decimal price, decimal minPrice = 0m, decimal maxPrice = 100000m)
        {
            return price >= minPrice && price <= maxPrice;
        }

        /// 验证评分值
        /// <param name="rating">评分</param>
        /// <param name="minRating">最小评分（默认1）</param>
        /// <param name="maxRating">最大评分（默认5）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidRating(int rating, int minRating = 1, int maxRating = 5)
        {
            return rating >= minRating && rating <= maxRating;
        }

        /// 验证字符串长度
        /// <param name="text">文本</param>
        /// <param name="minLength">最小长度</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="allowEmpty">是否允许为空（默认false）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidLength(string text, int minLength, int maxLength, bool allowEmpty = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return allowEmpty;

            return text.Length >= minLength && text.Length <= maxLength;
        }

        /// 验证验证码格式
        /// <param name="code">验证码</param>
        /// <param name="length">验证码长度（默认6位）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidVerificationCode(string code, int length = 6)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (code.Length != length)
                return false;

            // 验证码只能包含数字
            return Regex.IsMatch(code, @"^\d+$");
        }

        /// 验证图片文件扩展名
        /// <param name="fileName">文件名</param>
        /// <returns>是否为有效图片格式</returns>
        public static bool IsValidImageFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string extension = System.IO.Path.GetExtension(fileName)?.ToLower();
            
            return extension == ".jpg" || extension == ".jpeg" || 
                   extension == ".png" || extension == ".gif" || 
                   extension == ".bmp";
        }

        /// 验证文件大小
        /// <param name="fileSizeInBytes">文件大小（字节）</param>
        /// <param name="maxSizeInBytes">最大允许大小（字节，默认5MB）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidFileSize(long fileSizeInBytes, long maxSizeInBytes = 5 * 1024 * 1024)
        {
            return fileSizeInBytes > 0 && fileSizeInBytes <= maxSizeInBytes;
        }
    }
}
