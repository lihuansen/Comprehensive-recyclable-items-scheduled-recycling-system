# 全品类可回收物预约回收系统 - 架构设计文档

## 目录
1. [系统概述](#系统概述)
2. [架构设计](#架构设计)
3. [数据模型](#数据模型)
4. [业务逻辑层详解](#业务逻辑层详解)
5. [数据访问层详解](#数据访问层详解)
6. [控制器层详解](#控制器层详解)
7. [核心业务流程](#核心业务流程)
8. [安全机制](#安全机制)
9. [扩展性考虑](#扩展性考虑)

---

## 系统概述

### 项目背景
这是一个基于 ASP.NET MVC 的**可回收物品预约回收管理系统**，旨在为用户提供便捷的回收物品预约服务，同时为回收员提供高效的订单管理平台。

### 核心功能
1. **用户端功能**
   - 多种登录方式（用户名密码、手机验证码、邮箱验证码）
   - 可回收物品查询与价格估算
   - 预约回收服务（支持多品类、紧急预约）
   - 订单管理与追踪
   - 与回收员实时消息沟通

2. **回收员功能**
   - 接收和管理回收订单
   - 查看订单详情和品类信息
   - 与用户消息沟通
   - 完成订单流程

3. **管理员功能**
   - 系统数据管理
   - 用户和回收员管理
   - 订单监控

---

## 架构设计

### 整体架构模式
系统采用经典的 **三层架构（3-Tier Architecture）+ MVC 模式**：

```
┌─────────────────────────────────────────────┐
│         Presentation Layer (UI)             │
│    ASP.NET MVC Controllers + Views          │
│  (UserController, HomeController, etc.)     │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│       Business Logic Layer (BLL)            │
│   (UserBLL, AppointmentBLL, OrderBLL)       │
│   - 业务规则验证                              │
│   - 业务流程控制                              │
│   - 数据转换                                  │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│       Data Access Layer (DAL)               │
│   (UserDAL, AppointmentDAL, OrderDAL)       │
│   - SQL 查询执行                             │
│   - 数据库连接管理                            │
│   - 事务处理                                  │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│          Database (SQL Server)              │
│   - Users, Recyclers, Appointments          │
│   - Messages, RecyclableItems, etc.         │
└─────────────────────────────────────────────┘
```

### 项目结构
```
Solution: 全品类可回收物预约回收系统（解决方案）
├── recycling.Web.UI         # 表示层（ASP.NET MVC Web应用）
│   ├── Controllers/         # MVC控制器
│   ├── Views/              # Razor视图
│   ├── Scripts/            # JavaScript文件
│   └── App_Start/          # 应用配置
│
├── recycling.BLL            # 业务逻辑层
│   ├── UserBLL.cs          # 用户业务逻辑
│   ├── AppointmentBLL.cs   # 预约业务逻辑
│   ├── OrderBLL.cs         # 订单业务逻辑
│   ├── MessageBLL.cs       # 消息业务逻辑
│   └── ...
│
├── recycling.DAL            # 数据访问层
│   ├── UserDAL.cs          # 用户数据访问
│   ├── AppointmentDAL.cs   # 预约数据访问
│   ├── OrderDAL.cs         # 订单数据访问
│   └── ...
│
├── recycling.Model          # 数据模型层
│   ├── Users.cs            # 用户实体
│   ├── Appointments.cs     # 预约实体
│   ├── RecyclableItems.cs  # 可回收物实体
│   └── *ViewModel.cs       # 视图模型
│
└── recycling.Common         # 公共工具层
    └── EmailService.cs     # 邮件服务
```

---

## 数据模型

### 核心实体关系图（ER Diagram）

```
┌──────────────┐         ┌──────────────────┐
│    Users     │         │   Appointments   │
│──────────────│         │──────────────────│
│ UserID (PK)  │◄────────│ UserID (FK)      │
│ Username     │    1:N  │ AppointmentID(PK)│
│ PasswordHash │         │ AppointmentType  │
│ PhoneNumber  │         │ AppointmentDate  │
│ Email        │         │ Status           │
└──────────────┘         │ RecyclerID (FK)  │
                         └────────┬─────────┘
                                  │
                                  │ 1:N
                                  ▼
                         ┌──────────────────────┐
                         │ AppointmentCategories│
                         │──────────────────────│
                         │ CategoryID (PK)      │
                         │ AppointmentID (FK)   │
                         │ CategoryName         │
                         │ QuestionsAnswers     │
                         └──────────────────────┘

┌──────────────┐         ┌──────────────────┐
│  Recyclers   │         │   Appointments   │
│──────────────│         │──────────────────│
│RecyclerID(PK)│────────►│ RecyclerID (FK)  │
│ Username     │    1:N  │ (回收员接单)       │
│ PasswordHash │         └──────────────────┘
│ PhoneNumber  │
│ Region       │
│ Rating       │
└──────────────┘

┌──────────────┐
│   Messages   │         关联到 Appointments
│──────────────│
│ MessageID(PK)│
│ OrderID (FK) │────────► Appointments
│ SenderType   │          (通过OrderID)
│ SenderID     │
│ Content      │
│ SentTime     │
└──────────────┘

┌──────────────────┐
│ RecyclableItems  │    独立的物品价格表
│──────────────────│
│ ItemId (PK)      │
│ Name             │
│ Category         │
│ PricePerKg       │
│ IsActive         │
└──────────────────┘
```

### 主要实体说明

#### 1. Users（用户表）
```csharp
public partial class Users
{
    public int UserID { get; set; }              // 主键
    public string Username { get; set; }         // 用户名（唯一）
    public string PasswordHash { get; set; }     // SHA256密码哈希
    public string PhoneNumber { get; set; }      // 手机号（唯一）
    public string Email { get; set; }            // 邮箱（唯一）
    public DateTime RegistrationDate { get; set; }  // 注册时间
    public DateTime? LastLoginDate { get; set; }    // 最后登录时间
}
```
**设计要点：**
- 用户名、手机号、邮箱均为唯一约束
- 密码使用 SHA256 哈希存储，不存储明文
- 支持多种登录方式（用户名+密码、手机验证码、邮箱验证码）

#### 2. Appointments（预约订单表）
```csharp
public partial class Appointments
{
    public int AppointmentID { get; set; }         // 主键
    public int UserID { get; set; }                // 外键：用户ID
    public string AppointmentType { get; set; }    // 预约类型（上门回收/自送等）
    public DateTime AppointmentDate { get; set; }  // 预约日期
    public string TimeSlot { get; set; }           // 时间段
    public decimal EstimatedWeight { get; set; }   // 预估重量
    public bool IsUrgent { get; set; }             // 是否紧急
    public string Address { get; set; }            // 回收地址
    public string ContactName { get; set; }        // 联系人
    public string ContactPhone { get; set; }       // 联系电话
    public string SpecialInstructions { get; set; } // 特殊说明
    public decimal? EstimatedPrice { get; set; }   // 预估价格
    public string Status { get; set; }             // 订单状态
    public DateTime CreatedDate { get; set; }      // 创建时间
    public DateTime UpdatedDate { get; set; }      // 更新时间
    public int? RecyclerID { get; set; }          // 外键：回收员ID（接单后分配）
}
```
**订单状态流转：**
```
已预约 → 进行中 → 已完成
   ↓
已取消
```

#### 3. AppointmentCategories（预约品类详情表）
```csharp
public partial class AppointmentCategories
{
    public int CategoryID { get; set; }           // 主键
    public int AppointmentID { get; set; }        // 外键：预约ID
    public string CategoryName { get; set; }      // 品类名称（玻璃、金属等）
    public string CategoryKey { get; set; }       // 品类键名（glass, metal等）
    public string QuestionsAnswers { get; set; }  // JSON格式的问答数据
    public DateTime CreatedDate { get; set; }     // 创建时间
}
```
**QuestionsAnswers 示例：**
```json
{
  "question1": "玻璃类型",
  "answer1": "平板玻璃",
  "question2": "是否含杂质",
  "answer2": "无"
}
```

#### 4. Recyclers（回收员表）
```csharp
public partial class Recyclers
{
    public int RecyclerID { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public bool Available { get; set; }          // 是否可接单
    public string PhoneNumber { get; set; }
    public string FullName { get; set; }
    public string Region { get; set; }           // 负责区域
    public decimal? Rating { get; set; }         // 评分
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; }
    public string AvatarURL { get; set; }
}
```

#### 5. Messages（消息表）
```csharp
public partial class Messages
{
    public int MessageID { get; set; }
    public int? OrderID { get; set; }            // 关联的订单ID
    public string SenderType { get; set; }       // 发送者类型（user/recycler）
    public int? SenderID { get; set; }           // 发送者ID
    public string Content { get; set; }          // 消息内容
    public DateTime? SentTime { get; set; }      // 发送时间
    public bool? IsRead { get; set; }            // 是否已读
}
```

#### 6. Conversations（会话表）
```csharp
public partial class Conversations
{
    public int ConversationID { get; set; }
    public int? OrderID { get; set; }
    public int? UserID { get; set; }
    public int? RecyclerID { get; set; }
    public string Status { get; set; }           // 会话状态
    public DateTime? CreatedTime { get; set; }
    public DateTime? EndedTime { get; set; }     // 会话结束时间
}
```

#### 7. RecyclableItems（可回收物品价格表）
```csharp
public partial class RecyclableItems
{
    public int ItemId { get; set; }
    public string Name { get; set; }             // 物品名称
    public string Category { get; set; }         // 品类代码
    public string CategoryName { get; set; }     // 品类名称
    public decimal PricePerKg { get; set; }      // 每公斤价格
    public string Description { get; set; }      // 描述
    public int SortOrder { get; set; }           // 排序
    public bool IsActive { get; set; }           // 是否有效
}
```

---

## 业务逻辑层详解

### 设计原则
1. **单一职责**：每个 BLL 类负责一个业务领域
2. **验证集中**：所有业务规则和数据验证在 BLL 层完成
3. **异常封装**：将 DAL 层异常转换为业务友好的错误信息
4. **无状态**：BLL 对象不保存状态，便于并发和扩展

### 核心 BLL 类

#### 1. UserBLL（用户业务逻辑）

**主要职责：**
- 用户注册验证
- 多种登录方式处理
- 密码管理（修改、重置）
- 用户信息更新
- 验证码生成与验证

**关键方法：**

```csharp
public class UserBLL
{
    private UserDAL _userDAL = new UserDAL();
    
    // 用户注册（包含完整的验证逻辑）
    public string Register(RegisterViewModel model)
    {
        // 验证优先级：用户名 > 密码 > 手机号 > 邮箱
        // 1. 检查用户名是否已存在
        // 2. 检查密码一致性
        // 3. 检查手机号格式和唯一性
        // 4. 检查邮箱格式和唯一性
        // 5. 创建用户并插入数据库
        // 返回 null 表示成功，否则返回错误信息
    }
    
    // 用户名密码登录
    public string Login(LoginViewModel model)
    {
        // 1. 验证用户名是否存在
        // 2. 验证密码是否正确（SHA256哈希比对）
        // 3. 验证验证码
        // 返回 null 表示成功
    }
    
    // 手机验证码登录
    public (string ErrorMessage, Users User) PhoneLogin(string phoneNumber, string verificationCode)
    {
        // 1. 检查手机号是否已注册
        // 2. 验证验证码（有效期5分钟，一次性使用）
        // 3. 获取用户信息
    }
    
    // 邮箱验证码登录
    public (string ErrorMsg, Users User) EmailLogin(string email, string verificationCode)
    
    // 生成验证码（6位数字，5分钟有效）
    public string GenerateVerificationCode(string phoneNumber)
    {
        var random = new Random();
        string code = random.Next(100000, 999999).ToString();
        _verificationCodes[phoneNumber] = (code, DateTime.Now.AddMinutes(5));
        return code;
    }
    
    // 验证验证码（一次有效）
    public bool VerifyVerificationCode(string phoneNumber, string inputCode)
    
    // 密码哈希处理（SHA256）
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
```

**验证码存储设计：**
```csharp
// 线程安全的验证码存储（手机号/邮箱 -> 验证码+过期时间）
private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpireTime)> 
    _verificationCodes = new ConcurrentDictionary<string, (string, DateTime)>();
```

#### 2. AppointmentBLL（预约业务逻辑）

**主要职责：**
- 处理完整的预约提交流程
- 品类数据转换和处理
- 价格估算

**关键方法：**

```csharp
public class AppointmentBLL
{
    private AppointmentDAL _appointmentDAL = new AppointmentDAL();
    
    // 提交完整预约信息（含事务处理）
    public (bool Success, int AppointmentId, string ErrorMessage) 
        SubmitAppointment(AppointmentSubmissionModel submission, int userId)
    {
        // 1. 创建预约基础信息对象
        var appointment = new Appointments
        {
            UserID = userId,
            AppointmentType = submission.BasicInfo.AppointmentType,
            Status = "已预约",
            CreatedDate = DateTime.Now,
            // ... 其他字段
        };
        
        // 2. 创建品类详情列表
        var categories = new List<AppointmentCategories>();
        foreach (var categoryKey in submission.BasicInfo.SelectedCategories)
        {
            var category = new AppointmentCategories
            {
                CategoryName = GetCategoryDisplayName(categoryKey),
                CategoryKey = categoryKey,
                QuestionsAnswers = JsonConvert.SerializeObject(categoryAnswers),
                CreatedDate = DateTime.Now
            };
            categories.Add(category);
        }
        
        // 3. 调用 DAL 层在事务中插入完整数据
        return _appointmentDAL.InsertCompleteAppointment(appointment, categories);
    }
}
```

#### 3. OrderBLL（订单业务逻辑）

**主要职责：**
- 订单列表查询和筛选
- 订单详情获取
- 订单状态管理
- 订单统计

**关键方法：**

```csharp
public class OrderBLL
{
    private OrderDAL _orderDAL = new OrderDAL();
    
    // 获取用户订单列表（支持状态筛选）
    public List<AppointmentOrder> GetUserOrders(int userId, string status = "all")
    
    // 获取订单详情（含品类信息和回收员信息）
    public OrderDetail GetOrderDetail(int appointmentId, int userId)
    
    // 获取各状态订单数量统计
    public OrderStatistics GetOrderStatistics(int userId)
    {
        return new OrderStatistics
        {
            Total = GetUserOrders(userId, "all").Count,
            Pending = GetUserOrders(userId, "pending").Count,
            Confirmed = GetUserOrders(userId, "confirmed").Count,
            Completed = GetUserOrders(userId, "completed").Count,
            Cancelled = GetUserOrders(userId, "cancelled").Count
        };
    }
    
    // 取消订单（只能取消"已预约"状态的订单）
    public (bool Success, string Message) CancelOrder(int appointmentId, int userId)
    
    // 完成订单
    public (bool Success, string Message) CompleteOrder(int appointmentId, int recyclerId)
}
```

#### 4. MessageBLL（消息业务逻辑）

**主要职责：**
- 发送消息（用户和回收员）
- 获取聊天记录
- 标记消息已读

**关键方法：**

```csharp
public class MessageBLL
{
    private readonly MessageDAL _messageDAL = new MessageDAL();
    
    // 发送消息
    public (bool Success, string Message) SendMessage(SendMessageRequest request)
    {
        var message = new Messages
        {
            OrderID = request.OrderID,
            SenderType = request.SenderType,  // "user" 或 "recycler"
            SenderID = request.SenderID,
            Content = request.Content,
            SentTime = DateTime.Now,
            IsRead = false
        };
        // ...
    }
    
    // 获取订单聊天记录
    public List<Messages> GetOrderMessages(int orderId)
    
    // 标记消息为已读
    public bool MarkMessagesAsRead(int orderId, string readerType, int readerId)
    
    // 用户发送消息给回收员（含状态验证）
    public (bool Success, string Message) UserSendMessage(SendMessageRequest request)
    {
        // 校验订单状态为"进行中"才允许发送
    }
}
```

#### 5. RecyclerOrderBLL（回收员订单业务逻辑）

**主要职责：**
- 回收员订单列表（支持筛选和分页）
- 接收订单
- 完成订单判断逻辑
- 回收员消息管理

**关键方法：**

```csharp
public class RecyclerOrderBLL
{
    // 获取回收员订单列表（含 CanComplete 判断）
    public PagedResult<RecyclerOrderViewModel> 
        GetRecyclerOrders(OrderFilterModel filter, int recyclerId = 0)
    {
        // CanComplete 判断规则：
        // 1. 订单状态为"进行中"
        // 2. 存在最近一次结束会话
        // 3. 在会话结束后没有新消息
    }
    
    // 回收员接收订单
    public (bool Success, string Message) AcceptOrder(int appointmentId, int recyclerId)
    
    // 获取回收员订单统计
    public RecyclerOrderStatistics GetRecyclerOrderStatistics(int recyclerId)
}
```

#### 6. StaffBLL（工作人员业务逻辑）

**主要职责：**
- 工作人员登录验证（回收员、管理员、超级管理员）
- 密码验证（与 UserBLL 一致的 SHA256 哈希）

**关键方法：**

```csharp
public class StaffBLL
{
    // 工作人员登录（根据角色区分）
    public (string ErrorMsg, object Staff) Login(string role, string username, string password)
    {
        string passwordHash = HashPassword(password);
        
        switch (role.ToLower())
        {
            case "recycler":
                return ValidateRecycler(username, passwordHash);
            case "admin":
                return ValidateAdmin(username, passwordHash);
            case "superadmin":
                return ValidateSuperAdmin(username, passwordHash);
            default:
                return ("无效的角色", null);
        }
    }
}
```

---

## 数据访问层详解

### 设计原则
1. **职责分离**：DAL 只负责数据访问，不包含业务逻辑
2. **参数化查询**：所有 SQL 使用参数化防止 SQL 注入
3. **异常处理**：捕获数据库异常并向上抛出
4. **事务支持**：关键操作使用事务保证数据一致性

### 核心 DAL 类

#### 1. UserDAL（用户数据访问）

**数据库连接：**
```csharp
private string _connectionString = 
    ConfigurationManager.ConnectionStrings["RecyclingDB"].ConnectionString;
```

**关键方法：**

```csharp
public class UserDAL
{
    // 检查用户名是否已存在
    public bool IsUsernameExists(string username)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
    }
    
    // 插入新用户
    public int InsertUser(Users user)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string sql = @"INSERT INTO Users (Username, PasswordHash, PhoneNumber, Email, RegistrationDate)
                          VALUES (@Username, @PasswordHash, @PhoneNumber, @Email, @RegistrationDate);
                          SELECT SCOPE_IDENTITY();";
            
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            // ... 其他参数
            
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());  // 返回新插入的 UserID
        }
    }
    
    // 根据用户名查询用户
    public Users GetUserByUsername(string username)
    {
        // 使用 SqlDataReader 读取数据
        // 返回 Users 对象
    }
    
    // 更新最后登录时间
    public void UpdateLastLoginDate(int userId, DateTime lastLoginDate)
    
    // 更新密码
    public bool UpdatePasswordByPhone(string phoneNumber, string newPasswordHash)
    
    // 更新用户基本信息
    public bool UpdateUserProfile(int userId, string username, string phoneNumber, string email)
}
```

#### 2. AppointmentDAL（预约数据访问）

**关键方法：**

```csharp
public class AppointmentDAL
{
    // 在事务中插入完整的预约信息
    public (bool Success, int AppointmentId, string ErrorMessage) 
        InsertCompleteAppointment(Appointments appointment, List<AppointmentCategories> categories)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. 插入预约基础信息，获取 AppointmentID
                    int appointmentId = Convert.ToInt32(appointmentCmd.ExecuteScalar());
                    
                    // 2. 插入所有品类详情
                    foreach (var category in categories)
                    {
                        category.AppointmentID = appointmentId;
                        // 执行插入品类的 SQL
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
```

**事务设计要点：**
- 使用 `SqlTransaction` 确保预约和品类数据的原子性
- 失败时回滚，保证数据一致性
- 返回新创建的 `AppointmentID` 供后续使用

#### 3. OrderDAL（订单数据访问）

**关键方法：**

```csharp
public class OrderDAL
{
    // 根据用户ID和状态获取订单列表
    public List<AppointmentOrder> GetOrdersByUserAndStatus(int userId, string status = "all")
    {
        // SQL 使用 STUFF 和 FOR XML PATH 聚合品类名称
        string sql = @"
        SELECT 
            a.*,
            STUFF((
                SELECT DISTINCT ', ' + ac.CategoryName
                FROM AppointmentCategories ac
                WHERE ac.AppointmentID = a.AppointmentID
                FOR XML PATH('')
            ), 1, 2, '') AS CategoryNames
        FROM Appointments a
        WHERE a.UserID = @UserID";
        
        // 根据状态筛选
        if (status != "all")
        {
            sql += " AND a.Status = @Status";
        }
        // ...
    }
    
    // 获取订单详情（含品类信息和回收员信息）
    public OrderDetail GetOrderDetail(int appointmentId, int userId)
    {
        // LEFT JOIN Recyclers 获取回收员信息
        // LEFT JOIN AppointmentCategories 获取品类详情
        // 使用 SqlDataReader 逐行读取并组装对象
    }
    
    // 取消订单
    public bool CancelOrder(int appointmentId, int userId)
    {
        // 只能取消状态为"已预约"的订单
        string sql = @"
        UPDATE Appointments 
        SET Status = '已取消', UpdatedDate = @UpdatedDate
        WHERE AppointmentID = @AppointmentID 
          AND UserID = @UserID 
          AND Status = '已预约'";
    }
    
    // 更新订单状态
    public bool UpdateOrderStatus(int appointmentId, string newStatus)
}
```

#### 4. MessageDAL、ConversationDAL 等

遵循相同的设计模式，提供基础的 CRUD 操作。

---

## 控制器层详解

### MVC 控制器设计

#### 1. UserController（用户控制器）

**主要功能：**
- 用户登录（密码、手机、邮箱）
- 用户注册
- 忘记密码
- 退出登录

**关键 Action：**

```csharp
public class UserController : Controller
{
    private UserBLL _userBLL = new UserBLL();
    
    // GET: User/Login - 显示登录页面
    [HttpGet]
    public ActionResult Login()
    {
        if (Session["LoginUser"] != null)
        {
            return RedirectToAction("Index", "Home");  // 已登录则跳转首页
        }
        return View(new LoginViewModel());
    }
    
    // POST: User/Login - 处理密码登录
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(LoginViewModel model)
    {
        // 1. 模型验证
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // 2. 调用 BLL 层验证登录
        string errorMsg = _userBLL.Login(model);
        if (errorMsg != null)
        {
            // 根据错误类型清空对应字段
            ModelState.AddModelError("", errorMsg);
            return View(model);
        }
        
        // 3. 登录成功，获取用户信息
        Users user = _userBLL.GetUserByUsername(model.Username);
        
        // 4. 更新最后登录时间
        _userBLL.UpdateLastLoginDate(user.UserID);
        
        // 5. 存储会话信息
        Session["LoginUser"] = user;
        Session.Timeout = 30;
        
        return RedirectToAction("Index", "Home");
    }
    
    // 退出登录
    public ActionResult Logout()
    {
        Session.Clear();
        Session.Abandon();
        return RedirectToAction("Index", "Home");
    }
}
```

**Session 管理：**
- `Session["LoginUser"]`：存储登录的用户对象
- `Session.Timeout = 30`：会话超时时间 30 分钟

#### 2. HomeController（首页控制器）

**主要功能：**
- 首页展示（可回收物列表）
- 订单管理页面
- 用户个人中心
- 消息页面

**关键 Action：**

```csharp
public class HomeController : Controller
{
    private readonly RecyclableItemBLL _recyclableItemBLL = new RecyclableItemBLL();
    private readonly OrderBLL _orderBLL = new OrderBLL();
    
    // GET: Home/Index - 首页（可回收物列表）
    [HttpGet]
    public ActionResult Index(RecyclableQueryModel query)
    {
        // 检查是否是工作人员登录
        if (Session["LoginStaff"] != null)
        {
            var staffRole = Session["StaffRole"] as string;
            // 根据角色跳转不同的工作台
            switch (staffRole)
            {
                case "recycler":
                    return RedirectToAction("RecyclerDashboard", "Staff");
                case "admin":
                    return RedirectToAction("AdminDashboard", "Staff");
                // ...
            }
        }
        
        // 确保数据存在
        _recyclableItemBLL.EnsureDataExists();
        
        // 获取品类列表供下拉框使用
        ViewBag.CategoryList = _recyclableItemBLL.GetAllCategories();
        
        // 获取分页数据
        var pageResult = _recyclableItemBLL.GetPagedItems(query);
        
        return View(pageResult);
    }
    
    // GET: Home/Order - 订单管理页面
    [HttpGet]
    public ActionResult Order()
    {
        // 检查登录状态
        if (Session["LoginUser"] == null)
        {
            return RedirectToAction("LoginSelect", "Home");
        }
        
        var user = (Users)Session["LoginUser"];
        
        // 获取订单统计信息
        var statistics = _orderBLL.GetOrderStatistics(user.UserID);
        ViewBag.OrderStatistics = statistics;
        
        // 获取全部订单
        var orders = _orderBLL.GetUserOrders(user.UserID, "all");
        ViewBag.Orders = orders;
        
        return View();
    }
}
```

#### 3. StaffController（工作人员控制器）

**主要功能：**
- 工作人员登录（回收员、管理员、超级管理员）
- 回收员工作台
- 管理员工作台
- 超级管理员工作台

**关键 Action：**

```csharp
public class StaffController : Controller
{
    private readonly StaffBLL _staffBLL = new StaffBLL();
    private readonly RecyclerOrderBLL _recyclerOrderBLL = new RecyclerOrderBLL();
    
    // POST: Staff/Login - 处理工作人员登录
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(StaffLoginViewModel model)
    {
        // 1. 验证码验证
        // 2. 模型验证
        // 3. 调用 BLL 验证登录
        var (errorMsg, staff) = _staffBLL.Login(model.StaffRole, model.Username, model.Password);
        if (!string.IsNullOrEmpty(errorMsg))
        {
            ModelState.AddModelError("", errorMsg);
            return View(model);
        }
        
        // 4. 登录成功，存储 Session
        Session["LoginStaff"] = staff;
        Session["StaffRole"] = model.StaffRole;
        Session.Timeout = 30;
        
        return RedirectToAction("Index", "Home");
    }
    
    // 回收员工作台
    public ActionResult RecyclerDashboard()
    {
        if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "recycler")
            return RedirectToAction("Login", "Staff");
        
        var recycler = (Recyclers)Session["LoginStaff"];
        ViewBag.StaffName = recycler.Username;
        ViewBag.DisplayName = "回收员";
        ViewBag.StaffRole = "recycler";
        
        return View();
    }
}
```

---

## 核心业务流程

### 1. 用户注册流程

```
用户填写注册表单
    ↓
前端验证（用户名、密码、手机号、邮箱格式）
    ↓
提交到 UserController.Register()
    ↓
UserBLL.Register() 验证
    ├─ 检查用户名是否已存在
    ├─ 检查密码一致性
    ├─ 检查手机号格式和唯一性
    └─ 检查邮箱格式和唯一性
    ↓
UserDAL.InsertUser() 插入数据库
    ├─ 密码使用 SHA256 哈希
    └─ 返回新用户ID
    ↓
注册成功，跳转到登录页
```

### 2. 用户登录流程（密码登录）

```
用户输入用户名、密码、验证码
    ↓
提交到 UserController.Login()
    ↓
UserBLL.Login() 验证
    ├─ 验证用户名是否存在
    ├─ 验证密码哈希是否匹配（SHA256）
    └─ 验证验证码
    ↓
登录成功
    ├─ 调用 UserBLL.UpdateLastLoginDate() 更新登录时间
    ├─ 将用户对象存入 Session["LoginUser"]
    └─ 设置 Session 超时时间 30 分钟
    ↓
跳转到首页
```

### 3. 手机验证码登录流程

```
用户输入手机号
    ↓
点击"发送验证码"
    ↓
前端调用 AJAX 到 UserController.SendVerificationCode()
    ↓
UserBLL.GenerateVerificationCode() 生成6位验证码
    ├─ 存储到 ConcurrentDictionary（手机号 -> (验证码, 过期时间)）
    └─ 有效期 5 分钟
    ↓
用户输入验证码并提交
    ↓
UserBLL.PhoneLogin() 验证
    ├─ 检查手机号是否已注册
    ├─ 验证验证码是否正确且未过期
    └─ 验证码验证后立即删除（一次性使用）
    ↓
登录成功，处理同密码登录
```

### 4. 预约回收流程

```
用户在首页查看可回收物价格
    ↓
点击"预约回收"
    ↓
填写预约表单
    ├─ 选择回收类型（上门回收/自送）
    ├─ 选择日期和时间段
    ├─ 选择品类（玻璃、金属、塑料、纸类、纺织品）
    ├─ 填写详细问卷（每个品类有不同问题）
    ├─ 填写地址和联系方式
    └─ 是否紧急
    ↓
提交到 UserController.SubmitAppointment()
    ↓
AppointmentBLL.SubmitAppointment() 处理
    ├─ 创建 Appointments 对象（Status = "已预约"）
    ├─ 创建 AppointmentCategories 列表（含 JSON 格式的问答）
    └─ 调用 DAL 层在事务中插入
    ↓
AppointmentDAL.InsertCompleteAppointment() 执行事务
    ├─ 插入 Appointments 表，获取 AppointmentID
    ├─ 遍历插入 AppointmentCategories 表
    ├─ 提交事务
    └─ 返回 AppointmentID
    ↓
预约成功，跳转到订单详情页
```

### 5. 回收员接单流程

```
回收员登录系统
    ↓
进入回收员工作台
    ↓
查看待接单订单列表（Status = "已预约"）
    ↓
点击"接收订单"
    ↓
RecyclerOrderBLL.AcceptOrder() 处理
    ├─ 验证订单存在且状态为"已预约"
    └─ 更新 Appointments 表
        ├─ Status = "进行中"
        ├─ RecyclerID = 当前回收员ID
        └─ UpdatedDate = 当前时间
    ↓
接单成功
    ├─ 用户可以看到订单状态变为"进行中"
    └─ 用户和回收员可以互相发送消息
```

### 6. 消息通信流程

```
订单状态为"进行中"
    ↓
用户/回收员点击"发送消息"
    ↓
填写消息内容并提交
    ↓
MessageBLL.SendMessage() / UserSendMessage()
    ├─ 验证订单状态为"进行中"
    ├─ 创建 Messages 对象
    │   ├─ OrderID：关联的订单ID
    │   ├─ SenderType："user" 或 "recycler"
    │   ├─ SenderID：发送者ID
    │   ├─ Content：消息内容
    │   ├─ SentTime：发送时间
    │   └─ IsRead：false
    └─ 插入 Messages 表
    ↓
接收方查看消息列表
    ├─ 根据 OrderID 获取所有消息
    └─ 按 SentTime 排序显示
    ↓
接收方点击查看消息
    ↓
MessageBLL.MarkMessagesAsRead() 标记已读
    └─ 更新 IsRead = true
```

### 7. 订单完成流程

```
回收员完成回收工作
    ↓
在订单详情页点击"完成订单"
    ↓
判断是否可以完成
    ├─ 订单状态为"进行中"
    ├─ 存在最近一次结束的会话（EndedTime 不为空）
    └─ 在会话结束后没有新消息
    ↓
OrderBLL.CompleteOrder() 处理
    └─ 更新 Appointments 表
        ├─ Status = "已完成"
        └─ UpdatedDate = 当前时间
    ↓
订单完成
    ├─ 用户可以看到订单状态变为"已完成"
    └─ 不再允许发送消息
```

### 8. 订单取消流程

```
用户在订单列表查看订单
    ↓
点击"取消订单"（只能取消"已预约"状态的订单）
    ↓
OrderBLL.CancelOrder() 验证
    ├─ 检查订单是否属于该用户
    └─ 检查订单状态是否为"已预约"
    ↓
OrderDAL.CancelOrder() 执行
    └─ 更新 Appointments 表
        ├─ Status = "已取消"
        └─ UpdatedDate = 当前时间
    ↓
取消成功
```

---

## 安全机制

### 1. 密码安全

**密码哈希算法：SHA256**
```csharp
private string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
```

**安全特性：**
- 密码不以明文存储
- 使用 SHA256 单向哈希
- 哈希值存储在 `PasswordHash` 字段

**建议改进：**
- 考虑使用更安全的算法（如 bcrypt、PBKDF2）
- 添加盐值（Salt）增强安全性
- 实现密码强度要求

### 2. SQL 注入防护

**所有 SQL 查询使用参数化：**
```csharp
string sql = "SELECT * FROM Users WHERE Username = @Username";
SqlCommand cmd = new SqlCommand(sql, conn);
cmd.Parameters.AddWithValue("@Username", username);
```

**防护机制：**
- 使用 `SqlParameter` 参数化所有用户输入
- 避免字符串拼接构建 SQL

### 3. 跨站请求伪造（CSRF）防护

**使用 Anti-Forgery Token：**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Login(LoginViewModel model)
{
    // ...
}
```

**视图中：**
```html
@Html.AntiForgeryToken()
```

### 4. Session 管理

**Session 安全设置：**
```csharp
Session["LoginUser"] = user;
Session.Timeout = 30;  // 30 分钟超时
```

**登录验证：**
```csharp
if (Session["LoginUser"] == null)
{
    return RedirectToAction("LoginSelect", "Home");
}
```

### 5. 验证码机制

**验证码特性：**
- 4 位字母数字组合
- 移除易混淆字符（I, O, 0, 1）
- 不区分大小写验证

**手机/邮箱验证码：**
- 6 位数字
- 5 分钟有效期
- 一次性使用（验证后立即删除）
- 线程安全存储（ConcurrentDictionary）

### 6. 输入验证

**多层验证：**
1. **前端验证**：基本格式验证（必填、长度、格式）
2. **模型验证**：`[Required]`、`[StringLength]`、`[RegularExpression]` 等特性
3. **业务验证**：BLL 层进行业务规则验证

**示例：用户名验证**
```csharp
[Required(ErrorMessage = "请输入用户名")]
[StringLength(50, ErrorMessage = "用户名不能超过50个字符")]
public string Username { get; set; }
```

---

## 扩展性考虑

### 1. 分层架构的优势

**易于维护：**
- 每层职责清晰，修改某层不影响其他层
- 便于单元测试

**易于扩展：**
- 可以替换数据访问层（如从 ADO.NET 迁移到 Entity Framework）
- 可以添加缓存层
- 可以添加日志层

### 2. 建议的改进方向

#### 2.1 数据访问层改进
- **使用 Entity Framework 或 Dapper**：简化数据访问代码
- **实现仓储模式（Repository Pattern）**：统一数据访问接口
- **工作单元模式（Unit of Work）**：更好的事务管理

#### 2.2 业务逻辑层改进
- **依赖注入（DI）**：降低耦合度
- **接口抽象**：定义 IBL 接口，便于测试和替换实现
- **异步方法**：使用 `async/await` 提升性能

#### 2.3 安全性改进
- **密码哈希**：使用 bcrypt 或 PBKDF2 替代 SHA256
- **JWT 认证**：替代 Session，支持无状态认证
- **OAuth2 集成**：支持第三方登录（微信、支付宝）

#### 2.4 性能优化
- **缓存机制**：使用 Redis 缓存可回收物价格等数据
- **数据库索引**：为常用查询字段添加索引
- **分页优化**：使用存储过程或 SQL 优化分页查询

#### 2.5 可维护性改进
- **日志系统**：集成 log4net 或 NLog
- **配置管理**：使用配置文件管理应用参数
- **API 文档**：为 Controller 添加 XML 注释

#### 2.6 功能扩展
- **支付集成**：接入支付宝/微信支付
- **实时通知**：使用 SignalR 实现实时消息推送
- **评价系统**：用户对回收员的评价和评分
- **数据分析**：订单统计、回收量分析

### 3. 当前架构的局限性

**耦合度较高：**
- BLL 直接依赖 DAL 的具体实现
- 没有使用依赖注入

**缺少抽象层：**
- 没有定义接口，难以进行单元测试
- 难以替换实现

**同步操作：**
- 所有数据库操作都是同步的
- 高并发场景下可能成为性能瓶颈

**验证码存储：**
- 使用内存存储验证码，在分布式环境下会有问题
- 建议使用 Redis 等分布式缓存

---

## 总结

### 系统优势
1. **架构清晰**：三层架构，职责分离
2. **功能完整**：涵盖用户、回收员、管理员的完整业务流程
3. **安全性良好**：密码哈希、参数化查询、CSRF 防护
4. **用户体验**：多种登录方式，实时消息通信

### 关键设计模式
1. **MVC 模式**：表示层使用 ASP.NET MVC
2. **分层架构**：UI → BLL → DAL → Database
3. **事务模式**：关键操作使用事务保证一致性
4. **验证码模式**：线程安全的验证码存储和验证

### 数据流向
```
用户请求
  ↓
Controller（验证、调用 BLL）
  ↓
BLL（业务逻辑、数据验证、调用 DAL）
  ↓
DAL（SQL 执行、事务处理）
  ↓
Database（SQL Server）
  ↓
DAL 返回结果
  ↓
BLL 处理结果
  ↓
Controller 渲染视图
  ↓
返回给用户
```

### 后续开发建议
1. 理解核心业务流程后，可以从以下方向扩展：
   - 添加新的回收物品类型
   - 实现支付功能
   - 添加实时通知
   - 优化数据库查询性能
   - 实现 RESTful API 供移动端使用

2. 代码质量改进：
   - 添加单元测试
   - 使用依赖注入
   - 实现日志系统
   - 添加异常处理中间件

3. 用户体验优化：
   - 实现前端 SPA（单页应用）
   - 添加地图定位功能
   - 实现图片上传（回收物照片）
   - 添加用户反馈和评价系统

---

**文档版本：1.0**  
**更新日期：2025-11-03**  
**适用系统版本：全品类可回收物预约回收系统**
