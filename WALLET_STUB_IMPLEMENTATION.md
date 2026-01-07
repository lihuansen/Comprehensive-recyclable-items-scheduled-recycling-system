# 钱包功能存根实现说明

## 概述

本次修改将"我的钱包"功能改为存根模式（Stub Mode），保留UI界面和样式，但暂不实现具体功能，以避免系统持续出现的数据库相关错误。

## 修改内容

### 1. WalletTransactionBLL.cs 修改

**文件位置**: `recycling.BLL/WalletTransactionBLL.cs`

**修改的方法**:

- `Recharge()` - 充值功能
  - **修改前**: 访问数据库表 UserPaymentAccounts 和 WalletTransactions
  - **修改后**: 直接返回 "充值功能即将上线，敬请期待！"

- `Withdraw()` - 提现功能
  - **修改前**: 访问数据库表并执行提现逻辑
  - **修改后**: 直接返回 "提现功能即将上线，敬请期待！"

- `GetTransactionsByUserId()` - 获取交易记录
  - **修改前**: 从数据库查询交易记录
  - **修改后**: 返回空列表

- `GetTransactionById()` - 获取交易详情
  - **修改前**: 从数据库查询单个交易
  - **修改后**: 返回 null

- `GetUserTransactionStatistics()` - 获取统计数据
  - **修改前**: 从数据库统计交易数据
  - **修改后**: 返回默认值 (0, 0, 0)

- `GetWalletViewModel()` - 获取钱包视图模型
  - **修改前**: 查询支付账户和交易记录
  - **修改后**: 返回空列表，仅保留用户基本信息

### 2. PaymentAccountBLL.cs 修改

**文件位置**: `recycling.BLL/PaymentAccountBLL.cs`

**修改的方法**:

- `AddPaymentAccount()` - 添加支付账户
  - **修改前**: 将账户信息保存到数据库
  - **修改后**: 返回 "添加支付账户功能开发中"

- `GetPaymentAccountsByUserId()` - 获取支付账户列表
  - **修改前**: 从数据库查询用户的支付账户
  - **修改后**: 返回空列表

- `GetPaymentAccountById()` - 获取支付账户详情
  - **修改前**: 从数据库查询单个账户
  - **修改后**: 返回 null

- `DeletePaymentAccount()` - 删除支付账户
  - **修改前**: 从数据库删除账户
  - **修改后**: 返回 "删除功能开发中"

- `SetDefaultAccount()` - 设置默认账户
  - **修改前**: 更新数据库中的默认账户设置
  - **修改后**: 返回 "设置默认账户功能开发中"

- `VerifyAccount()` - 验证账户
  - **修改前**: 更新账户验证状态
  - **修改后**: 返回 "验证账户功能开发中"

## UI保持不变

### MyWallet.cshtml 视图

**文件位置**: `recycling.Web.UI/Views/Home/MyWallet.cshtml`

该视图**无需修改**，因为：

1. ✅ 已包含"即将上线"标识
   - 所有钱包功能按钮都有 `disabled` 样式
   - 点击按钮显示 "功能即将上线，敬请期待！" 提示

2. ✅ 已正确处理空数据
   - 支付账户为空时，显示"您还没有绑定支付账户"
   - 交易记录为空时，显示"暂无交易记录"

3. ✅ 显示用户余额
   - 余额仍从 User.money 字段读取（默认为 0）
   - 统计数据显示为 0（累计收入、累计支出、本月交易）

### HomeController.cs

**文件位置**: `recycling.Web.UI/Controllers/HomeController.cs`

`MyWallet()` 方法**无需修改**，因为它调用的 `GetWalletViewModel()` 方法已经被修改为返回安全的默认数据。

## 用户体验

### 访问"我的钱包"时

1. ✅ 页面正常显示，不会出现错误
2. ✅ 显示钱包余额（默认 ¥0.00）
3. ✅ 显示资金概览（全部为 0）
4. ✅ 支付账户管理区域显示"您还没有绑定支付账户"
5. ✅ 交易记录显示"暂无交易记录"
6. ✅ 所有功能按钮显示"即将上线"标识
7. ✅ 点击任何操作按钮都会提示"功能开发中"

### 从个人中心访问

**文件**: `recycling.Web.UI/Views/Home/Profile.cshtml`

- ✅ "我的钱包"链接仍然存在
- ✅ 点击后正常跳转到钱包页面

## 优势

### 1. 避免数据库错误
- 不再访问可能不存在的 `UserPaymentAccounts` 表
- 不再访问可能不存在的 `WalletTransactions` 表
- 系统不会因为缺少数据库表而崩溃

### 2. 保持UI完整性
- 用户可以看到完整的钱包界面设计
- 界面清晰显示"即将上线"状态
- 用户体验良好，不会感到困惑

### 3. 易于后续开发
- 当准备好实现功能时，只需：
  1. 创建所需的数据库表
  2. 恢复 BLL 层的原有实现代码
  3. 更新 UI 中的"即将上线"标识
- 不需要重新设计UI或控制器逻辑

### 4. 代码可维护性
- 每个被存根化的方法都有清晰的注释
- 说明该功能"暂未开发"
- 方便其他开发者理解代码状态

## 未来实现完整功能时需要做的事

### 数据库准备
1. 创建 `UserPaymentAccounts` 表
2. 创建 `WalletTransactions` 表
3. 为 `Users` 表添加 `money` 字段（如果不存在）

可以使用现有的SQL脚本：
- `Database/CreateWalletTables.sql`
- `Database/AddWalletTablesToExistingDatabase.sql`

### 代码恢复
1. 恢复 `WalletTransactionBLL.cs` 中的完整实现
2. 恢复 `PaymentAccountBLL.cs` 中的完整实现
3. 移除 UI 中的"即将上线"标识和 disabled 样式
4. 将按钮的 `onclick` 事件改为实际功能调用

### 测试
1. 测试充值功能
2. 测试提现功能
3. 测试支付账户管理
4. 测试交易记录查询

## 总结

本次修改采用最小化变更原则：
- ✅ 仅修改 BLL 层（业务逻辑层）
- ✅ 不修改 UI 视图
- ✅ 不修改控制器
- ✅ 不需要数据库表
- ✅ 系统稳定运行
- ✅ UI 完整保留
- ✅ 用户体验良好

这是一个临时但有效的解决方案，既解决了当前的错误问题，又为未来的功能开发预留了空间。
