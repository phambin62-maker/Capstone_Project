# Phân Tích Logic Chat Giữa Customer và Staff

## 1. Logic Hiện Tại

### 1.1. Database Schema

**Bảng `Chat`:**
- `Id` (int): Primary key
- `StaffId` (int): Foreign key → User (RoleId = 2 - Staff)
- `CustomerId` (int): Foreign key → User (RoleId = 3 - Customer)
- `Message` (string, max 500): Nội dung tin nhắn
- `ChatType` (enum): 
  - `Support = 1`: Tin nhắn hỗ trợ
  - `System = 2`: Tin nhắn hệ thống
- `SentDate` (DateTime): Thời gian gửi

**Quan hệ:**
- Một Customer có thể chat với nhiều Staff
- Một Staff có thể chat với nhiều Customer
- Một cuộc trò chuyện = một cặp (CustomerId, StaffId)

### 1.2. DAO Layer (Đã có)

`ChatDAO` đã có các method:
- ✅ `AddChatAsync()`: Thêm tin nhắn mới
- ✅ `GetChatsByCustomerIdAndStaffIdAsync()`: Lấy lịch sử chat giữa customer và staff
- ✅ `GetChatsByCustomerIdAsync()`: Lấy tất cả chat của customer
- ✅ `GetChatsByStaffIdAsync()`: Lấy tất cả chat của staff
- ✅ `GetChatByIdAsync()`: Lấy chat theo ID
- ✅ `UpdateChatAsync()`: Cập nhật chat
- ✅ `DeleteChatByIdAsync()`: Xóa chat

### 1.3. Logic Hoạt Động

**Flow cơ bản:**
1. Customer gửi tin nhắn → Lưu vào DB với `CustomerId` và `StaffId`
2. Staff nhận tin nhắn → Xem trong danh sách chat
3. Staff phản hồi → Lưu vào DB với cùng `CustomerId` và `StaffId`
4. Customer xem phản hồi → Load lại danh sách chat

**Vấn đề hiện tại:**
- ❌ Chưa có API Controller/Service
- ❌ Chưa có Frontend View
- ❌ Chưa có Real-time messaging (SignalR)
- ❌ Chưa có phân quyền rõ ràng
- ❌ Chưa có notification khi có tin nhắn mới

## 2. Giải Pháp Đề Xuất

### 2.1. Kiến Trúc Tổng Quan

```
┌─────────────────┐
│   Frontend      │
│  (MVC Views)    │
└────────┬────────┘
         │ HTTP/WebSocket
         ▼
┌─────────────────┐
│  API Controllers│
│  (ChatController)│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Chat Service   │
│  (Business Logic)│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Chat DAO      │
│  (Data Access)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Database      │
│    (Chat Table) │
└─────────────────┘
```

### 2.2. Các Component Cần Tạo

#### A. Backend (API)

1. **DTOs:**
   - `ChatDTO.cs`: DTO cho chat message
   - `CreateChatDTO.cs`: DTO để tạo chat mới
   - `ChatConversationDTO.cs`: DTO cho cuộc trò chuyện

2. **Service Layer:**
   - `IChatService.cs`: Interface
   - `ChatService.cs`: Implementation

3. **Controller:**
   - `ChatController.cs`: API endpoints

4. **SignalR Hub (Optional - cho real-time):**
   - `ChatHub.cs`: SignalR hub cho real-time messaging

#### B. Frontend

1. **Controllers:**
   - `ChatWebController.cs`: MVC controller

2. **Views:**
   - `Chat/Index.cshtml`: Trang danh sách cuộc trò chuyện (cho Staff)
   - `Chat/Conversation.cshtml`: Trang chat với customer cụ thể
   - `Profile/Chat.cshtml`: Trang chat của Customer

3. **JavaScript:**
   - `chat.js`: Xử lý gửi/nhận tin nhắn, polling hoặc SignalR

### 2.3. API Endpoints Đề Xuất

#### Cho Customer:
- `GET /api/chat/my-conversations` - Lấy danh sách cuộc trò chuyện của customer
- `GET /api/chat/conversation/{staffId}` - Lấy lịch sử chat với staff cụ thể
- `POST /api/chat/send` - Gửi tin nhắn đến staff

#### Cho Staff:
- `GET /api/chat/conversations` - Lấy danh sách tất cả cuộc trò chuyện
- `GET /api/chat/conversation/{customerId}` - Lấy lịch sử chat với customer cụ thể
- `POST /api/chat/send` - Gửi tin nhắn đến customer
- `GET /api/chat/unread-count` - Số tin nhắn chưa đọc

### 2.4. Phân Quyền

- **Customer (RoleId = 3):**
  - Chỉ xem được chat của chính mình
  - Chỉ gửi tin nhắn đến staff đã từng chat với mình
  - Không thể xem chat của customer khác

- **Staff (RoleId = 2):**
  - Xem được tất cả cuộc trò chuyện
  - Có thể chat với bất kỳ customer nào
  - Có thể xem danh sách customer đang chờ phản hồi

- **Admin (RoleId = 1):**
  - Có thể xem tất cả chat (nếu cần)

### 2.5. Real-time Messaging (Optional)

**Option 1: Polling (Đơn giản)**
- Frontend gọi API mỗi 2-3 giây để lấy tin nhắn mới
- Dễ implement, không cần SignalR
- Tốn tài nguyên hơn

**Option 2: SignalR (Khuyến nghị)**
- Real-time messaging
- Hiệu quả hơn
- Cần cài đặt SignalR

### 2.6. Notification System

Khi có tin nhắn mới:
1. Tạo notification trong bảng `Notification`
2. Nếu dùng SignalR, gửi real-time notification
3. Cập nhật badge số tin nhắn chưa đọc

## 3. Implementation Plan

### Phase 1: Basic Chat (Không Real-time)
1. ✅ Tạo DTOs
2. ✅ Tạo ChatService
3. ✅ Tạo ChatController (API)
4. ✅ Tạo ChatWebController (MVC)
5. ✅ Tạo Views
6. ✅ Tạo JavaScript (polling)

### Phase 2: Real-time (Optional)
1. ✅ Cài đặt SignalR
2. ✅ Tạo ChatHub
3. ✅ Cập nhật JavaScript để dùng SignalR
4. ✅ Thêm notification real-time

### Phase 3: Advanced Features
1. ✅ Đánh dấu đã đọc/chưa đọc
2. ✅ Tìm kiếm tin nhắn
3. ✅ Gửi file/ảnh
4. ✅ Typing indicator

## 4. Database Schema Cần Bổ Sung (Nếu cần)

**Thêm vào bảng Chat:**
- `IsRead` (bit): Đánh dấu đã đọc
- `ReadDate` (DateTime): Thời gian đọc
- `SenderId` (int): Người gửi (CustomerId hoặc StaffId)
- `IsDeleted` (bit): Soft delete

**Hoặc tạo bảng mới:**
- `ChatMessage`: Lưu từng tin nhắn riêng lẻ
- `ChatConversation`: Lưu thông tin cuộc trò chuyện

## 5. Security Considerations

1. **Authorization:**
   - Customer chỉ xem được chat của mình
   - Staff xem được tất cả nhưng không thể sửa/xóa chat của customer khác

2. **Validation:**
   - Validate message không rỗng
   - Validate message length (max 500)
   - Validate StaffId và CustomerId tồn tại

3. **Rate Limiting:**
   - Giới hạn số tin nhắn gửi trong 1 phút
   - Chống spam

## 6. Testing Checklist

- [ ] Customer có thể gửi tin nhắn đến staff
- [ ] Staff có thể phản hồi customer
- [ ] Customer chỉ xem được chat của mình
- [ ] Staff xem được tất cả cuộc trò chuyện
- [ ] Tin nhắn được lưu đúng vào database
- [ ] Hiển thị đúng thời gian gửi
- [ ] Phân trang tin nhắn (nếu có nhiều)
- [ ] Real-time update (nếu dùng SignalR)

