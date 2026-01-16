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
        private readonly BaseStaffNotificationBLL _baseStaffNotificationBLL = new BaseStaffNotificationBLL();

        /// <summary>
        /// 创建入库单（状态为"待入库"，不写入库存）
        /// Create warehouse receipt with "Pending" status, without writing to inventory
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

                // 4. 创建入库单（状态为"待入库"）
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

                // 5. 发送入库单创建通知给基地工作人员
                try
                {
                    _baseStaffNotificationBLL.SendWarehouseReceiptCreatedNotification(
                        receiptId,
                        receiptNumber,
                        transportOrderId,
                        transportOrder.OrderNumber,
                        totalWeight,
                        workerId);
                }
                catch (Exception notifyEx)
                {
                    // 通知失败不影响入库操作
                    System.Diagnostics.Debug.WriteLine($"发送基地工作人员入库单创建通知失败: {notifyEx.Message}");
                }

                return (true, "入库单创建成功", receiptId, receiptNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateWarehouseReceipt BLL Error: {ex.Message}");
                return (false, $"创建入库单失败: {ex.Message}", 0, null);
            }
        }

        /// <summary>
        /// 处理入库单入库（将状态更新为"已入库"并写入库存）
        /// Process warehouse receipt (update status to "Warehoused" and write to inventory)
        /// </summary>
        public (bool success, string message) ProcessWarehouseReceipt(int receiptId)
        {
            try
            {
                // 1. 获取入库单信息
                var receipt = _dal.GetWarehouseReceiptById(receiptId);
                if (receipt == null)
                {
                    return (false, "入库单不存在");
                }

                if (receipt.Status != "待入库")
                {
                    return (false, $"入库单状态不正确，当前状态：{receipt.Status}");
                }

                // 2. 处理入库（写入库存并更新状态）
                bool result = _dal.ProcessWarehouseReceipt(receiptId);

                if (result)
                {
                    // 3. 获取运输单信息用于通知
                    var transportOrder = _transportDAL.GetTransportationOrderById(receipt.TransportOrderID);

                    // 4. 发送入库完成通知给回收员
                    try
                    {
                        _notificationBLL.SendNotification(
                            receipt.RecyclerID,
                            "入库完成",
                            $"您的运输单 {transportOrder?.OrderNumber ?? ""} 已成功入库至基地，入库单号：{receipt.ReceiptNumber}，总重量：{receipt.TotalWeight}kg",
                            "WarehouseReceipt",
                            receiptId);
                    }
                    catch (Exception notifyEx)
                    {
                        // 通知失败不影响入库操作
                        System.Diagnostics.Debug.WriteLine($"发送入库通知失败: {notifyEx.Message}");
                    }

                    // 5. 发送仓库库存写入通知给基地工作人员
                    try
                    {
                        _baseStaffNotificationBLL.SendWarehouseInventoryWrittenNotification(
                            receiptId,
                            receipt.ReceiptNumber,
                            receipt.ItemCategories ?? "未分类",
                            receipt.TotalWeight);
                    }
                    catch (Exception notifyEx)
                    {
                        // 通知失败不影响入库操作
                        System.Diagnostics.Debug.WriteLine($"发送仓库库存写入通知失败: {notifyEx.Message}");
                    }

                    return (true, "入库成功");
                }
                else
                {
                    return (false, "入库处理失败");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessWarehouseReceipt BLL Error: {ex.Message}");
                return (false, $"处理入库单失败: {ex.Message}");
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

        /// <summary>
        /// 获取仓库库存汇总（按类别分组）- 从入库单数据中统计
        /// Get warehouse inventory summary grouped by category - from warehouse receipts
        /// </summary>
        public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetWarehouseSummary()
        {
            try
            {
                return _dal.GetWarehouseSummary();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseSummary BLL Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取仓库库存明细（包含回收员信息）- 从入库单数据中提取
        /// Get warehouse inventory detail with recycler info - from warehouse receipts
        /// </summary>
        public PagedResult<InventoryDetailViewModel> GetWarehouseDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null)
        {
            try
            {
                if (pageIndex < 1) pageIndex = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                return _dal.GetWarehouseDetailWithRecycler(pageIndex, pageSize, categoryKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetWarehouseDetailWithRecycler BLL Error: {ex.Message}");
                throw;
            }
        }
    }
}
