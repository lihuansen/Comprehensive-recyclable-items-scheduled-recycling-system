using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class OrderReviewBLL
    {
        private readonly OrderReviewDAL _reviewDAL = new OrderReviewDAL();

        /// 添加订单评价
        public (bool Success, string Message) AddReview(int orderId, int userId, int recyclerId, int starRating, string reviewText, string pictureUrl = null)
        {
            if (orderId <= 0 || userId <= 0 || recyclerId <= 0)
            {
                return (false, "参数无效");
            }

            if (starRating < 1 || starRating > 5)
            {
                return (false, "评分必须在1-5星之间");
            }

            // 评价文字和图片至少需要提供一项
            bool hasText = !string.IsNullOrWhiteSpace(reviewText);
            bool hasPicture = !string.IsNullOrWhiteSpace(pictureUrl);
            if (!hasText && !hasPicture)
            {
                return (false, "请至少填写评价内容或上传评价图片");
            }

            // 验证图片数量不超过6张
            if (hasPicture)
            {
                var urls = pictureUrl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (urls.Length > 6)
                {
                    return (false, "评价图片最多上传6张");
                }
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
                CreatedDate = DateTime.Now,
                PictureUrl = pictureUrl
            };

            bool success = _reviewDAL.AddReview(review);
            return success ? (true, "评价成功") : (false, "评价失败");
        }

        /// 检查订单是否已评价
        public bool HasReviewed(int orderId, int userId)
        {
            return _reviewDAL.HasReviewed(orderId, userId);
        }

        /// 获取订单评价
        public OrderReviews GetReview(int orderId)
        {
            return _reviewDAL.GetReview(orderId);
        }

        /// 获取回收员收到的所有评价
        public List<OrderReviews> GetReviewsByRecyclerId(int recyclerId)
        {
            return _reviewDAL.GetReviewsByRecyclerId(recyclerId);
        }

        /// 获取回收员的平均评分和评价总数
        public (decimal AverageRating, int TotalReviews) GetRecyclerRatingSummary(int recyclerId)
        {
            return _reviewDAL.GetRecyclerRatingSummary(recyclerId);
        }

        /// 获取回收员评价的星级分布
        public Dictionary<int, int> GetRecyclerRatingDistribution(int recyclerId)
        {
            return _reviewDAL.GetRecyclerRatingDistribution(recyclerId);
        }
    }
}
