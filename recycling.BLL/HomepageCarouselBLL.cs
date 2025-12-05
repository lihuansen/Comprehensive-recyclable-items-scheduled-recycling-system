using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.DAL;
using recycling.Model;

namespace recycling.BLL
{
    public class HomepageCarouselBLL
    {
        private readonly HomepageCarouselDAL _carouselDAL = new HomepageCarouselDAL();

        /// <summary>
        /// Get all active carousel items for homepage display
        /// </summary>
        public List<HomepageCarousel> GetAllActive()
        {
            try
            {
                return _carouselDAL.GetAllActive();
            }
            catch (Exception ex)
            {
                throw new Exception("获取轮播内容失败：" + ex.Message);
            }
        }

        /// <summary>
        /// Get paged carousel items for admin management
        /// </summary>
        public PagedResult<HomepageCarousel> GetPaged(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            try
            {
                return _carouselDAL.GetPaged(page, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("获取轮播列表失败：" + ex.Message);
            }
        }

        /// <summary>
        /// Get carousel item by ID
        /// </summary>
        public HomepageCarousel GetById(int carouselId)
        {
            if (carouselId <= 0)
            {
                throw new ArgumentException("无效的轮播ID");
            }

            try
            {
                return _carouselDAL.GetById(carouselId);
            }
            catch (Exception ex)
            {
                throw new Exception("获取轮播内容失败：" + ex.Message);
            }
        }

        /// <summary>
        /// Add new carousel item
        /// </summary>
        public (bool Success, string Message) Add(HomepageCarousel carousel, int adminId)
        {
            // Validation
            if (string.IsNullOrEmpty(carousel.MediaType))
            {
                return (false, "媒体类型不能为空");
            }

            if (carousel.MediaType != "Image" && carousel.MediaType != "Video")
            {
                return (false, "媒体类型必须是 Image 或 Video");
            }

            if (string.IsNullOrEmpty(carousel.MediaUrl))
            {
                return (false, "媒体URL不能为空");
            }

            if (carousel.DisplayOrder < 0)
            {
                carousel.DisplayOrder = 0;
            }

            // Set metadata
            carousel.CreatedDate = DateTime.Now;
            carousel.CreatedBy = adminId;
            carousel.UpdatedDate = DateTime.Now;
            carousel.IsActive = true;

            try
            {
                bool result = _carouselDAL.Add(carousel);
                return result ? (true, "添加轮播内容成功") : (false, "添加轮播内容失败");
            }
            catch (Exception ex)
            {
                return (false, $"添加轮播内容失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Update carousel item
        /// </summary>
        public (bool Success, string Message) Update(HomepageCarousel carousel)
        {
            // Validation
            if (carousel.CarouselID <= 0)
            {
                return (false, "无效的轮播ID");
            }

            if (string.IsNullOrEmpty(carousel.MediaType))
            {
                return (false, "媒体类型不能为空");
            }

            if (carousel.MediaType != "Image" && carousel.MediaType != "Video")
            {
                return (false, "媒体类型必须是 Image 或 Video");
            }

            if (string.IsNullOrEmpty(carousel.MediaUrl))
            {
                return (false, "媒体URL不能为空");
            }

            if (carousel.DisplayOrder < 0)
            {
                carousel.DisplayOrder = 0;
            }

            // Update timestamp
            carousel.UpdatedDate = DateTime.Now;

            try
            {
                bool result = _carouselDAL.Update(carousel);
                return result ? (true, "更新轮播内容成功") : (false, "更新轮播内容失败");
            }
            catch (Exception ex)
            {
                return (false, $"更新轮播内容失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Delete carousel item (soft delete)
        /// </summary>
        public (bool Success, string Message) Delete(int carouselId)
        {
            if (carouselId <= 0)
            {
                return (false, "无效的轮播ID");
            }

            try
            {
                bool result = _carouselDAL.Delete(carouselId);
                return result ? (true, "删除轮播内容成功") : (false, "删除轮播内容失败");
            }
            catch (Exception ex)
            {
                return (false, $"删除轮播内容失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Hard delete carousel item
        /// </summary>
        public (bool Success, string Message) HardDelete(int carouselId)
        {
            if (carouselId <= 0)
            {
                return (false, "无效的轮播ID");
            }

            try
            {
                bool result = _carouselDAL.HardDelete(carouselId);
                return result ? (true, "永久删除轮播内容成功") : (false, "永久删除轮播内容失败");
            }
            catch (Exception ex)
            {
                return (false, $"永久删除轮播内容失败：{ex.Message}");
            }
        }

        /// <summary>
        /// Get the maximum DisplayOrder value
        /// </summary>
        public int GetMaxDisplayOrder()
        {
            try
            {
                return _carouselDAL.GetMaxDisplayOrder();
            }
            catch (Exception ex)
            {
                throw new Exception("获取最大排序值失败：" + ex.Message);
            }
        }
    }
}
