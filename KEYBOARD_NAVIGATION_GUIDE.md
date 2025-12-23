# 键盘导航功能说明 (Keyboard Navigation Feature)

## 概述 (Overview)

本系统已为所有登录和注册页面添加了键盘导航功能，用户可以使用键盘上的方向键快速在输入框之间切换，无需使用鼠标点击每个输入框。

This system now includes keyboard navigation functionality for all login and registration pages, allowing users to quickly switch between input fields using keyboard arrow keys without needing to click each input field with a mouse.

## 功能特性 (Features)

### 支持的页面 (Supported Pages)
- ✅ 用户登录页面 (User Login) - `/User/Login`
- ✅ 手机号登录页面 (Phone Login) - `/User/PhoneLogin`
- ✅ 邮箱登录页面 (Email Login) - `/User/EmailLogin`
- ✅ 工作人员登录页面 (Staff Login) - `/Staff/Login`
- ✅ 用户注册页面 (User Registration) - `/User/Register`
- ✅ 忘记密码页面 (Forgot Password) - `/User/Forgot`

### 支持的输入类型 (Supported Input Types)
- 文本输入框 (Text inputs)
- 密码输入框 (Password inputs)
- 邮箱输入框 (Email inputs)
- 电话号码输入框 (Tel inputs)
- 数字输入框 (Number inputs)
- 下拉选择框 (Select dropdowns)
- 文本域 (Textareas)

## 使用方法 (Usage)

### 键盘快捷键 (Keyboard Shortcuts)

| 按键 (Key) | 功能 (Function) |
|-----------|----------------|
| **↑ 向上箭头** | 移动到上一个输入框 (Move to previous input field) |
| **↓ 向下箭头** | 移动到下一个输入框 (Move to next input field) |
| **Enter 回车键** | 移动到下一个输入框，如果是最后一个输入框则提交表单 (Move to next input field, submit form if on last field) |
| **Tab 键** | 标准Tab导航依然可用 (Standard Tab navigation still available) |

### 特性说明 (Features Description)

1. **自动聚焦 (Auto Focus)**
   - 页面加载后自动聚焦到第一个可用的输入框
   - Automatically focuses on the first available input field after page load

2. **智能跳过 (Smart Skip)**
   - 自动跳过禁用 (disabled) 和隐藏 (hidden) 的输入框
   - Automatically skips disabled and hidden input fields

3. **文本选中 (Text Selection)**
   - 切换到新输入框时自动选中其中的文本，方便快速替换
   - Automatically selects text in the new input field for quick replacement

4. **循环导航 (Circular Navigation)**
   - 在最后一个输入框按向下键会回到第一个输入框
   - Pressing down on the last input wraps to the first input
   - 在第一个输入框按向上键会跳到最后一个输入框
   - Pressing up on the first input wraps to the last input

## 技术实现 (Technical Implementation)

### 核心文件 (Core Files)
- `Scripts/keyboard-navigation.js` - 键盘导航核心工具库

### 集成方式 (Integration Method)

在需要键盘导航的页面中添加以下代码：

```html
<!-- 引入键盘导航脚本 -->
<script src="~/Scripts/keyboard-navigation.js"></script>

<script>
    // 在页面加载后初始化
    document.addEventListener('DOMContentLoaded', function() {
        // 初始化键盘导航，指定容器选择器（可选）
        KeyboardNavigation.init('.container-class');
    });
</script>
```

### API 说明 (API Documentation)

#### `KeyboardNavigation.init(containerSelector)`

初始化键盘导航功能。

**参数 (Parameters):**
- `containerSelector` (String, 可选): 容器选择器，用于限制导航范围。如果不提供，将在整个文档中查找输入框。

**示例 (Example):**
```javascript
// 在特定容器中初始化
KeyboardNavigation.init('.login-container');

// 在整个文档中初始化
KeyboardNavigation.init();
```

## 浏览器兼容性 (Browser Compatibility)

- ✅ Chrome/Edge (现代版本)
- ✅ Firefox (现代版本)
- ✅ Safari (现代版本)
- ✅ Opera (现代版本)
- ⚠️ IE11 (基本支持，某些功能可能受限)

## 注意事项 (Notes)

1. 键盘导航不会影响原有的Tab键导航功能
2. 对于文本域 (textarea)，Enter键仍然用于换行，不会触发导航
3. 键盘导航会自动跳过不可见和禁用的输入框
4. 该功能对屏幕阅读器友好，不会影响可访问性

## 测试验证 (Testing)

已创建独立的测试页面用于验证键盘导航功能：
- 测试页面位置: `/tmp/keyboard-navigation-test.html`
- 测试结果: ✅ 所有功能正常工作

### 测试截图 (Test Screenshots)

1. **初始加载** - 自动聚焦到第一个输入框
   ![Initial State](https://github.com/user-attachments/assets/d73b6b74-86fd-476b-a594-274100322b95)

2. **按下向下箭头键** - 移动到邮箱输入框
   ![After Down Arrow](https://github.com/user-attachments/assets/046ad97d-dc81-4dc6-86e0-06b9b4b2b763)

3. **继续导航** - 移动到国家选择框
   ![Navigation to Country](https://github.com/user-attachments/assets/b46d806e-6602-49df-b14b-d7f16339f787)

## 维护和扩展 (Maintenance and Extension)

### 添加到新页面 (Adding to New Pages)

如需在新页面中添加键盘导航功能：

1. 引入脚本文件：
   ```html
   <script src="~/Scripts/keyboard-navigation.js"></script>
   ```

2. 在页面加载时初始化：
   ```javascript
   document.addEventListener('DOMContentLoaded', function() {
       KeyboardNavigation.init('.your-container-class');
   });
   ```

### 自定义配置 (Custom Configuration)

如需修改支持的输入框类型，可编辑 `keyboard-navigation.js` 中的 `inputSelector` 属性。

## 反馈和问题 (Feedback and Issues)

如遇到任何问题或有改进建议，请联系开发团队。

---

**版本 (Version):** 1.0.0  
**最后更新 (Last Updated):** 2024-12-23  
**作者 (Author):** Development Team
