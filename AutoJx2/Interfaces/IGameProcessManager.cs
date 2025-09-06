using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoJx2.Models;

namespace AutoJx2.Interfaces
{
    /// <summary>
    /// Interface quản lý process game
    /// </summary>
    public interface IGameProcessManager : IDisposable
    {
        /// <summary>
        /// Khởi động game với account cụ thể
        /// </summary>
        /// <param name="account">Thông tin account</param>
        /// <param name="gamePath">Đường dẫn đến file so2game.exe</param>
        /// <returns>Process của game đã khởi động</returns>
        Process? StartGame(Models.GameAccount account, string gamePath);

        /// <summary>
        /// Dừng game process
        /// </summary>
        /// <param name="processId">ID của process cần dừng</param>
        void StopGame(int processId);

        /// <summary>
        /// Dừng tất cả game process
        /// </summary>
        void StopAllGames();

        /// <summary>
        /// Lấy danh sách game process đang chạy
        /// </summary>
        /// <returns>Danh sách process</returns>
        List<Process> GetRunningGameProcesses();

        /// <summary>
        /// Kiểm tra xem game có đang chạy không
        /// </summary>
        /// <param name="processId">ID của process</param>
        /// <returns>True nếu đang chạy</returns>
        bool IsGameRunning(int processId);

        /// <summary>
        /// Lấy thông tin process theo account
        /// </summary>
        /// <param name="accountId">ID của account</param>
        /// <returns>Process hoặc null nếu không tìm thấy</returns>
        Process? GetProcessByAccount(string accountId);

        /// <summary>
        /// Sự kiện khi game được khởi động
        /// </summary>
        event EventHandler<GameProcessEventArgs> GameStarted;

        /// <summary>
        /// Sự kiện khi game bị dừng
        /// </summary>
        event EventHandler<GameProcessEventArgs> GameStopped;

        /// <summary>
        /// Sự kiện khi có lỗi xảy ra
        /// </summary>
        event EventHandler<string> ErrorOccurred;
    }

    /// <summary>
    /// Event args cho sự kiện game process
    /// </summary>
    public class GameProcessEventArgs : EventArgs
    {
        public Models.GameAccount Account { get; set; } = null!;
        public Process Process { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
