using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using recycling.Model;

namespace recycling.DAL
{
    public class HomepageCarouselDAL
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;

        /// <summary>
        /// Get all active carousel items ordered by DisplayOrder
        /// </summary>
        public List<HomepageCarousel> GetAllActive()
        {
            var items = new List<HomepageCarousel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT * FROM HomepageCarousel 
                              WHERE IsActive = 1 
                              ORDER BY DisplayOrder ASC, CarouselID ASC";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapCarouselFromReader(reader));
                    }
                }
            }
            return items;
        }

        /// <summary>
        /// Get all carousel items (including inactive) with pagination
        /// </summary>
        public PagedResult<HomepageCarousel> GetPaged(int page = 1, int pageSize = 20)
        {
            var result = new PagedResult<HomepageCarousel>
            {
                PageIndex = page,
                PageSize = pageSize,
                Items = new List<HomepageCarousel>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Get total count
                string countSql = "SELECT COUNT(*) FROM HomepageCarousel";
                SqlCommand countCmd = new SqlCommand(countSql, conn);
                result.TotalCount = (int)countCmd.ExecuteScalar();

                // Get paged data
                string sql = @"SELECT * FROM HomepageCarousel 
                              ORDER BY DisplayOrder ASC, CarouselID ASC 
                              OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Items.Add(MapCarouselFromReader(reader));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get carousel item by ID
        /// </summary>
        public HomepageCarousel GetById(int carouselId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM HomepageCarousel WHERE CarouselID = @CarouselID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CarouselID", carouselId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapCarouselFromReader(reader);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Add new carousel item
        /// </summary>
        public bool Add(HomepageCarousel carousel)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO HomepageCarousel 
                              (MediaType, MediaUrl, Title, Description, DisplayOrder, IsActive, CreatedDate, CreatedBy, UpdatedDate)
                              VALUES (@MediaType, @MediaUrl, @Title, @Description, @DisplayOrder, @IsActive, @CreatedDate, @CreatedBy, @UpdatedDate)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MediaType", carousel.MediaType);
                cmd.Parameters.AddWithValue("@MediaUrl", carousel.MediaUrl);
                cmd.Parameters.AddWithValue("@Title", carousel.Title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", carousel.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DisplayOrder", carousel.DisplayOrder);
                cmd.Parameters.AddWithValue("@IsActive", carousel.IsActive);
                cmd.Parameters.AddWithValue("@CreatedDate", carousel.CreatedDate);
                cmd.Parameters.AddWithValue("@CreatedBy", carousel.CreatedBy);
                cmd.Parameters.AddWithValue("@UpdatedDate", carousel.UpdatedDate ?? (object)DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Update carousel item
        /// </summary>
        public bool Update(HomepageCarousel carousel)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE HomepageCarousel SET 
                              MediaType = @MediaType,
                              MediaUrl = @MediaUrl,
                              Title = @Title,
                              Description = @Description,
                              DisplayOrder = @DisplayOrder,
                              IsActive = @IsActive,
                              UpdatedDate = @UpdatedDate
                              WHERE CarouselID = @CarouselID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CarouselID", carousel.CarouselID);
                cmd.Parameters.AddWithValue("@MediaType", carousel.MediaType);
                cmd.Parameters.AddWithValue("@MediaUrl", carousel.MediaUrl);
                cmd.Parameters.AddWithValue("@Title", carousel.Title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", carousel.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DisplayOrder", carousel.DisplayOrder);
                cmd.Parameters.AddWithValue("@IsActive", carousel.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedDate", carousel.UpdatedDate ?? (object)DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Delete carousel item (soft delete by setting IsActive = false)
        /// </summary>
        public bool Delete(int carouselId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE HomepageCarousel SET IsActive = 0, UpdatedDate = GETDATE() 
                              WHERE CarouselID = @CarouselID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CarouselID", carouselId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Hard delete carousel item
        /// </summary>
        public bool HardDelete(int carouselId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM HomepageCarousel WHERE CarouselID = @CarouselID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CarouselID", carouselId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Map carousel from database reader
        /// </summary>
        private HomepageCarousel MapCarouselFromReader(SqlDataReader reader)
        {
            return new HomepageCarousel
            {
                CarouselID = reader.GetInt32(reader.GetOrdinal("CarouselID")),
                MediaType = reader.GetString(reader.GetOrdinal("MediaType")),
                MediaUrl = reader.GetString(reader.GetOrdinal("MediaUrl")),
                Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? null : reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
            };
        }
    }
}
