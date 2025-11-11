# Cách Thức Hoạt Động của [Authorize] trong ASP.NET Core

## 1. Tổng Quan

`[Authorize]` là một **Authorization Filter** trong ASP.NET Core được sử dụng để bảo vệ các action hoặc controller, yêu cầu người dùng phải được xác thực (authenticated) trước khi truy cập.

## 2. Luồng Xử Lý Request

```
Client Request
    ↓
Middleware Pipeline
    ↓
Authentication Middleware (JWT Bearer)
    ↓
Authorization Middleware
    ↓
[Authorize] Filter Check
    ↓
    ├─→ Nếu chưa đăng nhập → 401 Unauthorized
    ├─→ Nếu đã đăng nhập nhưng không đủ quyền → 403 Forbidden
    └─→ Nếu đã đăng nhập và đủ quyền → Cho phép truy cập Action
```

## 3. Các Biến Thể của [Authorize]

### 3.1. [Authorize] - Cơ Bản
```csharp
[Authorize]
public async Task<IActionResult> GetProfile()
{
    // Yêu cầu: Người dùng PHẢI đăng nhập (bất kỳ role nào)
    // Nếu chưa đăng nhập → Trả về 401 Unauthorized
}
```

**Ví dụ trong code của bạn:**
```csharp
[Authorize] // Cần đăng nhập để xem booking
public async Task<IActionResult> GetById(int id)
{
    // Chỉ người dùng đã đăng nhập mới có thể truy cập
}
```

### 3.2. [Authorize(Roles = "...")] - Theo Role
```csharp
[Authorize(Roles = "Admin,Staff")]
public async Task<IActionResult> AddFeature([FromBody] FeatureDTO feature)
{
    // Yêu cầu: Người dùng PHẢI đăng nhập VÀ có role là "Admin" HOẶC "Staff"
    // Nếu chưa đăng nhập → 401 Unauthorized
    // Nếu đã đăng nhập nhưng không có role Admin hoặc Staff → 403 Forbidden
}
```

**Ví dụ trong code của bạn:**
```csharp
[Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff mới được thêm feature
public async Task<IActionResult> AddFeature([FromBody] FeatureDTO feature)
{
    // Chỉ Admin hoặc Staff mới có thể thêm feature
}
```

**Lưu ý:** 
- `Roles = "Admin,Staff"` có nghĩa là **Admin HOẶC Staff** (OR logic)
- Nếu muốn yêu cầu cả 2 roles, phải dùng Policy

### 3.3. [Authorize] ở Controller Level
```csharp
[Authorize]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    // TẤT CẢ các action trong controller này đều yêu cầu đăng nhập
    // Trừ khi action nào đó có [AllowAnonymous]
}
```

**Ví dụ trong code của bạn:**
```csharp
[Authorize]
public class WishlistController : ControllerBase
{
    // Tất cả action trong controller này đều yêu cầu đăng nhập
}
```

### 3.4. [AllowAnonymous] - Bỏ Qua Authorization
```csharp
[AllowAnonymous] // Tất cả đều có thể xem features
public async Task<IActionResult> GetActiveFeatures()
{
    // KHÔNG yêu cầu đăng nhập, ai cũng có thể truy cập
    // Override [Authorize] ở controller level nếu có
}
```

**Ví dụ trong code của bạn:**
```csharp
[HttpGet("active")]
[AllowAnonymous] // Tất cả đều có thể xem features
public async Task<IActionResult> GetActiveFeatures()
{
    // Public endpoint, không cần đăng nhập
}
```

## 4. Cấu Hình Authentication trong Program.cs

Trong code của bạn:

```csharp
// 1. Đăng ký JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Cấu hình validation cho JWT token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

// 2. Đăng ký Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrStaff", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// 3. Sử dụng Authentication và Authorization Middleware
app.UseAuthentication();  // Phải đặt TRƯỚC UseAuthorization
app.UseAuthorization();
```

## 5. Cách JWT Token Được Xử Lý

### 5.1. Client Gửi Request
```http
GET /api/Feature/active
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 5.2. Authentication Middleware Xử Lý
1. **Đọc token** từ header `Authorization: Bearer <token>`
2. **Validate token:**
   - Kiểm tra signature
   - Kiểm tra expiration
   - Kiểm tra issuer và audience
3. **Tạo ClaimsPrincipal** từ token nếu hợp lệ
4. **Lưu vào HttpContext.User**

### 5.3. Authorization Middleware Kiểm Tra
1. **Kiểm tra [Authorize] attribute:**
   - Nếu có `[Authorize]` → Kiểm tra `HttpContext.User.Identity.IsAuthenticated`
   - Nếu có `[Authorize(Roles = "...")]` → Kiểm tra user có role trong token không
2. **Quyết định:**
   - ✅ Authenticated và có quyền → Cho phép truy cập
   - ❌ Chưa authenticated → Trả về 401 Unauthorized
   - ❌ Đã authenticated nhưng không có quyền → Trả về 403 Forbidden

## 6. Ví Dụ Cụ Thể Từ Code Của Bạn

### Ví dụ 1: Public Endpoint
```csharp
[HttpGet("active")]
[AllowAnonymous] // Không cần đăng nhập
public async Task<IActionResult> GetActiveFeatures()
{
    // Ai cũng có thể truy cập
    // Không cần JWT token
}
```

**Request:**
```http
GET /api/Feature/active
(No Authorization header needed)
```

**Response:** ✅ 200 OK với danh sách features

---

### Ví dụ 2: Yêu Cầu Đăng Nhập
```csharp
[Authorize] // Cần đăng nhập
public async Task<IActionResult> GetProfile()
{
    // Chỉ người dùng đã đăng nhập mới truy cập được
}
```

**Request (chưa đăng nhập):**
```http
GET /api/Auth/profile
(No Authorization header)
```

**Response:** ❌ 401 Unauthorized

**Request (đã đăng nhập):**
```http
GET /api/Auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:** ✅ 200 OK với thông tin profile

---

### Ví dụ 3: Yêu Cầu Role Cụ Thể
```csharp
[HttpPost]
[Authorize(Roles = "Admin,Staff")] // Chỉ Admin hoặc Staff
public async Task<IActionResult> AddFeature([FromBody] FeatureDTO feature)
{
    // Chỉ Admin hoặc Staff mới có thể thêm feature
}
```

**Request (user thường - role: Customer):**
```http
POST /api/Feature
Authorization: Bearer <token_của_customer>
Content-Type: application/json
{
  "icon": "bi bi-star",
  "title": "New Feature"
}
```

**Response:** ❌ 403 Forbidden (Đã đăng nhập nhưng không có quyền)

**Request (Admin hoặc Staff):**
```http
POST /api/Feature
Authorization: Bearer <token_của_admin>
Content-Type: application/json
{
  "icon": "bi bi-star",
  "title": "New Feature"
}
```

**Response:** ✅ 201 Created

## 7. Thứ Tự Ưu Tiên

1. **Action Level** có độ ưu tiên cao nhất
2. **Controller Level** áp dụng cho tất cả action trong controller
3. **Global Filter** (nếu có) áp dụng cho toàn bộ ứng dụng

**Ví dụ:**
```csharp
[Authorize] // Controller level - tất cả action đều yêu cầu đăng nhập
public class FeatureController : ControllerBase
{
    [AllowAnonymous] // Action level - override controller level
    public async Task<IActionResult> GetActiveFeatures()
    {
        // Không cần đăng nhập
    }
    
    [Authorize(Roles = "Admin")] // Action level - override controller level
    public async Task<IActionResult> DeleteFeature(int id)
    {
        // Yêu cầu role Admin (chặt chẽ hơn controller level)
    }
}
```

## 8. Lấy Thông Tin User Trong Action

Khi đã pass qua `[Authorize]`, bạn có thể lấy thông tin user:

```csharp
[Authorize]
public async Task<IActionResult> GetMyProfile()
{
    // Lấy username
    var username = User.Identity?.Name;
    
    // Lấy claims
    var userId = User.FindFirst("UserId")?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    
    // Lấy tất cả claims
    var allClaims = User.Claims;
    
    return Ok(new { username, userId, role });
}
```

## 9. Tóm Tắt

| Attribute | Yêu Cầu | Kết Quả Nếu Không Đáp Ứng |
|-----------|---------|---------------------------|
| `[Authorize]` | Đăng nhập | 401 Unauthorized |
| `[Authorize(Roles = "Admin")]` | Đăng nhập + Role Admin | 401 hoặc 403 |
| `[AllowAnonymous]` | Không yêu cầu gì | Luôn cho phép |

## 10. Best Practices

1. ✅ **Sử dụng [AllowAnonymous] cho public endpoints** (GET tours, reviews, etc.)
2. ✅ **Sử dụng [Authorize] cho protected endpoints** (POST, PUT, DELETE)
3. ✅ **Sử dụng [Authorize(Roles = "...")] cho role-based access**
4. ✅ **Đặt [Authorize] ở controller level** nếu tất cả action đều cần bảo vệ
5. ✅ **Sử dụng Policy** cho logic phức tạp hơn

