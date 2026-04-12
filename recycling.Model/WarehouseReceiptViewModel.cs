using System;
using System.Collections.Generic;

namespace recycling.Model
{
    // 中文注释
    /// 入库单视图模型
    /// 中文注释
    // 中文注释
    public class WarehouseReceiptViewModel
    {
        // 中文注释
        /// 入库单ID
        // 中文注释
        public int ReceiptID { get; set; }

        // 中文注释
        /// 入库单号
        // 中文注释
        public string ReceiptNumber { get; set; }

        // 中文注释
        /// 运输单ID
        // 中文注释
        public int TransportOrderID { get; set; }

        // 中文注释
        /// 运输单号
        // 中文注释
        public string TransportOrderNumber { get; set; }

        // 中文注释
        /// 回收员ID
        // 中文注释
        public int RecyclerID { get; set; }

        // 中文注释
        /// 回收员姓名
        // 中文注释
        public string RecyclerName { get; set; }

        // 中文注释
        /// 基地人员ID
        // 中文注释
        public int WorkerID { get; set; }

        // 中文注释
        /// 基地人员姓名
        // 中文注释
        public string WorkerName { get; set; }

        // 中文注释
        /// 入库总重量（kg）
        // 中文注释
        public decimal TotalWeight { get; set; }

        // 中文注释
        /// 物品类别（JSON格式）
        // 中文注释
        public string ItemCategories { get; set; }

        // 中文注释
        /// 状态
        // 中文注释
        public string Status { get; set; }

        // 中文注释
        /// 备注信息
        // 中文注释
        public string Notes { get; set; }

        // 中文注释
        /// 入库时间
        // 中文注释
        public DateTime CreatedDate { get; set; }

        // 中文注释
        /// 运输单指定的基地工作人员ID
        /// 中文注释
        // 中文注释
        public int? AssignedWorkerID { get; set; }

        // 中文注释
        /// 是否已完成细分
        // 中文注释
        public bool IsRefined { get; set; }
    }

    // 中文注释
    /// 运输中通知视图模型
    /// 中文注释
    // 中文注释
    public class TransportNotificationViewModel
    {
        // 中文注释
        /// 运输单ID
        // 中文注释
        public int TransportOrderID { get; set; }

        // 中文注释
        /// 运输单号
        // 中文注释
        public string OrderNumber { get; set; }

        // 中文注释
        /// 回收员姓名
        // 中文注释
        public string RecyclerName { get; set; }

        // 中文注释
        /// 运输人员姓名
        // 中文注释
        public string TransporterName { get; set; }

        // 中文注释
        /// 预估重量
        // 中文注释
        public decimal EstimatedWeight { get; set; }

        // 中文注释
        /// 物品类别
        // 中文注释
        public string ItemCategories { get; set; }

        // 中文注释
        /// 创建时间
        // 中文注释
        public DateTime? CreatedDate { get; set; }

        // 中文注释
        /// 状态
        // 中文注释
        public string Status { get; set; }

        // 中文注释
        /// 指定的基地工作人员ID
        /// 中文注释
        // 中文注释
        public int? AssignedWorkerID { get; set; }

        // 中文注释
        /// 基地联系人
        /// 中文注释
        // 中文注释
        public string BaseContactPerson { get; set; }

        // 中文注释
        /// 是否已创建入库单
        /// 中文注释
        // 中文注释
        public bool HasReceipt { get; set; }
    }

    // 中文注释
    /// 入库单细分页面视图模型
    // 中文注释
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
        public List<WarehouseReceiptCategoryItemViewModel> ExistingSubdivisions { get; set; } = new List<WarehouseReceiptCategoryItemViewModel>();
    }

    // 中文注释
    /// 入库单类别项
    // 中文注释
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

    // 中文注释
    /// 细分模板分组（按大类）
    // 中文注释
    public class WarehouseSubdivisionTemplateGroupViewModel
    {
        public string ParentCategoryKey { get; set; }
        public string ParentCategoryName { get; set; }
        public List<WarehouseSubdivisionTemplateOptionViewModel> Options { get; set; } = new List<WarehouseSubdivisionTemplateOptionViewModel>();
    }

    // 中文注释
    /// 细分模板选项
    // 中文注释
    public class WarehouseSubdivisionTemplateOptionViewModel
    {
        public string SubCategoryKey { get; set; }
        public string SubCategoryName { get; set; }
        public decimal PricePerKg { get; set; }
    }
}
