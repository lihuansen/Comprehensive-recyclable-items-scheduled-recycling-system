# 账号管理功能实现说明

## 概述

本次更新实现了以下功能：

1. **修复下拉导航问题** - 解决无法点击下拉菜单项的问题
2. **重构导航结构** - 将"账号管理"重命名为"角色管理"
3. **新增账号自管理功能** - 管理员可以管理自己的账号信息（无权限限制）

## 问题背景

### 问题1：下拉导航无法选择

之前实现的下拉导航存在一个问题：当用户将鼠标移到下拉菜单上时，由于菜单与触发器之间有5px的间隙（margin-top），导致鼠标离开触发器区域后菜单立即消失，用户无法点击菜单项。

### 问题2：导航结构需要调整

根据需求：
- 原来的"账号管理"应该改名为"角色管理"
- 在"角色管理"下新增一个"账号管理"功能
- 新的"账号管理"用于管理员自我管理，不需要权限限制

## 解决方案

### 1. 修复下拉导航问题

**修改文件：** `recycling.Web.UI/Views/Shared/_AdminLayout.cshtml`

**问题原因：**
```css
.dropdown-menu-custom {
    margin-top: 5px;  /* 这个间隙导致hover状态中断 */
}
```

**解决方法：**
```css
.dropdown-menu-custom {
    margin-top: 0px;  /* 移除间隙，确保hover连续 */
}
```

### 2. 重构导航结构

**修改文件：** `recycling.Web.UI/Views/Shared/_AdminLayout.cshtml`

**变更前：**
```html
<span class="dropdown-toggle">账号管理</span>
<div class="dropdown-menu-custom">
    @Html.ActionLink("用户管理", "UserManagement", "Staff")
    @Html.ActionLink("回收员管理", "RecyclerManagement", "Staff")
    @Html.ActionLink("运输人员管理", "TransporterManagement", "Staff")
    @Html.ActionLink("基地人员管理", "SortingCenterWorkerManagement", "Staff")
</div>
```

**变更后：**
```html
<span class="dropdown-toggle">角色管理</span>
<div class="dropdown-menu-custom">
    @Html.ActionLink("账号管理", "AccountSelfManagement", "Staff")
    @Html.ActionLink("用户管理", "UserManagement", "Staff")
    @Html.ActionLink("回收员管理", "RecyclerManagement", "Staff")
    @Html.ActionLink("运输人员管理", "TransporterManagement", "Staff")
    @Html.ActionLink("基地人员管理", "SortingCenterWorkerManagement", "Staff")
</div>
```

**变更说明：**
- 下拉菜单标题从"账号管理"改为"角色管理"
- 在第一个位置添加新的"账号管理"链接，指向 `AccountSelfManagement`
- 保持其他菜单项不变

### 3. 实现账号自管理功能

#### 3.1 控制器实现

**文件：** `recycling.Web.UI/Controllers/StaffController.cs`

**新增方法：**

1. **AccountSelfManagement()** - 账号管理页面
```csharp
/// <summary>
/// 管理员账号自我管理（无权限限制）
/// </summary>
public ActionResult AccountSelfManagement()
{
    if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "admin")
        return RedirectToAction("Login", "Staff");

    var admin = (Admins)Session["LoginStaff"];
    ViewBag.StaffName = admin.Username;
    ViewBag.DisplayName = "管理员";
    ViewBag.StaffRole = "admin";

    return View();
}
```

2. **GetSelfAccountInfo()** - 获取当前管理员信息 API
```csharp
/// <summary>
/// 管理员 - 获取自己的账号信息（无权限限制）
/// </summary>
[HttpGet]
public ContentResult GetSelfAccountInfo()
{
    if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "admin")
    {
        return JsonContent(new { success = false, message = "未登录" });
    }

    try
    {
        var currentAdmin = (Admins)Session["LoginStaff"];
        var admin = _adminBLL.GetAdminById(currentAdmin.AdminID);
        return JsonContent(new {
            success = true,
            data = new {
                adminId = admin.AdminID,
                username = admin.Username,
                fullName = admin.FullName,
                isActive = admin.IsActive,
                createdAt = admin.CreatedDate,
                lastLogin = admin.LastLoginDate
            }
        });
    }
    catch (Exception ex)
    {
        return JsonContent(new { success = false, message = ex.Message });
    }
}
```

3. **UpdateSelfAccount()** - 更新当前管理员信息 API
```csharp
/// <summary>
/// 管理员 - 更新自己的账号信息（无权限限制）
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public JsonResult UpdateSelfAccount(string fullName, string oldPassword, string newPassword)
{
    // 实现逻辑包括：
    // 1. 验证旧密码（如需修改密码）
    // 2. 更新姓名（如果提供）
    // 3. 更新密码（如果提供）
    // 4. 更新Session中的管理员信息
    // 5. 记录操作日志
}
```

4. **HashPasswordSHA256()** - 密码哈希辅助方法
```csharp
/// <summary>
/// Hash password using SHA256 (same as AdminBLL)
/// </summary>
private string HashPasswordSHA256(string password)
{
    using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
    {
        byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}
```

#### 3.2 视图实现

**文件：** `recycling.Web.UI/Views/Staff/AccountSelfManagement.cshtml`

**功能特性：**

1. **账号基本信息显示区域**
   - 用户名（只读）
   - 姓名（只读，可在下方修改）
   - 账号状态（激活/禁用）
   - 创建时间
   - 最后登录时间

2. **信息修改表单**
   - 姓名修改（可选）
   - 密码修改（可选）
     - 旧密码验证
     - 新密码输入
     - 新密码确认

3. **前端验证**
   - 至少修改一项信息
   - 修改密码时必须输入旧密码
   - 新密码长度至少6个字符
   - 确认密码必须一致

4. **AJAX交互**
   - 页面加载时自动获取账号信息
   - 表单提交使用AJAX，无需页面刷新
   - 成功后自动刷新显示的信息
   - 包含CSRF令牌保护

#### 3.3 日志模块扩展

**文件：** `recycling.BLL/OperationLogBLL.cs`

**新增模块：**
```csharp
public static class Modules
{
    // ... 现有模块 ...
    public const string AccountManagement = "AccountManagement";
}

public static string GetModuleDisplayName(string module)
{
    switch (module)
    {
        // ... 现有映射 ...
        case Modules.AccountManagement: return "账号管理";
        default: return module;
    }
}
```

## 技术细节

### 权限控制

**特殊设计：无权限限制**

根据需求，账号自管理功能不需要权限限制，只要是登录的管理员就可以访问。这是因为：
1. 管理员管理自己的账号是基本权利
2. 不涉及其他用户或系统数据的管理
3. 只能修改自己的信息，安全性有保障

**实现方式：**
```csharp
// 只验证登录状态和角色，不检查具体权限
if (Session["LoginStaff"] == null || Session["StaffRole"] as string != "admin")
    return RedirectToAction("Login", "Staff");
```

### 密码安全

**密码处理流程：**

1. **验证旧密码**
   - 获取数据库中的密码哈希值
   - 对用户输入的旧密码进行SHA256哈希
   - 比较两个哈希值

2. **设置新密码**
   - 对新密码进行SHA256哈希
   - 存储哈希值到数据库
   - 不存储明文密码

3. **哈希算法**
```csharp
SHA256 → 字节数组 → 十六进制字符串
"mypassword" → [bytes] → "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8"
```

### CSRF防护

**实现方式：**

1. **视图中生成令牌**
```razor
@Html.AntiForgeryToken()
```

2. **AJAX请求中包含令牌**
```javascript
data: {
    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
    // ... 其他数据
}
```

3. **控制器验证令牌**
```csharp
[ValidateAntiForgeryToken]
public JsonResult UpdateSelfAccount(...)
```

### 数据模型

**Admins表字段（相关）：**
```csharp
- AdminID: int - 管理员ID（主键）
- Username: string(50) - 用户名
- PasswordHash: string(255) - 密码哈希
- FullName: string(100) - 姓名
- CreatedDate: DateTime? - 创建时间
- LastLoginDate: DateTime? - 最后登录时间
- IsActive: bool? - 是否激活
- Character: string(50) - 权限字符串
```

## 用户使用流程

### 访问账号管理

1. **登录系统**
   - 使用管理员账号登录

2. **打开账号管理**
   - 将鼠标移到导航栏的"角色管理"
   - 点击下拉菜单中的"账号管理"

3. **查看账号信息**
   - 页面显示当前账号的基本信息
   - 包括用户名、姓名、状态等

### 修改账号信息

#### 场景1：只修改姓名

1. 在"姓名"输入框输入新姓名
2. 点击"保存修改"按钮
3. 系统提示"账号信息更新成功！"
4. 页面自动刷新显示新姓名

#### 场景2：只修改密码

1. 在"旧密码"输入框输入当前密码
2. 在"新密码"输入框输入新密码（至少6个字符）
3. 在"确认新密码"输入框再次输入新密码
4. 点击"保存修改"按钮
5. 系统提示"账号信息更新成功！"

#### 场景3：同时修改姓名和密码

1. 输入新姓名
2. 输入旧密码
3. 输入新密码
4. 确认新密码
5. 点击"保存修改"按钮
6. 系统提示"账号信息更新成功！"

### 错误处理

**可能的错误及处理：**

| 错误场景 | 系统提示 | 处理建议 |
|---------|---------|---------|
| 未填写任何信息 | "请至少修改一项信息" | 至少输入姓名或密码 |
| 修改密码但未输入旧密码 | "修改密码时必须输入旧密码" | 输入当前密码 |
| 新密码太短 | "新密码长度至少为6个字符" | 使用更长的密码 |
| 两次密码不一致 | "两次输入的新密码不一致" | 确保两次输入相同 |
| 旧密码错误 | "旧密码错误" | 检查当前密码是否正确 |

## 文件清单

### 修改的文件

1. **recycling.Web.UI/Views/Shared/_AdminLayout.cshtml**
   - 修改行数：约5行
   - 变更内容：
     - 修复下拉菜单margin-top
     - 重命名导航标题
     - 添加账号管理链接

2. **recycling.Web.UI/Controllers/StaffController.cs**
   - 新增行数：约120行
   - 变更内容：
     - AccountSelfManagement 视图方法
     - GetSelfAccountInfo API方法
     - UpdateSelfAccount API方法
     - HashPasswordSHA256 辅助方法

3. **recycling.BLL/OperationLogBLL.cs**
   - 新增行数：2行
   - 变更内容：
     - 添加 AccountManagement 模块常量
     - 添加模块显示名称映射

### 新增的文件

1. **recycling.Web.UI/Views/Staff/AccountSelfManagement.cshtml**
   - 行数：约270行
   - 内容：
     - CSS样式定义
     - HTML页面结构
     - JavaScript交互逻辑

## 测试建议

### 功能测试

#### 测试用例1：下拉导航可用性

**测试步骤：**
1. 以管理员身份登录系统
2. 将鼠标移动到"角色管理"导航项
3. 观察下拉菜单是否显示
4. 将鼠标移动到下拉菜单项上
5. 点击"账号管理"

**预期结果：**
- ✅ 下拉菜单正确显示
- ✅ 鼠标在菜单项上时菜单不消失
- ✅ 可以成功点击菜单项
- ✅ 跳转到账号管理页面

#### 测试用例2：账号信息显示

**测试步骤：**
1. 访问账号管理页面
2. 观察页面显示的信息

**预期结果：**
- ✅ 显示当前登录的用户名
- ✅ 显示姓名（或"--"如果未设置）
- ✅ 显示账号状态（激活/禁用）
- ✅ 显示创建时间
- ✅ 显示最后登录时间

#### 测试用例3：修改姓名

**测试步骤：**
1. 在"姓名"输入框输入新姓名（例如："张三"）
2. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示成功提示
- ✅ 页面自动刷新显示新姓名
- ✅ 表单输入框被清空

#### 测试用例4：修改密码（成功）

**前置条件：** 假设当前密码是 "admin123"

**测试步骤：**
1. 在"旧密码"输入框输入 "admin123"
2. 在"新密码"输入框输入 "newpass123"
3. 在"确认新密码"输入框输入 "newpass123"
4. 点击"保存修改"按钮
5. 退出登录
6. 使用新密码 "newpass123" 重新登录

**预期结果：**
- ✅ 显示成功提示
- ✅ 表单输入框被清空
- ✅ 可以使用新密码登录
- ✅ 无法使用旧密码登录

#### 测试用例5：修改密码（旧密码错误）

**测试步骤：**
1. 在"旧密码"输入框输入错误的密码
2. 在"新密码"和"确认新密码"输入框输入新密码
3. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示"旧密码错误"提示
- ✅ 密码未被修改
- ✅ 仍可使用原密码登录

#### 测试用例6：同时修改姓名和密码

**测试步骤：**
1. 输入新姓名
2. 输入正确的旧密码
3. 输入新密码并确认
4. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示成功提示
- ✅ 姓名和密码都被更新
- ✅ 可以使用新密码登录
- ✅ 页面显示新姓名

### 验证测试

#### 验证1：至少修改一项

**测试步骤：**
1. 不输入任何内容
2. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示"请至少修改一项信息"

#### 验证2：修改密码时需要旧密码

**测试步骤：**
1. 只输入新密码和确认密码
2. 不输入旧密码
3. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示"修改密码时必须输入旧密码"

#### 验证3：新密码长度验证

**测试步骤：**
1. 输入旧密码
2. 输入少于6个字符的新密码（例如："123"）
3. 确认新密码
4. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示"新密码长度至少为6个字符"

#### 验证4：密码确认一致性

**测试步骤：**
1. 输入旧密码
2. 输入新密码 "password123"
3. 确认密码输入 "password456"（不一致）
4. 点击"保存修改"按钮

**预期结果：**
- ✅ 显示"两次输入的新密码不一致"

### 安全测试

#### 安全测试1：未登录访问

**测试步骤：**
1. 确保未登录
2. 直接访问 `/Staff/AccountSelfManagement`

**预期结果：**
- ✅ 自动重定向到登录页面

#### 安全测试2：非管理员访问

**测试步骤：**
1. 以其他角色登录（如回收员）
2. 直接访问 `/Staff/AccountSelfManagement`

**预期结果：**
- ✅ 无法访问或重定向到登录页面

#### 安全测试3：CSRF保护

**测试步骤：**
1. 使用工具（如Postman）直接发送POST请求到 `/Staff/UpdateSelfAccount`
2. 不包含 `__RequestVerificationToken`

**预期结果：**
- ✅ 请求被拒绝
- ✅ 返回400或403错误

#### 安全测试4：密码哈希验证

**测试步骤：**
1. 修改密码
2. 检查数据库中的 `PasswordHash` 字段

**预期结果：**
- ✅ 存储的是哈希值，不是明文
- ✅ 哈希值长度为64个字符（SHA256）
- ✅ 每次使用相同密码生成相同哈希值

### 浏览器兼容性测试

**测试浏览器：**
- [ ] Chrome (最新版)
- [ ] Firefox (最新版)
- [ ] Edge (最新版)
- [ ] Safari (最新版)

**测试内容：**
- [ ] 下拉导航功能正常
- [ ] 页面样式正常显示
- [ ] AJAX请求正常工作
- [ ] 日期格式正常显示

## 性能影响

### 页面加载

- **影响**：微小
- **原因**：只添加了一个新页面，不影响其他页面
- **优化**：使用AJAX减少页面刷新

### 数据库查询

- **新增查询**：
  1. GetAdminById - 查询当前管理员信息
  2. UpdateAdmin - 更新管理员信息
- **优化措施**：
  - 使用主键查询（高效）
  - 只在需要时才查询数据库

### 内存占用

- **影响**：可忽略
- **原因**：只在Session中存储管理员对象

## 安全性分析

### 安全措施

1. **会话验证**
   - 所有操作都需要验证登录状态
   - 检查用户角色

2. **CSRF防护**
   - 使用 `[ValidateAntiForgeryToken]` 特性
   - 前端提交令牌

3. **密码安全**
   - 使用SHA256哈希
   - 不存储明文密码
   - 验证旧密码后才能修改

4. **SQL注入防护**
   - 使用参数化查询（继承自AdminBLL）

5. **XSS防护**
   - Razor自动HTML编码

### 安全检查结果

**CodeQL扫描：** ✅ 通过（0个告警）

## 已知限制

1. **构建环境**
   - 需要Windows环境和Visual Studio
   - 基于.NET Framework 4.8

2. **密码策略**
   - 当前只要求最小长度6个字符
   - 未强制要求复杂度（大小写、数字、特殊字符）

3. **密码重置**
   - 如果忘记密码，需要超级管理员手动重置
   - 未实现自助密码重置功能

## 后续改进建议

### 功能增强

1. **密码强度要求**
   - 要求包含大小写字母
   - 要求包含数字和特殊字符
   - 实时显示密码强度

2. **头像上传**
   - 允许管理员上传个人头像
   - 在导航栏显示头像

3. **登录历史**
   - 显示最近的登录记录
   - 包括登录时间、IP地址等

4. **安全设置**
   - 两步验证
   - 登录通知
   - 密码定期更换提醒

### 用户体验

1. **实时验证**
   - 输入时即时验证
   - 显示具体的错误位置

2. **密码可见性切换**
   - 添加"显示/隐藏密码"按钮

3. **表单自动保存**
   - 防止意外关闭页面导致数据丢失

4. **操作确认**
   - 修改密码时要求二次确认

## 总结

### 实现内容

✅ **已完成：**
1. 修复下拉导航选择问题
2. 重命名"账号管理"为"角色管理"
3. 新增账号自管理功能
4. 实现姓名修改功能
5. 实现密码修改功能
6. 添加完整的前端验证
7. 添加CSRF防护
8. 添加操作日志记录
9. 通过安全检查

### 代码统计

| 类型 | 文件数 | 代码行数 |
|-----|-------|---------|
| 修改的文件 | 3 | ~130行 |
| 新增的文件 | 1 | ~270行 |
| 总计 | 4 | ~400行 |

### 核心价值

1. **用户体验优化**
   - 解决了下拉导航的可用性问题
   - 提供了清晰的导航结构

2. **功能完善**
   - 管理员可以自主管理账号信息
   - 无需超级管理员协助

3. **安全性保障**
   - 完整的密码验证机制
   - CSRF防护
   - 通过CodeQL安全扫描

4. **代码质量**
   - 遵循现有代码风格
   - 完整的注释和文档
   - 易于维护和扩展

---

**实现日期**：2025-12-29  
**修改文件数**：4个  
**新增代码行数**：约400行  
**安全检查**：✅ 通过  
**测试状态**：待用户验证
