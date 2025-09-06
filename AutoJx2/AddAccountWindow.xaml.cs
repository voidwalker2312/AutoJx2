using AutoJx2.Interfaces;
using AutoJx2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AutoJx2
{
    /// <summary>
    /// Window để thêm account mới
    /// </summary>
    public partial class AddAccountWindow : Window
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly ILogger<AddAccountWindow>? _logger;
        private Models.GameAccount? _editingAccount;

        public event EventHandler<Models.GameAccount>? AccountSaved;

        public AddAccountWindow(IConfigurationManager configurationManager, ILogger<AddAccountWindow>? logger)
        {
            InitializeComponent();
            _configurationManager = configurationManager;
            _logger = logger;
            
            // Focus vào textbox đầu tiên
            UsernameTextBox.Focus();
        }

        /// <summary>
        /// Constructor để chỉnh sửa account
        /// </summary>
        public AddAccountWindow(IConfigurationManager configurationManager, ILogger<AddAccountWindow>? logger, Models.GameAccount account)
            : this(configurationManager, logger)
        {
            _editingAccount = account;
            Title = "Chỉnh sửa Account";
            LoadAccountData();
        }

        private void LoadAccountData()
        {
            if (_editingAccount == null) return;

            try
            {
                UsernameTextBox.Text = _editingAccount.Username;
                ServerComboBox.Text = _editingAccount.Server;
                NotesTextBox.Text = _editingAccount.Notes ?? "";
                
                // Không hiển thị mật khẩu cũ vì lý do bảo mật
                PasswordBox.Password = "";
                
                _logger?.LogInformation("Account data loaded for editing: {Username}", _editingAccount.Username);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading account data");
                ShowValidationMessage("Lỗi khi tải dữ liệu account");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var account = new Models.GameAccount
                {
                    Username = UsernameTextBox.Text.Trim(),
                    Password = EncryptPassword(PasswordBox.Password),
                    Server = ServerComboBox.Text.Trim(),
                    Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim(),
                    LastUpdated = DateTime.Now
                };

                if (_editingAccount != null)
                {
                    // Chỉnh sửa account
                    account.Id = _editingAccount.Id;
                    account.CreatedAt = _editingAccount.CreatedAt;
                    account.IsHidden = _editingAccount.IsHidden;
                    account.ProcessId = _editingAccount.ProcessId;
                    account.IsRunning = _editingAccount.IsRunning;
                    
                    _configurationManager.UpdateAccount(account);
                    _logger?.LogInformation("Account updated: {Username}", account.Username);
                }
                else
                {
                    // Thêm account mới
                    _configurationManager.AddAccount(account);
                    _logger?.LogInformation("Account added: {Username}", account.Username);
                }

                AccountSaved?.Invoke(this, account);
                DialogResult = true;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                ShowValidationMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving account");
                ShowValidationMessage($"Lỗi khi lưu account: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            ClearValidationMessage();

            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowValidationMessage("Vui lòng nhập tên đăng nhập");
                UsernameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowValidationMessage("Vui lòng nhập mật khẩu");
                PasswordBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ServerComboBox.Text))
            {
                ShowValidationMessage("Vui lòng chọn server");
                ServerComboBox.Focus();
                return false;
            }

            // Kiểm tra độ dài
            if (UsernameTextBox.Text.Length < 3)
            {
                ShowValidationMessage("Tên đăng nhập phải có ít nhất 3 ký tự");
                UsernameTextBox.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 4)
            {
                ShowValidationMessage("Mật khẩu phải có ít nhất 4 ký tự");
                PasswordBox.Focus();
                return false;
            }

            return true;
        }

        private void ShowValidationMessage(string message)
        {
            ValidationMessage.Text = message;
            ValidationMessage.Visibility = Visibility.Visible;
        }

        private void ClearValidationMessage()
        {
            ValidationMessage.Text = "";
            ValidationMessage.Visibility = Visibility.Collapsed;
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
                _logger?.LogError(ex, "Error encrypting password");
                throw new InvalidOperationException("Lỗi khi mã hóa mật khẩu");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Đặt focus vào textbox đầu tiên
            UsernameTextBox.Focus();
        }

        private void ServerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
