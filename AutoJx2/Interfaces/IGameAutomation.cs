using System;
using System.Diagnostics;

namespace AutoJx2.Interfaces
{
    /// <summary>
    /// Interface cho việc tự động hóa game
    /// </summary>
    public interface IGameAutomation : IDisposable
    {
        /// <summary>
        /// Kết nối với process game
        /// </summary>
        /// <param name="process">Process của game</param>
        void AttachToProcess(Process process);

        /// <summary>
        /// Bắt đầu tự động hóa
        /// </summary>
        void StartAutomation();

        /// <summary>
        /// Dừng tự động hóa
        /// </summary>
        void StopAutomation();

        /// <summary>
        /// Kiểm tra xem tự động hóa có đang chạy không
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Bật/tắt tính năng auto farming
        /// </summary>
        /// <param name="enabled">Bật hay tắt</param>
        void SetAutoFarming(bool enabled);

        /// <summary>
        /// Bật/tắt tính năng auto quest
        /// </summary>
        /// <param name="enabled">Bật hay tắt</param>
        void SetAutoQuest(bool enabled);

        /// <summary>
        /// Bật/tắt tính năng auto combat
        /// </summary>
        /// <param name="enabled">Bật hay tắt</param>
        void SetAutoCombat(bool enabled);

        /// <summary>
        /// Bật/tắt tính năng auto heal
        /// </summary>
        /// <param name="enabled">Bật hay tắt</param>
        void SetAutoHeal(bool enabled);

        /// <summary>
        /// Thiết lập delay giữa các hành động
        /// </summary>
        /// <param name="milliseconds">Thời gian delay tính bằng milliseconds</param>
        void SetDelay(int milliseconds);

        /// <summary>
        /// Thiết lập ngưỡng HP để tự động heal
        /// </summary>
        /// <param name="percentage">Phần trăm HP</param>
        void SetMaxHpThreshold(int percentage);

        /// <summary>
        /// Thiết lập ngưỡng MP để tự động heal
        /// </summary>
        /// <param name="percentage">Phần trăm MP</param>
        void SetMaxMpThreshold(int percentage);

        /// <summary>
        /// Sự kiện khi trạng thái thay đổi
        /// </summary>
        event EventHandler<string> StatusChanged;

        /// <summary>
        /// Sự kiện khi có lỗi xảy ra
        /// </summary>
        event EventHandler<string> ErrorOccurred;
    }
}
