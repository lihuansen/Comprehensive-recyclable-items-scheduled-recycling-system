# 用户反馈功能故障排除指南

## 问题：反馈功能"还是没实现"

如果用户反馈功能无法正常工作，请按以下步骤排查：

## 检查清单

### 1. 数据库表是否存在？

**检查方法**：
```sql
USE RecyclingSystemDB;
GO

-- 检查表是否存在
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'UserFeedback';

-- 如果表存在，检查表结构
EXEC sp_help 'UserFeedback';
```

**如果表不存在**：
```sql
-- 运行创建脚本
-- 文件位置: Database/CreateUserFeedbackTable.sql
```

### 2. 用户是否已登录？

**症状**：点击"提交反馈"后显示"请先登录"

**解决方法**：
1. 访问登录页面
2. 使用有效的用户账号登录
3. 登录成功后再访问反馈页面

### 3. JavaScript 是否正常加载？

**检查方法**：
1. 打开浏览器开发者工具（F12）
2. 进入 Console 标签
3. 查看是否有 JavaScript 错误

**常见错误**：
- `$ is not defined` → jQuery 未加载
- `__RequestVerificationToken is not found` → AntiForgeryToken 问题

**解决方法**：
确保 `_Layout.cshtml` 中包含 jQuery：
```html
<script src="~/Scripts/jquery-3.x.x.min.js"></script>
```

### 4. AJAX 请求是否成功？

**检查方法**：
1. 打开浏览器开发者工具（F12）
2. 进入 Network 标签
3. 提交反馈表单
4. 查看 `/Home/SubmitFeedback` 请求

**可能的响应**：
- **200 OK + success: true** → 成功！
- **200 OK + success: false** → 服务器端验证失败，查看 message
- **400 Bad Request** → 请求格式错误
- **500 Internal Server Error** → 服务器端异常

### 5. 数据库连接是否正常？

**检查连接字符串**（Web.config）：
```xml
<connectionStrings>
  <add name="RecyclingDB" 
       connectionString="data source=.;initial catalog=RecyclingSystemDB;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**测试连接**：
1. 打开 SQL Server Management Studio
2. 使用相同的服务器名和数据库名连接
3. 确认可以访问 UserFeedback 表

### 6. 服务器端验证错误

**常见验证失败原因**：

| 错误消息 | 原因 | 解决方法 |
|---------|------|---------|
| "反馈对象不能为空" | 创建 UserFeedback 对象失败 | 检查 Controller 代码 |
| "用户ID无效" | UserID <= 0 | 确保用户正确登录 |
| "请选择反馈类型" | FeedbackType 为空 | 检查表单是否正确提交 |
| "反馈类型无效" | 类型不在允许范围内 | 确保使用：问题反馈、功能建议、投诉举报、其他 |
| "请输入反馈主题" | Subject 为空 | 填写主题字段 |
| "反馈主题不能超过100字" | Subject.Length > 100 | 缩短主题长度 |
| "请输入详细描述" | Description 为空 | 填写描述字段 |
| "详细描述至少需要10个字符" | Description.Length < 10 | 增加描述内容 |
| "详细描述不能超过1000字" | Description.Length > 1000 | 缩短描述长度 |
| "邮箱格式不正确" | 邮箱格式无效 | 使用有效的邮箱格式 |

## 调试步骤

### 步骤 1：检查导航链接
1. 访问网站首页
2. 查看导航栏是否有"问题反馈"链接
3. 点击链接是否跳转到 `/Home/Feedback`

### 步骤 2：检查页面加载
1. 访问 `/Home/Feedback`
2. 确认页面正常显示反馈表单
3. 查看浏览器 Console 是否有错误

### 步骤 3：测试表单提交
1. 选择反馈类型：问题反馈
2. 输入主题：测试主题（少于100字）
3. 输入描述：这是一个测试描述，用于验证反馈功能是否正常工作（至少10字）
4. 可选输入邮箱：test@example.com
5. 点击"提交反馈"按钮

### 步骤 4：查看响应
- 应该显示"反馈提交成功！感谢您的反馈"
- 表单应该被清空
- 2秒后自动跳转到首页

### 步骤 5：验证数据库
```sql
-- 查看最新的反馈记录
SELECT TOP 1 * 
FROM UserFeedback 
ORDER BY CreatedDate DESC;
```

应该看到刚提交的测试反馈。

## 常见问题和解决方案

### 问题 1：点击提交没有反应

**可能原因**：
- JavaScript 错误
- jQuery 未加载
- 表单验证失败

**解决方法**：
1. 打开浏览器 Console 查看错误
2. 确认 jQuery 已加载
3. 检查所有必填字段是否填写

### 问题 2：提交后显示"请先登录"

**原因**：用户未登录或 Session 过期

**解决方法**：
1. 点击登录
2. 使用有效账号登录
3. 重新访问反馈页面

### 问题 3：提交后显示"提交失败"

**原因**：数据库操作失败

**可能的原因**：
- 数据库连接失败
- UserFeedback 表不存在
- 外键约束失败（UserID 在 Users 表中不存在）
- SQL 权限不足

**解决方法**：
1. 检查数据库连接字符串
2. 运行 CreateUserFeedbackTable.sql
3. 确认 UserID 有效
4. 检查 SQL Server 日志

### 问题 4：数据库中找不到记录

**检查**：
```sql
-- 检查是否有任何记录
SELECT COUNT(*) FROM UserFeedback;

-- 检查特定用户的记录
SELECT * FROM UserFeedback WHERE UserID = 1;

-- 检查今天的记录
SELECT * FROM UserFeedback 
WHERE CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE);
```

**如果没有记录**：
- 提交可能失败了
- 检查服务器日志
- 确认 INSERT 语句执行成功

### 问题 5：AntiForgeryToken 验证失败

**症状**：提交时返回 400 错误

**原因**：CSRF Token 不匹配

**解决方法**：
1. 确认表单中包含 `@Html.AntiForgeryToken()`
2. 确认 AJAX 请求中包含 `__RequestVerificationToken`
3. 刷新页面重新获取 Token

## 启用详细日志

### 在 Controller 中添加日志
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public JsonResult SubmitFeedback(string FeedbackType, string Subject, 
                                  string Description, string ContactEmail)
{
    try
    {
        // 添加日志
        System.Diagnostics.Debug.WriteLine($"收到反馈提交: Type={FeedbackType}, Subject={Subject}");
        
        if (Session["LoginUser"] == null)
        {
            System.Diagnostics.Debug.WriteLine("用户未登录");
            return Json(new { success = false, message = "请先登录" });
        }

        var user = (Users)Session["LoginUser"];
        System.Diagnostics.Debug.WriteLine($"用户ID: {user.UserID}");

        var feedback = new UserFeedback
        {
            UserID = user.UserID,
            FeedbackType = FeedbackType,
            Subject = Subject,
            Description = Description,
            ContactEmail = ContactEmail,
            Status = "反馈中",
            CreatedDate = DateTime.Now
        };

        var (success, message) = _feedbackBLL.AddFeedback(feedback);
        System.Diagnostics.Debug.WriteLine($"BLL 结果: success={success}, message={message}");

        return Json(new { success = success, message = message });
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"异常: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
        return Json(new { success = false, message = "提交失败：" + ex.Message });
    }
}
```

### 查看日志
在 Visual Studio 中：
1. View → Output
2. 选择 "Debug"
3. 提交反馈时查看输出

## 快速验证脚本

运行以下 SQL 脚本快速验证功能：

```sql
-- 1. 检查表是否存在
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserFeedback')
    PRINT '✓ UserFeedback 表存在'
ELSE
    PRINT '✗ UserFeedback 表不存在 - 需要运行 CreateUserFeedbackTable.sql'
GO

-- 2. 检查表结构
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserFeedback'
ORDER BY ORDINAL_POSITION;
GO

-- 3. 测试插入（需要有效的 UserID）
DECLARE @TestUserID INT = 1; -- 修改为实际存在的 UserID

IF EXISTS (SELECT 1 FROM Users WHERE UserID = @TestUserID)
BEGIN
    INSERT INTO UserFeedback 
    (UserID, FeedbackType, Subject, Description, ContactEmail, Status, CreatedDate)
    VALUES 
    (@TestUserID, N'问题反馈', N'测试主题', N'这是一个测试描述，用于验证功能是否正常', 
     'test@example.com', N'反馈中', GETDATE());
    
    IF @@ROWCOUNT > 0
        PRINT '✓ 测试数据插入成功'
    ELSE
        PRINT '✗ 测试数据插入失败'
        
    -- 查看插入的数据
    SELECT TOP 1 * FROM UserFeedback ORDER BY CreatedDate DESC;
END
ELSE
    PRINT '✗ UserID 不存在，无法测试插入'
GO
```

## 联系支持

如果以上步骤都无法解决问题，请收集以下信息：

1. **错误消息**：浏览器 Console 中的完整错误
2. **网络请求**：Network 标签中 SubmitFeedback 请求的详细信息
3. **服务器日志**：IIS 日志或 Visual Studio 输出
4. **数据库状态**：运行上面的验证脚本结果
5. **环境信息**：
   - .NET Framework 版本
   - SQL Server 版本
   - 浏览器版本
   - IIS 版本

提供这些信息将有助于快速诊断和解决问题。
