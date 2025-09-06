using System;

namespace AutoJx2.Models
{
    /// <summary>
    /// Model chứa thông tin account game
    /// </summary>
    public class GameAccount
    {
        /// <summary>
        /// ID duy nhất của account
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu (đã được mã hóa)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Tên server
        /// </summary>
        public string Server { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Thời gian cập nhật cuối
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Có bị ẩn không
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// Ghi chú bổ sung
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Process ID của game đang chạy (nếu có)
        /// </summary>
        public int? ProcessId { get; set; }

        /// <summary>
        /// Có đang chạy game không
        /// </summary>
        public bool IsRunning { get; set; } = false;

        /// <summary>
        /// Có được chọn trong ListView không (không lưu vào file)
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsSelected { get; set; } = false;
    }
}
