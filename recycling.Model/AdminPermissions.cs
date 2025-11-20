namespace recycling.Model
{
    /// <summary>
    /// 管理员权限常量定义
    /// </summary>
    public static class AdminPermissions
    {
        // 权限常量
        public const string UserManagement = "user_management";
        public const string RecyclerManagement = "recycler_management";
        public const string FeedbackManagement = "feedback_management";
        public const string HomepageManagement = "homepage_management";
        public const string FullAccess = "full_access";

        // 权限显示名称映射
        public static string GetDisplayName(string permission)
        {
            switch (permission)
            {
                case UserManagement:
                    return "用户管理";
                case RecyclerManagement:
                    return "回收员管理";
                case FeedbackManagement:
                    return "反馈管理";
                case HomepageManagement:
                    return "首页页面管理";
                case FullAccess:
                    return "全部权限";
                default:
                    return "未知权限";
            }
        }

        // 获取所有权限列表
        public static string[] GetAllPermissions()
        {
            return new string[]
            {
                UserManagement,
                RecyclerManagement,
                FeedbackManagement,
                HomepageManagement,
                FullAccess
            };
        }

        // 检查权限是否有效
        public static bool IsValidPermission(string permission)
        {
            return permission == UserManagement ||
                   permission == RecyclerManagement ||
                   permission == FeedbackManagement ||
                   permission == HomepageManagement ||
                   permission == FullAccess;
        }

        // 检查管理员是否有指定权限
        public static bool HasPermission(string adminCharacter, string requiredPermission)
        {
            // 如果没有设置权限或权限为空，默认拒绝访问
            if (string.IsNullOrEmpty(adminCharacter))
            {
                return false;
            }

            // 如果是全部权限，则有所有权限
            if (adminCharacter == FullAccess)
            {
                return true;
            }

            // 检查是否有指定权限
            return adminCharacter == requiredPermission;
        }
    }
}
