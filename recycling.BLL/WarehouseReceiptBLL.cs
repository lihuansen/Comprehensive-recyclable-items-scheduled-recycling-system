using System;
using System.Collections.Generic;
using recycling.Model;
using recycling.DAL;

namespace recycling.BLL
{
    /// <summary>
    /// 入库单业务逻辑层
    /// Warehouse Receipt Business Logic Layer
    /// </summary>
    public class WarehouseReceiptBLL
    {
        private readonly WarehouseReceiptDAL _dal = new WarehouseReceiptDAL();
        private readonly TransportationOrderDAL _transportDAL = new TransportationOrderDAL();
        private readonly UserNotificationBLL _notificationBLL = new UserNotificationBLL();

        /// <summary>
        /// 创建入库单（并清零暂存点重量）
        /// </summary>
        public (bool success, string message, int receiptId, string receiptNumber) CreateWarehouseReceipt(
            int transportOrderId, 
            int workerId, 
            decimal totalWeight, 
            string itemCategories, 
            string notes)
        {
            try
            {
                // 1. 验证运输单
                var transportOrder = _transportDAL.GetTransportationOrderById(transportOrderId);
                if (transportOrder == null)
                {
                    return (false, "运输单不存在", 0, null);
                }

                if (transportOrder.Status != "已完成")
                {
                    return (false, "只能为已完成的运输单创建入库单", 0, null);
                }

                // 2. 检查是否已经创建过入库单
                var existingReceipt = _dal.GetWarehouseReceiptByTransportOrderId(transportOrderId);
                if (existingReceipt != null)
                {
                    return (false, "该运输单已创建入库单", 0, null);
                }

                // 3. 验证重量
                if (totalWeight <= 0)
                {
                    return (false, "入库重量必须大于0", 0, null);
                }

                // 4. 创建入库单
                var receipt = new WarehouseReceipts
                {
                    TransportOrderID = transportOrderId,
                    RecyclerID = transportOrder.RecyclerID,
                    WorkerID = workerId,
                    TotalWeight = totalWeight,
                    ItemCategories = itemCategories,
                    Notes = notes,
                    CreatedBy = workerId
                };

                var (receiptId, receiptNumber) = _dal.CreateWarehouseReceipt(receipt);

                // 5. 发送通知给回收员
                try
                {
                    _notificationBLL.SendNotification(
                        transportOrder.RecyclerID,
                        "入库完成",
                        $"您的运输单 {transportOrder.OrderNumber} 已成功入库至基地，入库单号：{receiptNumber}，总重量：{totalWeight}kg",
                        "WarehouseReceipt",
                        receiptId);
                }
                catch (Exception notifyEx)
                {
                    // 通知失败不影响入库操作
                    System.Diagnostics.Debug.WriteLine($"发送入库通知失败: {notifyEx.Message}");
                }

                return (true, "入库成功", receiptId, receiptNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWarehouseReceipt BLL Error: {ex.Message}");
                return (false, $"创建入库单失败: {ex.Message}", 0, null);
            }
        }

        /// <summary>
        /// 获取入库单列表
        /// </summary>
        public List<WarehouseReceiptViewModel> GetWarehouseReceipts(int page = 1, int pageSize = 20, string status = null, int? workerId = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                return _dal.GetWarehouseReceipts(page, pageSize, status, workerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseReceipts BLL Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取运输中的订单列表
        /// </summary>
        public List<TransportNotificationViewModel> GetInTransitOrders()
        {
            try
            {
                return _dal.GetInTransitOrders();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInTransitOrders BLL Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取已完成的运输单列表（用于入库）
        /// </summary>
        public List<TransportNotificationViewModel> GetCompletedTransportOrders()
        {
            try
            {
                return _dal.GetCompletedTransportOrders();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCompletedTransportOrders BLL Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 检查运输单是否已创建入库单
        /// </summary>
        public bool HasWarehouseReceipt(int transportOrderId)
        {
            try
            {
                var receipt = _dal.GetWarehouseReceiptByTransportOrderId(transportOrderId);
                return receipt != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasWarehouseReceipt BLL Error: {ex.Message}");
                return false;
            }
        }
    }
}
