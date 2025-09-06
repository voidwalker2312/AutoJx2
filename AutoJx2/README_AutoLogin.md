# Auto Login Feature - Hướng dẫn sử dụng

## Tổng quan
Tính năng Auto Login cho phép người dùng quản lý nhiều account game và tự động khởi động game với thông tin đăng nhập đã lưu.

## Các tính năng đã triển khai

### 1. Quản lý đường dẫn game
- **Chọn đường dẫn game**: Click nút "📁 Chọn" để chọn file `so2game.exe`
- **Lưu đường dẫn**: Click nút "💾 Lưu" để lưu đường dẫn game vào file cấu hình
- **Tự động load**: Ứng dụng sẽ tự động load đường dẫn đã lưu khi khởi động

### 2. Quản lý Account
- **Thêm account mới**: Click nút "➕ Thêm Account"
  - Nhập tên đăng nhập, mật khẩu, server
  - Thêm ghi chú (tùy chọn)
  - Mật khẩu được mã hóa an toàn
- **Chỉnh sửa account**: Click chuột phải vào account → "✏️ Chỉnh sửa"
- **Xóa account**: Click chuột phải vào account → "🗑️ Xóa"
- **Ẩn/Hiện account**: Click chuột phải vào account → "👁️ Ẩn/Hiện"

### 3. Khởi động Game
- **Double-click**: Double-click vào account để khởi động game
- **Context menu**: Click chuột phải → "▶️ Khởi động Game"
- **Dừng game**: Click chuột phải → "⏹️ Dừng Game"

### 4. Quản lý hàng loạt
- **Chọn nhiều account**: Sử dụng checkbox để chọn nhiều account
- **Xóa hàng loạt**: Click nút "🗑️ Xóa đã chọn"

## Cấu trúc dữ liệu

### File cấu hình: `config.json`
```json
{
  "gamePath": "C:\\Path\\To\\so2game.exe",
  "accounts": [
    {
      "id": "unique-id",
      "username": "player1",
      "password": "encrypted-password",
      "server": "Server 1",
      "createdAt": "2024-01-01T00:00:00",
      "lastUpdated": "2024-01-01T00:00:00",
      "isHidden": false,
      "notes": "Main account",
      "processId": null,
      "isRunning": false
    }
  ],
  "automation": {
    "delay": 1000,
    "maxHpThreshold": 80,
    "maxMpThreshold": 70,
    "autoFarming": false,
    "autoQuest": false,
    "autoCombat": false,
    "autoHeal": false
  },
  "ui": {
    "logLevel": "Information",
    "hotkey": "F12",
    "autoStartWithWindows": false,
    "minimizeToTray": false
  }
}
```

## Bảo mật

### Mã hóa mật khẩu
- Mật khẩu được mã hóa bằng `ProtectedData.Protect()` của Windows
- Chỉ có thể giải mã trên cùng máy tính và user account
- Mật khẩu không được lưu dưới dạng plain text

### Lưu trữ an toàn
- Dữ liệu được lưu trong file `config.json` trong thư mục ứng dụng
- File cấu hình có thể được backup và restore

## Xử lý lỗi

### Lỗi thường gặp
1. **File game không tồn tại**: Kiểm tra đường dẫn game
2. **Không thể khởi động game**: Kiểm tra quyền truy cập file
3. **Account đã tồn tại**: Username phải duy nhất
4. **Lỗi mã hóa mật khẩu**: Kiểm tra quyền Windows

### Log
- Tất cả hoạt động được ghi log vào file `logs/autojx2-YYYYMMDD.log`
- Log level có thể thay đổi trong Settings tab

## Cách sử dụng

1. **Lần đầu sử dụng**:
   - Mở tab "🔐 Auto Login"
   - Click "📁 Chọn" để chọn file `so2game.exe`
   - Click "💾 Lưu" để lưu đường dẫn
   - Click "➕ Thêm Account" để thêm account đầu tiên

2. **Sử dụng hàng ngày**:
   - Double-click vào account để khởi động game
   - Sử dụng context menu để quản lý account
   - Theo dõi trạng thái trong ListView

3. **Quản lý account**:
   - Thêm account mới khi cần
   - Chỉnh sửa thông tin account
   - Ẩn account không sử dụng
   - Xóa account không cần thiết

## Lưu ý quan trọng

- **Backup dữ liệu**: Thường xuyên backup file `config.json`
- **Bảo mật**: Không chia sẻ file cấu hình với người khác
- **Cập nhật**: Kiểm tra đường dẫn game khi game được cập nhật
- **Quyền truy cập**: Đảm bảo ứng dụng có quyền đọc/ghi file

## Troubleshooting

### Game không khởi động
1. Kiểm tra đường dẫn game có đúng không
2. Kiểm tra file `so2game.exe` có tồn tại không
3. Kiểm tra quyền truy cập file
4. Thử chạy game thủ công trước

### Account không lưu được
1. Kiểm tra quyền ghi file trong thư mục ứng dụng
2. Kiểm tra disk space
3. Kiểm tra file `config.json` có bị lock không

### Lỗi mã hóa mật khẩu
1. Chạy ứng dụng với quyền Administrator
2. Kiểm tra Windows Credential Manager
3. Thử tạo account mới
