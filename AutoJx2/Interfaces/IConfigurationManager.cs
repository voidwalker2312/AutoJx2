using System.Collections.Generic;
using AutoJx2.Models;

namespace AutoJx2.Interfaces
{
    /// <summary>
    /// Interface quản lý cấu hình ứng dụng
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Lưu đường dẫn game
        /// </summary>
        /// <param name="gamePath">Đường dẫn đến file so2game.exe</param>
        void SaveGamePath(string gamePath);

        /// <summary>
        /// Lấy đường dẫn game
        /// </summary>
        /// <returns>Đường dẫn game hoặc null nếu chưa được thiết lập</returns>
        string? GetGamePath();

        /// <summary>
        /// Lưu danh sách account
        /// </summary>
        /// <param name="accounts">Danh sách account</param>
        void SaveAccounts(List<GameAccount> accounts);

        /// <summary>
        /// Lấy danh sách account
        /// </summary>
        /// <returns>Danh sách account</returns>
        List<GameAccount> GetAccounts();

        /// <summary>
        /// Thêm account mới
        /// </summary>
        /// <param name="account">Thông tin account</param>
        void AddAccount(GameAccount account);

        /// <summary>
        /// Cập nhật account
        /// </summary>
        /// <param name="account">Thông tin account đã cập nhật</param>
        void UpdateAccount(GameAccount account);

        /// <summary>
        /// Xóa account
        /// </summary>
        /// <param name="accountId">ID của account cần xóa</param>
        void RemoveAccount(string accountId);

        /// <summary>
        /// Lưu cấu hình tổng thể
        /// </summary>
        /// <param name="config">Cấu hình</param>
        void SaveConfiguration(AppConfiguration config);

        /// <summary>
        /// Lấy cấu hình tổng thể
        /// </summary>
        /// <returns>Cấu hình ứng dụng</returns>
        AppConfiguration GetConfiguration();
    }
}
