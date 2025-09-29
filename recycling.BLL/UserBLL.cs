using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using recycling.Model;
using recycling.DAL;
using System.Web;
using System.Collections;
using System.Collections.Concurrent;

namespace recycling.BLL
{
    public class UserBLL
    {
        private UserDAL _userDAL = new UserDAL();
        // 线程安全的验证码存储（手机号 -> 验证码+过期时间）
        private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpireTime)> _verificationCodes =
            new ConcurrentDictionary<string, (string, DateTime)>();

        /// <summary>
        /// 注册新用户，返回错误信息（按优先级排序）
        /// 优先级：用户名 > 密码 > 手机号 > 邮箱
        /// </summary>
        public string Register(RegisterViewModel model)
        {
            // 1. 检查用户名是否已存在
            if (_userDAL.IsUsernameExists(model.Username))
            {
                return "用户名已存在，请更换其他用户名";
            }

            // 2. 检查密码一致性（前端已验证，后端再次确认）
            if (model.Password != model.ConfirmPassword)
            {
                return "两次输入的密码不一致，请重新输入";
            }

            // 3. 检查手机号格式（前端已验证，后端再次确认）
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, @"^1[3-9]\d{9}$"))
            {
                return "请输入正确的手机号格式";
            }

            // 4. 检查手机号是否已存在
            if (_userDAL.IsPhoneExists(model.PhoneNumber))
            {
                return "该手机号已被注册，请使用其他手机号";
            }

            // 5. 检查邮箱格式（前端已验证，后端再次确认）
            try
            {
                var addr = new System.Net.Mail.MailAddress(model.Email);
            }
            catch
            {
                return "请输入正确的邮箱格式";
            }

            // 6. 检查邮箱是否已存在
            if (_userDAL.IsEmailExists(model.Email))
            {
                return "该邮箱已被注册，请使用其他邮箱";
            }

            // 所有验证通过，创建用户并插入数据库
            var user = new Users
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password), // 密码哈希处理
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                RegistrationDate = DateTime.Now
            };

            int newUserId = _userDAL.InsertUser(user);
            if (newUserId > 0)
            {
                return null; // 注册成功，返回null表示无错误
            }
            else
            {
                return "注册失败，请稍后重试";
            }
        }

        /// <summary>
        /// 用户登录验证
        /// </summary>
        /// <param name="model">登录视图模型</param>
        /// <returns>错误信息（null表示验证通过）</returns>
        public string Login(LoginViewModel model)
        {
            // 1. 验证用户名是否存在
            if (!_userDAL.IsUsernameExists(model.Username))
            {
                return "用户名不存在，请先注册";
            }

            // 2. 获取用户信息并验证密码
            Users user = _userDAL.GetUserByUsername(model.Username);
            if (user == null)
            {
                return "用户名不存在，请先注册"; // 双重验证，确保安全性
            }

            // 计算输入密码的哈希值（与注册时算法一致）
            string inputPasswordHash = HashPassword(model.Password);
            if (inputPasswordHash != user.PasswordHash)
            {
                return "密码错误，请重新输入";
            }

            // 3. 验证验证码
            if (string.IsNullOrEmpty(model.GeneratedCaptcha) || 
                !string.Equals(model.Captcha, model.GeneratedCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                return "验证码不正确";
            }

            // 所有验证通过
            return null;
        }

        // 新增：根据用户名获取用户信息（供UI层登录成功后获取用户详情）
        public Users GetUserByUsername(string username)
        {
            return _userDAL.GetUserByUsername(username);
        }

        /// <summary>
        /// 密码哈希处理（使用SHA256）
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// 更新用户最后登录时间（登录成功后调用）
        /// </summary>
        /// <param name="userId">用户ID</param>
        public void UpdateLastLoginDate(int userId)
        {
            // 使用当前时间作为最后登录时间（与注册时间RegistrationDate保持时间格式一致）
            DateTime loginTime = DateTime.Now;
            try
            {
                _userDAL.UpdateLastLoginDate(userId, loginTime);
            }
            catch (Exception ex)
            {
                // 传递异常到UI层处理
                throw new Exception("更新登录记录失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 检查手机号是否已注册（封装DAL方法）
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns>是否存在</returns>
        public bool IsPhoneExists(string phoneNumber)
        {
            try
            {
                // 调用DAL层的IsPhoneExists方法
                return _userDAL.IsPhoneExists(phoneNumber);
            }
            catch (Exception ex)
            {
                throw new Exception("验证手机号时发生错误，请稍后重试", ex);
            }
        }

        /// <summary>
        /// 生成验证码（有效期5分钟）
        /// </summary>
        public string GenerateVerificationCode(string phoneNumber)
        {
            var random = new Random();
            string code = random.Next(100000, 999999).ToString(); // 6位数字
            _verificationCodes[phoneNumber] = (code, DateTime.Now.AddMinutes(5));
            return code;
        }

        /// <summary>
        /// 验证验证码有效性（一次有效）
        /// </summary>
        public bool VerifyVerificationCode(string phoneNumber, string inputCode)
        {
            if (_verificationCodes.TryGetValue(phoneNumber, out var storedData))
            {
                // 验证验证码是否正确且未过期
                bool isValid = storedData.Code == inputCode && DateTime.Now <= storedData.ExpireTime;

                // 无论验证成功与否，都移除验证码（一次有效）
                _verificationCodes.TryRemove(phoneNumber, out _);

                return isValid;
            }
            return false;
        }

        /// <summary>
        /// 重置密码（包含新密码与原密码不同的验证）
        /// </summary>
        public string ResetUserPassword(string phoneNumber, string newPassword)
        {
            // 1. 检查手机号是否存在
            if (!_userDAL.IsPhoneExists(phoneNumber))
            {
                return "系统中未找到该手机号的注册信息";
            }

            // 2. 获取原密码哈希并与新密码哈希比对
            string originalHash = _userDAL.GetOriginalPasswordHash(phoneNumber);
            string newHash = HashPassword(newPassword);

            if (originalHash == newHash)
            {
                return "新密码不能与原密码相同，请重新设置";
            }

            // 3. 执行密码更新
            bool updateSuccess = _userDAL.UpdatePasswordByPhone(phoneNumber, newHash);
            return updateSuccess ? null : "密码重置失败，请稍后重试";
        }
    }
}
