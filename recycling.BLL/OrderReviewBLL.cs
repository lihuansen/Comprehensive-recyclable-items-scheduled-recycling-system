using System;
using System.Collections.Generic;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class OrderReviewBLL
    {
        private readonly OrderReviewDAL _reviewDAL = new OrderReviewDAL();

        /// <summary>
        /// 添加订单评价
        /// </summary>
        public (bool Success, string Message) AddReview(int orderId, int userId, int recyclerId, int starRating, string reviewText)
        {
            if (orderId <= 0 || userId <= 0 || recyclerId <= 0)
            {
                return (false, "参数无效");
            }

            if (starRating < 1 || starRating > 5)
            {
                return (false, "评分必须在1-5星之间");
            }

            // 检查是否已评价
            if (_reviewDAL.HasReviewed(orderId, userId))
            {
                return (false, "该订单已经评价过了");
            }

            var review = new OrderReviews
            {
                OrderID = orderId,
                UserID = userId,
                RecyclerID = recyclerId,
                StarRating = starRating,
                ReviewText = reviewText,
                CreatedDate = DateTime.Now
            };

            bool success = _reviewDAL.AddReview(review);
            return success ? (true, "评价成功") : (false, "评价失败");
        }

        /// <summary>
        /// 检查订单是否已评价
        /// </summary>
        public bool HasReviewed(int orderId, int userId)
        {
            return _reviewDAL.HasReviewed(orderId, userId);
        }

        /// <summary>
        /// 获取订单评价
        /// </summary>
        public OrderReviews GetReview(int orderId)
        {
            return _reviewDAL.GetReview(orderId);
        }
    }
}
