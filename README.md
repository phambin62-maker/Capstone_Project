| Layer              | Mục đích                                              | Ví dụ                                 |
| ------------------ | ----------------------------------------------------- | ------------------------------------- |
| **Domain**         | Chứa entity và logic nghiệp vụ cơ bản                 | `User`, `Train`, `Booking`            |
| **Infrastructure** | Truy cập DB, EF Core, Repository                      | `BookingRepository`, `DbContext`      |
| **Application**    | Xử lý logic (service, DTO, validation)                | `BookingService`, `UserService`       |
| **API**            | Giao tiếp HTTP (controller, routing, JWT, middleware) | `BookingController`, `AuthController` |
