# Hướng Dẫn Triển Khai Hệ Thống Chat với SignalR

## Tổng Quan

Hệ thống chat real-time giữa Customer và Staff sử dụng SignalR để gửi/nhận tin nhắn tức thời.

## Cấu Trúc Files

### Backend (API)

```
Capstone_Project/
├── Application/
│   └── ChatManagement/
│       ├── Controllers/
│       │   └── ChatController.cs          ✅ API endpoints
│       ├── DTOs/
│       │   ├── ChatDTO.cs                 ✅ DTO cho message
│       │   ├── CreateChatDTO.cs           ✅ DTO để tạo message
│       │   └── ChatConversationDTO.cs     ✅ DTO cho conversation
│       ├── Hubs/
│       │   └── ChatHub.cs                 ✅ SignalR Hub
│       └── Services/
│           ├── Interfaces/
│           │   └── IChatService.cs        ✅ Interface
│           └── ChatService.cs             ✅ Business logic
├── DAO/
│   └── ChatDAO.cs                         ✅ Data access (đã có, đã cập nhật)
└── Program.cs                              ✅ Đã cập nhật SignalR config
```

### Frontend (MVC)

```
FE_Capstone_Project/
├── Controllers/
│   └── ChatWebController.cs               ✅ MVC Controller
├── Models/
│   └── ChatModels.cs                      ✅ ViewModels
├── Views/
│   └── Chat/
│       ├── Index.cshtml                   ✅ Danh sách conversations (Customer)
│       ├── Conversation.cshtml            ✅ Chat với staff (Customer)
│       ├── Manage.cshtml                  ✅ Quản lý chat (Staff)
│       └── StaffConversation.cshtml       ✅ Chat với customer (Staff)
├── wwwroot/
│   ├── js/
│   │   └── chat.js                        ✅ SignalR client
│   └── css/
│       └── chat.css                       ✅ Chat styles
```

## Cài Đặt

### 1. Backend Packages

```bash
cd Capstone_Project
dotnet add package Microsoft.AspNetCore.SignalR
```

### 2. Frontend

SignalR JavaScript client được load từ CDN trong Views (không cần cài thêm).

## Cấu Hình

### Backend (Program.cs)

Đã được cấu hình:
- ✅ SignalR service registration
- ✅ JWT authentication cho SignalR
- ✅ CORS với credentials
- ✅ ChatHub mapping

### Frontend

Đã được cấu hình:
- ✅ JWT token injection trong Views
- ✅ SignalR connection với JWT
- ✅ API helper với authentication

## API Endpoints

### 1. Gửi Tin Nhắn
```
POST /api/chat/send
Authorization: Bearer {token}
Body: {
    "customerId": int,
    "staffId": int,
    "message": string,
    "chatType": int (1 = Support),
    "senderId": int
}
```

### 2. Lấy Lịch Sử Chat
```
GET /api/chat/conversation/{customerId}/{staffId}
Authorization: Bearer {token}
```

### 3. Lấy Danh Sách Conversations
```
GET /api/chat/my-conversations
Authorization: Bearer {token}
```

### 4. Lấy Tất Cả Conversations (Staff/Admin)
```
GET /api/chat/conversations
Authorization: Bearer {token}
Roles: Admin, Staff
```

### 5. Số Tin Nhắn Chưa Đọc
```
GET /api/chat/unread-count
Authorization: Bearer {token}
```

## SignalR Hub Methods

### Client → Server

Không có method nào cần gọi trực tiếp từ client. Tất cả đều qua API.

### Server → Client

1. **ReceiveMessage**: Nhận tin nhắn mới
   ```javascript
   connection.on("ReceiveMessage", (message) => {
       // message: { id, staffId, customerId, message, sentDate, senderId }
   });
   ```

2. **NewMessageNotification**: Thông báo tin nhắn mới
   ```javascript
   connection.on("NewMessageNotification", (notification) => {
       // notification: { senderName, preview, timestamp }
   });
   ```

## Routes

### Customer
- `/ChatWeb/Index` - Danh sách conversations
- `/ChatWeb/Conversation/{staffId}` - Chat với staff

### Staff
- `/ChatWeb/Manage` - Quản lý tất cả conversations
- `/ChatWeb/Manage/Conversation/{customerId}` - Chat với customer

## Phân Quyền

- **Customer (RoleId = 3)**:
  - Chỉ xem được conversations của mình
  - Chỉ gửi tin nhắn đến staff đã từng chat

- **Staff (RoleId = 2)**:
  - Xem được tất cả conversations
  - Có thể chat với bất kỳ customer nào

- **Admin (RoleId = 1)**:
  - Xem được tất cả conversations (nếu cần)

## Testing

### 1. Test API với Postman

1. Login để lấy JWT token
2. Gửi tin nhắn:
   ```
   POST https://localhost:7160/api/chat/send
   Headers: Authorization: Bearer {token}
   Body: {
       "customerId": 3,
       "staffId": 2,
       "message": "Hello!",
       "chatType": 1,
       "senderId": 3
   }
   ```

3. Lấy conversations:
   ```
   GET https://localhost:7160/api/chat/my-conversations
   Headers: Authorization: Bearer {token}
   ```

### 2. Test Frontend

1. **Customer:**
   - Login với tài khoản Customer
   - Truy cập `/ChatWeb/Index`
   - Click vào một conversation hoặc tạo mới
   - Gửi tin nhắn

2. **Staff:**
   - Login với tài khoản Staff
   - Truy cập `/ChatWeb/Manage`
   - Click vào customer để chat
   - Gửi tin nhắn

3. **Test Real-time:**
   - Mở 2 browser windows
   - Một window với Customer, một với Staff
   - Gửi tin nhắn từ một bên → Tin nhắn hiển thị ngay ở bên kia

## Troubleshooting

### 1. SignalR không kết nối

**Kiểm tra:**
- JWT token có hợp lệ không
- CORS đã được cấu hình đúng chưa
- SignalR Hub đã được map chưa (`/chathub`)

**Log:**
- Mở Browser Console để xem lỗi
- Kiểm tra Network tab → Xem WebSocket connection

### 2. Tin nhắn không hiển thị real-time

**Kiểm tra:**
- SignalR connection đã established chưa
- `ReceiveMessage` event đã được subscribe chưa
- Message có đúng format không

### 3. 401 Unauthorized

**Kiểm tra:**
- JWT token có trong session không
- Token có được inject vào View không
- Token có hết hạn không

### 4. CORS Error

**Kiểm tra:**
- `AllowCredentials()` đã được set chưa
- Origins đã được cấu hình đúng chưa
- SignalR URL có đúng không

## Tính Năng Nâng Cao (Có Thể Thêm)

1. **Typing Indicator**
   - Gửi event khi user đang gõ
   - Hiển thị "User is typing..."

2. **Read Receipts**
   - Đánh dấu tin nhắn đã đọc
   - Hiển thị "Seen" hoặc "Read"

3. **File Upload**
   - Gửi file/ảnh trong chat
   - Lưu file vào server

4. **Search Messages**
   - Tìm kiếm trong lịch sử chat
   - Filter theo date range

5. **Message Reactions**
   - Emoji reactions
   - Like/Dislike

## Security Considerations

1. **Authorization:**
   - Customer chỉ xem được chat của mình
   - Staff xem được tất cả nhưng không thể sửa/xóa

2. **Validation:**
   - Validate message không rỗng
   - Validate message length (max 500)
   - Validate StaffId và CustomerId tồn tại

3. **Rate Limiting:**
   - Giới hạn số tin nhắn gửi trong 1 phút
   - Chống spam

4. **XSS Protection:**
   - Sanitize message content
   - Escape HTML trong messages

## Performance

1. **Pagination:**
   - Load messages theo trang (20-50 messages/page)
   - Lazy load khi scroll lên

2. **Caching:**
   - Cache conversations list
   - Cache recent messages

3. **Database Indexing:**
   - Index trên `CustomerId`, `StaffId`, `SentDate`

## Monitoring

1. **Logging:**
   - Log tất cả messages (có thể tắt trong production)
   - Log connection/disconnection events

2. **Metrics:**
   - Số lượng active connections
   - Số messages per minute
   - Average response time

