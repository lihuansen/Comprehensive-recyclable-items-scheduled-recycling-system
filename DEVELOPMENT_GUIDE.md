# 开发工作流程指南

## 目录
1. [开发环境搭建](#开发环境搭建)
2. [代码编写规范](#代码编写规范)
3. [常见开发场景](#常见开发场景)
4. [调试技巧](#调试技巧)
5. [测试指南](#测试指南)

---

## 开发环境搭建

### 必需软件
1. **Visual Studio 2019/2022**
   - 安装 ASP.NET 和 Web 开发工作负载
   - 安装 .NET Framework 4.7.2 或更高版本

2. **SQL Server**
   - SQL Server 2017 或更高版本
   - SQL Server Management Studio (SSMS)

3. **Git**
   - 用于版本控制

### 项目克隆和配置

```bash
# 1. 克隆仓库
git clone https://github.com/lihuansen/Comprehensive-recyclable-items-scheduled-recycling-system.git

# 2. 进入项目目录
cd Comprehensive-recyclable-items-scheduled-recycling-system
```

### 数据库配置

1. **创建数据库**
   ```sql
   CREATE DATABASE RecyclingDB;
   ```

2. **运行数据库脚本**
   - 执行表结构创建脚本
   - 执行初始数据插入脚本

3. **配置连接字符串**
   
   在 `recycling.Web.UI/Web.config` 中配置：
   ```xml
   <connectionStrings>
     <add name="RecyclingDB" 
          connectionString="Data Source=YOUR_SERVER;Initial Catalog=RecyclingDB;Integrated Security=True" 
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```
   
   在 `recycling.DAL/App.config` 中也需要相同配置。

### NuGet 包还原

在 Visual Studio 中：
1. 右键解决方案
2. 选择"还原 NuGet 包"
3. 等待所有包下载完成

### 构建和运行

1. 设置 `recycling.Web.UI` 为启动项目
2. 按 F5 或点击"调试" → "开始调试"
3. 浏览器会自动打开首页

---

## 代码编写规范

### 1. 命名规范

#### 类命名
- **PascalCase**（首字母大写）
- 例如：`UserBLL`、`AppointmentDAL`、`Users`

#### 方法命名
- **PascalCase**
- 动词开头，表示行为
- 例如：`Register()`、`GetUserById()`、`InsertAppointment()`

#### 变量命名
- **camelCase**（首字母小写）
- 私有字段使用 `_` 前缀
- 例如：`userId`、`_userDAL`、`_connectionString`

#### 常量命名
- **UPPER_SNAKE_CASE**（全大写，下划线分隔）
- 例如：`MAX_RETRY_COUNT`、`DEFAULT_PAGE_SIZE`

### 2. 代码结构规范

#### 类结构顺序
```csharp
public class UserBLL
{
    // 1. 私有字段
    private UserDAL _userDAL = new UserDAL();
    private static readonly ConcurrentDictionary<string, (string, DateTime)> _verificationCodes;
    
    // 2. 构造函数
    public UserBLL()
    {
        // 初始化代码
    }
    
    // 3. 公共方法
    public string Register(RegisterViewModel model)
    {
        // 实现
    }
    
    // 4. 私有方法
    private string HashPassword(string password)
    {
        // 实现
    }
}
```

### 3. 注释规范

#### XML 文档注释
```csharp
/// <summary>
/// 用户注册方法，返回错误信息（按优先级排序）
/// 优先级：用户名 > 密码 > 手机号 > 邮箱
/// </summary>
/// <param name="model">注册视图模型</param>
/// <returns>错误信息（null表示注册成功）</returns>
public string Register(RegisterViewModel model)
{
    // 实现
}
```

#### 行内注释
```csharp
// 1. 检查用户名是否已存在
if (_userDAL.IsUsernameExists(model.Username))
{
    return "用户名已存在，请更换其他用户名";
}

// 2. 检查密码一致性
if (model.Password != model.ConfirmPassword)
{
    return "两次输入的密码不一致，请重新输入";
}
```

### 4. 异常处理规范

```csharp
// BLL 层：捕获并转换为业务友好的错误信息
public string Register(RegisterViewModel model)
{
    try
    {
        // 业务逻辑
        int newUserId = _userDAL.InsertUser(user);
        if (newUserId > 0)
        {
            return null; // 成功
        }
        else
        {
            return "注册失败，请稍后重试";
        }
    }
    catch (Exception ex)
    {
        return $"注册失败：{ex.Message}";
    }
}

// DAL 层：抛出异常，由上层处理
public int InsertUser(Users user)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            // SQL 操作
        }
    }
    catch (Exception ex)
    {
        throw new Exception("数据库操作失败：" + ex.Message);
    }
}

// Controller 层：捕获并显示错误
[HttpPost]
public ActionResult Register(RegisterViewModel model)
{
    try
    {
        string errorMsg = _userBLL.Register(model);
        if (errorMsg != null)
        {
            ModelState.AddModelError("", errorMsg);
            return View(model);
        }
        return RedirectToAction("Login");
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", "系统错误：" + ex.Message);
        return View(model);
    }
}
```

---

## 常见开发场景

### 场景1：添加新的用户功能

**需求：** 添加用户头像上传功能

**步骤：**

1. **修改数据模型（Model）**
   ```csharp
   // Users.cs
   public partial class Users
   {
       // 添加新属性
       [StringLength(255)]
       public string AvatarURL { get; set; }
   }
   ```

2. **修改数据库表**
   ```sql
   ALTER TABLE Users ADD AvatarURL NVARCHAR(255) NULL;
   ```

3. **修改视图模型（ViewModel）**
   ```csharp
   // UpdateProfileViewModel.cs
   public class UpdateProfileViewModel
   {
       // 添加新字段
       public HttpPostedFileBase AvatarFile { get; set; }
   }
   ```

4. **修改 DAL 层**
   ```csharp
   // UserDAL.cs
   public bool UpdateUserAvatar(int userId, string avatarURL)
   {
       using (SqlConnection conn = new SqlConnection(_connectionString))
       {
           string sql = "UPDATE Users SET AvatarURL = @AvatarURL WHERE UserID = @UserID";
           SqlCommand cmd = new SqlCommand(sql, conn);
           cmd.Parameters.AddWithValue("@AvatarURL", avatarURL);
           cmd.Parameters.AddWithValue("@UserID", userId);
           
           conn.Open();
           return cmd.ExecuteNonQuery() > 0;
       }
   }
   ```

5. **修改 BLL 层**
   ```csharp
   // UserBLL.cs
   public (bool Success, string Message) UploadAvatar(int userId, HttpPostedFileBase file)
   {
       if (file == null || file.ContentLength == 0)
       {
           return (false, "请选择图片文件");
       }
       
       // 验证文件类型
       string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
       string extension = Path.GetExtension(file.FileName).ToLower();
       if (!allowedExtensions.Contains(extension))
       {
           return (false, "只支持 JPG、PNG、GIF 格式的图片");
       }
       
       // 保存文件
       string fileName = $"{userId}_{DateTime.Now.Ticks}{extension}";
       string uploadPath = HttpContext.Current.Server.MapPath("~/Uploads/Avatars/");
       string filePath = Path.Combine(uploadPath, fileName);
       
       file.SaveAs(filePath);
       
       // 更新数据库
       string avatarURL = $"/Uploads/Avatars/{fileName}";
       bool success = _userDAL.UpdateUserAvatar(userId, avatarURL);
       
       return success ? (true, "头像上传成功") : (false, "头像上传失败");
   }
   ```

6. **修改 Controller**
   ```csharp
   // UserController.cs
   [HttpPost]
   public ActionResult UploadAvatar(HttpPostedFileBase avatarFile)
   {
       if (Session["LoginUser"] == null)
       {
           return RedirectToAction("Login");
       }
       
       var user = (Users)Session["LoginUser"];
       var (success, message) = _userBLL.UploadAvatar(user.UserID, avatarFile);
       
       if (success)
       {
           TempData["SuccessMessage"] = message;
       }
       else
       {
           TempData["ErrorMessage"] = message;
       }
       
       return RedirectToAction("Profile", "Home");
   }
   ```

7. **修改视图（View）**
   ```html
   @* Profile.cshtml *@
   <div class="avatar-section">
       <img src="@Model.AvatarURL" alt="头像" />
       <form method="post" action="@Url.Action("UploadAvatar", "User")" enctype="multipart/form-data">
           @Html.AntiForgeryToken()
           <input type="file" name="avatarFile" accept="image/*" />
           <button type="submit">上传头像</button>
       </form>
   </div>
   ```

### 场景2：添加新的订单状态

**需求：** 添加"待支付"状态

**步骤：**

1. **修改数据库枚举值**
   - 确认 `Appointments.Status` 字段可以存储新值

2. **修改订单状态转换逻辑**
   ```
   已预约 → 待支付 → 进行中 → 已完成
      ↓         ↓
   已取消    已取消
   ```

3. **修改 BLL 层逻辑**
   ```csharp
   // OrderBLL.cs
   public (bool Success, string Message) MarkAsPendingPayment(int appointmentId, int userId)
   {
       // 只能将"已预约"状态的订单标记为"待支付"
       var order = _orderDAL.GetOrderDetail(appointmentId, userId);
       if (order.Appointment.Status != "已预约")
       {
           return (false, "只能将已预约的订单标记为待支付");
       }
       
       bool success = _orderDAL.UpdateOrderStatus(appointmentId, "待支付");
       return success ? (true, "订单已标记为待支付") : (false, "操作失败");
   }
   ```

4. **修改视图逻辑**
   - 在订单列表中显示"待支付"状态
   - 添加"去支付"按钮

### 场景3：添加新的回收员功能

**需求：** 回收员可以查看历史订单统计

**步骤：**

1. **创建视图模型**
   ```csharp
   // RecyclerStatisticsViewModel.cs
   public class RecyclerStatisticsViewModel
   {
       public int TotalOrders { get; set; }
       public int CompletedOrders { get; set; }
       public decimal TotalWeight { get; set; }
       public decimal AverageRating { get; set; }
       public List<MonthlyStatistics> MonthlyStats { get; set; }
   }
   
   public class MonthlyStatistics
   {
       public string Month { get; set; }
       public int OrderCount { get; set; }
       public decimal TotalWeight { get; set; }
   }
   ```

2. **创建 DAL 方法**
   ```csharp
   // RecyclerOrderDAL.cs
   public RecyclerStatisticsViewModel GetRecyclerStatistics(int recyclerId)
   {
       using (SqlConnection conn = new SqlConnection(_connectionString))
       {
           string sql = @"
           SELECT 
               COUNT(*) AS TotalOrders,
               SUM(CASE WHEN Status = '已完成' THEN 1 ELSE 0 END) AS CompletedOrders,
               SUM(EstimatedWeight) AS TotalWeight,
               AVG(Rating) AS AverageRating
           FROM Appointments
           WHERE RecyclerID = @RecyclerID";
           
           // 执行查询并返回结果
       }
   }
   ```

3. **创建 BLL 方法**
   ```csharp
   // RecyclerOrderBLL.cs
   public RecyclerStatisticsViewModel GetStatistics(int recyclerId)
   {
       return _recyclerOrderDAL.GetRecyclerStatistics(recyclerId);
   }
   ```

4. **添加 Controller Action**
   ```csharp
   // StaffController.cs
   public ActionResult Statistics()
   {
       if (Session["LoginStaff"] == null)
           return RedirectToAction("Login");
       
       var recycler = (Recyclers)Session["LoginStaff"];
       var statistics = _recyclerOrderBLL.GetStatistics(recycler.RecyclerID);
       
       return View(statistics);
   }
   ```

5. **创建视图**
   ```html
   @* Views/Staff/Statistics.cshtml *@
   @model RecyclerStatisticsViewModel
   
   <h2>我的统计数据</h2>
   <div class="statistics">
       <p>总订单数：@Model.TotalOrders</p>
       <p>已完成订单：@Model.CompletedOrders</p>
       <p>总回收重量：@Model.TotalWeight kg</p>
       <p>平均评分：@Model.AverageRating</p>
   </div>
   ```

---

## 调试技巧

### 1. 使用 Visual Studio 调试器

**设置断点：**
- 在代码行号左侧点击，设置断点（红色圆点）
- 按 F9 也可以设置/取消断点

**调试快捷键：**
- F5：开始调试
- F10：单步跳过（Step Over）
- F11：单步进入（Step Into）
- Shift+F11：跳出（Step Out）
- Ctrl+F10：运行到光标处

**查看变量值：**
- 鼠标悬停在变量上查看值
- 使用"监视"窗口（Ctrl+Alt+W, 1）
- 使用"即时窗口"（Ctrl+Alt+I）执行表达式

### 2. 使用 Debug.WriteLine 输出调试信息

```csharp
public (bool Success, int AppointmentId, string ErrorMessage) SubmitAppointment(
    AppointmentSubmissionModel submission, int userId)
{
    System.Diagnostics.Debug.WriteLine("BLL层开始处理预约提交...");
    System.Diagnostics.Debug.WriteLine($"UserID={userId}, Type={submission.BasicInfo.AppointmentType}");
    
    // 业务逻辑
    
    System.Diagnostics.Debug.WriteLine($"DAL层返回: Success={result.Success}, AppointmentId={result.AppointmentId}");
    
    return result;
}
```

查看输出：
- Visual Studio → 视图 → 输出
- 或按 Ctrl+Alt+O

### 3. 使用 SQL Server Profiler 追踪 SQL

1. 打开 SQL Server Profiler
2. 新建跟踪
3. 选择事件（如 SQL:BatchCompleted）
4. 运行应用，查看执行的 SQL 语句

### 4. 浏览器开发者工具

**F12 打开开发者工具：**
- **Console**：查看 JavaScript 错误和 console.log 输出
- **Network**：查看 AJAX 请求和响应
- **Elements**：检查 HTML 结构和 CSS

**查看 AJAX 请求：**
```javascript
// 在前端代码中添加
$.ajax({
    url: '/User/SendVerificationCode',
    type: 'POST',
    data: { phoneNumber: phone },
    success: function(response) {
        console.log('响应数据:', response);
    },
    error: function(xhr, status, error) {
        console.error('请求失败:', error);
    }
});
```

### 5. 常见问题排查

**问题：登录后 Session 丢失**
```csharp
// 检查 Session 配置
// Web.config
<system.web>
  <sessionState mode="InProc" timeout="30" />
</system.web>

// 检查是否正确设置 Session
Session["LoginUser"] = user;
Session.Timeout = 30;
```

**问题：SQL 参数化查询报错**
```csharp
// 错误写法
cmd.Parameters.AddWithValue("@UserID", null);  // 不能传 null

// 正确写法
cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
```

**问题：事务未提交**
```csharp
// 确保 transaction.Commit() 在 try 块中
try
{
    // SQL 操作
    transaction.Commit();  // 必须调用
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## 测试指南

### 1. 单元测试（建议添加）

使用 MSTest 或 NUnit 框架：

```csharp
// UserBLLTests.cs
[TestClass]
public class UserBLLTests
{
    private UserBLL _userBLL;
    
    [TestInitialize]
    public void Setup()
    {
        _userBLL = new UserBLL();
    }
    
    [TestMethod]
    public void Register_ValidInput_ReturnsNull()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Username = "testuser",
            Password = "Test@123",
            ConfirmPassword = "Test@123",
            PhoneNumber = "13800138000",
            Email = "test@example.com"
        };
        
        // Act
        string result = _userBLL.Register(model);
        
        // Assert
        Assert.IsNull(result);
    }
    
    [TestMethod]
    public void Register_DuplicateUsername_ReturnsError()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Username = "existinguser",  // 已存在的用户名
            // ...
        };
        
        // Act
        string result = _userBLL.Register(model);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("用户名已存在"));
    }
}
```

### 2. 集成测试

测试完整的业务流程：

```csharp
[TestClass]
public class AppointmentFlowTests
{
    [TestMethod]
    public void CompleteAppointmentFlow_Success()
    {
        // 1. 用户注册
        // 2. 用户登录
        // 3. 创建预约
        // 4. 回收员接单
        // 5. 发送消息
        // 6. 完成订单
        
        // 验证每个步骤的结果
    }
}
```

### 3. 手动测试清单

**用户功能测试：**
- [ ] 注册功能（各种验证场景）
- [ ] 登录功能（密码、手机、邮箱）
- [ ] 忘记密码
- [ ] 修改个人信息
- [ ] 修改密码
- [ ] 创建预约
- [ ] 查看订单列表
- [ ] 查看订单详情
- [ ] 取消订单
- [ ] 发送消息
- [ ] 退出登录

**回收员功能测试：**
- [ ] 登录
- [ ] 查看待接单订单
- [ ] 接收订单
- [ ] 查看我的订单
- [ ] 发送消息
- [ ] 完成订单
- [ ] 退出登录

**边界测试：**
- [ ] 输入超长字符串
- [ ] 输入特殊字符（SQL 注入测试）
- [ ] 未登录状态访问需要登录的页面
- [ ] Session 过期后的行为
- [ ] 并发操作测试

### 4. 性能测试

**数据库查询性能：**
```sql
-- 查看执行计划
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

SELECT * FROM Appointments WHERE UserID = 1;

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
```

**压力测试（使用工具如 JMeter）：**
- 模拟 100 个并发用户登录
- 模拟 50 个并发用户创建预约
- 观察响应时间和错误率

---

## 常用 Git 命令

```bash
# 查看状态
git status

# 添加文件到暂存区
git add .

# 提交更改
git commit -m "添加用户头像上传功能"

# 推送到远程仓库
git push origin main

# 拉取最新代码
git pull origin main

# 创建新分支
git checkout -b feature/avatar-upload

# 切换分支
git checkout main

# 合并分支
git merge feature/avatar-upload
```

---

**文档版本：1.0**  
**更新日期：2025-11-03**  
**适用系统版本：全品类可回收物预约回收系统**
