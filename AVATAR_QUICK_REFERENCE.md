# 用户头像功能 - 快速参考

## 快速开始

### 用户操作（3步）
1. 登录 → 进入个人中心
2. 点击头像或"修改头像"按钮
3. 选择上传图片或选择默认头像

### 开发者集成

#### 1. 在页面中显示用户头像
```csharp
@if (!string.IsNullOrEmpty(user.url))
{
    <img src="@user.url" alt="头像" style="width: 80px; height: 80px; border-radius: 50%; object-fit: cover;" />
}
else
{
    <i class="fas fa-user"></i>
}
```

#### 2. 调用上传API
```javascript
var formData = new FormData();
formData.append('avatarFile', file);
formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

$.ajax({
    url: '/Home/UploadAvatar',
    type: 'POST',
    data: formData,
    processData: false,
    contentType: false,
    success: function(response) {
        if (response.success) {
            // 更新头像显示
            updateAvatar(response.avatarUrl);
        }
    }
});
```

#### 3. 调用默认头像API
```javascript
$.ajax({
    url: '/Home/SetDefaultAvatar',
    type: 'POST',
    data: { 
        avatarName: 'avatar1.svg',
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        if (response.success) {
            updateAvatar(response.avatarUrl);
        }
    }
});
```

## API 端点

### POST /Home/UploadAvatar
**功能**: 上传自定义头像  
**参数**: 
- `avatarFile` (HttpPostedFileBase) - 图片文件
- `__RequestVerificationToken` - CSRF令牌

**返回**:
```json
{
    "success": true,
    "message": "头像上传成功",
    "avatarUrl": "/Uploads/Avatars/user_1_638364758123456789.jpg"
}
```

### POST /Home/SetDefaultAvatar
**功能**: 设置默认头像  
**参数**:
- `avatarName` (string) - 头像文件名 (avatar1.svg ~ avatar5.svg)
- `__RequestVerificationToken` - CSRF令牌

**返回**:
```json
{
    "success": true,
    "message": "默认头像设置成功",
    "avatarUrl": "/Uploads/Avatars/Default/avatar1.svg"
}
```

## 数据库字段

| 字段名 | 类型 | 说明 | 示例 |
|--------|------|------|------|
| url | nvarchar(50) | 头像URL路径 | /Uploads/Avatars/user_1_xxx.jpg |

## 文件路径

| 类型 | 路径 |
|------|------|
| 默认头像 | `/Uploads/Avatars/Default/avatar[1-5].svg` |
| 上传头像 | `/Uploads/Avatars/user_{userId}_{timestamp}.{ext}` |

## 文件限制

| 限制项 | 值 |
|--------|-----|
| 最大文件大小 | 5 MB |
| 支持格式 | JPG, JPEG, PNG, GIF, BMP |

## CSS 圆形头像

```css
.avatar {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid #fff;
}
```

## 默认头像颜色

| 文件 | 颜色 | 色值 |
|------|------|------|
| avatar1.svg | 紫蓝色 | #667eea |
| avatar2.svg | 紫色 | #764ba2 |
| avatar3.svg | 红色 | #ff6b6b |
| avatar4.svg | 绿色 | #51cf66 |
| avatar5.svg | 橙色 | #ffa94d |

## 常见问题

### Q: 如何获取当前用户的头像URL？
```csharp
string avatarUrl = _userBLL.GetUserAvatarUrl(userId);
```

### Q: 如何在其他页面显示用户头像？
从Session获取用户信息：
```csharp
var user = (Users)Session["LoginUser"];
string avatarUrl = user?.url ?? "/Uploads/Avatars/Default/avatar1.svg";
```

### Q: 如何验证上传的文件？
服务器端自动验证：
- 文件类型检查
- 文件大小检查
- 用户认证检查
- CSRF令牌验证

### Q: 如何删除旧头像？
系统不自动删除旧头像，如需要可以：
```csharp
// 获取旧头像路径
var oldAvatar = user.url;
if (!string.IsNullOrEmpty(oldAvatar) && !oldAvatar.Contains("/Default/"))
{
    var oldPath = Server.MapPath("~" + oldAvatar);
    if (System.IO.File.Exists(oldPath))
    {
        System.IO.File.Delete(oldPath);
    }
}
```

## 测试清单

- [ ] 上传JPG格式图片
- [ ] 上传PNG格式图片
- [ ] 上传超过5MB的图片（应失败）
- [ ] 上传不支持格式的文件（应失败）
- [ ] 未登录状态上传（应失败）
- [ ] 选择每个默认头像
- [ ] 头像在个人中心显示
- [ ] 头像在导航栏显示
- [ ] 更换头像后立即更新
- [ ] CSRF保护工作正常

## 故障排除

### 上传失败
1. 检查文件大小和格式
2. 检查用户登录状态
3. 检查CSRF令牌
4. 检查服务器权限

### 头像不显示
1. 检查文件路径是否正确
2. 检查文件是否存在
3. 检查URL是否正确
4. 清除浏览器缓存

### 默认头像不显示
1. 检查SVG文件是否存在于 `/Uploads/Avatars/Default/`
2. 检查路径拼写是否正确

## 性能建议

1. **定期清理** - 清理长期未使用的头像文件
2. **CDN** - 考虑使用CDN加速头像加载
3. **压缩** - 对上传的大图片进行压缩
4. **缓存** - 使用浏览器缓存头像

## 安全检查

✅ CSRF保护  
✅ 文件类型验证  
✅ 文件大小限制  
✅ 用户认证  
✅ 唯一文件命名  
✅ CodeQL扫描通过  

## 相关文档

- 📖 [用户使用指南](USER_AVATAR_FEATURE.md)
- 📋 [实现总结](AVATAR_IMPLEMENTATION_SUMMARY.md)
- 🎨 [视觉指南](AVATAR_FEATURE_VISUAL_GUIDE.md)

## 支持

如有问题，请查阅：
1. 代码注释
2. 相关文档
3. 提交Issue
