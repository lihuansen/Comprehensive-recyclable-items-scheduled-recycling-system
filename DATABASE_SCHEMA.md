# 数据库架构设计文档

## 目录
1. [数据库概述](#数据库概述)
2. [表结构详解](#表结构详解)
3. [关系图](#关系图)
4. [索引设计](#索引设计)
5. [数据字典](#数据字典)

---

## 数据库概述

### 数据库信息
- **数据库类型**：Microsoft SQL Server
- **连接字符串配置**：`RecyclingDB`（在 Web.config/App.config 中配置）
- **字符编码**：UTF-8

### 核心表列表

| 表名 | 中文名 | 用途 | 关键字段 |
|------|--------|------|----------|
| Users | 用户表 | 存储普通用户信息 | UserID, Username, PhoneNumber, Email |
| Recyclers | 回收员表 | 存储回收员信息 | RecyclerID, Username, Region, Rating |
| Transporters | 运输人员表 | 存储运输人员信息 | TransporterID, Username, VehiclePlateNumber, Region |
| SortingCenterWorkers | 基地工作人员表 | 存储基地工作人员信息 | WorkerID, Username, SortingCenterID, Position |
| Admins | 管理员表 | 存储管理员信息 | AdminID, Username |
| SuperAdmins | 超级管理员表 | 存储超级管理员信息 | SuperAdminID, Username |
| Appointments | 预约订单表 | 存储回收预约信息 | AppointmentID, UserID, RecyclerID, Status |
| AppointmentCategories | 预约品类表 | 存储预约的品类详情 | CategoryID, AppointmentID, CategoryName |
| RecyclableItems | 可回收物品表 | 存储可回收物品类型和价格 | ItemId, Name, Category, PricePerKg |
| Messages | 消息表 | 存储用户和回收员的消息 | MessageID, OrderID, SenderType, SenderID |
| Conversations | 会话表 | 存储会话状态 | ConversationID, OrderID, UserID, RecyclerID |

---

## 表结构详解

### 1. Users（用户表）

**用途**：存储普通用户的基本信息和认证信息

```sql
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),        -- 用户ID（自增主键）
    Username NVARCHAR(50) NOT NULL UNIQUE,       -- 用户名（唯一）
    PasswordHash NVARCHAR(255) NOT NULL,         -- 密码哈希（SHA256）
    PhoneNumber NVARCHAR(20) NOT NULL UNIQUE,    -- 手机号（唯一）
    Email NVARCHAR(100) NOT NULL UNIQUE,         -- 邮箱（唯一）
    RegistrationDate DATETIME2 NOT NULL,         -- 注册时间
    LastLoginDate DATETIME2 NULL                 -- 最后登录时间
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| UserID | INT | PK, IDENTITY | 用户唯一标识 |
| Username | NVARCHAR(50) | NOT NULL, UNIQUE | 用户名，用于登录 |
| PasswordHash | NVARCHAR(255) | NOT NULL | SHA256 密码哈希值 |
| PhoneNumber | NVARCHAR(20) | NOT NULL, UNIQUE | 手机号，用于手机登录和联系 |
| Email | NVARCHAR(100) | NOT NULL, UNIQUE | 邮箱，用于邮箱登录 |
| RegistrationDate | DATETIME2 | NOT NULL | 用户注册时间 |
| LastLoginDate | DATETIME2 | NULL | 最后一次登录时间，初始为 NULL |

**索引建议：**
```sql
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
```

**业务规则：**
- 用户名、手机号、邮箱必须唯一
- 密码必须经过 SHA256 哈希后存储
- 注册时 `RegistrationDate` 设置为当前时间
- 登录成功后更新 `LastLoginDate`

---

### 2. Recyclers（回收员表）

**用途**：存储回收员的基本信息和工作状态

```sql
CREATE TABLE Recyclers (
    RecyclerID INT PRIMARY KEY IDENTITY(1,1),    -- 回收员ID（自增主键）
    Username NVARCHAR(50) NOT NULL UNIQUE,       -- 用户名（唯一）
    PasswordHash NVARCHAR(255) NOT NULL,         -- 密码哈希
    Available BIT NOT NULL DEFAULT 1,            -- 是否可接单
    PhoneNumber NVARCHAR(20) NOT NULL,           -- 手机号
    FullName NVARCHAR(100) NULL,                 -- 真实姓名
    Region NVARCHAR(100) NOT NULL,               -- 负责区域
    Rating DECIMAL(3, 2) NULL,                   -- 评分（0-5）
    CreatedDate DATETIME2 NULL,                  -- 创建时间
    LastLoginDate DATETIME2 NULL,                -- 最后登录时间
    IsActive BIT NOT NULL DEFAULT 1,             -- 是否激活
    AvatarURL NVARCHAR(255) NULL                 -- 头像 URL
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| RecyclerID | INT | PK, IDENTITY | 回收员唯一标识 |
| Username | NVARCHAR(50) | NOT NULL, UNIQUE | 用户名 |
| PasswordHash | NVARCHAR(255) | NOT NULL | SHA256 密码哈希值 |
| Available | BIT | NOT NULL, DEFAULT 1 | 是否可接单（1=可接单，0=不可接单）|
| PhoneNumber | NVARCHAR(20) | NOT NULL | 手机号 |
| FullName | NVARCHAR(100) | NULL | 真实姓名 |
| Region | NVARCHAR(100) | NOT NULL | 负责的区域 |
| Rating | DECIMAL(3,2) | NULL | 评分（例如 4.85）|
| CreatedDate | DATETIME2 | NULL | 账号创建时间 |
| LastLoginDate | DATETIME2 | NULL | 最后登录时间 |
| IsActive | BIT | NOT NULL, DEFAULT 1 | 账号是否激活 |
| AvatarURL | NVARCHAR(255) | NULL | 头像图片 URL |

**索引建议：**
```sql
CREATE UNIQUE INDEX IX_Recyclers_Username ON Recyclers(Username);
CREATE INDEX IX_Recyclers_Region ON Recyclers(Region);
CREATE INDEX IX_Recyclers_Available ON Recyclers(Available);
```

**业务规则：**
- 只有 `Available = 1` 且 `IsActive = 1` 的回收员可以接单
- `Rating` 为用户评分的平均值
- 回收员可以通过管理员设置负责的 `Region`

---

### 3. Transporters（运输人员表）

**用途**：存储运输人员信息，负责将回收物品从回收员处运输到基地

```sql
CREATE TABLE Transporters (
    TransporterID INT PRIMARY KEY IDENTITY(1,1),   -- 运输人员ID（自增主键）
    Username NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名（唯一）
    PasswordHash NVARCHAR(255) NOT NULL,           -- 密码哈希（SHA256）
    FullName NVARCHAR(100) NULL,                   -- 真实姓名
    PhoneNumber NVARCHAR(20) NOT NULL,             -- 手机号
    IDNumber NVARCHAR(18) NULL,                    -- 身份证号
    VehicleType NVARCHAR(50) NOT NULL,             -- 车辆类型
    VehiclePlateNumber NVARCHAR(20) NOT NULL,      -- 车牌号
    VehicleCapacity DECIMAL(10, 2) NULL,           -- 车辆载重能力（kg）
    LicenseNumber NVARCHAR(50) NULL,               -- 驾驶证号
    Region NVARCHAR(100) NOT NULL,                 -- 负责区域
    Available BIT NOT NULL DEFAULT 1,              -- 是否可接任务
    CurrentStatus NVARCHAR(20) NOT NULL DEFAULT N'空闲', -- 当前状态
    TotalTrips INT NOT NULL DEFAULT 0,             -- 总运输次数
    TotalWeight DECIMAL(12, 2) NOT NULL DEFAULT 0, -- 总运输重量（kg）
    Rating DECIMAL(3, 2) NULL,                     -- 评分（0-5）
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
    LastLoginDate DATETIME2 NULL,                  -- 最后登录时间
    IsActive BIT NOT NULL DEFAULT 1,               -- 是否激活
    AvatarURL NVARCHAR(255) NULL,                  -- 头像URL
    Notes NVARCHAR(500) NULL                       -- 备注信息
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| TransporterID | INT | PK, IDENTITY | 运输人员唯一标识 |
| Username | NVARCHAR(50) | NOT NULL, UNIQUE | 用户名 |
| PasswordHash | NVARCHAR(255) | NOT NULL | SHA256 密码哈希值 |
| FullName | NVARCHAR(100) | NULL | 真实姓名 |
| PhoneNumber | NVARCHAR(20) | NOT NULL | 手机号 |
| IDNumber | NVARCHAR(18) | NULL | 身份证号 |
| VehicleType | NVARCHAR(50) | NOT NULL | 车辆类型（小型货车、中型货车等）|
| VehiclePlateNumber | NVARCHAR(20) | NOT NULL | 车牌号 |
| VehicleCapacity | DECIMAL(10,2) | NULL | 车辆载重能力（公斤）|
| LicenseNumber | NVARCHAR(50) | NULL | 驾驶证号 |
| Region | NVARCHAR(100) | NOT NULL | 负责的运输区域 |
| Available | BIT | NOT NULL, DEFAULT 1 | 是否可接任务 |
| CurrentStatus | NVARCHAR(20) | NOT NULL | 当前状态（空闲、运输中、休息、离线）|
| TotalTrips | INT | NOT NULL, DEFAULT 0 | 累计运输次数 |
| TotalWeight | DECIMAL(12,2) | NOT NULL, DEFAULT 0 | 累计运输重量 |
| Rating | DECIMAL(3,2) | NULL | 评分（0-5）|
| CreatedDate | DATETIME2 | NOT NULL | 创建时间 |
| LastLoginDate | DATETIME2 | NULL | 最后登录时间 |
| IsActive | BIT | NOT NULL, DEFAULT 1 | 是否激活 |
| AvatarURL | NVARCHAR(255) | NULL | 头像URL |
| Notes | NVARCHAR(500) | NULL | 备注信息 |

**索引建议：**
```sql
CREATE UNIQUE INDEX IX_Transporters_Username ON Transporters(Username);
CREATE INDEX IX_Transporters_Region ON Transporters(Region);
CREATE INDEX IX_Transporters_Available ON Transporters(Available);
CREATE INDEX IX_Transporters_CurrentStatus ON Transporters(CurrentStatus);
CREATE INDEX IX_Transporters_VehiclePlateNumber ON Transporters(VehiclePlateNumber);
```

**业务规则：**
- 只有 `Available = 1` 且 `IsActive = 1` 的运输人员可以接任务
- `CurrentStatus` 表示实时状态：空闲、运输中、休息、离线
- `Rating` 为用户对运输服务的平均评分
- `TotalTrips` 和 `TotalWeight` 用于统计绩效

---

### 4. SortingCenterWorkers（基地工作人员表）

**用途**：存储基地工作人员信息，负责对运输到基地的回收物品进行分类、质检和处理

```sql
CREATE TABLE SortingCenterWorkers (
    WorkerID INT PRIMARY KEY IDENTITY(1,1),        -- 工作人员ID（自增主键）
    Username NVARCHAR(50) NOT NULL UNIQUE,         -- 用户名（唯一）
    PasswordHash NVARCHAR(255) NOT NULL,           -- 密码哈希（SHA256）
    FullName NVARCHAR(100) NULL,                   -- 真实姓名
    PhoneNumber NVARCHAR(20) NOT NULL,             -- 手机号
    IDNumber NVARCHAR(18) NULL,                    -- 身份证号
    SortingCenterID INT NOT NULL,                  -- 所属基地ID
    SortingCenterName NVARCHAR(100) NOT NULL,      -- 基地名称
    Position NVARCHAR(50) NOT NULL DEFAULT N'分拣员', -- 职位
    WorkStation NVARCHAR(50) NULL,                 -- 工位编号
    Specialization NVARCHAR(100) NULL,             -- 专长品类
    ShiftType NVARCHAR(20) NOT NULL DEFAULT N'白班', -- 班次类型
    Available BIT NOT NULL DEFAULT 1,              -- 是否可工作
    CurrentStatus NVARCHAR(20) NOT NULL DEFAULT N'待命', -- 当前状态
    TotalItemsProcessed INT NOT NULL DEFAULT 0,    -- 总处理物品数量
    TotalWeightProcessed DECIMAL(12, 2) NOT NULL DEFAULT 0, -- 总处理重量（kg）
    AccuracyRate DECIMAL(5, 2) NULL,               -- 分拣准确率（%）
    Rating DECIMAL(3, 2) NULL,                     -- 评分（0-5）
    HireDate DATE NULL,                            -- 入职日期
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(), -- 创建时间
    LastLoginDate DATETIME2 NULL,                  -- 最后登录时间
    IsActive BIT NOT NULL DEFAULT 1,               -- 是否激活
    AvatarURL NVARCHAR(255) NULL,                  -- 头像URL
    Notes NVARCHAR(500) NULL                       -- 备注信息
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| WorkerID | INT | PK, IDENTITY | 工作人员唯一标识 |
| Username | NVARCHAR(50) | NOT NULL, UNIQUE | 用户名 |
| PasswordHash | NVARCHAR(255) | NOT NULL | SHA256 密码哈希值 |
| FullName | NVARCHAR(100) | NULL | 真实姓名 |
| PhoneNumber | NVARCHAR(20) | NOT NULL | 手机号 |
| IDNumber | NVARCHAR(18) | NULL | 身份证号 |
| SortingCenterID | INT | NOT NULL | 所属基地ID |
| SortingCenterName | NVARCHAR(100) | NOT NULL | 基地名称 |
| Position | NVARCHAR(50) | NOT NULL | 职位（分拣员、质检员、组长、主管）|
| WorkStation | NVARCHAR(50) | NULL | 工位编号 |
| Specialization | NVARCHAR(100) | NULL | 专长品类 |
| ShiftType | NVARCHAR(20) | NOT NULL | 班次类型（白班、夜班、轮班）|
| Available | BIT | NOT NULL, DEFAULT 1 | 是否可工作 |
| CurrentStatus | NVARCHAR(20) | NOT NULL | 当前状态（待命、分拣中、休息、离岗、离线）|
| TotalItemsProcessed | INT | NOT NULL, DEFAULT 0 | 累计处理物品数量 |
| TotalWeightProcessed | DECIMAL(12,2) | NOT NULL, DEFAULT 0 | 累计处理重量 |
| AccuracyRate | DECIMAL(5,2) | NULL | 分拣准确率 |
| Rating | DECIMAL(3,2) | NULL | 评分（0-5）|
| HireDate | DATE | NULL | 入职日期 |
| CreatedDate | DATETIME2 | NOT NULL | 创建时间 |
| LastLoginDate | DATETIME2 | NULL | 最后登录时间 |
| IsActive | BIT | NOT NULL, DEFAULT 1 | 是否激活 |
| AvatarURL | NVARCHAR(255) | NULL | 头像URL |
| Notes | NVARCHAR(500) | NULL | 备注信息 |

**索引建议：**
```sql
CREATE UNIQUE INDEX IX_SortingCenterWorkers_Username ON SortingCenterWorkers(Username);
CREATE INDEX IX_SortingCenterWorkers_SortingCenterID ON SortingCenterWorkers(SortingCenterID);
CREATE INDEX IX_SortingCenterWorkers_Position ON SortingCenterWorkers(Position);
CREATE INDEX IX_SortingCenterWorkers_Available ON SortingCenterWorkers(Available);
CREATE INDEX IX_SortingCenterWorkers_ShiftType ON SortingCenterWorkers(ShiftType);
```

**业务规则：**
- 只有 `Available = 1` 且 `IsActive = 1` 的工作人员可以接任务
- `Position` 职位层级：分拣员 < 质检员 < 组长 < 主管
- `CurrentStatus` 表示实时状态：待命、分拣中、休息、离岗、离线
- `AccuracyRate` 用于评估分拣质量
- `ShiftType` 用于排班管理

---

### 5. Admins（管理员表）

**用途**：存储管理员信息

```sql
CREATE TABLE Admins (
    AdminID INT PRIMARY KEY IDENTITY(1,1),       -- 管理员ID
    Username NVARCHAR(50) NOT NULL UNIQUE,       -- 用户名
    PasswordHash NVARCHAR(255) NOT NULL,         -- 密码哈希
    FullName NVARCHAR(100) NOT NULL,             -- 真实姓名
    CreatedDate DATETIME2 NULL,                  -- 创建时间
    LastLoginDate DATETIME2 NULL,                -- 最后登录时间
    IsActive BIT NULL DEFAULT 1                  -- 是否激活
);
```

---

### 6. SuperAdmins（超级管理员表）

**用途**：存储超级管理员信息

```sql
CREATE TABLE SuperAdmins (
    SuperAdminID INT PRIMARY KEY IDENTITY(1,1),  -- 超级管理员ID
    Username NVARCHAR(50) NOT NULL UNIQUE,       -- 用户名
    PasswordHash NVARCHAR(255) NOT NULL,         -- 密码哈希
    FullName NVARCHAR(100) NOT NULL,             -- 真实姓名
    CreatedDate DATETIME2 NULL,                  -- 创建时间
    LastLoginDate DATETIME2 NULL,                -- 最后登录时间
    IsActive BIT NULL DEFAULT 1                  -- 是否激活
);
```

---

### 7. Appointments（预约订单表）

**用途**：存储用户的回收预约信息

```sql
CREATE TABLE Appointments (
    AppointmentID INT PRIMARY KEY IDENTITY(1,1),  -- 预约ID（自增主键）
    UserID INT NOT NULL,                          -- 用户ID（外键）
    AppointmentType NVARCHAR(50) NOT NULL,        -- 预约类型
    AppointmentDate DATE NOT NULL,                -- 预约日期
    TimeSlot NVARCHAR(50) NOT NULL,               -- 时间段
    EstimatedWeight DECIMAL(10, 2) NOT NULL,      -- 预估重量（kg）
    IsUrgent BIT NOT NULL DEFAULT 0,              -- 是否紧急
    Address NVARCHAR(200) NOT NULL,               -- 回收地址
    ContactName NVARCHAR(50) NOT NULL,            -- 联系人
    ContactPhone NVARCHAR(20) NOT NULL,           -- 联系电话
    SpecialInstructions NVARCHAR(500) NULL,       -- 特殊说明
    EstimatedPrice DECIMAL(10, 2) NULL,           -- 预估价格
    Status NVARCHAR(20) NOT NULL,                 -- 订单状态
    CreatedDate DATETIME2 NOT NULL,               -- 创建时间
    UpdatedDate DATETIME2 NOT NULL,               -- 更新时间
    RecyclerID INT NULL,                          -- 回收员ID（外键，接单后分配）
    
    CONSTRAINT FK_Appointments_Users FOREIGN KEY (UserID) 
        REFERENCES Users(UserID),
    CONSTRAINT FK_Appointments_Recyclers FOREIGN KEY (RecyclerID) 
        REFERENCES Recyclers(RecyclerID)
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| AppointmentID | INT | PK, IDENTITY | 预约订单唯一标识 |
| UserID | INT | NOT NULL, FK | 发起预约的用户ID |
| AppointmentType | NVARCHAR(50) | NOT NULL | 预约类型（如"上门回收"、"自送"）|
| AppointmentDate | DATE | NOT NULL | 预约的回收日期 |
| TimeSlot | NVARCHAR(50) | NOT NULL | 时间段（如"上午8-10点"）|
| EstimatedWeight | DECIMAL(10,2) | NOT NULL | 预估回收物重量（单位：kg）|
| IsUrgent | BIT | NOT NULL, DEFAULT 0 | 是否紧急（1=紧急，0=普通）|
| Address | NVARCHAR(200) | NOT NULL | 回收地址 |
| ContactName | NVARCHAR(50) | NOT NULL | 联系人姓名 |
| ContactPhone | NVARCHAR(20) | NOT NULL | 联系电话 |
| SpecialInstructions | NVARCHAR(500) | NULL | 特殊说明或备注 |
| EstimatedPrice | DECIMAL(10,2) | NULL | 预估回收价格 |
| Status | NVARCHAR(20) | NOT NULL | 订单状态 |
| CreatedDate | DATETIME2 | NOT NULL | 订单创建时间 |
| UpdatedDate | DATETIME2 | NOT NULL | 订单更新时间 |
| RecyclerID | INT | NULL, FK | 接单的回收员ID（初始为 NULL）|

**订单状态枚举：**
- `已预约`：用户提交预约，等待回收员接单
- `进行中`：回收员已接单，正在处理
- `已完成`：回收完成
- `已取消`：用户取消预约

**状态转换规则：**
```
已预约 ──接单──> 进行中 ──完成──> 已完成
  │
  └──取消──> 已取消
```

**索引建议：**
```sql
CREATE INDEX IX_Appointments_UserID ON Appointments(UserID);
CREATE INDEX IX_Appointments_RecyclerID ON Appointments(RecyclerID);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);
CREATE INDEX IX_Appointments_AppointmentDate ON Appointments(AppointmentDate);
CREATE INDEX IX_Appointments_CreatedDate ON Appointments(CreatedDate);
```

**业务规则：**
- 用户只能取消 `Status = '已预约'` 的订单
- 回收员只能接收 `Status = '已预约'` 的订单
- 接单后 `Status` 变为 `进行中`，同时设置 `RecyclerID`
- 完成订单后 `Status` 变为 `已完成`

---

### 8. AppointmentCategories（预约品类表）

**用途**：存储每个预约订单的品类详细信息

```sql
CREATE TABLE AppointmentCategories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),     -- 品类ID（自增主键）
    AppointmentID INT NOT NULL,                   -- 预约ID（外键）
    CategoryName NVARCHAR(50) NOT NULL,           -- 品类名称（如"玻璃"）
    CategoryKey NVARCHAR(50) NOT NULL,            -- 品类键名（如"glass"）
    QuestionsAnswers NVARCHAR(MAX) NULL,          -- JSON格式的问答数据
    CreatedDate DATETIME2 NOT NULL,               -- 创建时间
    
    CONSTRAINT FK_AppointmentCategories_Appointments FOREIGN KEY (AppointmentID) 
        REFERENCES Appointments(AppointmentID) ON DELETE CASCADE
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| CategoryID | INT | PK, IDENTITY | 品类记录唯一标识 |
| AppointmentID | INT | NOT NULL, FK | 所属预约订单ID |
| CategoryName | NVARCHAR(50) | NOT NULL | 品类显示名称（中文）|
| CategoryKey | NVARCHAR(50) | NOT NULL | 品类键名（英文标识）|
| QuestionsAnswers | NVARCHAR(MAX) | NULL | 品类详细问答数据（JSON格式）|
| CreatedDate | DATETIME2 | NOT NULL | 记录创建时间 |

**QuestionsAnswers JSON 结构示例：**
```json
{
  "玻璃类型": "平板玻璃",
  "是否含杂质": "无",
  "破损程度": "完好"
}
```

**索引建议：**
```sql
CREATE INDEX IX_AppointmentCategories_AppointmentID 
    ON AppointmentCategories(AppointmentID);
```

**业务规则：**
- 一个预约订单（Appointment）可以有多个品类（AppointmentCategories）
- 关系为 1:N（一对多）
- 使用 `ON DELETE CASCADE`，删除预约时自动删除关联的品类记录

---

### 9. RecyclableItems（可回收物品表）

**用途**：存储可回收物品类型和价格信息

```sql
CREATE TABLE RecyclableItems (
    ItemId INT PRIMARY KEY IDENTITY(1,1),         -- 物品ID（自增主键）
    Name NVARCHAR(50) NOT NULL,                   -- 物品名称
    Category NVARCHAR(20) NOT NULL,               -- 品类代码
    CategoryName NVARCHAR(20) NOT NULL,           -- 品类名称
    PricePerKg DECIMAL(10, 2) NOT NULL,           -- 每公斤价格
    Description NVARCHAR(200) NULL,               -- 描述
    SortOrder INT NOT NULL DEFAULT 0,             -- 排序顺序
    IsActive BIT NOT NULL DEFAULT 1               -- 是否有效
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| ItemId | INT | PK, IDENTITY | 物品唯一标识 |
| Name | NVARCHAR(50) | NOT NULL | 物品名称（如"矿泉水瓶"）|
| Category | NVARCHAR(20) | NOT NULL | 品类代码（如"plastic"）|
| CategoryName | NVARCHAR(20) | NOT NULL | 品类名称（如"塑料"）|
| PricePerKg | DECIMAL(10,2) | NOT NULL | 每公斤回收价格 |
| Description | NVARCHAR(200) | NULL | 物品描述 |
| SortOrder | INT | NOT NULL, DEFAULT 0 | 显示排序顺序 |
| IsActive | BIT | NOT NULL, DEFAULT 1 | 是否有效（用于软删除）|

**品类代码枚举：**
- `glass`：玻璃
- `metal`：金属
- `plastic`：塑料
- `paper`：纸类
- `fabric`：纺织品

**索引建议：**
```sql
CREATE INDEX IX_RecyclableItems_Category ON RecyclableItems(Category);
CREATE INDEX IX_RecyclableItems_IsActive ON RecyclableItems(IsActive);
CREATE INDEX IX_RecyclableItems_SortOrder ON RecyclableItems(SortOrder);
```

**业务规则：**
- `IsActive = 1` 的物品才会在首页展示
- 同一品类的物品按 `SortOrder` 排序

---

### 10. Messages（消息表）

**用途**：存储用户和回收员之间的消息

```sql
CREATE TABLE Messages (
    MessageID INT PRIMARY KEY IDENTITY(1,1),      -- 消息ID（自增主键）
    OrderID INT NULL,                             -- 关联的订单ID
    SenderType NVARCHAR(20) NULL,                 -- 发送者类型
    SenderID INT NULL,                            -- 发送者ID
    Content NVARCHAR(1000) NULL,                  -- 消息内容
    SentTime DATETIME2 NULL,                      -- 发送时间
    IsRead BIT NULL DEFAULT 0,                    -- 是否已读
    
    CONSTRAINT FK_Messages_Appointments FOREIGN KEY (OrderID) 
        REFERENCES Appointments(AppointmentID)
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| MessageID | INT | PK, IDENTITY | 消息唯一标识 |
| OrderID | INT | NULL, FK | 关联的订单ID |
| SenderType | NVARCHAR(20) | NULL | 发送者类型（"user" 或 "recycler"）|
| SenderID | INT | NULL | 发送者ID（对应 UserID 或 RecyclerID）|
| Content | NVARCHAR(1000) | NULL | 消息内容 |
| SentTime | DATETIME2 | NULL | 消息发送时间 |
| IsRead | BIT | NULL, DEFAULT 0 | 是否已读（0=未读，1=已读）|

**索引建议：**
```sql
CREATE INDEX IX_Messages_OrderID ON Messages(OrderID);
CREATE INDEX IX_Messages_SenderType_SenderID ON Messages(SenderType, SenderID);
CREATE INDEX IX_Messages_SentTime ON Messages(SentTime);
CREATE INDEX IX_Messages_IsRead ON Messages(IsRead);
```

**业务规则：**
- 只有状态为 `进行中` 的订单才允许发送消息
- `SenderType` 为 `user` 时，`SenderID` 为 `UserID`
- `SenderType` 为 `recycler` 时，`SenderID` 为 `RecyclerID`
- 消息按 `SentTime` 倒序显示

---

### 11. Conversations（会话表）

**用途**：存储订单的会话状态

```sql
CREATE TABLE Conversations (
    ConversationID INT PRIMARY KEY IDENTITY(1,1), -- 会话ID（自增主键）
    OrderID INT NULL,                             -- 关联的订单ID
    UserID INT NULL,                              -- 用户ID
    RecyclerID INT NULL,                          -- 回收员ID
    Status NVARCHAR(20) NULL,                     -- 会话状态
    CreatedTime DATETIME2 NULL,                   -- 创建时间
    EndedTime DATETIME2 NULL,                     -- 结束时间
    
    CONSTRAINT FK_Conversations_Appointments FOREIGN KEY (OrderID) 
        REFERENCES Appointments(AppointmentID),
    CONSTRAINT FK_Conversations_Users FOREIGN KEY (UserID) 
        REFERENCES Users(UserID),
    CONSTRAINT FK_Conversations_Recyclers FOREIGN KEY (RecyclerID) 
        REFERENCES Recyclers(RecyclerID)
);
```

**字段说明：**

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|----------|------|------|
| ConversationID | INT | PK, IDENTITY | 会话唯一标识 |
| OrderID | INT | NULL, FK | 关联的订单ID |
| UserID | INT | NULL, FK | 用户ID |
| RecyclerID | INT | NULL, FK | 回收员ID |
| Status | NVARCHAR(20) | NULL | 会话状态（如"active"、"ended"）|
| CreatedTime | DATETIME2 | NULL | 会话创建时间 |
| EndedTime | DATETIME2 | NULL | 会话结束时间 |

**索引建议：**
```sql
CREATE INDEX IX_Conversations_OrderID ON Conversations(OrderID);
CREATE INDEX IX_Conversations_UserID ON Conversations(UserID);
CREATE INDEX IX_Conversations_RecyclerID ON Conversations(RecyclerID);
```

**业务规则：**
- 一个订单可以有多次会话
- 会话结束后设置 `EndedTime`
- 用于判断订单是否可以完成（需要检查会话是否结束且无新消息）

---

## 关系图

### ER 图（Entity-Relationship Diagram）

```
┌─────────────┐
│   Users     │
│─────────────│
│ UserID (PK) │◄─────────┐
│ Username    │          │
│ PhoneNumber │          │ 1
│ Email       │          │
└─────────────┘          │
                         │
                         │
                         │ N
                    ┌────┴──────────────┐
                    │   Appointments    │
                    │───────────────────│
                    │ AppointmentID(PK) │
                    │ UserID (FK)       │◄───────┐
                    │ RecyclerID (FK)   │        │
                    │ Status            │        │ 1
                    │ AppointmentDate   │        │
                    │ CreatedDate       │        │
                    └────┬──────────────┘        │
                         │                       │
                         │ 1                     │
                         │                       │
                         │ N                     │
                    ┌────▼──────────────────┐   │
                    │AppointmentCategories  │   │
                    │───────────────────────│   │
                    │ CategoryID (PK)       │   │
                    │ AppointmentID (FK)    │   │
                    │ CategoryName          │   │
                    │ QuestionsAnswers(JSON)│   │
                    └───────────────────────┘   │
                                                 │
┌─────────────┐                                 │
│  Recyclers  │                                 │
│─────────────│                                 │
│RecyclerID(PK)├─────────────────────────────────┘
│ Username    │          N
│ Region      │
│ Rating      │
└─────────────┘


┌─────────────┐
│  Messages   │          关联到 Appointments
│─────────────│
│MessageID(PK)│
│ OrderID(FK) ├─────────► Appointments (AppointmentID)
│ SenderType  │
│ SenderID    │
│ Content     │
└─────────────┘


┌──────────────────┐
│ RecyclableItems  │    独立的物品价格表
│──────────────────│
│ ItemId (PK)      │
│ Name             │
│ Category         │
│ PricePerKg       │
└──────────────────┘
```

### 关系说明

1. **Users ←→ Appointments（1:N）**
   - 一个用户可以有多个预约订单
   - 每个预约订单属于一个用户

2. **Recyclers ←→ Appointments（1:N）**
   - 一个回收员可以接多个订单
   - 每个订单只能分配给一个回收员

3. **Appointments ←→ AppointmentCategories（1:N）**
   - 一个预约订单可以包含多个品类
   - 每个品类记录属于一个预约订单

4. **Appointments ←→ Messages（1:N）**
   - 一个订单可以有多条消息
   - 每条消息关联到一个订单

5. **Appointments ←→ Conversations（1:N）**
   - 一个订单可以有多次会话
   - 每次会话关联到一个订单

---

## 索引设计

### 主键索引（自动创建）
所有表的主键字段都会自动创建聚集索引。

### 外键索引
为提升关联查询性能，建议为所有外键创建索引：

```sql
-- Appointments 表
CREATE INDEX IX_Appointments_UserID ON Appointments(UserID);
CREATE INDEX IX_Appointments_RecyclerID ON Appointments(RecyclerID);

-- AppointmentCategories 表
CREATE INDEX IX_AppointmentCategories_AppointmentID ON AppointmentCategories(AppointmentID);

-- Messages 表
CREATE INDEX IX_Messages_OrderID ON Messages(OrderID);

-- Conversations 表
CREATE INDEX IX_Conversations_OrderID ON Conversations(OrderID);
CREATE INDEX IX_Conversations_UserID ON Conversations(UserID);
CREATE INDEX IX_Conversations_RecyclerID ON Conversations(RecyclerID);
```

### 唯一索引
确保数据唯一性：

```sql
-- Users 表
CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
CREATE UNIQUE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- Recyclers 表
CREATE UNIQUE INDEX IX_Recyclers_Username ON Recyclers(Username);

-- Admins 表
CREATE UNIQUE INDEX IX_Admins_Username ON Admins(Username);

-- SuperAdmins 表
CREATE UNIQUE INDEX IX_SuperAdmins_Username ON SuperAdmins(Username);
```

### 组合索引
优化常用查询：

```sql
-- 按状态和日期查询订单
CREATE INDEX IX_Appointments_Status_AppointmentDate 
    ON Appointments(Status, AppointmentDate);

-- 按回收员和状态查询订单
CREATE INDEX IX_Appointments_RecyclerID_Status 
    ON Appointments(RecyclerID, Status);

-- 按发送者查询消息
CREATE INDEX IX_Messages_SenderType_SenderID 
    ON Messages(SenderType, SenderID);
```

---

## 数据字典

### 订单状态（Appointments.Status）

| 状态值 | 说明 | 可转换到的状态 |
|--------|------|----------------|
| 已预约 | 用户提交预约，等待回收员接单 | 进行中、已取消 |
| 进行中 | 回收员已接单，正在处理 | 已完成 |
| 已完成 | 回收完成 | （终态）|
| 已取消 | 用户取消预约 | （终态）|

### 消息发送者类型（Messages.SenderType）

| 类型值 | 说明 | SenderID 对应字段 |
|--------|------|-------------------|
| user | 用户发送的消息 | Users.UserID |
| recycler | 回收员发送的消息 | Recyclers.RecyclerID |

### 品类代码（RecyclableItems.Category）

| 代码 | 中文名 | 说明 |
|------|--------|------|
| glass | 玻璃 | 各类玻璃制品 |
| metal | 金属 | 铁、铜、铝等金属制品 |
| plastic | 塑料 | 塑料瓶、塑料袋等 |
| paper | 纸类 | 纸箱、书籍、报纸等 |
| fabric | 纺织品 | 衣服、布料等 |

### 预约类型（Appointments.AppointmentType）

| 类型 | 说明 |
|------|------|
| 上门回收 | 回收员上门取货 |
| 自送 | 用户自行送到回收点 |

---

**文档版本：1.0**  
**更新日期：2025-11-03**  
**适用系统版本：全品类可回收物预约回收系统**
