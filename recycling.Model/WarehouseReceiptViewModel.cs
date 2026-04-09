using System;
using System.Collections.Generic;

namespace recycling.Model
{
    /// <summary>
    /// 入库单视图模型
    /// Warehouse Receipt View Model
    /// </summary>
    public class WarehouseReceiptViewModel
    {
        /// <summary>
        /// 入库单ID
        /// </summary>
        public int ReceiptID { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// 运输单ID
        /// </summary>
        public int TransportOrderID { get; set; }

        /// <summary>
        /// 运输单号
        /// </summary>
        public string TransportOrderNumber { get; set; }

        /// <summary>
        /// 回收员ID
        /// </summary>
        public int RecyclerID { get; set; }

        /// <summary>
        /// 回收员姓名
        /// </summary>
        public string RecyclerName { get; set; }

        /// <summary>
        /// 基地人员ID
        /// </summary>
        public int WorkerID { get; set; }

        /// <summary>
        /// 基地人员姓名
        /// </summary>
        public string WorkerName { get; set; }

        /// <summary>
        /// 入库总重量（kg）
        /// </summary>
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// 物品类别（JSON格式）
        /// </summary>
        public string ItemCategories { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 运输单指定的基地工作人员ID
        /// Assigned worker ID from transport order
        /// </summary>
        public int? AssignedWorkerID { get; set; }

        /// <summary>
        /// 是否已完成细分
        /// </summary>
        public bool IsRefined { get; set; }
    }

    /// <summary>
    /// 运输中通知视图模型
    /// In-Transit Notification View Model
    /// </summary>
    public class TransportNotificationViewModel
    {
        /// <summary>
        /// 运输单ID
        /// </summary>
        public int TransportOrderID { get; set; }

        /// <summary>
        /// 运输单号
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// 回收员姓名
        /// </summary>
        public string RecyclerName { get; set; }

        /// <summary>
        /// 运输人员姓名
        /// </summary>
        public string TransporterName { get; set; }

        /// <summary>
        /// 预估重量
        /// </summary>
        public decimal EstimatedWeight { get; set; }

        /// <summary>
        /// 物品类别
        /// </summary>
        public string ItemCategories { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 指定的基地工作人员ID
        /// Assigned sorting center worker ID
        /// </summary>
        public int? AssignedWorkerID { get; set; }

        /// <summary>
        /// 基地联系人
        /// Base contact person name
        /// </summary>
        public string BaseContactPerson { get; set; }
    }

    /// <summary>
    /// 入库单细分页面视图模型
    /// </summary>
    public class WarehouseReceiptSubdivisionViewModel
    {
        public int ReceiptID { get; set; }
        public string ReceiptNumber { get; set; }
        public decimal TotalWeight { get; set; }
        public string Status { get; set; }
        public bool IsRefined { get; set; }
        public decimal ValidationTolerance { get; set; }
        public List<WarehouseReceiptCategoryItemViewModel> Categories { get; set; } = new List<WarehouseReceiptCategoryItemViewModel>();
        public List<WarehouseSubdivisionTemplateGroupViewModel> SubdivisionTemplates { get; set; } = new List<WarehouseSubdivisionTemplateGroupViewModel>();
    }

    /// <summary>
    /// 入库单类别项
    /// </summary>
    public class WarehouseReceiptCategoryItemViewModel
    {
        public string CategoryKey { get; set; }
        public string CategoryName { get; set; }
        public decimal Weight { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? TotalAmount { get; set; }
        public string ParentCategoryKey { get; set; }
        public string ParentCategoryName { get; set; }
    }

    /// <summary>
    /// 细分模板分组（按大类）
    /// </summary>
    public class WarehouseSubdivisionTemplateGroupViewModel
    {
        public string ParentCategoryKey { get; set; }
        public string ParentCategoryName { get; set; }
        public List<WarehouseSubdivisionTemplateOptionViewModel> Options { get; set; } = new List<WarehouseSubdivisionTemplateOptionViewModel>();
    }

    /// <summary>
    /// 细分模板选项
    /// </summary>
    public class WarehouseSubdivisionTemplateOptionViewModel
    {
        public string SubCategoryKey { get; set; }
        public string SubCategoryName { get; set; }
        public decimal PricePerKg { get; set; }
    }
}
