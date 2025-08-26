using System;

namespace AutoJx2.Models
{
    /// <summary>
    /// Model chứa thông tin đăng nhập game
    /// </summary>
    public class GameCredentials
    {
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
        /// Ghi chú bổ sung
        /// </summary>
        public string? Notes { get; set; }
    }
}
