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

        /// 获取所有启用的轮播图。
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

        /// 分页获取轮播图列表。
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

        /// 根据编号获取轮播图。
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

        /// 新增轮播图。
        public (bool Success, string Message) Add(HomepageCarousel carousel, int adminId)
        {
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

        /// 更新轮播图。
        public (bool Success, string Message) Update(HomepageCarousel carousel)
        {
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

        /// 删除轮播图。
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

        /// 彻底删除轮播图。
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

        /// 获取显示顺序。
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
