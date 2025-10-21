using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycling.Model;
using recycling.DAL;
using Newtonsoft.Json;

namespace recycling.BLL
{
    public class AppointmentBLL
    {
        private AppointmentDAL _appointmentDAL = new AppointmentDAL();

        /// <summary>
        /// 提交完整预约信息
        /// </summary>
        public (bool Success, int AppointmentId, string ErrorMessage) SubmitAppointment(
            AppointmentSubmissionModel submission, int userId)
        {
            try
            {
                // 1. 创建预约基础信息对象
                var appointment = new Appointments
                {
                    UserID = userId,
                    AppointmentType = submission.BasicInfo.AppointmentType,
                    AppointmentDate = submission.BasicInfo.AppointmentDate,
                    TimeSlot = submission.BasicInfo.TimeSlot,
                    EstimatedWeight = submission.BasicInfo.EstimatedWeight,
                    IsUrgent = submission.BasicInfo.IsUrgent,
                    Address = submission.BasicInfo.Address,
                    ContactName = submission.BasicInfo.ContactName,
                    ContactPhone = submission.BasicInfo.ContactPhone,
                    SpecialInstructions = submission.BasicInfo.SpecialRequirements,
                    EstimatedPrice = submission.FinalPrice,
                    Status = "待确认",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                // 2. 创建品类详情列表
                var categories = new List<AppointmentCategories>();
                foreach (var categoryKey in submission.BasicInfo.SelectedCategories)
                {
                    var categoryAnswers = submission.CategoryAnswers.ContainsKey(categoryKey) ?
                                        submission.CategoryAnswers[categoryKey] : new Dictionary<string, string>();

                    var category = new AppointmentCategories
                    {
                        CategoryName = GetCategoryDisplayName(categoryKey),
                        CategoryKey = categoryKey,
                        QuestionsAnswers = JsonConvert.SerializeObject(categoryAnswers),
                        CreatedDate = DateTime.Now
                    };

                    categories.Add(category);
                }

                // 3. 调用DAL层在事务中插入完整数据
                var result = _appointmentDAL.InsertCompleteAppointment(appointment, categories);
                return result;
            }
            catch (Exception ex)
            {
                return (false, 0, $"提交预约失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 根据品类键名获取显示名称
        /// </summary>
        private string GetCategoryDisplayName(string categoryKey)
        {
            var categoryMap = new Dictionary<string, string>
            {
                { "waste_paper", "废纸" },
                { "plastic", "塑料" },
                { "metal", "金属" },
                { "glass", "玻璃" },
                { "fabric", "织物" }
            };

            return categoryMap.ContainsKey(categoryKey) ? categoryMap[categoryKey] : categoryKey;
        }
    }
}
