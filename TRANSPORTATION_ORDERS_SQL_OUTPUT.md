# TransportationOrders 表的 SQL Server 语言代码
# SQL Server Code for TransportationOrders Table

本文档提供了 TransportationOrders（运输单表）的完整 SQL Server 建表代码。

This document provides the complete SQL Server table creation code for TransportationOrders.

---

## 完整 SQL Server 代码 (Complete SQL Server Code)

```sql
-- ==============================================================================
-- 运输单表（TransportationOrders）建表脚本
-- Transportation Orders Table Creation Script
-- 
-- 用途: 存储回收员联系运输人员的运输单信息
-- Purpose: Store transportation orders created by recyclers to contact transporters
--
-- 作者: System
-- 创建日期: 2026-01-03
-- ==============================================================================

USE RecyclingDB;
GO

-- ==============================================================================
-- TransportationOrders 表（运输单表）
-- 实体类: recycling.Model.TransportationOrders
-- 用途: 存储从回收员暂存点到基地的运输单信息
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TransportationOrders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TransportationOrders] (
        [TransportOrderID] INT PRIMARY KEY IDENTITY(1,1),    -- 运输单ID（自增主键）
        [OrderNumber] NVARCHAR(50) NOT NULL UNIQUE,          -- 运输单号（唯一，格式：TO+年月日+序号）
        [RecyclerID] INT NOT NULL,                           -- 回收员ID（外键）
        [TransporterID] INT NOT NULL,                        -- 运输人员ID（外键）
        [PickupAddress] NVARCHAR(200) NOT NULL,              -- 取货地址
        [DestinationAddress] NVARCHAR(200) NOT NULL,         -- 目的地地址（基地地址）
        [ContactPerson] NVARCHAR(50) NOT NULL,               -- 联系人（回收员姓名）
        [ContactPhone] NVARCHAR(20) NOT NULL,                -- 联系电话
        [EstimatedWeight] DECIMAL(10, 2) NOT NULL,           -- 预估总重量（kg）
        [ActualWeight] DECIMAL(10, 2) NULL,                  -- 实际重量（kg）
        [ItemCategories] NVARCHAR(MAX) NULL,                 -- 物品类别（JSON格式存储）
        [SpecialInstructions] NVARCHAR(500) NULL,            -- 特殊说明
        [Status] NVARCHAR(20) NOT NULL DEFAULT N'待接单',    -- 状态（待接单、已接单、运输中、已完成、已取消）
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),  -- 创建时间
        [AcceptedDate] DATETIME2 NULL,                       -- 接单时间
        [PickupDate] DATETIME2 NULL,                         -- 取货时间
        [DeliveryDate] DATETIME2 NULL,                       -- 送达时间
        [CompletedDate] DATETIME2 NULL,                      -- 完成时间
        [CancelledDate] DATETIME2 NULL,                      -- 取消时间
        [CancelReason] NVARCHAR(200) NULL,                   -- 取消原因
        [TransporterNotes] NVARCHAR(500) NULL,               -- 运输人员备注
        [RecyclerRating] INT NULL,                           -- 回收员评分（1-5）
        [RecyclerReview] NVARCHAR(500) NULL,                 -- 回收员评价
        
        -- 约束
        CONSTRAINT FK_TransportationOrders_Recyclers FOREIGN KEY ([RecyclerID]) 
            REFERENCES [dbo].[Recyclers]([RecyclerID]),
        CONSTRAINT FK_TransportationOrders_Transporters FOREIGN KEY ([TransporterID]) 
            REFERENCES [dbo].[Transporters]([TransporterID]),
        CONSTRAINT CK_TransportationOrders_Status 
            CHECK ([Status] IN (N'待接单', N'已接单', N'运输中', N'已完成', N'已取消')),
        CONSTRAINT CK_TransportationOrders_RecyclerRating 
            CHECK ([RecyclerRating] IS NULL OR ([RecyclerRating] >= 1 AND [RecyclerRating] <= 5)),
        CONSTRAINT CK_TransportationOrders_EstimatedWeight 
            CHECK ([EstimatedWeight] > 0),
        CONSTRAINT CK_TransportationOrders_ActualWeight 
            CHECK ([ActualWeight] IS NULL OR [ActualWeight] > 0)
    );

    -- 创建索引
    CREATE UNIQUE INDEX IX_TransportationOrders_OrderNumber ON [dbo].[TransportationOrders]([OrderNumber]);
    CREATE INDEX IX_TransportationOrders_RecyclerID ON [dbo].[TransportationOrders]([RecyclerID]);
    CREATE INDEX IX_TransportationOrders_TransporterID ON [dbo].[TransportationOrders]([TransporterID]);
    CREATE INDEX IX_TransportationOrders_Status ON [dbo].[TransportationOrders]([Status]);
    CREATE INDEX IX_TransportationOrders_CreatedDate ON [dbo].[TransportationOrders]([CreatedDate] DESC);

    PRINT 'TransportationOrders 表创建成功';
END
ELSE
BEGIN
    PRINT 'TransportationOrders 表已存在';
END
GO
```

---

## 表结构详细说明 (Table Structure Details)

### 主键 (Primary Key)
- **TransportOrderID** (INT, IDENTITY): 运输单唯一标识，自增主键

### 基本信息字段 (Basic Information Fields)

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|---------|------|------|
| TransportOrderID | INT | PRIMARY KEY IDENTITY(1,1) | 运输单唯一标识（主键，自增） |
| OrderNumber | NVARCHAR(50) | NOT NULL, UNIQUE | 运输单号，格式为 TO+YYYYMMDD+序号，如 TO202601030001 |
| RecyclerID | INT | NOT NULL, FOREIGN KEY | 发起运输的回收员ID |
| TransporterID | INT | NOT NULL, FOREIGN KEY | 负责运输的运输人员ID |
| PickupAddress | NVARCHAR(200) | NOT NULL | 回收员暂存点地址（取货地址） |
| DestinationAddress | NVARCHAR(200) | NOT NULL | 目的地地址（通常是分拣中心或基地） |
| ContactPerson | NVARCHAR(50) | NOT NULL | 联系人姓名（回收员姓名） |
| ContactPhone | NVARCHAR(20) | NOT NULL | 联系电话（回收员电话） |

### 重量和物品信息 (Weight and Item Information)

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|---------|------|------|
| EstimatedWeight | DECIMAL(10, 2) | NOT NULL, CHECK > 0 | 预估总重量（公斤） |
| ActualWeight | DECIMAL(10, 2) | NULL, CHECK > 0 | 实际运输重量（公斤），由运输人员在完成后填写 |
| ItemCategories | NVARCHAR(MAX) | NULL | 物品类别JSON，例如：[{"categoryName":"纸类","weight":10.5}] |
| SpecialInstructions | NVARCHAR(500) | NULL | 特殊说明或备注信息 |

### 状态和时间字段 (Status and Timestamp Fields)

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|---------|------|------|
| Status | NVARCHAR(20) | NOT NULL, DEFAULT '待接单' | 运输单状态（待接单、已接单、运输中、已完成、已取消） |
| CreatedDate | DATETIME2 | NOT NULL, DEFAULT GETDATE() | 运输单创建时间 |
| AcceptedDate | DATETIME2 | NULL | 运输人员接单时间 |
| PickupDate | DATETIME2 | NULL | 运输人员取货时间 |
| DeliveryDate | DATETIME2 | NULL | 运输人员送达时间 |
| CompletedDate | DATETIME2 | NULL | 运输单完成时间 |
| CancelledDate | DATETIME2 | NULL | 取消时间 |
| CancelReason | NVARCHAR(200) | NULL | 取消原因 |

### 备注和评价字段 (Notes and Review Fields)

| 字段名 | 数据类型 | 约束 | 说明 |
|--------|---------|------|------|
| TransporterNotes | NVARCHAR(500) | NULL | 运输人员备注信息 |
| RecyclerRating | INT | NULL, CHECK 1-5 | 回收员对运输服务的评分（1-5分） |
| RecyclerReview | NVARCHAR(500) | NULL | 回收员对运输服务的评价文字 |

---

## 约束说明 (Constraints)

### 外键约束 (Foreign Key Constraints)
1. **FK_TransportationOrders_Recyclers**: 
   - 字段: RecyclerID
   - 引用: Recyclers(RecyclerID)
   - 说明: 确保运输单关联的回收员存在

2. **FK_TransportationOrders_Transporters**: 
   - 字段: TransporterID
   - 引用: Transporters(TransporterID)
   - 说明: 确保运输单关联的运输人员存在

### 检查约束 (Check Constraints)
1. **CK_TransportationOrders_Status**:
   - 约束: Status 只能是 '待接单'、'已接单'、'运输中'、'已完成'、'已取消' 之一

2. **CK_TransportationOrders_RecyclerRating**:
   - 约束: RecyclerRating 必须为 NULL 或在 1-5 之间

3. **CK_TransportationOrders_EstimatedWeight**:
   - 约束: EstimatedWeight 必须大于 0

4. **CK_TransportationOrders_ActualWeight**:
   - 约束: ActualWeight 必须为 NULL 或大于 0

---

## 索引说明 (Indexes)

| 索引名称 | 索引类型 | 字段 | 说明 |
|---------|---------|------|------|
| IX_TransportationOrders_OrderNumber | UNIQUE | OrderNumber | 确保运输单号唯一，提高查询性能 |
| IX_TransportationOrders_RecyclerID | NONCLUSTERED | RecyclerID | 提高按回收员查询的性能 |
| IX_TransportationOrders_TransporterID | NONCLUSTERED | TransporterID | 提高按运输人员查询的性能 |
| IX_TransportationOrders_Status | NONCLUSTERED | Status | 提高按状态查询的性能 |
| IX_TransportationOrders_CreatedDate | NONCLUSTERED | CreatedDate DESC | 提高按创建时间倒序查询的性能 |

---

## 业务规则 (Business Rules)

### 1. 运输单号格式 (Order Number Format)
- 格式: TO + YYYYMMDD + 4位序号
- 示例: TO202601030001, TO202601030002
- 规则: 系统自动生成，确保唯一性

### 2. 状态流转规则 (Status Transition Rules)
```
待接单 → 已接单 → 运输中 → 已完成
         ↓
      已取消（可在任何状态下取消，需填写取消原因）
```

### 3. 数据验证规则 (Data Validation Rules)
- RecyclerID 和 TransporterID 必须存在于对应的表中
- EstimatedWeight 必须大于 0
- ActualWeight 只有在运输完成后由运输人员填写
- ItemCategories 以 JSON 格式存储物品类别和重量信息
- 只有状态为"已完成"的运输单才能评分

### 4. 区域匹配规则 (Region Matching Rules)
- 回收员只能选择同区域的运输人员
- 运输人员必须满足以下条件：
  - IsActive = 1 (账号激活)
  - Available = 1 (可接单)
  - CurrentStatus = '空闲' (当前空闲)

---

## 示例数据 (Sample Data)

```sql
-- 插入示例运输单
INSERT INTO [dbo].[TransportationOrders] 
    (OrderNumber, RecyclerID, TransporterID, PickupAddress, DestinationAddress, 
     ContactPerson, ContactPhone, EstimatedWeight, ItemCategories, Status)
VALUES 
    (N'TO202601030001', 1, 1, N'深圳市罗湖区XX街道XX号', N'深圳市罗湖区分拣中心',
     N'张回收员', N'13800138000', 45.50, 
     N'[{"categoryName":"纸类","weight":20.5},{"categoryName":"塑料","weight":15.0},{"categoryName":"金属","weight":10.0}]',
     N'待接单');

-- 查询运输单
SELECT 
    TransportOrderID,
    OrderNumber,
    PickupAddress,
    DestinationAddress,
    EstimatedWeight,
    Status,
    CreatedDate
FROM [dbo].[TransportationOrders]
ORDER BY CreatedDate DESC;
```

---

## 相关查询语句 (Common Queries)

### 1. 查询回收员的所有运输单
```sql
SELECT 
    TransportOrderID,
    OrderNumber,
    Status,
    EstimatedWeight,
    CreatedDate
FROM TransportationOrders
WHERE RecyclerID = @RecyclerID
ORDER BY CreatedDate DESC;
```

### 2. 查询运输人员的待接单列表
```sql
SELECT 
    TransportOrderID,
    OrderNumber,
    PickupAddress,
    EstimatedWeight,
    CreatedDate
FROM TransportationOrders
WHERE TransporterID = @TransporterID 
    AND Status = N'待接单'
ORDER BY CreatedDate DESC;
```

### 3. 统计运输单状态分布
```sql
SELECT 
    Status,
    COUNT(*) AS OrderCount,
    SUM(EstimatedWeight) AS TotalWeight
FROM TransportationOrders
GROUP BY Status
ORDER BY OrderCount DESC;
```

### 4. 查询指定日期范围的运输单
```sql
SELECT 
    OrderNumber,
    ContactPerson,
    Status,
    EstimatedWeight,
    CreatedDate
FROM TransportationOrders
WHERE CreatedDate BETWEEN @StartDate AND @EndDate
ORDER BY CreatedDate DESC;
```

### 5. 查询已完成且未评价的运输单
```sql
SELECT 
    TransportOrderID,
    OrderNumber,
    RecyclerID,
    CompletedDate
FROM TransportationOrders
WHERE Status = N'已完成' 
    AND RecyclerRating IS NULL
ORDER BY CompletedDate DESC;
```

---

## 维护和管理 (Maintenance and Management)

### 查看表结构
```sql
-- 查看表定义
EXEC sp_help 'TransportationOrders';

-- 查看列信息
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TransportationOrders'
ORDER BY ORDINAL_POSITION;
```

### 查看约束
```sql
-- 查看所有约束
SELECT 
    CONSTRAINT_NAME,
    CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME = 'TransportationOrders';
```

### 查看索引
```sql
-- 查看所有索引
EXEC sp_helpindex 'TransportationOrders';
```

### 性能优化建议
```sql
-- 定期更新统计信息
UPDATE STATISTICS TransportationOrders;

-- 重建索引（当索引碎片率高时）
ALTER INDEX ALL ON TransportationOrders REBUILD;
```

---

## 注意事项 (Important Notes)

1. **数据完整性**: 
   - 使用事务确保运输单创建的原子性
   - 所有外键引用必须有效

2. **性能考虑**:
   - 索引已针对常见查询模式优化
   - ItemCategories 字段使用 NVARCHAR(MAX) 存储 JSON，查询时注意性能

3. **安全性**:
   - 敏感信息（如联系电话）需要适当的访问控制
   - 记录操作日志以便审计

4. **扩展性**:
   - 表结构设计支持未来功能扩展
   - JSON 格式的 ItemCategories 便于灵活存储不同类型的物品信息

---

## 版本信息 (Version Information)

- **版本**: 1.0
- **创建日期**: 2026-01-03
- **最后更新**: 2026-01-03
- **兼容性**: SQL Server 2012 及以上版本
- **字符集**: 使用 NVARCHAR 支持 Unicode 中文字符

---

## 相关文件 (Related Files)

- SQL 脚本文件: `Database/CreateTransportationOrdersTable.sql`
- 实体类文件: `recycling.Model/TransportationOrders.cs`
- 数据访问层: `recycling.DAL/TransportationOrderDAL.cs`
- 业务逻辑层: `recycling.BLL/TransportationOrderBLL.cs`
- 实现文档: `TRANSPORTATION_ORDER_IMPLEMENTATION.md`
