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
                    cmd.Parameters.AddWithValue("@OrderID", review.OrderID);
                    cmd.Parameters.AddWithValue("@UserID", review.UserID);
                    cmd.Parameters.AddWithValue("@RecyclerID", review.RecyclerID);
                    cmd.Parameters.AddWithValue("@StarRating", review.StarRating);
                    cmd.Parameters.AddWithValue("@ReviewText", review.ReviewText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedDate", review.CreatedDate);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
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
    }
}
