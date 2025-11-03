# 全品类可回收物预约回收系统

## 项目简介

这是一个基于 ASP.NET MVC 的**可回收物品预约回收管理系统**，为用户提供便捷的回收物品预约服务，同时为回收员提供高效的订单管理平台。

## 核心功能

### 用户端
- 🔐 多种登录方式（用户名密码、手机验证码、邮箱验证码）
- 📋 可回收物品查询与价格查看
- 📅 预约回收服务（支持多品类、紧急预约）
- 📦 订单管理与追踪
- 💬 与回收员实时消息沟通
- 👤 个人信息管理

### 回收员端
- 📥 接收和管理回收订单
- 📊 查看订单详情和品类信息
- 💬 与用户消息沟通
- ✅ 完成订单流程

### 管理员端
- 👥 用户和回收员管理
- 📈 系统数据统计
- 🔍 订单监控

## 技术栈

- **后端框架**: ASP.NET MVC (.NET Framework 4.7.2+)
- **数据库**: Microsoft SQL Server
- **数据访问**: ADO.NET (手动 SQL)
- **前端**: jQuery + Bootstrap + Razor Views
- **身份验证**: Session-based Authentication
- **密码加密**: SHA256 Hash

## 项目架构

```
┌─────────────────────────────────────────────┐
│         Presentation Layer (UI)             │
│    ASP.NET MVC Controllers + Views          │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│       Business Logic Layer (BLL)            │
│   业务规则验证、流程控制、数据转换            │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│       Data Access Layer (DAL)               │
│   SQL 执行、数据库连接、事务处理              │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│          Database (SQL Server)              │
└─────────────────────────────────────────────┘
```

## 快速开始

### 环境要求

- Visual Studio 2019/2022
- .NET Framework 4.7.2 或更高版本
- SQL Server 2017 或更高版本
- SQL Server Management Studio (SSMS)

### 安装步骤

1. **克隆仓库**
   ```bash
   git clone https://github.com/lihuansen/Comprehensive-recyclable-items-scheduled-recycling-system.git
   cd Comprehensive-recyclable-items-scheduled-recycling-system
   ```

2. **创建数据库**
   - 在 SQL Server 中创建数据库 `RecyclingDB`
   - 执行数据库脚本创建表结构
   - 执行初始数据脚本

3. **配置连接字符串**
   
   修改 `recycling.Web.UI/Web.config`:
   ```xml
   <connectionStrings>
     <add name="RecyclingDB" 
          connectionString="Data Source=YOUR_SERVER;Initial Catalog=RecyclingDB;Integrated Security=True" 
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

4. **还原 NuGet 包**
   - 在 Visual Studio 中打开解决方案
   - 右键解决方案 → 还原 NuGet 包

5. **运行项目**
   - 设置 `recycling.Web.UI` 为启动项目
   - 按 F5 开始调试
   - 浏览器会自动打开首页

## 项目结构

```
Solution Root
├── recycling.Web.UI/         # 表示层 (ASP.NET MVC)
│   ├── Controllers/          # MVC 控制器
│   ├── Views/               # Razor 视图
│   ├── Scripts/             # JavaScript 文件
│   └── App_Start/           # 应用配置
│
├── recycling.BLL/           # 业务逻辑层
│   ├── UserBLL.cs
│   ├── AppointmentBLL.cs
│   ├── OrderBLL.cs
│   └── ...
│
├── recycling.DAL/           # 数据访问层
│   ├── UserDAL.cs
│   ├── AppointmentDAL.cs
│   ├── OrderDAL.cs
│   └── ...
│
├── recycling.Model/         # 数据模型层
│   ├── Users.cs
│   ├── Appointments.cs
│   └── ...
│
└── recycling.Common/        # 公共工具层
    └── EmailService.cs
```

## 文档

本项目提供了完整的中文文档：

- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - 系统架构设计文档
  - 完整的架构设计说明
  - 数据模型和实体关系
  - 业务逻辑层、数据访问层详解
  - 核心业务流程图
  - 安全机制说明

- **[DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md)** - 数据库架构文档
  - 完整的表结构定义
  - ER 关系图
  - 索引设计建议
  - 数据字典

- **[DEVELOPMENT_GUIDE.md](./DEVELOPMENT_GUIDE.md)** - 开发工作流程指南
  - 开发环境搭建
  - 代码编写规范
  - 常见开发场景示例
  - 调试技巧
  - 测试指南

## 核心业务流程

### 用户注册流程
```
用户填写注册信息 → 前端验证 → 后端验证（用户名、手机、邮箱唯一性）
→ 密码 SHA256 哈希 → 插入数据库 → 注册成功
```

### 预约回收流程
```
用户查看价格 → 填写预约表单（选择品类、日期、地址）
→ 提交预约 → BLL 验证 → DAL 事务插入（预约+品类）
→ 预约成功（状态：已预约）
```

### 回收员接单流程
```
回收员登录 → 查看待接单列表 → 接收订单
→ 更新状态为"进行中" → 分配回收员ID
→ 用户和回收员可以消息沟通 → 完成订单
```

## 数据库表

主要数据表：

| 表名 | 说明 | 关键字段 |
|------|------|----------|
| Users | 用户表 | UserID, Username, PhoneNumber, Email |
| Recyclers | 回收员表 | RecyclerID, Username, Region, Rating |
| Appointments | 预约订单表 | AppointmentID, UserID, RecyclerID, Status |
| AppointmentCategories | 预约品类表 | CategoryID, AppointmentID, CategoryName |
| RecyclableItems | 可回收物品表 | ItemId, Name, Category, PricePerKg |
| Messages | 消息表 | MessageID, OrderID, SenderType, Content |

详细的表结构和关系请参考 [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md)

## 安全特性

- ✅ 密码使用 SHA256 哈希存储
- ✅ SQL 参数化查询防止注入
- ✅ CSRF 防护（Anti-Forgery Token）
- ✅ Session 管理（30分钟超时）
- ✅ 验证码机制（图形验证码、短信验证码、邮箱验证码）
- ✅ 多层输入验证（前端 + 模型验证 + 业务验证）

## 常见问题

### Q1: 如何重置管理员密码？
A: 直接在数据库中更新密码哈希值，或通过 SQL 脚本重置。

### Q2: 如何添加新的回收物品类型？
A: 在 `RecyclableItems` 表中插入新记录，设置品类代码、名称和价格。

### Q3: 订单状态转换规则是什么？
A: 
```
已预约 → 进行中 → 已完成
  ↓
已取消
```
只有"已预约"状态可以取消，"进行中"状态可以完成。

### Q4: 如何扩展新的用户角色？
A: 参考现有的 `Recyclers`、`Admins` 表结构，创建新的角色表，并在 `StaffBLL` 中添加相应的登录验证逻辑。

## 未来改进方向

- [ ] 使用 Entity Framework 简化数据访问
- [ ] 实现依赖注入（DI）降低耦合
- [ ] 添加单元测试和集成测试
- [ ] 使用 bcrypt 替代 SHA256 提升密码安全性
- [ ] 实现 RESTful API 支持移动端
- [ ] 添加支付功能（支付宝/微信支付）
- [ ] 使用 SignalR 实现实时消息推送
- [ ] 添加用户评价和评分系统
- [ ] 实现数据分析和可视化

## 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 联系方式

- 项目维护者：lihuansen
- GitHub：[@lihuansen](https://github.com/lihuansen)

---

⭐ 如果觉得这个项目有帮助，请给个 Star！
