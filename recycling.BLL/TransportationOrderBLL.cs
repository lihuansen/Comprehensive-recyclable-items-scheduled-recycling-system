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

                if (string.IsNullOrWhiteSpace(order.DestinationAddress))
                    throw new ArgumentException("目的地地址不能为空");

                if (string.IsNullOrWhiteSpace(order.ContactPerson))
                    throw new ArgumentException("联系人不能为空");

                if (string.IsNullOrWhiteSpace(order.ContactPhone))
                    throw new ArgumentException("联系电话不能为空");

                if (order.EstimatedWeight <= 0)
                    throw new ArgumentException("预估重量必须大于0");

                // 调用DAL创建运输单
                return _dal.CreateTransportationOrder(order);
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
        /// <param name="region">运输人员负责的区域</param>
        /// <returns>运输单列表</returns>
        public List<TransportationOrders> GetTransportationOrdersByTransporter(int transporterId, string region)
        {
            try
            {
                if (transporterId <= 0)
                    throw new ArgumentException("运输人员ID无效");

                return _dal.GetTransportationOrdersByTransporter(transporterId, region);
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
        /// </summary>
        /// <param name="orderId">运输单ID</param>
        /// <returns>是否更新成功</returns>
        public bool StartTransportation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("运输单ID无效");

                return _dal.StartTransportation(orderId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartTransportation BLL Error: {ex.Message}");
                throw;
            }
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

                return _dal.CompleteTransportation(orderId, actualWeight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompleteTransportation BLL Error: {ex.Message}");
                throw;
            }
        }
    }
}
