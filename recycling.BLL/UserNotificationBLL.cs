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
        /// 发送订单创建通知
        /// </summary>
        public bool SendOrderCreatedNotification(int userId, int orderId)
        {
            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderCreated,
                Title = "预约订单已生成",
                Content = $"您的预约订单 #AP{orderId:D6} 已成功创建，请等待回收员接单。",
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
            if (userId <= 0) return false;

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderAccepted,
                Title = "回收员已接单",
                Content = $"您的订单 #AP{orderId:D6} 已被回收员 {recyclerName} 接单，请保持电话畅通。",
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
            if (userId <= 0) return false;

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.OrderCompleted,
                Title = "订单已完成",
                Content = $"您的订单 #AP{orderId:D6} 已完成，感谢您支持环保回收！",
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
            if (userId <= 0) return false;

            var notification = new UserNotifications
            {
                UserID = userId,
                NotificationType = NotificationTypes.ReviewReminder,
                Title = "评价提醒",
                Content = $"您的订单 #AP{orderId:D6} 已完成，请对本次服务进行评价，帮助我们提升服务质量！",
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
                Content = $"您的订单 #AP{orderId:D6} 已成功取消。",
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
            if (userId <= 0) return false;

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
        /// 获取用户通知列表
        /// </summary>
        public List<UserNotifications> GetUserNotifications(int userId, int pageIndex = 1, int pageSize = 20)
        {
            if (userId <= 0) return new List<UserNotifications>();
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;

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
