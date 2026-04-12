using System;
using System.Collections.Generic;
using recycling.Model;
using recycling.DAL;

namespace recycling.BLL
{
    // 中文注释
    /// 运输单业务逻辑层
    /// 中文注释
    // 中文注释
    public class TransportationOrderBLL
    {
        private readonly TransportationOrderDAL _dal = new TransportationOrderDAL();
        private readonly BaseStaffNotificationBLL _notificationBLL = new BaseStaffNotificationBLL();
        private readonly StaffDAL _staffDAL = new StaffDAL();

        // 中文注释
        /// 创建运输单
        // 中文注释
        /// <param name="order">运输单信息</param>
        /// <param name="categories">品类明细列表（可选）</param>
        /// <returns>中文注释</returns>
        public (int orderId, string orderNumber) CreateTransportationOrder(TransportationOrders order, List<TransportationOrderCategories> categories = null)
        {
            try
            {
                // 验证必填字段
                if (order.RecyclerID <= 0)
                    throw new ArgumentException("回收员ID无效");

                if (order.TransporterID <= 0)
                    throw new ArgumentException("运输人员ID无效");

                if (string.IsNullOrWhiteSpace(order.PickupAddress))
                    throw new ArgumentException("取货地址不能为空");

                // 注意：DestinationAddress 现在由Controller自动填充为"深圳基地"，此处不再验证

                if (order.EstimatedWeight <= 0)
                    throw new ArgumentException("预估重量必须大于0");

                // 调用DAL创建运输单
                var (orderId, orderNumber) = _dal.CreateTransportationOrder(order, categories);

                // 设置运输人员状态为"工作中"
                if (orderId > 0 && order.TransporterID.HasValue && order.TransporterID.Value > 0)
                {
                    try
                    {
                        _dal.UpdateTransporterStatus(order.TransporterID.Value, "工作中");
                        System.Diagnostics.Debug.WriteLine($"运输人员 {order.TransporterID.Value} 状态已更新为工作中");
                    }
                    catch (Exception statusEx)
                    {
                        // 状态更新失败不影响运输单创建
                        System.Diagnostics.Debug.WriteLine($"更新运输人员状态失败: {statusEx.Message}");
                    }
                }

                // 设置指定基地工作人员状态为"工作中"
                if (orderId > 0 && order.AssignedWorkerID.HasValue && order.AssignedWorkerID.Value > 0)
                {
                    try
                    {
                        _dal.UpdateSortingCenterWorkerStatus(order.AssignedWorkerID.Value, "工作中");
                        System.Diagnostics.Debug.WriteLine($"基地工作人员 {order.AssignedWorkerID.Value} 状态已更新为工作中");
                    }
                    catch (Exception statusEx)
                    {
                        // 状态更新失败不影响运输单创建
                        System.Diagnostics.Debug.WriteLine($"更新基地工作人员状态失败: {statusEx.Message}");
                    }
                }

                // 发送通知给基地工作人员
                if (orderId > 0)
                {
                    try
                    {
                        // 获取回收员名字
                        var recycler = _staffDAL.GetRecyclerById(order.RecyclerID.Value);
                        string recyclerName = GetRecyclerName(recycler);

                        _notificationBLL.SendTransportOrderCreatedNotification(
                            orderId,
                            orderNumber,
                            recyclerName,
                            order.PickupAddress,
                            order.EstimatedWeight ?? 0,
                            order.AssignedWorkerID);
                    }
                    catch (Exception notifyEx)
                    {
                        // 通知失败不影响订单创建
                        System.Diagnostics.Debug.WriteLine($"发送运输单创建通知失败: {notifyEx.Message}");
                    }
                }

                return (orderId, orderNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTransportationOrder BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 检查回收员是否有未完成的运输单（即暂存点物品已创建了运输单但尚未完成）
        // 中文注释
        /// <param name="recyclerId">回收员ID</param>
        /// <returns>是否有未完成的运输单</returns>
        public bool HasActiveTransportOrdersForRecycler(int recyclerId)
        {
            try
            {
                if (recyclerId <= 0)
                    throw new ArgumentException("回收员ID无效");

                return _dal.HasActiveTransportOrdersForRecycler(recyclerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasActiveTransportOrdersForRecycler BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 获取回收员的运输单列表
        // 中文注释
        /// <param name="recyclerId">回收员ID</param>
        /// <returns>运输单列表</returns>
        public List<TransportationOrders> GetTransportationOrdersByRecycler(int recyclerId)
        {
            try
            {
                if (recyclerId <= 0)
                    throw new ArgumentException("回收员ID无效");

                return _dal.GetTransportationOrdersByRecycler(recyclerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransportationOrdersByRecycler BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 获取运输单详情
        // 中文注释
        /// <param name="orderId">运输单ID</param>
        /// <returns>运输单详情</returns>
        public TransportationOrders GetTransportationOrderById(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.GetTransportationOrderById(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransportationOrderById BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 更新运输单状态
        // 中文注释
        /// <param name="orderId">运输单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateTransportationOrderStatus(int orderId, string status)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("状态不能为空");

                // 验证状态值
                var validStatuses = new[] { "待接单", "已接单", "运输中", "已完成", "已取消" };
                if (Array.IndexOf(validStatuses, status) == -1)
                    throw new ArgumentException("无效的状态值");

                return _dal.UpdateTransportationOrderStatus(orderId, status);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTransportationOrderStatus BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 获取运输人员的运输单列表
        // 中文注释
        /// <param name="transporterId">运输人员ID</param>
        /// <param name="status">可选的状态筛选</param>
        /// <returns>运输单列表</returns>
        public List<TransportationOrders> GetTransportationOrdersByTransporter(int transporterId, string status = null)
        {
            try
            {
                if (transporterId <= 0)
                    throw new ArgumentException("运输人员ID无效");

                return _dal.GetTransportationOrdersByTransporter(transporterId, status);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTransportationOrdersByTransporter BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 运输人员接单
        // 中文注释
        /// <param name="orderId">运输单ID</param>
        /// <returns>是否接单成功</returns>
        public bool AcceptTransportationOrder(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                // 获取运输单信息以获取运输人员ID
                var order = _dal.GetTransportationOrderById(orderId);
                if (order == null)
                    throw new ArgumentException("运输单不存在");

                bool result = _dal.AcceptTransportationOrder(orderId);

                // 接单成功后，更新运输人员状态为"工作中"（如果尚未设置）
                if (result && order.TransporterID.HasValue && order.TransporterID.Value > 0)
                {
                    try
                    {
                        _dal.UpdateTransporterStatus(order.TransporterID.Value, "工作中");
                        System.Diagnostics.Debug.WriteLine($"运输人员 {order.TransporterID.Value} 状态已更新为工作中");
                    }
                    catch (Exception statusEx)
                    {
                        // 状态更新失败不影响接单结果
                        System.Diagnostics.Debug.WriteLine($"更新运输人员状态失败: {statusEx.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AcceptTransportationOrder BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 开始运输
        /// 中文注释
        // 中文注释
        /// <param name="orderId">运输单ID</param>
        /// <returns>是否更新成功</returns>
        public bool StartTransportation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                // 1. 获取运输单信息以获取回收员ID
                var order = _dal.GetTransportationOrderById(orderId);
                if (order == null)
                {
                    throw new ArgumentException("运输单不存在");
                }

                // 2. 更新运输单状态为"运输中"
                bool result = _dal.StartTransportation(orderId);

                // 3. 如果更新成功，执行后续操作
                if (result)
                {
                    try
                    {
                        // 3.1 清空该回收员的暂存点物品
                        // 因为这些物品已经被运输到基地，所以暂存点应该清空
                        var storagePointBLL = new StoragePointBLL();
                        storagePointBLL.ClearStoragePointForRecycler(order.RecyclerID.Value);
                        System.Diagnostics.Debug.WriteLine($"运输单 {order.OrderNumber} 开始运输，回收员 {order.RecyclerID} 的暂存点物品已清空");
                    }
                    catch (Exception clearEx)
                    {
                        // 清空暂存点失败不影响运输状态更新，但记录日志
                        System.Diagnostics.Debug.WriteLine($"清空暂存点失败: {clearEx.Message}");
                    }

                    try
                    {
                        // 3.2 发送"运输中"通知给被指派的基地人员
                        _notificationBLL.SendTransportOrderInTransitNotification(
                            order.TransportOrderID,
                            order.OrderNumber,
                            order.AssignedWorkerID);
                    }
                    catch (Exception notifyEx)
                    {
                        // 通知失败不影响运输状态更新
                        System.Diagnostics.Debug.WriteLine($"发送运输通知失败: {notifyEx.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartTransportation BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 确认收货地点
        // 中文注释
        public bool ConfirmPickupLocation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                var order = _dal.GetTransportationOrderById(orderId);
                if (order == null)
                    throw new ArgumentException("运输单不存在");

                bool result = _dal.ConfirmPickupLocation(orderId);

                // 确认取货地点后状态变为"运输中"，发送"运输中"通知
                if (result)
                {
                    try
                    {
                        _notificationBLL.SendTransportOrderInTransitNotification(
                            orderId,
                            order.OrderNumber,
                            order.AssignedWorkerID);
                    }
                    catch (Exception notifyEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"发送运输中通知失败: {notifyEx.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfirmPickupLocation BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 到达收货地点
        // 中文注释
        public bool ArriveAtPickupLocation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.ArriveAtPickupLocation(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ArriveAtPickupLocation BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 装货完毕
        // 中文注释
        public bool CompleteLoading(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                // 1. 获取运输单信息以获取回收员ID
                var order = _dal.GetTransportationOrderById(orderId);
                if (order == null)
                {
                    throw new ArgumentException("运输单不存在");
                }

                // 2. 完成装货
                bool result = _dal.CompleteLoading(orderId);

                // 3. 如果更新成功，执行后续操作
                if (result)
                {
                    try
                    {
                        // 清空该回收员的暂存点物品
                        var storagePointBLL = new StoragePointBLL();
                        storagePointBLL.ClearStoragePointForRecycler(order.RecyclerID.Value);
                        System.Diagnostics.Debug.WriteLine($"运输单 {order.OrderNumber} 装货完毕，回收员 {order.RecyclerID} 的暂存点物品已清空");
                    }
                    catch (Exception clearEx)
                    {
                        // 清空暂存点失败不影响运输状态更新，但记录日志
                        System.Diagnostics.Debug.WriteLine($"清空暂存点失败: {clearEx.Message}");
                    }

                    try
                    {
                        // 发送"运输中"通知给被指派的基地人员（装货完毕后继续运输中）
                        _notificationBLL.SendTransportOrderInTransitNotification(
                            order.TransportOrderID,
                            order.OrderNumber,
                            order.AssignedWorkerID);
                    }
                    catch (Exception notifyEx)
                    {
                        // 通知失败不影响运输状态更新
                        System.Diagnostics.Debug.WriteLine($"发送运输通知失败: {notifyEx.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompleteLoading BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 确认送货地点
        // 中文注释
        public bool ConfirmDeliveryLocation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.ConfirmDeliveryLocation(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfirmDeliveryLocation BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 到达送货地点
        // 中文注释
        public bool ArriveAtDeliveryLocation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.ArriveAtDeliveryLocation(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ArriveAtDeliveryLocation BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 完成运输
        // 中文注释
        /// <param name="orderId">运输单ID</param>
        /// <param name="actualWeight">实际重量</param>
        /// <returns>是否完成成功</returns>
        public bool CompleteTransportation(int orderId, decimal? actualWeight)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                if (actualWeight.HasValue && actualWeight.Value < 0)
                    throw new ArgumentException("实际重量不能为负数");

                // 获取运输单详情（在完成之前获取，以便获取运输人员ID）
                var order = _dal.GetTransportationOrderById(orderId);

                bool result = _dal.CompleteTransportation(orderId, actualWeight);

                // 发送通知给基地工作人员，并更新运输人员状态
                if (result)
                {
                    // 更新运输人员状态：如果没有其他未完成的运输单，则恢复为"空闲"
                    if (order != null && order.TransporterID.HasValue && order.TransporterID.Value > 0)
                    {
                        try
                        {
                            bool hasActiveOrders = _dal.HasActiveTransportOrdersForTransporter(order.TransporterID.Value);
                            if (!hasActiveOrders)
                            {
                                _dal.UpdateTransporterStatus(order.TransporterID.Value, "空闲");
                                System.Diagnostics.Debug.WriteLine($"运输人员 {order.TransporterID.Value} 所有运输单已完成，状态已更新为空闲");
                            }
                        }
                        catch (Exception statusEx)
                        {
                            // 状态更新失败不影响运输完成结果
                            System.Diagnostics.Debug.WriteLine($"更新运输人员状态失败: {statusEx.Message}");
                        }

                        // 累加运输人员总运输重量
                        decimal weightToAdd = actualWeight ?? order.EstimatedWeight ?? 0;
                        if (weightToAdd > 0)
                        {
                            try
                            {
                                _dal.UpdateTransporterTotalWeight(order.TransporterID.Value, weightToAdd);
                                System.Diagnostics.Debug.WriteLine($"运输人员 {order.TransporterID.Value} 总运输重量已增加 {weightToAdd} kg");
                            }
                            catch (Exception weightEx)
                            {
                                // 重量更新失败不影响运输完成结果
                                System.Diagnostics.Debug.WriteLine($"更新运输人员总重量失败: {weightEx.Message}");
                            }
                        }
                    }

                    try
                    {
                        if (order != null)
                        {
                            // 获取运输人员名字
                            string transporterName = "运输人员";
                            try
                            {
                                var staffDAL = new StaffDAL();
                                var transporter = staffDAL.GetTransporterById(order.TransporterID.Value);
                                if (transporter != null && !string.IsNullOrWhiteSpace(transporter.FullName))
                                {
                                    transporterName = transporter.FullName;
                                }
                            }
                            catch
                            {
                                // 如果获取失败，使用默认名称
                            }

                            // 发送"已到达"通知
                            _notificationBLL.SendTransportOrderCompletedNotification(
                                orderId,
                                order.OrderNumber,
                                transporterName,
                                actualWeight ?? order.EstimatedWeight ?? 0,
                                order.AssignedWorkerID);

                            // 发送"提示创建入库单"通知
                            _notificationBLL.SendCreateWarehouseReceiptPromptNotification(
                                orderId,
                                order.OrderNumber,
                                order.AssignedWorkerID);
                        }
                    }
                    catch (Exception notifyEx)
                    {
                        // 通知失败不影响运输完成
                        System.Diagnostics.Debug.WriteLine($"发送运输单完成通知失败: {notifyEx.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompleteTransportation BLL Error: {ex.Message}");
                throw;
            }
        }

        // 中文注释
        /// 获取回收员名称（辅助方法）
        // 中文注释
        private string GetRecyclerName(Recyclers recycler)
        {
            if (recycler == null)
            {
                return "未知回收员";
            }

            if (!string.IsNullOrWhiteSpace(recycler.FullName))
            {
                return recycler.FullName;
            }

            return recycler.Username ?? "未知回收员";
        }
    }
}
