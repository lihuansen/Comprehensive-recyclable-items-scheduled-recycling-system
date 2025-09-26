using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using recycling.Model;
using recycling.DAL;

namespace recycling.BLL
{
    public class UserBLL
    {
        private UserDAL _userDAL = new UserDAL();

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
    }
}
