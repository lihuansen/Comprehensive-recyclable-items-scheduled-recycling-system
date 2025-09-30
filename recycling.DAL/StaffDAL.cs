using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using recycling.Model;

namespace recycling.DAL
{
    public class StaffDAL
    {
        private readonly AppDbContext _context = new AppDbContext();

        #region 回收员登录验证
        public Recyclers GetRecyclerByUsername(string username)
        {
            return _context.Recyclers.FirstOrDefault(r => r.Username == username);
        }

        public void UpdateRecyclerLastLogin(int recyclerId)
        {
            var recycler = _context.Recyclers.Find(recyclerId);
            if (recycler != null)
            {
                recycler.LastLoginDate = DateTime.Now;
                _context.SaveChanges();
            }
        }
        #endregion

        #region 管理员登录验证
        public Admins GetAdminByUsername(string username)
        {
            return _context.Admins.FirstOrDefault(a => a.Username == username);
        }

        public void UpdateAdminLastLogin(int adminId)
        {
            var admin = _context.Admins.Find(adminId);
            if (admin != null)
            {
                admin.LastLoginDate = DateTime.Now;
                _context.SaveChanges();
            }
        }
        #endregion

        #region 超级管理员登录验证
        public SuperAdmins GetSuperAdminByUsername(string username)
        {
            return _context.SuperAdmins.FirstOrDefault(s => s.Username == username);
        }

        public void UpdateSuperAdminLastLogin(int superAdminId)
        {
            var superAdmin = _context.SuperAdmins.Find(superAdminId);
            if (superAdmin != null)
            {
                superAdmin.LastLoginDate = DateTime.Now;
                _context.SaveChanges();
            }
        }
        #endregion
    }
}

