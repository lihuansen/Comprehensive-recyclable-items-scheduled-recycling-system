using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;
using Newtonsoft.Json;

namespace recycling.DAL
{
    public class AppointmentDAL
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// 插入预约基础信息
        /// </summary>
        public int InsertAppointment(Appointments appointment)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
INSERT INTO Appointments (
    UserID, AppointmentType, AppointmentDate, TimeSlot, EstimatedWeight, 
    IsUrgent, Address, ContactName, ContactPhone, SpecialInstructions, 
    EstimatedPrice, Status, CreatedDate, UpdatedDate
) 
VALUES (
    @UserID, @AppointmentType, @AppointmentDate, @TimeSlot, @EstimatedWeight,
    @IsUrgent, @Address, @ContactName, @ContactPhone, @SpecialInstructions,
    @EstimatedPrice, @Status, @CreatedDate, @UpdatedDate
);
SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", appointment.UserID);
                cmd.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType);
                cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                cmd.Parameters.AddWithValue("@TimeSlot", appointment.TimeSlot);
                cmd.Parameters.AddWithValue("@EstimatedWeight", appointment.EstimatedWeight);
                cmd.Parameters.AddWithValue("@IsUrgent", appointment.IsUrgent);
                cmd.Parameters.AddWithValue("@Address", appointment.Address);
                cmd.Parameters.AddWithValue("@ContactName", appointment.ContactName);
                cmd.Parameters.AddWithValue("@ContactPhone", appointment.ContactPhone);
                cmd.Parameters.AddWithValue("@SpecialInstructions", (object)appointment.SpecialInstructions ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EstimatedPrice", (object)appointment.EstimatedPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", appointment.Status);
                cmd.Parameters.AddWithValue("@CreatedDate", appointment.CreatedDate);
                cmd.Parameters.AddWithValue("@UpdatedDate", appointment.UpdatedDate);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// 插入预约品类详情
        /// </summary>
        public bool InsertAppointmentCategory(AppointmentCategories category)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
INSERT INTO AppointmentCategories (
    AppointmentID, CategoryName, CategoryKey, QuestionsAnswers, Weight, CreatedDate
) 
VALUES (
    @AppointmentID, @CategoryName, @CategoryKey, @QuestionsAnswers, @Weight, @CreatedDate
)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AppointmentID", category.AppointmentID);
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                cmd.Parameters.AddWithValue("@CategoryKey", category.CategoryKey);
                cmd.Parameters.AddWithValue("@QuestionsAnswers", (object)category.QuestionsAnswers ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight", category.Weight);
                cmd.Parameters.AddWithValue("@CreatedDate", category.CreatedDate);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// 在事务中插入完整的预约信息
        /// </summary>
        public (bool Success, int AppointmentId, string ErrorMessage) InsertCompleteAppointment(
            Appointments appointment, List<AppointmentCategories> categories)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 插入预约基础信息
                        string appointmentSql = @"
INSERT INTO Appointments (
    UserID, AppointmentType, AppointmentDate, TimeSlot, EstimatedWeight, 
    IsUrgent, Address, ContactName, ContactPhone, SpecialInstructions, 
    EstimatedPrice, Status, CreatedDate, UpdatedDate
) 
VALUES (
    @UserID, @AppointmentType, @AppointmentDate, @TimeSlot, @EstimatedWeight,
    @IsUrgent, @Address, @ContactName, @ContactPhone, @SpecialInstructions,
    @EstimatedPrice, @Status, @CreatedDate, @UpdatedDate
);
SELECT SCOPE_IDENTITY();";

                        SqlCommand appointmentCmd = new SqlCommand(appointmentSql, conn, transaction);
                        appointmentCmd.Parameters.AddWithValue("@UserID", appointment.UserID);
                        appointmentCmd.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType);
                        appointmentCmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                        appointmentCmd.Parameters.AddWithValue("@TimeSlot", appointment.TimeSlot);
                        appointmentCmd.Parameters.AddWithValue("@EstimatedWeight", appointment.EstimatedWeight);
                        appointmentCmd.Parameters.AddWithValue("@IsUrgent", appointment.IsUrgent);
                        appointmentCmd.Parameters.AddWithValue("@Address", appointment.Address);
                        appointmentCmd.Parameters.AddWithValue("@ContactName", appointment.ContactName);
                        appointmentCmd.Parameters.AddWithValue("@ContactPhone", appointment.ContactPhone);
                        appointmentCmd.Parameters.AddWithValue("@SpecialInstructions", (object)appointment.SpecialInstructions ?? DBNull.Value);
                        appointmentCmd.Parameters.AddWithValue("@EstimatedPrice", (object)appointment.EstimatedPrice ?? DBNull.Value);
                        appointmentCmd.Parameters.AddWithValue("@Status", appointment.Status);
                        appointmentCmd.Parameters.AddWithValue("@CreatedDate", appointment.CreatedDate);
                        appointmentCmd.Parameters.AddWithValue("@UpdatedDate", appointment.UpdatedDate);

                        int appointmentId = Convert.ToInt32(appointmentCmd.ExecuteScalar());

                        // 2. 插入所有品类详情
                        foreach (var category in categories)
                        {
                            category.AppointmentID = appointmentId;

                            string categorySql = @"
INSERT INTO AppointmentCategories (
    AppointmentID, CategoryName, CategoryKey, QuestionsAnswers, Weight, CreatedDate
) 
VALUES (
    @AppointmentID, @CategoryName, @CategoryKey, @QuestionsAnswers, @Weight, @CreatedDate
)";

                            SqlCommand categoryCmd = new SqlCommand(categorySql, conn, transaction);
                            categoryCmd.Parameters.AddWithValue("@AppointmentID", category.AppointmentID);
                            categoryCmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                            categoryCmd.Parameters.AddWithValue("@CategoryKey", category.CategoryKey);
                            categoryCmd.Parameters.AddWithValue("@QuestionsAnswers", (object)category.QuestionsAnswers ?? DBNull.Value);
                            categoryCmd.Parameters.AddWithValue("@Weight", category.Weight);
                            categoryCmd.Parameters.AddWithValue("@CreatedDate", category.CreatedDate);

                            categoryCmd.ExecuteNonQuery();
                        }

                        // 3. 提交事务
                        transaction.Commit();
                        return (true, appointmentId, null);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, 0, $"数据库操作失败：{ex.Message}");
                    }
                }
            }
        }
    }
}
