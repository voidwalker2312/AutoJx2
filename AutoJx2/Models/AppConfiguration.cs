using System.Collections.Generic;

namespace AutoJx2.Models
{
    /// <summary>
    /// Model chứa cấu hình tổng thể của ứng dụng
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Đường dẫn đến file so2game.exe
        /// </summary>
        public string GamePath { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách account
        /// </summary>
        public List<GameAccount> Accounts { get; set; } = new List<GameAccount>();

        /// <summary>
        /// Cấu hình tự động hóa
        /// </summary>
        public AutomationSettings Automation { get; set; } = new AutomationSettings();

        /// <summary>
        /// Cấu hình giao diện
        /// </summary>
        public UISettings UI { get; set; } = new UISettings();
    }

    /// <summary>
    /// Cấu hình tự động hóa
    /// </summary>
    public class AutomationSettings
    {
        /// <summary>
        /// Delay giữa các hành động (ms)
        /// </summary>
        public int Delay { get; set; } = 1000;

        /// <summary>
        /// Ngưỡng HP để tự động heal (%)
        /// </summary>
        public int MaxHpThreshold { get; set; } = 80;

        /// <summary>
        /// Ngưỡng MP để tự động heal (%)
        /// </summary>
        public int MaxMpThreshold { get; set; } = 70;

        /// <summary>
        /// Bật/tắt auto farming
        /// </summary>
        public bool AutoFarming { get; set; } = false;

        /// <summary>
        /// Bật/tắt auto quest
        /// </summary>
        public bool AutoQuest { get; set; } = false;

        /// <summary>
        /// Bật/tắt auto combat
        /// </summary>
        public bool AutoCombat { get; set; } = false;

        /// <summary>
        /// Bật/tắt auto heal
        /// </summary>
        public bool AutoHeal { get; set; } = false;
    }

    /// <summary>
    /// Cấu hình giao diện
    /// </summary>
    public class UISettings
    {
        /// <summary>
        /// Mức độ log hiển thị
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Hotkey để bắt đầu/dừng automation
        /// </summary>
        public string Hotkey { get; set; } = "F12";

        /// <summary>
        /// Có tự động khởi động với Windows không
        /// </summary>
        public bool AutoStartWithWindows { get; set; } = false;

        /// <summary>
        /// Có ẩn vào system tray không
        /// </summary>
        public bool MinimizeToTray { get; set; } = false;
    }
}
