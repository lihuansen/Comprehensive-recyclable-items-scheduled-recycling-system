using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using recycling.Model;

namespace recycling.DAL
{
    public class OrderReviewDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 添加订单评价
        /// </summary>
        public bool AddReview(OrderReviews review)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO OrderReviews (OrderID, UserID, RecyclerID, StarRating, ReviewText, CreatedDate)
                    VALUES (@OrderID, @UserID, @RecyclerID, @StarRating, @ReviewText, @CreatedDate)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@OrderID", SqlDbType.Int).Value = review.OrderID;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = review.UserID;
                    cmd.Parameters.Add("@RecyclerID", SqlDbType.Int).Value = review.RecyclerID;
                    cmd.Parameters.Add("@StarRating", SqlDbType.Int).Value = review.StarRating;
                    cmd.Parameters.Add("@ReviewText", SqlDbType.NVarChar, 500).Value = (object)review.ReviewText ?? DBNull.Value;
                    cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime2).Value = review.CreatedDate;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    
                    // Update recycler's average rating after adding review
                    if (rows > 0 && review.RecyclerID.HasValue)
                    {
                        UpdateRecyclerRating(review.RecyclerID.Value, conn);
                    }
                    
                    return rows > 0;
                }
            }
        }

        /// <summary>
        /// 更新回收员的平均评分到Recyclers表
        /// </summary>
        public void UpdateRecyclerRating(int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                UpdateRecyclerRating(recyclerId, conn);
            }
        }

        /// <summary>
        /// 更新回收员的平均评分到Recyclers表（内部方法，使用已打开的连接）
        /// </summary>
        private void UpdateRecyclerRating(int recyclerId, SqlConnection conn)
        {
            string sql = @"
                UPDATE Recyclers 
                SET Rating = (
                    SELECT ISNULL(AVG(CAST(StarRating AS DECIMAL(10,2))), 0)
                    FROM OrderReviews
                    WHERE RecyclerID = @RecyclerID
                )
                WHERE RecyclerID = @RecyclerID";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 检查订单是否已评价
        /// </summary>
        public bool HasReviewed(int orderId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM OrderReviews WHERE OrderID = @OrderID AND UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// 获取订单评价
        /// </summary>
        public OrderReviews GetReview(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT ReviewID, OrderID, UserID, RecyclerID, StarRating, ReviewText, CreatedDate
                    FROM OrderReviews
                    WHERE OrderID = @OrderID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new OrderReviews
                            {
                                ReviewID = Convert.ToInt32(reader["ReviewID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                UserID = Convert.ToInt32(reader["UserID"]),
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                StarRating = Convert.ToInt32(reader["StarRating"]),
                                ReviewText = reader["ReviewText"] == DBNull.Value ? null : reader["ReviewText"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取回收员收到的所有评价
        /// </summary>
        public List<OrderReviews> GetReviewsByRecyclerId(int recyclerId)
        {
            List<OrderReviews> reviews = new List<OrderReviews>();
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT ReviewID, OrderID, UserID, RecyclerID, StarRating, ReviewText, CreatedDate
                    FROM OrderReviews
                    WHERE RecyclerID = @RecyclerID
                    ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reviews.Add(new OrderReviews
                            {
                                ReviewID = Convert.ToInt32(reader["ReviewID"]),
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                UserID = Convert.ToInt32(reader["UserID"]),
                                RecyclerID = Convert.ToInt32(reader["RecyclerID"]),
                                StarRating = Convert.ToInt32(reader["StarRating"]),
                                ReviewText = reader["ReviewText"] == DBNull.Value ? null : reader["ReviewText"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            return reviews;
        }

        /// <summary>
        /// 获取回收员的平均评分和评价总数
        /// </summary>
        public (decimal AverageRating, int TotalReviews) GetRecyclerRatingSummary(int recyclerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT 
                        ISNULL(AVG(CAST(StarRating AS DECIMAL(10,2))), 0) AS AvgRating,
                        COUNT(*) AS TotalCount
                    FROM OrderReviews
                    WHERE RecyclerID = @RecyclerID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal avgRating = reader["AvgRating"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AvgRating"]);
                            int totalCount = reader["TotalCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalCount"]);
                            return (avgRating, totalCount);
                        }
                    }
                }
            }

            return (0, 0);
        }

        /// <summary>
        /// 获取回收员评价的星级分布
        /// </summary>
        public Dictionary<int, int> GetRecyclerRatingDistribution(int recyclerId)
        {
            Dictionary<int, int> distribution = new Dictionary<int, int>
            {
                {5, 0}, {4, 0}, {3, 0}, {2, 0}, {1, 0}
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT StarRating, COUNT(*) AS Count
                    FROM OrderReviews
                    WHERE RecyclerID = @RecyclerID
                    GROUP BY StarRating";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecyclerID", recyclerId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int rating = Convert.ToInt32(reader["StarRating"]);
                            int count = Convert.ToInt32(reader["Count"]);
                            if (distribution.ContainsKey(rating))
                            {
                                distribution[rating] = count;
                            }
                        }
                    }
                }
            }

            return distribution;
        }
    }
}
