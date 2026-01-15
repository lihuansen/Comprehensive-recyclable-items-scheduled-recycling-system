using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    /// <summary>
    /// 基地工作人员通知业务逻辑层
    /// Base Staff Notifications Business Logic Layer
    /// </summary>
    public class BaseStaffNotificationBLL
    {
        private readonly BaseStaffNotificationDAL _notificationDAL = new BaseStaffNotificationDAL();

        /// <summary>
        /// 发送通用通知
        /// </summary>
        /// <param name="workerId">工作人员ID</param>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="notificationType">通知类型</param>
        /// <param name="relatedTransportOrderId">关联运输单ID（可选）</param>
        /// <param name="relatedWarehouseReceiptId">关联入库单ID（可选）</param>
        /// <returns>是否发送成功</returns>
        public bool SendNotification(int workerId, string title, string content, 
            string notificationType = null, int? relatedTransportOrderId = null, int? relatedWarehouseReceiptId = null)
        {
            if (workerId <= 0)
            {
                System.Diagnostics.Debug.WriteLine("工作人员ID无效，通知发送失败");
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

            var notification = new BaseStaffNotifications
            {
                WorkerID = workerId,
                NotificationType = notificationType,
                Title = title,
                Content = content,
                RelatedTransportOrderID = relatedTransportOrderId,
                RelatedWarehouseReceiptID = relatedWarehouseReceiptId,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            return _notificationDAL.AddNotification(notification);
        }

        /// <summary>
        /// 向所有基地工作人员发送通知
        /// </summary>
        public bool SendNotificationToAllWorkers(string title, string content, 
            string notificationType = null, int? relatedTransportOrderId = null, int? relatedWarehouseReceiptId = null)
        {
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

            return _notificationDAL.AddNotificationsToAllWorkers(
                notificationType, title, content, relatedTransportOrderId, relatedWarehouseReceiptId);
        }

        /// <summary>
        /// 发送运输单创建通知
        /// </summary>
        public bool SendTransportOrderCreatedNotification(int transportOrderId, string orderNumber, 
            string recyclerName, string pickupAddress, decimal estimatedWeight)
        {
            try
            {
                string title = "新运输单待处理";
                string content = $"回收员 {recyclerName} 创建了运输单 {orderNumber}，" +
                    $"取货地址：{pickupAddress}，预估重量：{estimatedWeight}kg。请关注运输进度。";

                return SendNotificationToAllWorkers(
                    title, 
                    content, 
                    BaseStaffNotificationTypes.TransportOrderCreated,
                    transportOrderId,
                    null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendTransportOrderCreatedNotification Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送运输单完成通知
        /// </summary>
        public bool SendTransportOrderCompletedNotification(int transportOrderId, string orderNumber, 
            string transporterName, decimal actualWeight)
        {
            try
            {
                string title = "运输单已完成";
                string content = $"运输人员 {transporterName} 已完成运输单 {orderNumber}，" +
                    $"实际重量：{actualWeight}kg。请及时创建入库单。";

                return SendNotificationToAllWorkers(
                    title, 
                    content, 
                    BaseStaffNotificationTypes.TransportOrderCompleted,
                    transportOrderId,
                    null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendTransportOrderCompletedNotification Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送入库单创建通知
        /// </summary>
        public bool SendWarehouseReceiptCreatedNotification(int warehouseReceiptId, string receiptNumber, 
            int transportOrderId, string orderNumber, decimal totalWeight, int createdByWorkerId)
        {
            try
            {
                string title = "入库单已创建";
                string content = $"入库单 {receiptNumber} 已创建，" +
                    $"关联运输单：{orderNumber}，总重量：{totalWeight}kg。请及时写入仓库。";

                // 发送给除创建者外的所有工作人员
                return SendNotificationToAllWorkers(
                    title, 
                    content, 
                    BaseStaffNotificationTypes.WarehouseReceiptCreated,
                    transportOrderId,
                    warehouseReceiptId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWarehouseReceiptCreatedNotification Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送仓库库存写入通知
        /// </summary>
        public bool SendWarehouseInventoryWrittenNotification(int warehouseReceiptId, string receiptNumber, 
            string itemCategories, decimal totalWeight)
        {
            try
            {
                string title = "仓库库存已更新";
                string content = $"入库单 {receiptNumber} 的物品已成功写入仓库，" +
                    $"物品类别：{itemCategories}，总重量：{totalWeight}kg。";

                return SendNotificationToAllWorkers(
                    title, 
                    content, 
                    BaseStaffNotificationTypes.WarehouseInventoryWritten,
                    null,
                    warehouseReceiptId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWarehouseInventoryWrittenNotification Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取工作人员通知列表
        /// </summary>
        public List<BaseStaffNotifications> GetWorkerNotifications(int workerId, int pageIndex = 1, int pageSize = 20)
        {
            if (workerId <= 0) return new List<BaseStaffNotifications>();
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _notificationDAL.GetWorkerNotifications(workerId, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取工作人员未读通知数量
        /// </summary>
        public int GetUnreadCount(int workerId)
        {
            if (workerId <= 0) return 0;
            return _notificationDAL.GetUnreadCount(workerId);
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public bool MarkAsRead(int notificationId, int workerId)
        {
            if (notificationId <= 0 || workerId <= 0) return false;
            return _notificationDAL.MarkAsRead(notificationId, workerId);
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        public bool MarkAllAsRead(int workerId)
        {
            if (workerId <= 0) return false;
            return _notificationDAL.MarkAllAsRead(workerId);
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public bool DeleteNotification(int notificationId, int workerId)
        {
            if (notificationId <= 0 || workerId <= 0) return false;
            return _notificationDAL.DeleteNotification(notificationId, workerId);
        }

        /// <summary>
        /// 获取工作人员通知总数
        /// </summary>
        public int GetTotalCount(int workerId)
        {
            if (workerId <= 0) return 0;
            return _notificationDAL.GetTotalCount(workerId);
        }
    }
}
