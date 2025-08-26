using AutoJx2.Models;

namespace AutoJx2.Interfaces
{
    public interface ICredentialManager
    {
        /// <summary>
        /// Lưu thông tin đăng nhập vào Windows Credential Manager
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="server">Tên server</param>
        void SaveCredentials(string username, string password, string server);

        /// <summary>
        /// Tải thông tin đăng nhập từ Windows Credential Manager
        /// </summary>
        /// <returns>Thông tin đăng nhập hoặc null nếu không tìm thấy</returns>
        GameCredentials? LoadCredentials();

        /// <summary>
        /// Xóa thông tin đăng nhập
        /// </summary>
        void DeleteCredentials();
    }
}
