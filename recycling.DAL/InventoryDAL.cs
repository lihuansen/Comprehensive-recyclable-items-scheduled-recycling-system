using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using recycling.Model;

namespace recycling.DAL
{
    public class InventoryDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// 添加库存记录（从订单的类别重量写入）
        public bool AddInventoryFromOrder(int orderId, int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 获取订单的类别和重量信息，以及订单的估算价格
                        string getCategoriesSql = @"
                            SELECT CategoryKey, CategoryName, Weight
                            FROM AppointmentCategories
                            WHERE AppointmentID = @OrderID AND Weight > 0";

                        List<(string key, string name, decimal weight)> categories = new List<(string, string, decimal)>();

                        using (SqlCommand cmd = new SqlCommand(getCategoriesSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    categories.Add((
                                        reader["CategoryKey"].ToString(),
                                        reader["CategoryName"].ToString(),
                                        Convert.ToDecimal(reader["Weight"])
                                    ));
                                }
                            }
                        }

                        if (categories.Count == 0)
                        {
                            trans.Rollback();
                            return false; // 没有类别数据
                        }

                        // 获取订单的估算价格
                        string getPriceSql = "SELECT EstimatedPrice FROM Appointments WHERE AppointmentID = @OrderID";
                        decimal? orderPrice = null;
                        using (SqlCommand cmd = new SqlCommand(getPriceSql, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            var result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                orderPrice = Convert.ToDecimal(result);
                            }
                        }

                        // 计算总重量用于按比例分配价格
                        decimal totalWeight = 0;
                        foreach (var category in categories)
                        {
                            totalWeight += category.weight;
                        }

                        // 插入库存记录
                        string insertSql = @"
                            INSERT INTO Inventory (OrderID, CategoryKey, CategoryName, Weight, Price, RecyclerID, CreatedDate, InventoryType)
                            VALUES (@OrderID, @CategoryKey, @CategoryName, @Weight, @Price, @RecyclerID, @CreatedDate, N'StoragePoint')";

                        foreach (var category in categories)
                        {
                            // 按照重量比例分配价格
                            decimal? categoryPrice = null;
                            if (orderPrice.HasValue && totalWeight > 0)
                            {
                                categoryPrice = orderPrice.Value * (category.weight / totalWeight);
                            }

                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@CategoryKey", category.key);
                                cmd.Parameters.AddWithValue("@CategoryName", category.name);
                                cmd.Parameters.AddWithValue("@Weight", category.weight);
                                cmd.Parameters.AddWithValue("@Price", categoryPrice.HasValue ? (object)categoryPrice.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// 直接从核实后的品类/重量/金额列表写入库存记录
        public bool AddInventoryWithItems(int orderId, int recyclerId, List<(string CategoryKey, string CategoryName, decimal Weight, decimal Price)> items)
        {
            if (orderId <= 0 || recyclerId <= 0 || items == null || items.Count == 0) return false;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string insertSql = @"
                            INSERT INTO Inventory (OrderID, CategoryKey, CategoryName, Weight, Price, RecyclerID, CreatedDate, InventoryType)
                            VALUES (@OrderID, @CategoryKey, @CategoryName, @Weight, @Price, @RecyclerID, @CreatedDate, N'StoragePoint')";

                        foreach (var item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand(insertSql, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@CategoryKey", item.CategoryKey);
                                cmd.Parameters.AddWithValue("@CategoryName", item.CategoryName);
                                cmd.Parameters.AddWithValue("@Weight", item.Weight);
                                cmd.Parameters.AddWithValue("@Price", item.Price);
                                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// 获取库存列表
        /// <param name="recyclerId">回收员ID（可选）</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="inventoryType">库存类型：StoragePoint(暂存点) 或 Warehouse(仓库)，默认为StoragePoint</param>
        public List<Inventory> GetInventoryList(int? recyclerId = null, int pageIndex = 1, int pageSize = 50, string inventoryType = "StoragePoint")
        {
            List<Inventory> list = new List<Inventory>();
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT InventoryID, OrderID, CategoryKey, CategoryName, Weight, Price, RecyclerID, CreatedDate, InventoryType
                    FROM Inventory
                    WHERE (@RecyclerID IS NULL OR RecyclerID = @RecyclerID)
                      AND InventoryType = @InventoryType
                    ORDER BY CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId.HasValue ? (object)recyclerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@InventoryType", inventoryType);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Inventory
                            {
                                InventoryID = Convert.ToInt32(reader["InventoryID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CategoryKey = reader["CategoryKey"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : Convert.ToDecimal(reader["Price"]),
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                InventoryType = reader["InventoryType"].ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        /// 获取库存汇总（按类别分组）
        /// <param name="recyclerId">回收员ID（可选）</param>
        /// <param name="inventoryType">库存类型：StoragePoint(暂存点) 或 Warehouse(仓库)，默认为Warehouse</param>
        public List<(string CategoryKey, string CategoryName, decimal TotalWeight, decimal TotalPrice)> GetInventorySummary(int? recyclerId = null, string inventoryType = "Warehouse")
        {
            var summary = new List<(string, string, decimal, decimal)>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        CategoryKey, 
                        CategoryName, 
                        SUM(Weight) AS TotalWeight,
                        SUM(ISNULL(Price, 0)) AS TotalPrice
                    FROM Inventory
                    WHERE (@RecyclerID IS NULL OR RecyclerID = @RecyclerID)
                      AND InventoryType = @InventoryType
                    GROUP BY CategoryKey, CategoryName
                    ORDER BY CategoryName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId.HasValue ? (object)recyclerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@InventoryType", inventoryType);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summary.Add((
                                reader["CategoryKey"].ToString(),
                                reader["CategoryName"].ToString(),
                                Convert.ToDecimal(reader["TotalWeight"]),
                                Convert.ToDecimal(reader["TotalPrice"])
                            ));
                        }
                    }
                }
            }

            return summary;
        }

        /// 获取库存明细（包含回收员信息）- 管理员端使用
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="categoryKey">类别键（可选）</param>
        /// <param name="inventoryType">库存类型：StoragePoint(暂存点) 或 Warehouse(仓库)，默认为Warehouse</param>
        public PagedResult<InventoryDetailViewModel> GetInventoryDetailWithRecycler(int pageIndex = 1, int pageSize = 20, string categoryKey = null, string inventoryType = "Warehouse")
        {
            var result = new PagedResult<InventoryDetailViewModel>
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 获取总数
                string countSql = @"
                    SELECT COUNT(*) 
                    FROM Inventory i
                    WHERE (@CategoryKey IS NULL OR @CategoryKey = '' OR i.CategoryKey = @CategoryKey)
                      AND i.InventoryType = @InventoryType";

                conn.Open();

                using (SqlCommand cmd = new SqlCommand(countSql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryKey", string.IsNullOrEmpty(categoryKey) ? (object)DBNull.Value : categoryKey);
                    cmd.Parameters.AddWithValue("@InventoryType", inventoryType);
                    result.TotalCount = (int)cmd.ExecuteScalar();
                }

                // 获取数据
                string sql = @"
                    SELECT 
                        i.InventoryID, 
                        i.OrderID, 
                        ISNULL(i.ReceiptID, i.OrderID) AS ReceiptIdForMatch,
                        ISNULL(wr.ReceiptNumber, 'WR' + RIGHT('000000' + CAST(i.OrderID AS VARCHAR(6)), 6)) AS OrderNumber,
                        i.CategoryKey, 
                        i.CategoryName, 
                        i.Weight, 
                        i.Price, 
                        i.RecyclerID, 
                        ISNULL(r.Username, '未知回收员') AS RecyclerName,
                        i.CreatedDate,
                        wr.ItemCategories
                    FROM Inventory i
                    LEFT JOIN Recyclers r ON i.RecyclerID = r.RecyclerID
                    LEFT JOIN WarehouseReceipts wr ON (i.ReceiptID = wr.ReceiptID OR (i.ReceiptID IS NULL AND i.OrderID = wr.ReceiptID))
                    WHERE (@CategoryKey IS NULL OR @CategoryKey = '' OR i.CategoryKey = @CategoryKey)
                      AND i.InventoryType = @InventoryType
                    ORDER BY i.CreatedDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryKey", string.IsNullOrEmpty(categoryKey) ? (object)DBNull.Value : categoryKey);
                    cmd.Parameters.AddWithValue("@InventoryType", inventoryType);
                    cmd.Parameters.AddWithValue("@Offset", (pageIndex - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var refinedCategoryMapCache = new Dictionary<int, Dictionary<string, WarehouseReceiptCategoryItemViewModel>>();

                        while (reader.Read())
                        {
                            var receiptIdForMatch = Convert.ToInt32(reader["ReceiptIdForMatch"]);
                            var currentCategoryKey = reader["CategoryKey"].ToString();
                            var currentCategoryName = reader["CategoryName"].ToString();
                            var itemCategoriesJson = reader["ItemCategories"] == DBNull.Value ? null : reader["ItemCategories"].ToString();

                            string parentCategoryName = string.Empty;
                            string subCategoryName = currentCategoryName;

                            if (inventoryType == "Warehouse" && receiptIdForMatch > 0 && !string.IsNullOrWhiteSpace(itemCategoriesJson))
                            {
                                if (!refinedCategoryMapCache.ContainsKey(receiptIdForMatch))
                                {
                                    refinedCategoryMapCache[receiptIdForMatch] = BuildRefinedCategoryMap(itemCategoriesJson);
                                }

                                var refinedMap = refinedCategoryMapCache[receiptIdForMatch];
                                if (refinedMap != null && refinedMap.ContainsKey(currentCategoryKey))
                                {
                                    var refinedItem = refinedMap[currentCategoryKey];
                                    parentCategoryName = refinedItem.ParentCategoryName ?? string.Empty;
                                    subCategoryName = string.IsNullOrWhiteSpace(refinedItem.CategoryName) ? currentCategoryName : refinedItem.CategoryName;
                                }
                            }

                            result.Items.Add(new InventoryDetailViewModel
                            {
                                InventoryID = Convert.ToInt32(reader["InventoryID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderNumber = reader["OrderNumber"].ToString(),
                                CategoryKey = reader["CategoryKey"].ToString(),
                                ParentCategoryName = parentCategoryName,
                                SubCategoryName = subCategoryName,
                                CategoryName = reader["CategoryName"].ToString(),
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : Convert.ToDecimal(reader["Price"]),
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                RecyclerName = reader["RecyclerName"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return result;
        }

        private Dictionary<string, WarehouseReceiptCategoryItemViewModel> BuildRefinedCategoryMap(string itemCategoriesJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemCategoriesJson))
                {
                    return new Dictionary<string, WarehouseReceiptCategoryItemViewModel>(StringComparer.OrdinalIgnoreCase);
                }

                var categories = JsonConvert.DeserializeObject<List<WarehouseReceiptCategoryItemViewModel>>(itemCategoriesJson)
                    ?? new List<WarehouseReceiptCategoryItemViewModel>();

                var map = new Dictionary<string, WarehouseReceiptCategoryItemViewModel>(StringComparer.OrdinalIgnoreCase);
                foreach (var category in categories.Where(c => c != null && !string.IsNullOrWhiteSpace(c.CategoryKey) && !string.IsNullOrWhiteSpace(c.ParentCategoryKey)))
                {
                    map[category.CategoryKey.Trim()] = category;
                }

                return map;
            }
            catch
            {
                return new Dictionary<string, WarehouseReceiptCategoryItemViewModel>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
