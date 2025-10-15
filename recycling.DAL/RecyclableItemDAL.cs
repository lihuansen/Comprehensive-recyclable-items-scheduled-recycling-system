using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class RecyclableItemDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 分页查询可回收物（支持多条件筛选）- 修正参数重复问题
        /// </summary>
        public PagedResult<RecyclableItems> GetPagedItems(RecyclableQueryModel query)
        {
            var result = new PagedResult<RecyclableItems>
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            // 构建查询条件和参数
            var whereConditions = new List<string> { "IsActive = 1" };
            var parameters = new Dictionary<string, object>();

            // 关键词筛选（名称或描述）
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                whereConditions.Add("(Name LIKE '%' + @Keyword + '%' OR Description LIKE '%' + @Keyword + '%')");
                parameters["@Keyword"] = query.Keyword;
            }

            // 品类筛选
            if (!string.IsNullOrEmpty(query.Category))
            {
                whereConditions.Add("CategoryName = @CategoryName");
                parameters["@CategoryName"] = query.Category;
            }

            // 价格区间筛选
            if (query.MinPrice.HasValue)
            {
                whereConditions.Add("PricePerKg >= @MinPrice");
                parameters["@MinPrice"] = query.MinPrice.Value;
            }
            if (query.MaxPrice.HasValue)
            {
                whereConditions.Add("PricePerKg <= @MaxPrice");
                parameters["@MaxPrice"] = query.MaxPrice.Value;
            }

            string whereClause = whereConditions.Count > 0
                ? "WHERE " + string.Join(" AND ", whereConditions)
                : "";

            // 1. 查询总记录数
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string countSql = $"SELECT COUNT(1) FROM RecyclableItems {whereClause}";
                SqlCommand cmd = new SqlCommand(countSql, conn);

                // 添加参数到命令
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                conn.Open();
                result.TotalCount = (int)cmd.ExecuteScalar();
            }

            // 2. 查询当前页数据
            if (result.TotalCount > 0)
            {
                int skip = (query.PageIndex - 1) * query.PageSize;
                string dataSql = $@"
                    SELECT * FROM (
                        SELECT ROW_NUMBER() OVER (ORDER BY SortOrder ASC, ItemId ASC) AS RowNum, *
                        FROM RecyclableItems
                        {whereClause}
                    ) AS T
                    WHERE RowNum > @Skip AND RowNum <= @Skip + @PageSize";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(dataSql, conn);

                    // 重新添加所有参数（包括分页参数）
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    // 添加分页参数
                    cmd.Parameters.AddWithValue("@Skip", skip);
                    cmd.Parameters.AddWithValue("@PageSize", query.PageSize);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Items.Add(new RecyclableItems
                            {
                                ItemId = Convert.ToInt32(reader["ItemId"]),
                                Name = reader["Name"].ToString(),
                                Category = reader["Category"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                PricePerKg = Convert.ToDecimal(reader["PricePerKg"]),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                SortOrder = Convert.ToInt32(reader["SortOrder"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取所有品类（去重，含中文名称）
        /// </summary>
        public Dictionary<string, string> GetAllCategories()
        {
            var categories = new Dictionary<string, string>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT DISTINCT Category, CategoryName 
                    FROM RecyclableItems 
                    WHERE IsActive = 1 
                    ORDER BY Category";

                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string category = reader["Category"].ToString();
                        string categoryName = reader["CategoryName"].ToString();
                        if (!categories.ContainsKey(category))
                        {
                            categories[category] = categoryName;
                        }
                    }
                }
            }
            return categories;
        }

        /// <summary>
        /// 检查数据库中是否有可回收物数据
        /// </summary>
        public bool HasData()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(1) FROM RecyclableItems WHERE IsActive = 1";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        /// <summary>
        /// 获取所有有效数据（用于调试）
        /// </summary>
        public List<RecyclableItems> GetAllActiveItems()
        {
            var items = new List<RecyclableItems>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM RecyclableItems WHERE IsActive = 1 ORDER BY SortOrder, ItemId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new RecyclableItems
                        {
                            ItemId = Convert.ToInt32(reader["ItemId"]),
                            Name = reader["Name"].ToString(),
                            Category = reader["Category"].ToString(),
                            CategoryName = reader["CategoryName"].ToString(),
                            PricePerKg = Convert.ToDecimal(reader["PricePerKg"]),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                            SortOrder = Convert.ToInt32(reader["SortOrder"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        });
                    }
                }
            }
            return items;
        }
    }
}
