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
        // 与UserDAL一致，从配置文件获取连接字符串
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 分页查询可回收物（支持多条件筛选）
        /// </summary>
        public PagedResult<RecyclableItems> GetPagedItems(RecyclableQueryModel query)
        {
            var result = new PagedResult<RecyclableItems>
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            // 构建查询条件和参数（模仿UserDAL的参数化方式）
            var whereConditions = new List<string> { "IsActive = 1" };
            var cmdParams = new List<SqlParameter>();

            // 关键词筛选（名称）
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                whereConditions.Add("Name LIKE '%' + @Keyword + '%'");
                cmdParams.Add(new SqlParameter("@Keyword", query.Keyword));
            }

            // 品类筛选
            if (!string.IsNullOrEmpty(query.Category))
            {
                whereConditions.Add("CategoryName = @CategoryName");
                cmdParams.Add(new SqlParameter("@CategoryName", query.Category));
            }

            // 价格区间筛选
            if (query.MinPrice.HasValue)
            {
                whereConditions.Add("PricePerKg >= @MinPrice");
                cmdParams.Add(new SqlParameter("@MinPrice", query.MinPrice.Value));
            }
            if (query.MaxPrice.HasValue)
            {
                whereConditions.Add("PricePerKg <= @MaxPrice");
                cmdParams.Add(new SqlParameter("@MaxPrice", query.MaxPrice.Value));
            }

            string whereClause = whereConditions.Count > 0
                ? "WHERE " + string.Join(" AND ", whereConditions)
                : "";

            // 1. 查询总记录数（模仿UserDAL的ExecuteScalar用法）
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string countSql = $"SELECT COUNT(1) FROM RecyclableItems {whereClause}";
                SqlCommand cmd = new SqlCommand(countSql, conn);
                cmd.Parameters.AddRange(cmdParams.ToArray());

                conn.Open();
                result.TotalCount = (int)cmd.ExecuteScalar();
            }

            // 2. 查询当前页数据（模仿UserDAL的SqlDataReader读取方式）
            if (result.TotalCount > 0)
            {
                int skip = (query.PageIndex - 1) * query.PageSize;
                string dataSql = $@"
SELECT * FROM (
    SELECT ROW_NUMBER() OVER (ORDER BY SortOrder ASC) AS RowNum, *
    FROM RecyclableItems
    {whereClause}
) AS T
WHERE RowNum > @Skip AND RowNum <= @Skip + @PageSize";

                // 添加分页参数
                cmdParams.Add(new SqlParameter("@Skip", skip));
                cmdParams.Add(new SqlParameter("@PageSize", query.PageSize));

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(dataSql, conn);
                    cmd.Parameters.AddRange(cmdParams.ToArray());

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
                string sql = "SELECT COUNT(1) FROM RecyclableItems";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }
}
