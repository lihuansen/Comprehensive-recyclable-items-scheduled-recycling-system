using System;
using System.Collections.Generic;

namespace recycling.Model
{
    /// 入库单视图模型
    /// 仓库入库单视图模型。
    public class WarehouseReceiptViewModel
    {
        /// 入库单ID
        public int ReceiptID { get; set; }

        /// 入库单号
        public string ReceiptNumber { get; set; }

        /// 运输单ID
        public int TransportOrderID { get; set; }

        /// 运输单号
        public string TransportOrderNumber { get; set; }

        /// 回收员ID
        public int RecyclerID { get; set; }

        /// 回收员姓名
        public string RecyclerName { get; set; }

        /// 基地人员ID
        public int WorkerID { get; set; }

        /// 基地人员姓名
        public string WorkerName { get; set; }

        /// 入库总重量（kg）
        public decimal TotalWeight { get; set; }

        /// 物品类别（JSON格式）
        public string ItemCategories { get; set; }

        /// 状态
        public string Status { get; set; }

        /// 备注信息
        public string Notes { get; set; }

        /// 入库时间
        public DateTime CreatedDate { get; set; }

        /// 运输单指定的基地工作人员ID
        /// 表示已分配分拣员编号。
        public int? AssignedWorkerID { get; set; }

        /// 是否已完成细分
        public bool IsRefined { get; set; }
    }

    /// 运输中通知视图模型
    /// 运输通知视图模型。
    public class TransportNotificationViewModel
    {
        /// 运输单ID
        public int TransportOrderID { get; set; }

        /// 运输单号
        public string OrderNumber { get; set; }

        /// 回收员姓名
        public string RecyclerName { get; set; }

        /// 运输人员姓名
        public string TransporterName { get; set; }

        /// 预估重量
        public decimal EstimatedWeight { get; set; }

        /// 物品类别
        public string ItemCategories { get; set; }

        /// 创建时间
        public DateTime? CreatedDate { get; set; }

        /// 状态
        public string Status { get; set; }

        /// 指定的基地工作人员ID
        /// 表示已分配分拣员编号。
        public int? AssignedWorkerID { get; set; }

        /// 基地联系人
        /// 表示基地联系人。
        public string BaseContactPerson { get; set; }

        /// 是否已创建入库单
        /// 表示是否已生成入库单。
        public bool HasReceipt { get; set; }
    }

    /// 入库单细分页面视图模型
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

    /// 入库单类别项
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

    /// 细分模板分组（按大类）
    public class WarehouseSubdivisionTemplateGroupViewModel
    {
        public string ParentCategoryKey { get; set; }
        public string ParentCategoryName { get; set; }
        public List<WarehouseSubdivisionTemplateOptionViewModel> Options { get; set; } = new List<WarehouseSubdivisionTemplateOptionViewModel>();
    }

    /// 细分模板选项
    public class WarehouseSubdivisionTemplateOptionViewModel
    {
        public string SubCategoryKey { get; set; }
        public string SubCategoryName { get; set; }
        public decimal PricePerKg { get; set; }
    }
}
