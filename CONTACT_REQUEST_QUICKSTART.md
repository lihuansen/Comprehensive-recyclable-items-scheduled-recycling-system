# Contact Request Feature - Quick Start Guide

## Overview

The redesigned "Contact Us" feature now uses a request-based system where users submit contact requests and administrators initiate conversations.

## Key Changes

### Before
- Users clicking "Contact Us" would automatically start a chat session
- History viewing feature was available
- Complex interaction flow

### After
- Users clicking "Contact Us" creates a contact request (status = true)
- Administrators see pending requests and choose who to contact
- Simplified user experience with no history feature
- System message "管理在线客服" appears centered when chat starts

## Quick Setup

### 1. Database Setup

Run the SQL script:
```bash
Database/CreateUserContactRequestsTable.sql
```

This creates the `UserContactRequests` table with:
- RequestID (Primary Key)
- UserID (Foreign Key to Users)
- RequestStatus (true = pending, false = handled)
- RequestTime
- ContactedTime
- AdminID

### 2. Build and Deploy

1. Open solution in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Deploy to server

### 3. Test the Feature

**User Side:**
1. Log in as a user
2. Go to Feedback page (`/Home/Feedback`)
3. Click "联系我们" (Contact Us)
4. See confirmation message
5. Wait for admin to initiate chat

**Admin Side:**
1. Log in as admin
2. Go to User Contact Management (`/Staff/UserContactManagement`)
3. Click "待联系" (Pending) tab
4. See list of users who requested contact
5. Click on a user to start chatting

## Architecture

```
User clicks "Contact Us"
    ↓
Record created in UserContactRequests (status = true)
    ↓
Admin sees request in pending list
    ↓
Admin clicks to start chat
    ↓
Request marked as handled (status = false)
Conversation created
System message sent: "管理在线客服"
    ↓
Chat active - both can send messages
```

## Files Added/Modified

### New Files
- `Database/CreateUserContactRequestsTable.sql`
- `recycling.Model/UserContactRequests.cs`
- `recycling.Model/UserContactRequestViewModel.cs`
- `recycling.DAL/UserContactRequestsDAL.cs`
- `recycling.BLL/UserContactRequestsBLL.cs`

### Modified Files
- `recycling.Web.UI/Controllers/HomeController.cs`
- `recycling.Web.UI/Controllers/StaffController.cs`
- `recycling.Web.UI/Views/Home/ContactAdmin.cshtml`
- `recycling.Web.UI/Views/Staff/UserContactManagement.cshtml`

## API Endpoints

### User Side

**GET** `/Home/ContactAdmin`
- Creates contact request
- Shows confirmation message
- Returns view

### Admin Side

**POST** `/Staff/GetPendingContactRequests`
- Returns list of pending contact requests
- Requires CSRF token
- Admin authentication required

**POST** `/Staff/StartContactWithUser`
- Parameters: `requestId`, `userId`
- Marks request as handled
- Creates conversation
- Sends system welcome message
- Requires CSRF token
- Admin authentication required

## Security Features

### CSRF Protection
All POST endpoints use `[ValidateAntiForgeryToken]` attribute.

Views include CSRF token:
```html
@Html.AntiForgeryToken()
```

JavaScript includes token in requests:
```javascript
formData.append('__RequestVerificationToken', getAntiForgeryToken());
```

### SQL Injection Prevention
All database operations use parameterized queries:
```csharp
cmd.Parameters.AddWithValue("@UserID", userId);
```

### Authentication
All endpoints check session state:
- User endpoints check `Session["LoginUser"]`
- Admin endpoints check `Session["LoginStaff"]`

## Database Schema

```sql
UserContactRequests
├── RequestID (INT, PK, IDENTITY)
├── UserID (INT, FK → Users.UserID)
├── RequestStatus (BIT, DEFAULT 1)
├── RequestTime (DATETIME2, DEFAULT GETDATE())
├── ContactedTime (DATETIME2, NULL)
└── AdminID (INT, NULL)

Indexes:
- IX_UserContactRequests_UserID
- IX_UserContactRequests_RequestStatus
- IX_UserContactRequests_RequestTime
```

## Troubleshooting

### Issue: Admin can't see pending requests

**Check:**
```sql
SELECT * FROM UserContactRequests WHERE RequestStatus = 1
```

If no results, no users have clicked "Contact Us" yet.

### Issue: CSRF validation fails

**Solution:**
1. Clear browser cache
2. Verify token is in view: `@Html.AntiForgeryToken()`
3. Check JavaScript includes token in POST

### Issue: Duplicate requests

**Expected behavior:** System prevents duplicate pending requests for same user.

## Maintenance

### View pending requests
```sql
SELECT u.Username, r.RequestTime 
FROM UserContactRequests r
JOIN Users u ON r.UserID = u.UserID
WHERE r.RequestStatus = 1
ORDER BY r.RequestTime DESC
```

### Clean old data (optional)
```sql
DELETE FROM UserContactRequests 
WHERE RequestStatus = 0 
  AND DATEDIFF(MONTH, ContactedTime, GETDATE()) > 6
```

## Configuration

### Database Connection (Web.config)
```xml
<connectionStrings>
  <add name="RecyclingDB" 
       connectionString="Data Source=SERVER;Initial Catalog=RecyclingDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Session Timeout (Web.config)
```xml
<system.web>
  <sessionState timeout="30" />
</system.web>
```

## User Interface

### User View (ContactAdmin.cshtml)
- Shows confirmation message after clicking "Contact Us"
- Chat interface hidden by default
- Activates when admin initiates conversation
- System message "管理在线客服" appears centered
- No history or conversation list

### Admin View (UserContactManagement.cshtml)
- Three tabs: "待联系" (Pending), "进行中" (Active), "已结束" (Ended)
- Default tab shows pending requests
- Each request shows:
  - User name
  - Phone number
  - Request time
  - Orange badge: "待联系"
- Click request to start conversation
- Auto-switches to chat interface

## Flow Diagrams

### User Flow
```
Login → Feedback Page → Click "Contact Us"
  → Request Recorded → Confirmation Shown
  → Wait for Admin → Chat Activated → Send Messages
```

### Admin Flow
```
Login → User Contact Management → "待联系" Tab
  → See Pending Requests → Click User
  → Request Marked Handled → Conversation Created
  → System Message Sent → Start Chatting
```

## Testing Checklist

- [ ] User can click "Contact Us"
- [ ] Confirmation message displays
- [ ] Request recorded in database
- [ ] Admin sees request in pending list
- [ ] Admin can click to start chat
- [ ] Request marked as handled
- [ ] Conversation created
- [ ] System message appears
- [ ] Both can send messages
- [ ] Messages display correctly
- [ ] No duplicate pending requests for same user

## Support

For detailed documentation, see:
- `CONTACT_REQUEST_IMPLEMENTATION.md` - Full technical documentation
- `DATABASE_SCHEMA.md` - Database design details
- `SECURITY_SUMMARY.md` - Security features

---

**Version:** 1.0  
**Last Updated:** 2024
