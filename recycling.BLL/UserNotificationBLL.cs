using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 用户通知业务逻辑层
    /// </summary>
    public class UserNotificationBLL
    {
        private readonly UserNotificationDAL _notificationDAL = new UserNotificationDAL();

        /// <summary>
        /// 格式化订单号
        /// </summary>
        private static string FormatOrderNumber(int orderId)
        {
            return $"#AP{orderId:D6}";
        }

        /// <summary>
        /// 发送通用通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="notificationType">通知类型</param>
        /// <param name="relatedId">关联ID（可选）</param>
        /// <returns>是否发送成功</returns>
        /// <exception cref="Exception">当数据库操作失败时可能抛出异常，调用方应处理异常</exception>
        public bool SendNotification(int userId, string title, string content, string notificationType = null, int? relatedId = null)
        {
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine("用户ID无效，通知发送失败");
                return false;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                System.Diagnostics.Debug.WriteLine("通知标题不能为空，通知发送失败");
                return false;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                System.Diagnostics.Debug.WriteLine("通知内容不能为空，通知发送失败");
                return false;
            }

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = notificationType,
                Title = title,
                Content = content,
                RelatedOrderID = relatedId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送订单创建通知
        /// </summary>
        public bool SendOrderCreatedNotification(int userId, int orderId)
        {
            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderCreated,
                Title = "预约订单已生成",
                Content = $"您的预约订单 {FormatOrderNumber(orderId)} 已成功创建，请等待回收员接单。",
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送回收员接单通知
        /// </summary>
        public bool SendOrderAcceptedNotification(int orderId, string recyclerName)
        {
            int userId = _notificationDAL.GetUserIdByOrderId(orderId);
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"无法获取订单 {orderId} 的用户ID，接单通知发送失败");
                return false;
            }

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderAccepted,
                Title = "回收员已接单",
                Content = $"您的订单 {FormatOrderNumber(orderId)} 已被回收员 {recyclerName} 接单，请保持电话畅通。",
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送订单完成通知
        /// </summary>
        public bool SendOrderCompletedNotification(int orderId)
        {
            int userId = _notificationDAL.GetUserIdByOrderId(orderId);
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"无法获取订单 {orderId} 的用户ID，完成通知发送失败");
                return false;
            }

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderCompleted,
                Title = "订单已完成",
                Content = $"您的订单 {FormatOrderNumber(orderId)} 已完成，感谢您支持环保回收！",
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送评价提醒通知
        /// </summary>
        public bool SendReviewReminderNotification(int orderId)
        {
            int userId = _notificationDAL.GetUserIdByOrderId(orderId);
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"无法获取订单 {orderId} 的用户ID，评价提醒发送失败");
                return false;
            }

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.ReviewReminder,
                Title = "评价提醒",
                Content = $"您的订单 {FormatOrderNumber(orderId)} 已完成，请对本次服务进行评价，帮助我们提升服务质量！",
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送订单取消通知
        /// </summary>
        public bool SendOrderCancelledNotification(int userId, int orderId)
        {
            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderCancelled,
                Title = "订单已取消",
                Content = $"您的订单 {FormatOrderNumber(orderId)} 已成功取消。",
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送轮播图更新通知（向所有用户发送）
        /// </summary>
        public bool SendCarouselUpdatedNotification(string action, string carouselTitle)
        {
            string actionText;
            switch (action.ToLower())
            {
                case "add":
                    actionText = "新增";
                    break;
                case "update":
                    actionText = "更新";
                    break;
                case "delete":
                    actionText = "删除";
                    break;
                default:
                    actionText = "调整";
                    break;
            }

            string title = "首页内容更新";
            string content = $"系统首页轮播内容已{actionText}：{carouselTitle}，欢迎查看最新内容！";

            return _notificationDAL.AddNotificationsToAllUsers(NotificationTypes.CarouselUpdated, title, content);
        }

        /// <summary>
        /// 发送反馈回复通知
        /// </summary>
        public bool SendFeedbackRepliedNotification(int feedbackId, string feedbackSubject)
        {
            int userId = _notificationDAL.GetUserIdByFeedbackId(feedbackId);
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"无法获取反馈 {feedbackId} 的用户ID，反馈回复通知发送失败");
                return false;
            }

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.FeedbackReplied,
                Title = "反馈已回复",
                Content = $"您提交的反馈「{feedbackSubject}」已收到管理员回复，请前往查看。",
                RelatedFeedbackID = feedbackId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送回收员消息通知
        /// </summary>
        public bool SendRecyclerMessageNotification(int orderId, string recyclerName, string messagePreview)
        {
            int userId = _notificationDAL.GetUserIdByOrderId(orderId);
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"无法获取订单 {orderId} 的用户ID，回收员消息通知发送失败");
                return false;
            }

            // 处理空或null的消息预览
            if (string.IsNullOrEmpty(messagePreview))
            {
                messagePreview = "（新消息）";
            }
            // 限制消息预览长度
            else if (messagePreview.Length > 50)
            {
                messagePreview = messagePreview.Substring(0, 50) + "...";
            }

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.RecyclerMessageReceived,
                Title = $"回收员 {recyclerName} 发来新消息",
                Content = messagePreview,
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 发送订单回退通知（回收员回退订单）
        /// </summary>
        public bool SendOrderRolledBackNotification(int orderId, string recyclerName, string reason = null)
        {
            int userId = _notificationDAL.GetUserIdByOrderId(orderId);
            if (userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"无法获取订单 {orderId} 的用户ID，订单回退通知发送失败");
                return false;
            }

            string reasonText = string.IsNullOrEmpty(reason) ? "回收员发现物品不符合回收要求" : reason;
            
            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderRolledBack,
                Title = "订单已回退",
                Content = $"您的订单 {FormatOrderNumber(orderId)} 已被回收员 {recyclerName} 回退。原因：{reasonText}。您可以继续与回收员沟通了解详情。",
                RelatedOrderID = orderId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 获取用户通知列表
        /// </summary>
        public List<UserNotifications> GetUserNotifications(int userId, int pageIndex = 1, int pageSize = 20)
        {
            if (userId <= 0) return new List<UserNotifications>();
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20; // 限制最大页面大小为100

            return _notificationDAL.GetUserNotifications(userId, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取用户未读通知数量
        /// </summary>
        public int GetUnreadCount(int userId)
        {
            if (userId <= 0) return 0;
            return _notificationDAL.GetUnreadCount(userId);
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public bool MarkAsRead(int notificationId, int userId)
        {
            if (notificationId <= 0 || userId <= 0) return false;
            return _notificationDAL.MarkAsRead(notificationId, userId);
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        public bool MarkAllAsRead(int userId)
        {
            if (userId <= 0) return false;
            return _notificationDAL.MarkAllAsRead(userId);
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public bool DeleteNotification(int notificationId, int userId)
        {
            if (notificationId <= 0 || userId <= 0) return false;
            return _notificationDAL.DeleteNotification(notificationId, userId);
        }

        /// <summary>
        /// 获取用户通知总数
        /// </summary>
        public int GetTotalCount(int userId)
        {
            if (userId <= 0) return 0;
            return _notificationDAL.GetTotalCount(userId);
        }
    }
}
