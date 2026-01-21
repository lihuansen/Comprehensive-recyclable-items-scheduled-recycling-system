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
using System.Text.RegularExpressions;
using recycling.Common;

namespace recycling.BLL
{
    public class UserBLL
    {
        private UserDAL _userDAL = new UserDAL();
        // 线程安全的验证码存储（手机号 -> 验证码+过期时间）
        private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpireTime)> _verificationCodes =
            new ConcurrentDictionary<string, (string, DateTime)>();
        private EmailService _emailService = new EmailService();  // 实例化邮件服务

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
        /// 检查邮箱是否已注册（封装DAL方法）
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>是否存在</returns>
        public bool IsEmailExists(string email)
        {
            try
            {
                // 调用DAL层的IsEmailExists方法
                return _userDAL.IsEmailExists(email);
            }
            catch (Exception ex)
            {
                throw new Exception("验证邮箱时发生错误，请稍后重试", ex);
            }
        }

        /// <summary>
        /// 手机号登录验证
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="verificationCode">验证码</param>
        /// <returns>验证结果：null表示成功，否则为错误信息</returns>
        public (string ErrorMessage, Users User) PhoneLogin(string phoneNumber, string verificationCode)
        {
            // 1. 检查手机号是否已注册
            if (!_userDAL.IsPhoneExists(phoneNumber))
            {
                return ("该手机号未注册，请先注册", null);
            }

            // 2. 验证验证码
            if (!VerifyVerificationCode(phoneNumber, verificationCode))
            {
                return ("验证码不正确或已过期", null);
            }

            // 3. 获取用户信息
            Users user = _userDAL.GetUserByPhone(phoneNumber);
            if (user == null)
            {
                return ("用户信息获取失败，请重试", null);
            }

            // 4. 验证成功
            return (null, user);
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

        /// <summary>
        /// 新增：邮箱登录逻辑
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="verificationCode">验证码</param>
        /// <returns>错误信息（null表示成功）+ 用户对象</returns>
        public (string ErrorMsg, Users User) EmailLogin(string email, string verificationCode)
        {
            // 1. 验证邮箱格式
            if (!Regex.IsMatch(email, @"^[^\s]+@[^\s]+\.[^\s]+$"))
            {
                return ("请输入有效的邮箱地址", null);
            }

            // 2. 验证验证码
            bool isCodeValid = VerifyVerificationCode(email, verificationCode);
            if (!isCodeValid)
            {
                return ("验证码不正确或已过期", null);
            }

            // 3. 通过邮箱查询用户
            Users user = _userDAL.GetUserByEmail(email);
            if (user == null)
            {
                return ("该邮箱未注册，请先注册", null);
            }

            // 4. 验证通过
            return (null, user);
        }

        /// <summary>
        /// 生成并发送邮箱验证码
        /// </summary>
        public bool GenerateAndSendEmailCode(string email)
        {
            // 1. 验证邮箱是否已注册
            if (!IsEmailExists(email))
            {
                return false;  // 邮箱未注册，不发送验证码（但前端提示保持中性）
            }

            // 2. 生成6位验证码
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            // 3. 存储验证码（5分钟有效期）
            _verificationCodes[email] = (code, DateTime.Now.AddMinutes(5));

            // 4. 调用邮件服务发送验证码
            return _emailService.SendVerificationCode(email, code);
        }

        /// <summary>
        /// 测试专用：获取指定邮箱的验证码（仅用于调试，生产环境需移除）
        /// </summary>
        public string GetVerificationCodeForTest(string email)
        {
            if (_verificationCodes.TryGetValue(email, out var codeInfo))
            {
                // 检查验证码是否过期
                if (codeInfo.ExpireTime > DateTime.Now)
                {
                    return codeInfo.Code;
                }
            }
            return ""; // 验证码不存在或已过期
        }

        /// <summary>
        /// 更新用户基本信息（增加与原有信息的对比检查）
        /// </summary>
        public string UpdateUserProfile(int userId, UpdateProfileViewModel model)
        {
            // 首先获取用户当前信息
            var currentUser = _userDAL.GetUserById(userId);
            if (currentUser == null)
            {
                return "用户不存在";
            }

            // 检查是否有任何修改
            bool hasChanges = false;
            string changeDetails = "";

            // 检查用户名是否修改
            if (currentUser.Username != model.Username)
            {
                hasChanges = true;
                changeDetails += "用户名 ";

                // 验证新用户名是否被其他用户使用
                var existingUser = _userDAL.GetUserByUsername(model.Username);
                if (existingUser != null && existingUser.UserID != userId)
                {
                    return "用户名已被其他用户使用，请更换其他用户名";
                }
            }

            // 检查手机号是否修改
            if (currentUser.PhoneNumber != model.PhoneNumber)
            {
                hasChanges = true;
                changeDetails += "手机号 ";

                // 验证新手机号是否被其他用户使用
                if (_userDAL.IsPhoneExists(model.PhoneNumber))
                {
                    var phoneUser = _userDAL.GetUserByPhone(model.PhoneNumber);
                    if (phoneUser != null && phoneUser.UserID != userId)
                    {
                        return "手机号已被其他用户使用，请更换其他手机号";
                    }
                }
            }

            // 检查邮箱是否修改
            if (currentUser.Email != model.Email)
            {
                hasChanges = true;
                changeDetails += "邮箱 ";

                // 验证新邮箱是否被其他用户使用
                if (_userDAL.IsEmailExists(model.Email))
                {
                    var emailUser = _userDAL.GetUserByEmail(model.Email);
                    if (emailUser != null && emailUser.UserID != userId)
                    {
                        return "邮箱已被其他用户使用，请更换其他邮箱";
                    }
                }
            }

            // 如果没有修改任何信息
            if (!hasChanges)
            {
                return "没有检测到任何修改，请至少修改一项信息";
            }

            // 执行更新
            bool success = _userDAL.UpdateUserProfile(userId, model.Username, model.PhoneNumber, model.Email);
            return success ? null : "更新失败，请稍后重试";
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public string ChangePassword(int userId, ChangePasswordViewModel model)
        {
            // 验证当前密码
            string currentHash = _userDAL.GetPasswordHashByUserId(userId);
            string inputCurrentHash = HashPassword(model.CurrentPassword);

            if (currentHash != inputCurrentHash)
            {
                return "当前密码不正确";
            }

            // 验证新密码不能与原密码相同
            string newHash = HashPassword(model.NewPassword);
            if (currentHash == newHash)
            {
                return "新密码不能与当前密码相同";
            }

            // 执行密码更新
            bool success = _userDAL.UpdatePasswordByUserId(userId, newHash);
            return success ? null : "密码修改失败，请稍后重试";
        }

        /// <summary>
        /// 通过用户ID获取用户信息
        /// </summary>
        public Users GetUserById(int userId)
        {
            return _userDAL.GetUserById(userId);
        }

        /// <summary>
        /// 更新用户头像
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="avatarUrl">头像URL路径</param>
        /// <returns>更新是否成功</returns>
        public bool UpdateUserAvatar(int userId, string avatarUrl)
        {
            return _userDAL.UpdateUserAvatar(userId, avatarUrl);
        }

        /// <summary>
        /// 获取用户头像URL
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>头像URL，如果没有则返回默认头像</returns>
        public string GetUserAvatarUrl(int userId)
        {
            var user = _userDAL.GetUserById(userId);
            if (user != null && !string.IsNullOrEmpty(user.URL))
            {
                return user.URL;
            }
            // 返回默认头像
            return "/Uploads/Avatars/Default/avatar1.svg";
        }
    }
}
