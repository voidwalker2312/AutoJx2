giờ hãy bắt đầu làm phần tính năng cho tool auto game:
1. Auto Login.
2. Auto Click không chiếm chuột.
3. Auto Buff.
4. Những phần đang phát triển (chưa làm)
Triển khai:
1. Auto Login:
- UI:
    + EditBox và một button để người dùng có thể trỏ đến đường dẫn mở game Tên là "so2game.exe". Khi người dùng tiến hành nhập xong sẽ lưu đường dẫn mở game vào một file data json hoặc xml để lần sau chỉ cần load lại từ file.
    + Button "Thêm Account". Khi bấm vào đây sẽ hiển thị ra một "Add Account" window để người dùng có thể nhập "Tên đăng nhập", "Mật khẩu", "Server", lưu account
    + Một listview với phần tử là checkbox với nội dung là tên đăng nhập, khi bấm vào sẽ tiến hành khởi động "so2game.exe" và lưu lại process của "so2game.exe" vừa mở để tiến hành những xử lý sau đó. Khi click chuột phải vào sẽ tiến hành show popup info gồm những mục như edit, hide, remove.
