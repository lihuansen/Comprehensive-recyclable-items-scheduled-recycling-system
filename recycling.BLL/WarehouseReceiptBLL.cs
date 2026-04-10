using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
        // 重量校验容差（单位kg），用于处理小数四舍五入导致的微小误差
        private const decimal WeightTolerance = 0.01m;
        private const int CategoryFieldMaxLength = 50;
        private const int MaxSubdivisionsPerParent = 10;
        private const decimal DefaultManualSubCategoryPricePerKg = 1.00m;

        private static readonly Dictionary<string, List<WarehouseSubdivisionTemplateOptionViewModel>> DefaultSubdivisionTemplates =
            new Dictionary<string, List<WarehouseSubdivisionTemplateOptionViewModel>>(StringComparer.OrdinalIgnoreCase)
            {
                ["glass"] = new List<WarehouseSubdivisionTemplateOptionViewModel>
                {
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "clear", SubCategoryName = "白玻璃", PricePerKg = 0.30m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "green", SubCategoryName = "绿玻璃", PricePerKg = 0.25m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "brown", SubCategoryName = "棕玻璃", PricePerKg = 0.28m }
                },
                ["metal"] = new List<WarehouseSubdivisionTemplateOptionViewModel>
                {
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "aluminum", SubCategoryName = "铝制品", PricePerKg = 6.50m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "steel", SubCategoryName = "钢铁制品", PricePerKg = 2.80m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "mixed", SubCategoryName = "混合金属", PricePerKg = 3.60m }
                },
                ["plastic"] = new List<WarehouseSubdivisionTemplateOptionViewModel>
                {
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "pet", SubCategoryName = "PET塑料", PricePerKg = 2.60m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "hdpe", SubCategoryName = "HDPE塑料", PricePerKg = 2.30m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "pp", SubCategoryName = "PP塑料", PricePerKg = 1.80m }
                },
                ["paper"] = new List<WarehouseSubdivisionTemplateOptionViewModel>
                {
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "newspaper", SubCategoryName = "报纸", PricePerKg = 1.20m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "cardboard", SubCategoryName = "纸板", PricePerKg = 0.95m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "mixed", SubCategoryName = "混合废纸", PricePerKg = 0.70m }
                },
                ["fabric"] = new List<WarehouseSubdivisionTemplateOptionViewModel>
                {
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "cotton", SubCategoryName = "棉织物", PricePerKg = 1.40m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "polyester", SubCategoryName = "化纤织物", PricePerKg = 1.10m },
                    new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "mixed", SubCategoryName = "混合织物", PricePerKg = 0.90m }
                }
            };
        private static readonly Dictionary<string, string> CategoryTemplateAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["塑料"] = "plastic",
            ["塑料类"] = "plastic",
            ["塑胶"] = "plastic",
            ["玻璃"] = "glass",
            ["金属"] = "metal",
            ["纸"] = "paper",
            ["纸类"] = "paper",
            ["织物"] = "fabric",
            ["纺织品"] = "fabric"
        };
        private static readonly Dictionary<string, string> CategoryTemplateKeywordAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["plastic"] = "plastic",
            ["塑料"] = "plastic",
            ["塑胶"] = "plastic",
            ["glass"] = "glass",
            ["玻璃"] = "glass",
            ["metal"] = "metal",
            ["金属"] = "metal",
            ["paper"] = "paper",
            ["纸"] = "paper",
            ["fabric"] = "fabric",
            ["织物"] = "fabric",
            ["纺织"] = "fabric"
        };

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

                // 4. 获取品类详细信息（优先使用TransportationOrderCategories表中的结构化数据）
                string finalItemCategories = itemCategories;
                try
                {
                    var categoriesDAL = new TransportationOrderCategoriesDAL();
                    if (categoriesDAL.TableExists())
                    {
                        var categoryDetails = categoriesDAL.GetCategoriesByTransportOrderId(transportOrderId);
                        if (categoryDetails != null && categoryDetails.Count > 0)
                        {
                            // 构建JSON格式的品类信息，确保数据对齐
                            var categoryList = new List<Dictionary<string, object>>();
                            foreach (var cat in categoryDetails)
                            {
                                categoryList.Add(new Dictionary<string, object>
                                {
                                    { "categoryKey", cat.CategoryKey },
                                    { "categoryName", cat.CategoryName },
                                    { "weight", cat.Weight },
                                    { "pricePerKg", cat.PricePerKg },
                                    { "totalAmount", cat.TotalAmount }
                                });
                            }
                            finalItemCategories = Newtonsoft.Json.JsonConvert.SerializeObject(categoryList);
                            System.Diagnostics.Debug.WriteLine($"从TransportationOrderCategories表获取了 {categoryDetails.Count} 条品类详细信息");
                        }
                        else if (string.IsNullOrEmpty(finalItemCategories))
                        {
                            // 如果没有结构化数据，也没有传入的itemCategories，尝试从运输单的ItemCategories字段获取
                            finalItemCategories = transportOrder.ItemCategories;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"获取品类详细信息失败: {ex.Message}，使用传入的itemCategories");
                }

                // 5. 创建入库单（状态为"待入库"）
                var receipt = new WarehouseReceipts
                {
                    TransportOrderID = transportOrderId,
                    RecyclerID = transportOrder.RecyclerID,
                    WorkerID = workerId,
                    TotalWeight = totalWeight,
                    ItemCategories = finalItemCategories,
                    Notes = notes
                };

                var (receiptId, receiptNumber) = _dal.CreateWarehouseReceipt(receipt);

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

                if (!_dal.IsReceiptRefined(receipt.ItemCategories))
                {
                    return (false, "请先完成入库单细分，再执行入库");
                }

                // 2. 处理入库（写入库存并更新状态）
                bool result = _dal.ProcessWarehouseReceipt(receiptId);

                if (result)
                {
                    // 3. 获取运输单信息用于通知和状态更新
                    var transportOrder = receipt.TransportOrderID.HasValue 
                        ? _transportDAL.GetTransportationOrderById(receipt.TransportOrderID.Value) 
                        : null;

                    // 3.1 入库成功后，检查并更新基地工作人员状态为"空闲"
                    try
                    {
                        if (transportOrder != null && transportOrder.AssignedWorkerID.HasValue)
                        {
                            int assignedWorkerId = transportOrder.AssignedWorkerID.Value;
                            // 检查该工作人员是否还有其他未完成的任务
                            if (!_transportDAL.HasPendingTasksForWorker(assignedWorkerId))
                            {
                                _transportDAL.UpdateSortingCenterWorkerStatus(assignedWorkerId, "空闲");
                                System.Diagnostics.Debug.WriteLine($"基地工作人员 {assignedWorkerId} 所有任务已完成，状态已更新为空闲");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"基地工作人员 {assignedWorkerId} 还有其他未完成任务，保持工作中状态");
                            }
                        }
                    }
                    catch (Exception statusEx)
                    {
                        // 状态更新失败不影响入库操作
                        System.Diagnostics.Debug.WriteLine($"更新基地工作人员状态失败: {statusEx.Message}");
                    }

                    // 4. 发送入库完成通知给回收员
                    try
                    {
                        if (receipt.RecyclerID.HasValue)
                        {
                            _notificationBLL.SendNotification(
                                receipt.RecyclerID.Value,
                                "入库完成",
                                $"您的运输单 {transportOrder?.OrderNumber ?? ""} 已成功入库至基地，入库单号：{receipt.ReceiptNumber}，总重量：{receipt.TotalWeight}kg",
                                "WarehouseReceipt",
                                receiptId);
                        }
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
                            receipt.TotalWeight ?? 0);
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

        /// <summary>
        /// 获取细分模板（按大类）
        /// </summary>
        public List<WarehouseSubdivisionTemplateGroupViewModel> GetSubdivisionTemplates(IEnumerable<WarehouseReceiptCategoryItemViewModel> categories)
        {
            var result = new List<WarehouseSubdivisionTemplateGroupViewModel>();
            var categoryList = categories?.ToList() ?? new List<WarehouseReceiptCategoryItemViewModel>();

            foreach (var category in categoryList)
            {
                var key = (category?.CategoryKey ?? string.Empty).Trim();
                var name = string.IsNullOrWhiteSpace(category?.CategoryName) ? "未命名大类" : category.CategoryName.Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                var templateKey = ResolveTemplateParentKey(key, name);
                if (!DefaultSubdivisionTemplates.TryGetValue(templateKey, out var options))
                {
                    options = new List<WarehouseSubdivisionTemplateOptionViewModel>
                    {
                        new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "normal", SubCategoryName = "标准分拣", PricePerKg = 1.00m },
                        new WarehouseSubdivisionTemplateOptionViewModel { SubCategoryKey = "high", SubCategoryName = "高质量分拣", PricePerKg = 1.20m }
                    };
                }

                result.Add(new WarehouseSubdivisionTemplateGroupViewModel
                {
                    ParentCategoryKey = key,
                    ParentCategoryName = name,
                    Options = options.Select(o => new WarehouseSubdivisionTemplateOptionViewModel
                    {
                        SubCategoryKey = o.SubCategoryKey,
                        SubCategoryName = o.SubCategoryName,
                        PricePerKg = o.PricePerKg
                    }).ToList()
                });
            }

            return result;
        }

        /// <summary>
        /// 根据细分类解析单价（优先系统模板，手写细类使用该大类模板均价）
        /// </summary>
        private decimal ResolvePricePerKg(string parentKey, string subKey, string subName, decimal? submittedPricePerKg)
        {
            var templateParentKey = ResolveTemplateParentKey(parentKey, null);
            if (!string.IsNullOrWhiteSpace(templateParentKey) &&
                DefaultSubdivisionTemplates.TryGetValue(templateParentKey, out var options) &&
                options != null && options.Any())
            {
                var matched = options.FirstOrDefault(o =>
                    (!string.IsNullOrWhiteSpace(subKey) &&
                      (string.Equals(o.SubCategoryKey, subKey, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals($"{parentKey}_{o.SubCategoryKey}", subKey, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals($"{templateParentKey}_{o.SubCategoryKey}", subKey, StringComparison.OrdinalIgnoreCase))) ||
                    (!string.IsNullOrWhiteSpace(subName) &&
                     string.Equals(o.SubCategoryName, subName, StringComparison.OrdinalIgnoreCase)));

                if (matched != null)
                {
                    return Math.Round(matched.PricePerKg, 2, MidpointRounding.AwayFromZero);
                }

                return Math.Round(options.Average(o => o.PricePerKg), 2, MidpointRounding.AwayFromZero);
            }

            if (submittedPricePerKg.HasValue && submittedPricePerKg.Value >= 0)
            {
                return Math.Round(submittedPricePerKg.Value, 2, MidpointRounding.AwayFromZero);
            }

            return DefaultManualSubCategoryPricePerKg;
        }

        private string ResolveTemplateParentKey(string parentKey, string parentName)
        {
            var key = (parentKey ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                key = (parentName ?? string.Empty).Trim();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (DefaultSubdivisionTemplates.ContainsKey(key))
            {
                return key;
            }

            if (CategoryTemplateAliases.TryGetValue(key, out var aliasKey))
            {
                return aliasKey;
            }

            var normalizedKey = NormalizeCategoryToken(key);
            if (CategoryTemplateAliases.TryGetValue(normalizedKey, out aliasKey))
            {
                return aliasKey;
            }

            foreach (var keywordAlias in CategoryTemplateKeywordAliases)
            {
                if (normalizedKey.IndexOf(keywordAlias.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return keywordAlias.Value;
                }
            }

            return key;
        }

        private static string NormalizeCategoryToken(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var chars = new List<char>(input.Length);
            foreach (var c in input)
            {
                if (char.IsWhiteSpace(c) || c == '_' || c == '-')
                {
                    continue;
                }
                chars.Add(c);
            }
            return new string(chars.ToArray());
        }

        /// <summary>
        /// 保存入库单细分数据
        /// </summary>
        public (bool success, string message) SaveWarehouseReceiptSubdivision(int receiptId, int workerId, string subdivisionsJson)
        {
            try
            {
                if (receiptId <= 0)
                {
                    return (false, "入库单参数无效");
                }

                if (string.IsNullOrWhiteSpace(subdivisionsJson))
                {
                    return (false, "细分数据不能为空");
                }

                var receipt = _dal.GetWarehouseReceiptById(receiptId);
                if (receipt == null)
                {
                    return (false, "入库单不存在");
                }

                if (receipt.Status != "待入库")
                {
                    return (false, "当前入库单状态不允许细分");
                }

                if (!receipt.WorkerID.HasValue || receipt.WorkerID.Value != workerId)
                {
                    return (false, "仅入库单创建人可执行细分");
                }

                var originalCategories = JsonConvert.DeserializeObject<List<WarehouseReceiptCategoryItemViewModel>>(
                    string.IsNullOrWhiteSpace(receipt.ItemCategories) ? "[]" : receipt.ItemCategories) ?? new List<WarehouseReceiptCategoryItemViewModel>();

                var originalWeightMap = originalCategories
                    .Where(c => c != null && !string.IsNullOrWhiteSpace(c.CategoryKey) && c.Weight > 0)
                    .GroupBy(c => c.CategoryKey.Trim())
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Weight));

                if (!originalWeightMap.Any())
                {
                    return (false, "原始品类数据无效，无法细分");
                }

                var submitted = JsonConvert.DeserializeObject<List<WarehouseReceiptCategoryItemViewModel>>(subdivisionsJson) ?? new List<WarehouseReceiptCategoryItemViewModel>();
                if (!submitted.Any())
                {
                    return (false, "请至少提供一条细分明细");
                }

                var normalized = new List<WarehouseReceiptCategoryItemViewModel>();
                var groupedWeight = new Dictionary<string, decimal>();
                var groupedSubCategoryKeys = new Dictionary<string, HashSet<string>>();
                decimal totalWeight = 0;
                decimal totalPrice = 0;

                foreach (var item in submitted)
                {
                    if (item == null) continue;

                    var parentKey = (item.ParentCategoryKey ?? string.Empty).Trim();
                    var parentName = (item.ParentCategoryName ?? string.Empty).Trim();
                    var subKey = (item.CategoryKey ?? string.Empty).Trim();
                    var subName = (item.CategoryName ?? string.Empty).Trim();
                    var weight = Math.Round(item.Weight, 2, MidpointRounding.AwayFromZero);

                    if (string.IsNullOrWhiteSpace(parentKey) || string.IsNullOrWhiteSpace(subKey) || string.IsNullOrWhiteSpace(subName))
                    {
                        return (false, "细分项存在空类别字段");
                    }

                    var normalizedSubKey = subKey.StartsWith(parentKey + "_", StringComparison.OrdinalIgnoreCase)
                        ? subKey
                        : $"{parentKey}_{subKey}";

                    if (parentKey.Length > CategoryFieldMaxLength || normalizedSubKey.Length > CategoryFieldMaxLength || subName.Length > CategoryFieldMaxLength)
                    {
                        return (false, "细分项类别字段长度超限");
                    }
                    subKey = normalizedSubKey;

                    if (!originalWeightMap.ContainsKey(parentKey))
                    {
                        return (false, $"细分项所属大类无效：{parentKey}");
                    }

                    if (weight <= 0)
                    {
                        return (false, "细分项重量必须大于0");
                    }

                    if (!groupedSubCategoryKeys.ContainsKey(parentKey))
                    {
                        groupedSubCategoryKeys[parentKey] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                    groupedSubCategoryKeys[parentKey].Add(subKey);

                    if (groupedSubCategoryKeys[parentKey].Count > MaxSubdivisionsPerParent)
                    {
                        return (false, $"大类 {parentKey} 细分类最多允许 {MaxSubdivisionsPerParent} 个");
                    }

                    var pricePerKg = ResolvePricePerKg(parentKey, subKey, subName, item.PricePerKg);
                    var amount = Math.Round(weight * pricePerKg, 2, MidpointRounding.AwayFromZero);

                    if (!groupedWeight.ContainsKey(parentKey))
                    {
                        groupedWeight[parentKey] = 0;
                    }
                    groupedWeight[parentKey] += weight;
                    totalWeight += weight;
                    totalPrice += amount;

                    normalized.Add(new WarehouseReceiptCategoryItemViewModel
                    {
                        CategoryKey = subKey,
                        CategoryName = subName,
                        ParentCategoryKey = parentKey,
                        ParentCategoryName = string.IsNullOrWhiteSpace(parentName)
                            ? originalCategories.FirstOrDefault(c => c?.CategoryKey == parentKey)?.CategoryName ?? parentKey
                            : parentName,
                        Weight = weight,
                        PricePerKg = pricePerKg,
                        TotalAmount = amount
                    });
                }

                foreach (var original in originalWeightMap)
                {
                    if (!groupedWeight.ContainsKey(original.Key))
                    {
                        return (false, $"大类 {original.Key} 尚未完成细分");
                    }

                    if (groupedWeight[original.Key] <= 0)
                    {
                        return (false, $"大类 {original.Key} 细分重量必须大于0");
                    }

                    if (groupedWeight[original.Key] - original.Value > WeightTolerance)
                    {
                        return (false, $"大类 {original.Key} 细分重量不能超过原重量");
                    }
                }

                var receiptTotalWeight = receipt.TotalWeight ?? 0;
                if (totalWeight - receiptTotalWeight > WeightTolerance)
                {
                    return (false, "细分总重量不能超过入库单总重量");
                }

                var normalizedJson = JsonConvert.SerializeObject(normalized);
                var updateResult = _dal.UpdateWarehouseReceiptSubdivision(receiptId, normalizedJson, Math.Round(totalPrice, 2, MidpointRounding.AwayFromZero));

                if (!updateResult)
                {
                    return (false, "保存失败，请确认入库单状态仍为待入库");
                }

                try
                {
                    TransportationOrders transportOrder = null;
                    if (receipt.TransportOrderID.HasValue)
                    {
                        try
                        {
                            transportOrder = _transportDAL.GetTransportationOrderById(receipt.TransportOrderID.Value);
                        }
                        catch (Exception transportEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"读取运输单失败（TransportOrderID={receipt.TransportOrderID.Value}）: {transportEx.Message}");
                        }
                    }

                    _baseStaffNotificationBLL.SendWarehouseReceiptCreatedNotification(
                        receipt.ReceiptID,
                        receipt.ReceiptNumber,
                        receipt.TransportOrderID,
                        transportOrder?.OrderNumber ?? string.Empty,
                        receipt.TotalWeight ?? 0,
                        workerId);
                }
                catch (Exception notifyEx)
                {
                    System.Diagnostics.Debug.WriteLine($"发送基地工作人员细分完成通知失败: {notifyEx.Message}");
                }

                return (true, "细分保存成功");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveWarehouseReceiptSubdivision BLL Error: {ex.Message}");
                return (false, $"保存细分失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取细分校验容差（kg）
        /// </summary>
        public decimal GetSubdivisionWeightTolerance()
        {
            return WeightTolerance;
        }
    }
}
