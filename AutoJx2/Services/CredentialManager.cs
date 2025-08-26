using AutoJx2.Interfaces;
using AutoJx2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace AutoJx2.Services
{
    /// <summary>
    /// Quản lý thông tin đăng nhập sử dụng Windows Credential Manager và DPAPI
    /// </summary>
    public class CredentialManager : ICredentialManager, IDisposable
    {
        private readonly ILogger<CredentialManager> _logger;
        private const string CREDENTIAL_TARGET = "AutoJx2_GameCredentials";
        private bool _disposed = false;

        public CredentialManager(ILogger<CredentialManager> logger)
        {
            _logger = logger;
        }

        public void SaveCredentials(string username, string password, string server)
        {
            try
            {
                var credentials = new GameCredentials
                {
                    Username = username,
                    Password = EncryptPassword(password),
                    Server = server,
                    LastUpdated = DateTime.Now
                };

                // Lưu vào Windows Credential Manager
                SaveToCredentialManager(credentials);
                
                _logger.LogInformation("Credentials saved successfully for user: {Username}", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving credentials");
                throw;
            }
        }

        public GameCredentials? LoadCredentials()
        {
            try
            {
                var credentials = LoadFromCredentialManager();
                if (credentials != null)
                {
                    // Giải mã mật khẩu
                    credentials.Password = DecryptPassword(credentials.Password);
                    _logger.LogInformation("Credentials loaded successfully for user: {Username}", credentials.Username);
                }
                return credentials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading credentials");
                return null;
            }
        }

        public void DeleteCredentials()
        {
            try
            {
                DeleteFromCredentialManager();
                _logger.LogInformation("Credentials deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting credentials");
                throw;
            }
        }

        private void SaveToCredentialManager(GameCredentials credentials)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(credentials);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            var credential = new NativeMethods.CREDENTIAL
            {
                Type = NativeMethods.CREDTYPE.GENERIC,
                TargetName = CREDENTIAL_TARGET,
                CredentialBlobSize = (uint)bytes.Length,
                CredentialBlob = Marshal.AllocHGlobal(bytes.Length),
                Persist = (uint)NativeMethods.CREDPERSIST.LOCAL_MACHINE
            };

            try
            {
                Marshal.Copy(bytes, 0, credential.CredentialBlob, bytes.Length);
                
                if (!NativeMethods.CredWrite(ref credential, 0))
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to save credentials. Error code: {error}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(credential.CredentialBlob);
            }
        }

        private GameCredentials? LoadFromCredentialManager()
        {
            if (!NativeMethods.CredRead(CREDENTIAL_TARGET, NativeMethods.CREDTYPE.GENERIC, 0, out var credentialPtr))
            {
                var error = Marshal.GetLastWin32Error();
                if (error == 1168) // ERROR_NOT_FOUND
                {
                    return null;
                }
                throw new InvalidOperationException($"Failed to read credentials. Error code: {error}");
            }

            try
            {
                var credential = Marshal.PtrToStructure<NativeMethods.CREDENTIAL>(credentialPtr);
                var bytes = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, bytes, 0, (int)credential.CredentialBlobSize);
                
                var json = Encoding.UTF8.GetString(bytes);
                return System.Text.Json.JsonSerializer.Deserialize<GameCredentials>(json);
            }
            finally
            {
                NativeMethods.CredFree(credentialPtr);
            }
        }

        private void DeleteFromCredentialManager()
        {
            if (!NativeMethods.CredDelete(CREDENTIAL_TARGET, NativeMethods.CREDTYPE.GENERIC, 0))
            {
                var error = Marshal.GetLastWin32Error();
                if (error != 1168) // ERROR_NOT_FOUND
                {
                    throw new InvalidOperationException($"Failed to delete credentials. Error code: {error}");
                }
            }
        }

        private string EncryptPassword(string password)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var encryptedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting password");
                throw;
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedPassword);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting password");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Cleanup managed resources
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Native methods để tương tác với Windows Credential Manager
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool CredWrite(ref CREDENTIAL credential, uint flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool CredRead(string targetName, CREDTYPE type, uint flags, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool CredDelete(string targetName, CREDTYPE type, uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool CredFree(IntPtr credentialPtr);

        internal enum CREDTYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7
        }

        internal enum CREDPERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CREDENTIAL
        {
            public uint Flags;
            public CREDTYPE Type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string UserName;
        }
    }
}
