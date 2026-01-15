using System;
using System.Collections.Generic;
using recycling.Model;
using recycling.DAL;

namespace recycling.BLL
{
    /// <summary>
    /// 运输单业务逻辑层
    /// Transportation Orders Business Logic Layer
    /// </summary>
    public class TransportationOrderBLL
    {
        private readonly TransportationOrderDAL _dal = new TransportationOrderDAL();
        private readonly BaseStaffNotificationBLL _notificationBLL = new BaseStaffNotificationBLL();
        private readonly StaffDAL _staffDAL = new StaffDAL();

        /// <summary>
        /// 创建运输单
        /// </summary>
        /// <param name="order">运输单信息</param>
        /// <returns>Tuple containing order ID and order number, (0, null) if failed</returns>
        public (int orderId, string orderNumber) CreateTransportationOrder(TransportationOrders order)
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
                var (orderId, orderNumber) = _dal.CreateTransportationOrder(order);

                // 发送通知给基地工作人员
                if (orderId > 0)
                {
                    try
                    {
                        // 获取回收员名字
                        var recycler = _staffDAL.GetRecyclerById(order.RecyclerID);
                        string recyclerName = recycler != null && !string.IsNullOrWhiteSpace(recycler.FullName) 
                            ? recycler.FullName 
                            : (recycler != null ? recycler.Username : "未知回收员");

                        _notificationBLL.SendTransportOrderCreatedNotification(
                            orderId,
                            orderNumber,
                            recyclerName,
                            order.PickupAddress,
                            order.EstimatedWeight);
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

        /// <summary>
        /// 获取回收员的运输单列表
        /// </summary>
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

        /// <summary>
        /// 获取运输单详情
        /// </summary>
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

        /// <summary>
        /// 更新运输单状态
        /// </summary>
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

        /// <summary>
        /// 获取运输人员的运输单列表
        /// </summary>
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

        /// <summary>
        /// 运输人员接单
        /// </summary>
        /// <param name="orderId">运输单ID</param>
        /// <returns>是否接单成功</returns>
        public bool AcceptTransportationOrder(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.AcceptTransportationOrder(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AcceptTransportationOrder BLL Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 开始运输
        /// DEPRECATED: Use ConfirmPickupLocation instead to follow new workflow
        /// </summary>
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
                        storagePointBLL.ClearStoragePointForRecycler(order.RecyclerID);
                        System.Diagnostics.Debug.WriteLine($"运输单 {order.OrderNumber} 开始运输，回收员 {order.RecyclerID} 的暂存点物品已清空");
                    }
                    catch (Exception clearEx)
                    {
                        // 清空暂存点失败不影响运输状态更新，但记录日志
                        System.Diagnostics.Debug.WriteLine($"清空暂存点失败: {clearEx.Message}");
                    }

                    try
                    {
                        // 3.2 发送通知给基地人员
                        // 注意：这里需要一个方法来通知所有基地人员
                        // 可以通过UserNotificationBLL来实现
                        SendTransportNotificationToBase(order);
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

        /// <summary>
        /// 确认收货地点
        /// </summary>
        public bool ConfirmPickupLocation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.ConfirmPickupLocation(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfirmPickupLocation BLL Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 到达收货地点
        /// </summary>
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

        /// <summary>
        /// 装货完毕
        /// </summary>
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
                        storagePointBLL.ClearStoragePointForRecycler(order.RecyclerID);
                        System.Diagnostics.Debug.WriteLine($"运输单 {order.OrderNumber} 装货完毕，回收员 {order.RecyclerID} 的暂存点物品已清空");
                    }
                    catch (Exception clearEx)
                    {
                        // 清空暂存点失败不影响运输状态更新，但记录日志
                        System.Diagnostics.Debug.WriteLine($"清空暂存点失败: {clearEx.Message}");
                    }

                    try
                    {
                        // 发送通知给基地人员
                        SendTransportNotificationToBase(order);
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

        /// <summary>
        /// 确认送货地点
        /// </summary>
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

        /// <summary>
        /// 到达送货地点
        /// </summary>
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

        /// <summary>
        /// 发送运输通知给基地人员
        /// </summary>
        private void SendTransportNotificationToBase(TransportationOrders order)
        {
            // 这里可以实现发送通知的逻辑
            // 由于系统中可能有多个基地人员，可以通过以下方式实现：
            // 1. 查询所有基地人员并逐个发送通知
            // 2. 使用广播机制发送通知
            // 暂时记录日志，实际实现可以根据需求调整
            System.Diagnostics.Debug.WriteLine($"运输单 {order.OrderNumber} 开始运输，需要通知基地人员");
        }

        /// <summary>
        /// 完成运输
        /// </summary>
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

                bool result = _dal.CompleteTransportation(orderId, actualWeight);

                // 发送通知给基地工作人员
                if (result)
                {
                    try
                    {
                        // 获取运输单详情
                        var order = _dal.GetTransportationOrderById(orderId);
                        if (order != null)
                        {
                            // 获取运输人员名字
                            string transporterName = "运输人员";
                            try
                            {
                                var staffDAL = new StaffDAL();
                                var transporter = staffDAL.GetTransporterById(order.TransporterID);
                                if (transporter != null && !string.IsNullOrWhiteSpace(transporter.FullName))
                                {
                                    transporterName = transporter.FullName;
                                }
                            }
                            catch
                            {
                                // 如果获取失败，使用默认名称
                            }

                            _notificationBLL.SendTransportOrderCompletedNotification(
                                orderId,
                                order.OrderNumber,
                                transporterName,
                                actualWeight ?? order.EstimatedWeight);
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
    }
}
