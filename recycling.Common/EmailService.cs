using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace recycling.Common
{
    /// <summary>
    /// 邮件发送服务（用于发送验证码）
    /// </summary>
    public class EmailService
    {
        // 邮件服务器配置（建议从Web.config读取，此处为示例）
        private readonly string _smtpServer = "smtp.qq.com";  // 例如QQ邮箱SMTP服务器
        private readonly int _smtpPort = 587;                 // 端口（QQ邮箱587）
        private readonly string _fromEmail = "424447025@qq.com";  // 发件人邮箱
        private readonly string _fromPassword = "xbhdgonczqkubhjh";  // 邮箱授权码（非登录密码）

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
