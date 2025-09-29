using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Configuration;

namespace recycling.Common
{
    /// <summary>
    /// 邮件发送服务（用于发送验证码）
    /// </summary>
    public class EmailService
    {
        // 从配置文件读取参数（避免硬编码）
        private readonly string _smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
        private readonly int _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
        private readonly string _fromEmail = ConfigurationManager.AppSettings["FromEmail"];
        private readonly string _fromPassword = ConfigurationManager.AppSettings["FromPassword"];

        /// <summary>
        /// 发送验证码到指定邮箱
        /// </summary>
        /// <param name="toEmail">收件人邮箱</param>
        /// <param name="verificationCode">验证码</param>
        /// <returns>是否发送成功</returns>
        public bool SendVerificationCode(string toEmail, string verificationCode)
        {
            try
            {
                // 创建邮件消息
                var mailMessage = new MailMessage(
                    from: _fromEmail,
                    to: toEmail,
                    subject: "【回收系统】登录验证码",
                    body: $"您的登录验证码为：<strong>{verificationCode}</strong>，5分钟内有效，请勿泄露给他人。"
                );
                mailMessage.IsBodyHtml = true;  // 允许HTML格式内容

                // 配置SMTP客户端
                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.EnableSsl = true;  // 启用SSL加密（必须）
                    smtpClient.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                    smtpClient.Send(mailMessage);  // 发送邮件
                }

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误日志（实际项目中应使用日志框架）
                Console.WriteLine($"发送邮件失败：{ex.Message}");
                return false;
            }
        }
    }
}
