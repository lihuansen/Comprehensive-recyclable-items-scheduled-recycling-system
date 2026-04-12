using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    // 中文注释
    /// 基地工作人员通知业务逻辑层
    /// 中文注释
    // 中文注释
    public class BaseStaffNotificationBLL
    {
        private readonly BaseStaffNotificationDAL _notificationDAL = new BaseStaffNotificationDAL();

        // 中文注释
        /// 发送通用通知
        // 中文注释
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

        // 中文注释
        /// 向所有基地工作人员发送通知
        // 中文注释
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

        // 中文注释
        /// 发送运输开始通知
        // 中文注释
        public bool SendTransportOrderCreatedNotification(int transportOrderId, string orderNumber, 
            string recyclerName, string pickupAddress, decimal estimatedWeight, int? assignedWorkerId = null)
        {
            try
            {
                string title = "运输单开始";
                string content = $"运输单 {orderNumber} 已开始往基地运输，" +
                    $"发起人：{recyclerName}，取货地址：{pickupAddress}，预估重量：{estimatedWeight}kg。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.TransportOrderCreated, transportOrderId, null, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendTransportOrderCreatedNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 发送运输中通知
        // 中文注释
        public bool SendTransportOrderInTransitNotification(int transportOrderId, string orderNumber,
            int? assignedWorkerId = null)
        {
            try
            {
                string title = "运输中";
                string content = $"运输单 {orderNumber} 正在运输中，请做好接收准备。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.TransportOrderInTransit, transportOrderId, null, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendTransportOrderInTransitNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 发送运输到达通知
        // 中文注释
        public bool SendTransportOrderCompletedNotification(int transportOrderId, string orderNumber, 
            string transporterName, decimal actualWeight, int? assignedWorkerId = null)
        {
            try
            {
                string title = "已到达";
                string content = $"运输单 {orderNumber} 已到达基地，" +
                    $"运输人员：{transporterName}，到达重量：{actualWeight}kg。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.TransportOrderCompleted, transportOrderId, null, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendTransportOrderCompletedNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 发送提示创建入库单通知
        // 中文注释
        public bool SendCreateWarehouseReceiptPromptNotification(int transportOrderId, string orderNumber,
            int? assignedWorkerId = null)
        {
            try
            {
                string title = "提示创建入库单";
                string content = $"运输单 {orderNumber} 货物已到达，请及时创建入库单进行收货登记。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.CreateWarehouseReceiptPrompt, transportOrderId, null, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendCreateWarehouseReceiptPromptNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 发送提示细分通知（入库单已创建）
        // 中文注释
        public bool SendWarehouseReceiptReceivedNotification(int warehouseReceiptId, string receiptNumber,
            int? transportOrderId, string orderNumber, decimal totalWeight, int? assignedWorkerId = null)
        {
            try
            {
                string title = "提示细分";
                string content = $"运输单 {orderNumber} 货物已到达基地，已创建入库单 {receiptNumber}，" +
                    $"总重量：{totalWeight}kg，请尽快完成细分操作。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.WarehouseReceiptReceived, transportOrderId, warehouseReceiptId, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWarehouseReceiptReceivedNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 发送提示入库通知（细分完成）
        // 中文注释
        public bool SendWarehouseReceiptCreatedNotification(int warehouseReceiptId, string receiptNumber, 
            int? transportOrderId, string orderNumber, decimal totalWeight, int createdByWorkerId,
            int? assignedWorkerId = null)
        {
            try
            {
                string title = "提示入库";
                string content = $"入库单 {receiptNumber} 细分已完成，" +
                    $"关联运输单：{orderNumber}，细分总重量：{totalWeight}kg，请尽快完成入库操作。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.WarehouseReceiptCreated, transportOrderId, warehouseReceiptId, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWarehouseReceiptCreatedNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 发送入库成功通知
        // 中文注释
        public bool SendWarehouseInventoryWrittenNotification(int warehouseReceiptId, string receiptNumber, 
            string itemCategories, decimal totalWeight, int? assignedWorkerId = null)
        {
            try
            {
                string title = "入库成功";
                string content = $"入库单 {receiptNumber} 已成功完成入库，" +
                    $"入库重量：{totalWeight}kg。";

                return SendToWorkerOrAll(assignedWorkerId, title, content,
                    BaseStaffNotificationTypes.WarehouseInventoryWritten, null, warehouseReceiptId, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWarehouseInventoryWrittenNotification Error: {ex.Message}");
                return false;
            }
        }

        // 中文注释
        /// 辅助方法：发送通知给指定工作人员（有 assignedWorkerId 时）或所有活跃工作人员
        // 中文注释
        private bool SendToWorkerOrAll(int? assignedWorkerId, string title, string content,
            string notificationType, int? relatedTransportOrderId, int? relatedWarehouseReceiptId,
            bool fallbackToAllWhenUnassigned = true)
        {
            if (assignedWorkerId.HasValue && assignedWorkerId.Value > 0)
            {
                return SendNotification(assignedWorkerId.Value, title, content,
                    notificationType, relatedTransportOrderId, relatedWarehouseReceiptId);
            }

            if (!fallbackToAllWhenUnassigned)
            {
                System.Diagnostics.Debug.WriteLine($"未指派基地工作人员，跳过任务通知发送：{notificationType}");
                return false;
            }

            return SendNotificationToAllWorkers(title, content,
                notificationType, relatedTransportOrderId, relatedWarehouseReceiptId);
        }

        // 中文注释
        /// 获取工作人员通知列表
        // 中文注释
        public List<BaseStaffNotifications> GetWorkerNotifications(int workerId, int pageIndex = 1, int pageSize = 20)
        {
            if (workerId <= 0) return new List<BaseStaffNotifications>();
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return _notificationDAL.GetWorkerNotifications(workerId, pageIndex, pageSize);
        }

        // 中文注释
        /// 获取工作人员未读通知数量
        // 中文注释
        public int GetUnreadCount(int workerId)
        {
            if (workerId <= 0) return 0;
            return _notificationDAL.GetUnreadCount(workerId);
        }

        // 中文注释
        /// 标记通知为已读
        // 中文注释
        public bool MarkAsRead(int notificationId, int workerId)
        {
            if (notificationId <= 0 || workerId <= 0) return false;
            return _notificationDAL.MarkAsRead(notificationId, workerId);
        }

        // 中文注释
        /// 标记所有通知为已读
        // 中文注释
        public bool MarkAllAsRead(int workerId)
        {
            if (workerId <= 0) return false;
            return _notificationDAL.MarkAllAsRead(workerId);
        }

        // 中文注释
        /// 删除通知
        // 中文注释
        public bool DeleteNotification(int notificationId, int workerId)
        {
            if (notificationId <= 0 || workerId <= 0) return false;
            return _notificationDAL.DeleteNotification(notificationId, workerId);
        }

        // 中文注释
        /// 获取工作人员通知总数
        // 中文注释
        public int GetTotalCount(int workerId)
        {
            if (workerId <= 0) return 0;
            return _notificationDAL.GetTotalCount(workerId);
        }
    }
}
