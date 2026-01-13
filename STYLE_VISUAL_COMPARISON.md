# 样式统一化视觉对比指南

## 1. 颜色方案统一

### 之前 (不统一)
```
登录按钮:    绿色 #5cb85c
注册按钮:    绿色 #5cb85c  
主页导航:    紫色渐变 #667eea → #764ba2
部分按钮:    蓝色 #337ab7
```

### 之后 (统一)
```
所有主要按钮:  紫色渐变 linear-gradient(135deg, #667eea 0%, #764ba2 100%)
成功操作:      绿色 #5cb85c
危险操作:      红色 #d9534f
次要按钮:      灰色 #6c757d
```

## 2. 按钮样式统一

### 之前
```css
/* 登录页 */
.btn-login {
    height: 45px;
    background-color: #5cb85c;  /* 绿色 */
    border-radius: 4px;
}

/* 其他页面 */
.btn-primary {
    height: 40px;
    background: #667eea;  /* 纯色 */
    border-radius: 8px;
}
```

### 之后
```css
/* 统一样式 */
.unified-btn-primary {
    height: 45px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 8px;
    transition: all 0.3s ease;
}

.unified-btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 5px 15px rgba(102, 126, 234, 0.3);
}
```

## 3. 卡片样式统一

### 之前
```css
/* 页面A */
.card {
    border-radius: 10px;
    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
}

/* 页面B */
.card {
    border-radius: 15px;
    box-shadow: 0 3px 10px rgba(0,0,0,0.05);
}

/* 页面C - 使用内联样式 */
<div style="border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.15);">
```

### 之后
```css
/* 统一样式 */
.unified-card {
    background: white;
    border-radius: 15px;
    padding: 30px;
    box-shadow: 0 5px 20px rgba(0,0,0,0.08);
}
```

## 4. 表单样式统一

### 之前
```html
<!-- 不同页面有不同样式 -->
<input style="height: 40px; border: 1px solid #ccc;">
<input class="form-control" style="height: 45px; border: 2px solid #ddd;">
<input style="height: 50px; border-radius: 10px;">
```

### 之后
```html
<!-- 统一使用 -->
<input class="unified-form-control">
```

```css
.unified-form-control {
    height: 45px;
    border-radius: 4px;
    border: 1px solid #ddd;
    padding: 10px 15px;
    font-size: 16px;
}

.unified-form-control:focus {
    border-color: #667eea;
    box-shadow: 0 0 8px rgba(102, 126, 234, 0.3);
}
```

## 5. 模态框样式统一

### 之前
```html
<!-- Profile.cshtml -->
<div id="avatarModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5);">
    <div style="background: white; padding: 30px; border-radius: 15px;">
        <div style="display: flex; justify-content: space-between;">
            <h3 style="margin: 0;">头像管理</h3>
            <button style="background: none; border: none;">×</button>
        </div>
    </div>
</div>
```

### 之后
```html
<!-- 使用统一类 -->
<div id="avatarModal" class="unified-modal">
    <div class="unified-modal-content">
        <div class="unified-modal-header">
            <h3 class="unified-modal-title">头像管理</h3>
            <button class="unified-modal-close">×</button>
        </div>
    </div>
</div>
```

## 6. 表格样式统一 (管理页面)

### 之前
```html
<!-- 不同的管理页面使用不同的表格样式 -->
<table class="table" style="background: white;">
<table style="border: 1px solid #ddd; border-radius: 5px;">
<table class="custom-table">
```

### 之后
```html
<!-- 统一使用 -->
<table class="data-table">
    <thead>
        <tr>
            <th>列名</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>数据</td>
        </tr>
    </tbody>
</table>
```

```css
.data-table {
    width: 100%;
    border-collapse: collapse;
    background: white;
    border-radius: 10px;
}

.data-table thead {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
}

.data-table tbody tr:hover {
    background-color: #f8f9fa;
}
```

## 7. 状态标签统一

### 之前
```html
<!-- 各种不同的状态显示方式 -->
<span style="color: green;">已完成</span>
<span class="badge badge-success">活跃</span>
<span style="background: #ffc107; color: white; padding: 3px 8px;">待处理</span>
```

### 之后
```html
<!-- 统一的状态标签 -->
<span class="status-badge status-completed">已完成</span>
<span class="status-badge status-active">活跃</span>
<span class="status-badge status-pending">待处理</span>
```

```css
.status-badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 600;
}

.status-completed {
    background: #d4edda;
    color: #155724;
}
```

## 8. 响应式设计统一

### 之前
```css
/* 各个页面有不同的断点 */
@media (max-width: 600px) { }
@media (max-width: 768px) { }
@media (max-width: 800px) { }
```

### 之后
```css
/* 统一的断点 */
@media (max-width: 992px) { /* 平板 */ }
@media (max-width: 768px) { /* 移动端 */ }
@media (max-width: 480px) { /* 小屏手机 */ }
```

## 9. 动画效果统一

### 之前
```css
/* 各种不同的过渡效果 */
transition: 0.2s;
transition: all 0.5s ease-in-out;
transition: transform 0.3s;
```

### 之后
```css
/* 统一的过渡效果 */
.unified-transition {
    transition: all 0.3s ease;
}

/* 统一的悬停效果 */
.unified-btn:hover {
    transform: translateY(-2px);
}
```

## 10. 间距标准化

### 之前
```html
<!-- 各种不同的间距值 -->
<div style="margin-bottom: 12px;">
<div style="padding: 18px;">
<div style="margin-top: 25px;">
```

### 之后
```html
<!-- 使用标准化的工具类 -->
<div class="unified-mb-2">  <!-- 10px -->
<div class="unified-p-3">   <!-- 15px -->
<div class="unified-mt-4">  <!-- 20px -->
```

```css
/* 标准间距值 */
--spacing-xs: 5px;   /* unified-*-1 */
--spacing-sm: 10px;  /* unified-*-2 */
--spacing-md: 15px;  /* unified-*-3 */
--spacing-lg: 20px;  /* unified-*-4 */
--spacing-xl: 30px;  /* unified-*-5 */
```

## 使用示例

### 创建统一的登录表单
```html
<div class="unified-container-form">
    <h2 class="unified-title">用户登录</h2>
    
    <form>
        <div class="unified-form-group">
            <label class="unified-form-label">用户名</label>
            <input type="text" class="unified-form-control">
        </div>
        
        <div class="unified-btn-group">
            <button type="submit" class="unified-btn unified-btn-primary">登录</button>
            <button type="button" class="unified-btn unified-btn-danger">取消</button>
        </div>
    </form>
</div>
```

### 创建统一的管理页面
```html
<div class="management-container">
    <div class="management-header">
        <h1 class="management-title">用户管理</h1>
        <p class="management-subtitle">管理系统用户账号</p>
    </div>
    
    <div class="management-card">
        <div class="filter-section">
            <div class="filter-row">
                <div class="filter-column">
                    <label class="filter-label">搜索</label>
                    <input type="text" class="filter-input">
                </div>
                <div class="filter-button-group">
                    <button class="filter-btn filter-btn-primary">搜索</button>
                    <button class="filter-btn filter-btn-secondary">重置</button>
                </div>
            </div>
        </div>
        
        <table class="data-table">
            <thead>
                <tr>
                    <th>用户名</th>
                    <th>状态</th>
                    <th>操作</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>张三</td>
                    <td><span class="status-badge status-active">活跃</span></td>
                    <td>
                        <button class="table-action-btn table-action-btn-edit">编辑</button>
                        <button class="table-action-btn table-action-btn-delete">删除</button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</div>
```

## 主要改进点

### ✅ 视觉统一
- 所有按钮使用相同的紫色渐变主题
- 卡片和容器圆角、阴影统一
- 表格样式在所有管理页面保持一致

### ✅ 代码简洁
- 用CSS类替代内联样式
- 减少重复代码
- 提高可维护性

### ✅ 用户体验
- 统一的动画和过渡效果
- 一致的响应式行为
- 清晰的视觉层次

### ✅ 开发效率
- 预定义的组件样式库
- 丰富的工具类
- 清晰的命名规范

---

**建议**: 新页面开发时优先使用统一样式系统中的类，避免使用内联样式或创建新的样式定义。
