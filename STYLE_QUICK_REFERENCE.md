# 统一样式系统快速参考

## 快速开始

### 1. 自动加载
所有CSS文件通过BundleConfig自动加载，无需手动引用：
```csharp
@Styles.Render("~/Content/css")
```

### 2. 主要文件
- `unified-style.css` - 全局统一样式
- `management-common.css` - 管理页面样式
- `login.css`, `register.css`, `forgot.css` - 认证页面样式

## 常用组件速查

### 按钮
```html
<!-- 主按钮 (紫色渐变) -->
<button class="unified-btn unified-btn-primary">主要操作</button>

<!-- 成功按钮 (绿色) -->
<button class="unified-btn unified-btn-success">保存</button>

<!-- 危险按钮 (红色) -->
<button class="unified-btn unified-btn-danger">删除</button>

<!-- 描边按钮 -->
<button class="unified-btn unified-btn-outline">次要操作</button>

<!-- 全宽按钮 -->
<button class="unified-btn unified-btn-primary unified-btn-full">登录</button>
```

### 卡片
```html
<!-- 基础卡片 -->
<div class="unified-card">
    <h3>标题</h3>
    <p>内容</p>
</div>

<!-- 带头部的卡片 -->
<div class="unified-card">
    <div class="unified-card-header">
        <!-- 紫色渐变头部 -->
    </div>
    <div class="unified-card-body">
        <!-- 内容 -->
    </div>
</div>
```

### 表单
```html
<div class="unified-form-group">
    <label class="unified-form-label">用户名</label>
    <input type="text" class="unified-form-control" placeholder="请输入用户名">
</div>

<div class="unified-form-group">
    <label class="unified-form-label">备注</label>
    <textarea class="unified-form-control"></textarea>
</div>
```

### 容器
```html
<!-- 大容器 (max-width: 1200px) -->
<div class="unified-container">
    ...
</div>

<!-- 小容器 (max-width: 800px) -->
<div class="unified-container-small">
    ...
</div>

<!-- 表单容器 (max-width: 450px) -->
<div class="unified-container-form">
    ...
</div>
```

### 模态框
```html
<div id="myModal" class="unified-modal">
    <div class="unified-modal-content">
        <div class="unified-modal-header">
            <h3 class="unified-modal-title">模态框标题</h3>
            <button class="unified-modal-close" onclick="closeModal()">&times;</button>
        </div>
        <div>
            <!-- 内容 -->
        </div>
    </div>
</div>

<script>
function showModal() {
    document.getElementById('myModal').classList.add('show');
}
function closeModal() {
    document.getElementById('myModal').classList.remove('show');
}
</script>
```

### 提示框
```html
<!-- 成功提示 -->
<div class="unified-alert unified-alert-success">
    <i class="fas fa-check-circle"></i> 操作成功！
</div>

<!-- 错误提示 -->
<div class="unified-alert unified-alert-danger">
    <i class="fas fa-exclamation-circle"></i> 操作失败！
</div>

<!-- 警告提示 -->
<div class="unified-alert unified-alert-warning">
    <i class="fas fa-exclamation-triangle"></i> 请注意！
</div>

<!-- 信息提示 -->
<div class="unified-alert unified-alert-info">
    <i class="fas fa-info-circle"></i> 提示信息
</div>
```

### 徽章
```html
<span class="unified-badge unified-badge-primary">新</span>
<span class="unified-badge unified-badge-success">已完成</span>
<span class="unified-badge unified-badge-danger">已关闭</span>
<span class="unified-badge unified-badge-warning">待处理</span>
```

## 管理页面组件

### 页面布局
```html
<div class="management-container">
    <!-- 头部 -->
    <div class="management-header">
        <h1 class="management-title">页面标题</h1>
        <p class="management-subtitle">页面描述</p>
    </div>
    
    <!-- 主要内容卡片 -->
    <div class="management-card">
        <!-- 内容 -->
    </div>
</div>
```

### 筛选区域
```html
<div class="filter-section">
    <div class="filter-row">
        <div class="filter-column">
            <label class="filter-label">搜索关键词</label>
            <input type="text" class="filter-input" placeholder="请输入">
        </div>
        <div class="filter-column">
            <label class="filter-label">状态</label>
            <select class="filter-input">
                <option>全部</option>
            </select>
        </div>
        <div class="filter-button-group">
            <button class="filter-btn filter-btn-primary">
                <i class="fas fa-search"></i> 搜索
            </button>
            <button class="filter-btn filter-btn-secondary">
                <i class="fas fa-redo"></i> 重置
            </button>
        </div>
    </div>
</div>
```

### 数据表格
```html
<div class="data-table-container">
    <table class="data-table">
        <thead>
            <tr>
                <th>ID</th>
                <th>名称</th>
                <th>状态</th>
                <th>操作</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>1</td>
                <td>示例数据</td>
                <td><span class="status-badge status-active">活跃</span></td>
                <td>
                    <button class="table-action-btn table-action-btn-view">
                        <i class="fas fa-eye"></i> 查看
                    </button>
                    <button class="table-action-btn table-action-btn-edit">
                        <i class="fas fa-edit"></i> 编辑
                    </button>
                    <button class="table-action-btn table-action-btn-delete">
                        <i class="fas fa-trash"></i> 删除
                    </button>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

### 状态标签
```html
<span class="status-badge status-active">活跃</span>
<span class="status-badge status-inactive">停用</span>
<span class="status-badge status-pending">待处理</span>
<span class="status-badge status-approved">已审批</span>
<span class="status-badge status-completed">已完成</span>
<span class="status-badge status-cancelled">已取消</span>
```

### 统计卡片
```html
<div class="stats-grid">
    <div class="stat-card">
        <div class="stat-card-icon">
            <i class="fas fa-users"></i>
        </div>
        <div class="stat-card-value">1,234</div>
        <div class="stat-card-label">总用户数</div>
    </div>
    
    <div class="stat-card">
        <div class="stat-card-icon">
            <i class="fas fa-box"></i>
        </div>
        <div class="stat-card-value">567</div>
        <div class="stat-card-label">总订单数</div>
    </div>
</div>
```

## 布局工具类

### 间距
```html
<!-- Margin Top -->
<div class="unified-mt-1">5px</div>
<div class="unified-mt-2">10px</div>
<div class="unified-mt-3">15px</div>
<div class="unified-mt-4">20px</div>
<div class="unified-mt-5">30px</div>

<!-- Margin Bottom -->
<div class="unified-mb-1">5px</div>
<div class="unified-mb-2">10px</div>
<div class="unified-mb-3">15px</div>
<div class="unified-mb-4">20px</div>
<div class="unified-mb-5">30px</div>

<!-- Padding -->
<div class="unified-p-1">5px</div>
<div class="unified-p-2">10px</div>
<div class="unified-p-3">15px</div>
<div class="unified-p-4">20px</div>
<div class="unified-p-5">30px</div>
```

### Flexbox
```html
<!-- 基础Flex -->
<div class="unified-flex">
    <div>项目1</div>
    <div>项目2</div>
</div>

<!-- 居中对齐 -->
<div class="unified-flex-center">
    <div>居中内容</div>
</div>

<!-- 两端对齐 -->
<div class="unified-flex-between">
    <div>左侧</div>
    <div>右侧</div>
</div>

<!-- 换行 -->
<div class="unified-flex unified-flex-wrap">
    <div>项目1</div>
    <div>项目2</div>
</div>

<!-- 间距 -->
<div class="unified-flex unified-gap-3">
    <div>项目1</div>
    <div>项目2</div>
</div>
```

### 网格
```html
<!-- 2列网格 -->
<div class="unified-grid unified-grid-2">
    <div>列1</div>
    <div>列2</div>
</div>

<!-- 3列网格 -->
<div class="unified-grid unified-grid-3">
    <div>列1</div>
    <div>列2</div>
    <div>列3</div>
</div>

<!-- 4列网格 -->
<div class="unified-grid unified-grid-4">
    <div>列1</div>
    <div>列2</div>
    <div>列3</div>
    <div>列4</div>
</div>
```

### 文本对齐
```html
<div class="unified-text-center">居中文本</div>
<div class="unified-text-left">左对齐文本</div>
<div class="unified-text-right">右对齐文本</div>
```

## 颜色变量

在CSS中使用：
```css
.my-element {
    background: var(--primary-gradient);
    color: var(--text-primary);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-md);
}
```

可用变量：
```css
/* 颜色 */
--primary-gradient
--primary-color
--primary-dark
--success-color
--danger-color
--warning-color
--info-color

/* 文字颜色 */
--text-primary
--text-secondary
--text-muted

/* 背景颜色 */
--bg-light
--bg-white
--bg-gray

/* 边框颜色 */
--border-color
--border-light

/* 阴影 */
--shadow-sm
--shadow-md
--shadow-lg

/* 圆角 */
--radius-sm
--radius-md
--radius-lg
--radius-xl

/* 间距 */
--spacing-xs
--spacing-sm
--spacing-md
--spacing-lg
--spacing-xl
--spacing-xxl
```

## 响应式断点

```css
/* 平板 */
@media (max-width: 992px) {
    /* 样式 */
}

/* 移动端 */
@media (max-width: 768px) {
    /* 样式 */
}

/* 小屏手机 */
@media (max-width: 480px) {
    /* 样式 */
}
```

## 最佳实践

### ✅ 推荐
```html
<!-- 使用统一的类名 -->
<button class="unified-btn unified-btn-primary">提交</button>

<!-- 使用工具类 -->
<div class="unified-mb-3 unified-p-4">内容</div>

<!-- 使用预定义组件 -->
<div class="unified-card">卡片内容</div>
```

### ❌ 避免
```html
<!-- 避免内联样式 -->
<button style="background: purple; padding: 10px;">提交</button>

<!-- 避免自定义样式 -->
<button class="my-custom-btn">提交</button>

<!-- 避免不一致的值 -->
<div style="margin-bottom: 17px; padding: 23px;">内容</div>
```

## 常见问题

### Q: 如何覆盖统一样式？
A: 在页面的 `<style>` 标签中使用更高的特异性：
```css
.my-page .unified-btn-primary {
    background: linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%);
}
```

### Q: 表格在移动端显示不全？
A: 使用 `.data-table-container` 包裹表格，它会自动添加横向滚动。

### Q: 如何创建自定义颜色的按钮？
A: 使用基础类并添加自定义背景：
```html
<button class="unified-btn" style="background: #your-color;">按钮</button>
```

### Q: 网格在移动端如何显示？
A: 统一网格会在移动端自动变为单列布局。

## 迁移指南

### 从旧样式迁移
```html
<!-- 旧样式 -->
<button class="btn btn-success" style="height: 45px; margin-right: 10px;">
    保存
</button>

<!-- 新样式 -->
<button class="unified-btn unified-btn-success">
    保存
</button>
```

```html
<!-- 旧样式 -->
<div style="max-width: 1200px; margin: 0 auto; padding: 20px;">
    内容
</div>

<!-- 新样式 -->
<div class="unified-container">
    内容
</div>
```

---

**提示**: 开发新功能时，优先查阅本快速参考，使用统一的组件和工具类，保持视觉风格的一致性。
