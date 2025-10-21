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
                System.Diagnostics.Debug.WriteLine("BLL层开始处理预约提交...");

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

                System.Diagnostics.Debug.WriteLine($"创建Appointment对象: UserID={appointment.UserID}, Type={appointment.AppointmentType}");

                // 2. 创建品类详情列表
                var categories = new List<AppointmentCategories>();
                System.Diagnostics.Debug.WriteLine($"选中的品类数量: {submission.BasicInfo.SelectedCategories.Count}");

                foreach (var categoryKey in submission.BasicInfo.SelectedCategories)
                {
                    var categoryAnswers = submission.CategoryAnswers.ContainsKey(categoryKey) ?
                                        submission.CategoryAnswers[categoryKey] : new Dictionary<string, string>();

                    System.Diagnostics.Debug.WriteLine($"处理品类 {categoryKey}, 答案数量: {categoryAnswers.Count}");

                    var category = new AppointmentCategories
                    {
                        CategoryName = GetCategoryDisplayName(categoryKey),
                        CategoryKey = categoryKey,
                        QuestionsAnswers = JsonConvert.SerializeObject(categoryAnswers),
                        CreatedDate = DateTime.Now
                    };

                    categories.Add(category);
                    System.Diagnostics.Debug.WriteLine($"品类 {categoryKey} 创建完成: {category.CategoryName}");
                }

                System.Diagnostics.Debug.WriteLine($"准备调用DAL层，预约对象和 {categories.Count} 个品类对象");

                // 3. 调用DAL层在事务中插入完整数据
                var result = _appointmentDAL.InsertCompleteAppointment(appointment, categories);
                System.Diagnostics.Debug.WriteLine($"DAL层返回: Success={result.Success}, AppointmentId={result.AppointmentId}");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BLL层异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"BLL层异常堆栈: {ex.StackTrace}");
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
        { "glass", "玻璃" },
        { "metal", "金属" },
        { "plastic", "塑料" },
        { "paper", "纸类" },
        { "fabric", "纺织品" }
    };

            return categoryMap.ContainsKey(categoryKey) ? categoryMap[categoryKey] : categoryKey;
        }
    }
}
