using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AutoJx2.Interfaces;
using AutoJx2.Services;
using AutoJx2.Models;
using Microsoft.Win32;

namespace AutoJx2
{
    public partial class MainWindow : Window
    {
        private ILogger<MainWindow>? _logger;
        private ICredentialManager? _credentialManager;
        private IGameAutomation? _gameAutomation;
        private IConfigurationManager? _configurationManager;
        private IGameProcessManager? _gameProcessManager;
        private readonly DispatcherTimer _timer;
        private Process? _attachedProcess;
        private bool _isAutomationRunning = false;
        private DateTime _startTime;
        private List<Models.GameAccount> _accounts = new List<Models.GameAccount>();

        public MainWindow()
        {
            InitializeComponent();
            
            // Khởi tạo _startTime
            _startTime = DateTime.Now;
            
            // Khởi tạo timer để cập nhật thời gian
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Lấy services từ DI container
            var app = Application.Current as App;
            if (app != null)
            {
                _logger = app.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
                _credentialManager = app.ServiceProvider.GetRequiredService<ICredentialManager>();
                _gameAutomation = app.ServiceProvider.GetRequiredService<IGameAutomation>();
                _configurationManager = app.ServiceProvider.GetRequiredService<IConfigurationManager>();
                _gameProcessManager = app.ServiceProvider.GetRequiredService<IGameProcessManager>();
            }
            
            // Khởi tạo giao diện
            InitializeUI();
        }

        public MainWindow(ILogger<MainWindow> logger, ICredentialManager credentialManager, IGameAutomation gameAutomation, IConfigurationManager configurationManager, IGameProcessManager gameProcessManager)
        {
            InitializeComponent();
            
            _logger = logger;
            _credentialManager = credentialManager;
            _gameAutomation = gameAutomation;
            _configurationManager = configurationManager;
            _gameProcessManager = gameProcessManager;
            
            // Khởi tạo _startTime
            _startTime = DateTime.Now;
            
            // Khởi tạo timer để cập nhật thời gian
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Khởi tạo giao diện
            InitializeUI();
            
            _logger?.LogInformation("MainWindow initialized successfully");
        }

        private void InitializeUI()
        {
            try
            {
                // Load credentials
                LoadCredentials();
                
                // Load settings
                LoadSettings();
                
                // Load game path and accounts
                LoadGamePath();
                LoadAccounts();
                
                // Refresh process list
                RefreshProcesses();
                
                // Set initial button states
                if (StartAutomationButton != null && StopAutomationButton != null)
                {
                    StartAutomationButton.IsEnabled = true;
                    StopAutomationButton.IsEnabled = false;
                }
                
                // Cập nhật status
                UpdateStatus("Ready");
                
                _logger?.LogInformation("UI initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing UI");
                MessageBox.Show($"Error initializing UI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_isAutomationRunning && FooterTimeTextBlock != null)
                {
                    var elapsed = DateTime.Now - _startTime;
                    FooterTimeTextBlock.Text = elapsed.ToString(@"hh\:mm\:ss");
                }
                else if (FooterTimeTextBlock != null)
                {
                    FooterTimeTextBlock.Text = "00:00:00";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in timer tick");
            }
        }

        private void RefreshProcessesButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        private void RefreshProcesses()
        {
            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                    .OrderBy(p => p.ProcessName)
                    .ToList();

                if (GameProcessComboBox != null)
                {
                    GameProcessComboBox.ItemsSource = processes;
                    _logger?.LogInformation("Refreshed process list. Found {Count} processes", processes.Count);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error refreshing processes");
                MessageBox.Show($"Error refreshing processes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AttachProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameProcessComboBox?.SelectedItem is Process process && _gameAutomation != null)
            {
                try
                {
                    _attachedProcess = process;
                    _gameAutomation.AttachToProcess(process);
                    UpdateStatus($"Attached to {process.ProcessName} (PID: {process.Id})");
                    
                    // Enable start automation button
                    if (StartAutomationButton != null)
                        StartAutomationButton.IsEnabled = true;
                    
                    _logger?.LogInformation("Attached to process: {ProcessName} (PID: {ProcessId})", process.ProcessName, process.Id);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error attaching to process");
                    MessageBox.Show($"Error attaching to process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a process first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement hotkey setting
            MessageBox.Show("Hotkey setting feature will be implemented soon.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StartAutomationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_attachedProcess == null || _gameAutomation == null)
                {
                    MessageBox.Show("Please attach to a game process first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _gameAutomation.StartAutomation();
                _isAutomationRunning = true;
                _startTime = DateTime.Now;
                UpdateStatus("Automation started");
                
                if (StartAutomationButton != null && StopAutomationButton != null)
                {
                    StartAutomationButton.IsEnabled = false;
                    StopAutomationButton.IsEnabled = true;
                }
                
                _logger?.LogInformation("Automation started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting automation");
                MessageBox.Show($"Error starting automation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopAutomationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_gameAutomation != null)
                {
                    _gameAutomation.StopAutomation();
                }
                _isAutomationRunning = false;
                UpdateStatus("Automation stopped");
                
                if (StartAutomationButton != null && StopAutomationButton != null)
                {
                    StartAutomationButton.IsEnabled = true;
                    StopAutomationButton.IsEnabled = false;
                }
                
                _logger?.LogInformation("Automation stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error stopping automation");
                MessageBox.Show($"Error stopping automation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCredentialsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_credentialManager == null)
                {
                    MessageBox.Show("Credential manager not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var username = UsernameTextBox?.Text ?? "";
                var password = PasswordBox?.Password ?? "";
                var server = ServerComboBox?.Text ?? "";

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Username and password are required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _credentialManager.SaveCredentials(username, password, server);
                UpdateStatus("Credentials saved successfully");
                _logger?.LogInformation("Credentials saved for user: {Username}", username);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving credentials");
                MessageBox.Show($"Error saving credentials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (Directory.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                }
                else
                {
                    MessageBox.Show("Logs directory not found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening logs directory");
                MessageBox.Show($"Error opening logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    ["Delay"] = DelayTextBox?.Text ?? "1000",
                    ["MaxHp"] = MaxHpTextBox?.Text ?? "80",
                    ["MaxMp"] = MaxMpTextBox?.Text ?? "70",
                    ["LogLevel"] = LogLevelComboBox?.Text ?? "Information"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                var fileName = $"AutoJx2_Settings_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = fileName,
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, json);
                    UpdateStatus("Settings exported successfully");
                    _logger?.LogInformation("Settings exported to: {FilePath}", saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting settings");
                MessageBox.Show($"Error exporting settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (settings != null)
                    {
                        if (settings.TryGetValue("Delay", out var delay) && DelayTextBox != null) 
                            DelayTextBox.Text = delay.ToString();
                        if (settings.TryGetValue("MaxHp", out var maxHp) && MaxHpTextBox != null) 
                            MaxHpTextBox.Text = maxHp.ToString();
                        if (settings.TryGetValue("MaxMp", out var maxMp) && MaxMpTextBox != null) 
                            MaxMpTextBox.Text = maxMp.ToString();
                        
                        UpdateStatus("Settings imported successfully");
                        _logger?.LogInformation("Settings imported from: {FilePath}", openFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing settings");
                MessageBox.Show($"Error importing settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DelayTextBox != null) DelayTextBox.Text = "1000";
                if (MaxHpTextBox != null) MaxHpTextBox.Text = "80";
                if (MaxMpTextBox != null) MaxMpTextBox.Text = "70";
                if (LogLevelComboBox != null) LogLevelComboBox.SelectedIndex = 0;
                
                UpdateStatus("Settings reset to default");
                _logger?.LogInformation("Settings reset to default values");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error resetting settings");
                MessageBox.Show($"Error resetting settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (LogsTextBox != null)
            {
                LogsTextBox.Clear();
                UpdateStatus("Logs cleared");
            }
        }

        private void RefreshLogsButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogs();
        }

        private void SaveLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LogsTextBox == null) return;

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"AutoJx2_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, LogsTextBox.Text);
                    UpdateStatus("Logs saved successfully");
                    _logger?.LogInformation("Logs saved to: {FilePath}", saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving logs");
                MessageBox.Show($"Error saving logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCredentials()
        {
            try
            {
                if (_credentialManager == null) return;

                var credentials = _credentialManager.LoadCredentials();
                if (credentials != null)
                {
                    if (UsernameTextBox != null) UsernameTextBox.Text = credentials.Username;
                    if (PasswordBox != null) PasswordBox.Password = credentials.Password;
                    if (ServerComboBox != null) ServerComboBox.Text = credentials.Server;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading credentials");
            }
        }

        private void LoadSettings()
        {
            try
            {
                // Load settings from configuration or use defaults
                if (DelayTextBox != null) DelayTextBox.Text = "1000";
                if (MaxHpTextBox != null) MaxHpTextBox.Text = "80";
                if (MaxMpTextBox != null) MaxMpTextBox.Text = "70";
                if (LogLevelComboBox != null) LogLevelComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading settings");
            }
        }

        private void RefreshLogs()
        {
            try
            {
                if (LogsTextBox == null) return;

                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (Directory.Exists(logPath))
                {
                    var logFiles = Directory.GetFiles(logPath, "*.log")
                        .OrderByDescending(f => File.GetLastWriteTime(f))
                        .FirstOrDefault();

                    if (logFiles != null)
                    {
                        var logContent = File.ReadAllText(logFiles);
                        LogsTextBox.Text = logContent;
                        LogsTextBox.ScrollToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error refreshing logs");
            }
        }

        private void UpdateStatus(string status)
        {
            if (StatusTextBlock != null)
                StatusTextBlock.Text = $"Status: {status}";
            if (FooterStatusTextBlock != null)
                FooterStatusTextBlock.Text = status;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Register global hotkey (F12)
            RegisterHotkey();
        }

        private void RegisterHotkey()
        {
            // TODO: Implement global hotkey registration
            _logger?.LogInformation("Hotkey registration will be implemented");
        }

        #region Auto Login Methods

        private void LoadGamePath()
        {
            try
            {
                if (_configurationManager != null && GamePathTextBox != null)
                {
                    var gamePath = _configurationManager.GetGamePath();
                    GamePathTextBox.Text = string.IsNullOrEmpty(gamePath) ? "Chưa chọn đường dẫn game" : gamePath;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading game path");
            }
        }

        private void LoadAccounts()
        {
            try
            {
                if (_configurationManager != null && AccountsListView != null)
                {
                    _accounts = _configurationManager.GetAccounts();
                    AccountsListView.ItemsSource = _accounts;
                    UpdateAutoLoginStatus($"Đã tải {_accounts.Count} account");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading accounts");
                UpdateAutoLoginStatus("Lỗi khi tải danh sách account");
            }
        }

        private void BrowseGamePathButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Chọn file so2game.exe",
                    Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                    FileName = "so2game.exe"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    if (GamePathTextBox != null)
                    {
                        GamePathTextBox.Text = openFileDialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error browsing game path");
                MessageBox.Show($"Lỗi khi chọn file game: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGamePathButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configurationManager == null || GamePathTextBox == null)
                    return;

                var gamePath = GamePathTextBox.Text.Trim();
                if (string.IsNullOrEmpty(gamePath) || gamePath == "Chưa chọn đường dẫn game")
                {
                    MessageBox.Show("Vui lòng chọn đường dẫn game trước khi lưu.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(gamePath))
                {
                    MessageBox.Show("File game không tồn tại. Vui lòng kiểm tra lại đường dẫn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _configurationManager.SaveGamePath(gamePath);
                UpdateAutoLoginStatus("Đường dẫn game đã được lưu");
                MessageBox.Show("Đường dẫn game đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving game path");
                MessageBox.Show($"Lỗi khi lưu đường dẫn game: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configurationManager == null || _logger == null)
                    return;

                var loggerFactory = (Application.Current as App)?.ServiceProvider?.GetRequiredService<ILoggerFactory>();
                var addAccountLogger = loggerFactory?.CreateLogger<AddAccountWindow>();
                var addAccountWindow = new AddAccountWindow(_configurationManager, addAccountLogger);
                addAccountWindow.Owner = this;
                addAccountWindow.AccountSaved += (s, account) =>
                {
                    LoadAccounts();
                    UpdateAutoLoginStatus($"Account '{account.Username}' đã được thêm");
                };

                addAccountWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening add account window");
                MessageBox.Show($"Lỗi khi mở cửa sổ thêm account: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
        }

        private void RemoveSelectedAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configurationManager == null || AccountsListView == null)
                    return;

                var selectedItems = new List<Models.GameAccount>();
                foreach (var item in AccountsListView.SelectedItems)
                {
                    if (item is Models.GameAccount account)
                    {
                        selectedItems.Add(account);
                    }
                }
                
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn account cần xóa.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa {selectedItems.Count} account đã chọn?", 
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var account in selectedItems)
                    {
                        _configurationManager.RemoveAccount(account.Id);
                    }
                    LoadAccounts();
                    UpdateAutoLoginStatus($"Đã xóa {selectedItems.Count} account");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing selected accounts");
                MessageBox.Show($"Lỗi khi xóa account: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AccountsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (AccountsListView.SelectedItem is Models.GameAccount account)
                {
                    StartGameForAccount(account);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting game on double click");
            }
        }

        private void StartGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountsListView.SelectedItem is Models.GameAccount account)
                {
                    StartGameForAccount(account);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting game from context menu");
            }
        }

        private void StopGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountsListView.SelectedItem is GameAccount account && account.ProcessId.HasValue)
                {
                    _gameProcessManager?.StopGame(account.ProcessId.Value);
                    UpdateAutoLoginStatus($"Đã dừng game cho account '{account.Username}'");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error stopping game from context menu");
            }
        }

        private void EditAccountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountsListView.SelectedItem is Models.GameAccount account && _configurationManager != null && _logger != null)
                {
                    var loggerFactory = (Application.Current as App)?.ServiceProvider?.GetRequiredService<ILoggerFactory>();
                    var editAccountLogger = loggerFactory?.CreateLogger<AddAccountWindow>();
                    var editAccountWindow = new AddAccountWindow(_configurationManager, editAccountLogger, account);
                    editAccountWindow.Owner = this;
                    editAccountWindow.AccountSaved += (s, updatedAccount) =>
                    {
                        LoadAccounts();
                        UpdateAutoLoginStatus($"Account '{updatedAccount.Username}' đã được cập nhật");
                    };

                    editAccountWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening edit account window");
                MessageBox.Show($"Lỗi khi mở cửa sổ chỉnh sửa account: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleVisibilityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountsListView.SelectedItem is Models.GameAccount account && _configurationManager != null)
                {
                    account.IsHidden = !account.IsHidden;
                    _configurationManager.UpdateAccount(account);
                    LoadAccounts();
                    UpdateAutoLoginStatus($"Account '{account.Username}' đã được {(account.IsHidden ? "ẩn" : "hiện")}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error toggling account visibility");
            }
        }

        private void RemoveAccountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccountsListView.SelectedItem is GameAccount account && _configurationManager != null)
                {
                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa account '{account.Username}'?", 
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        _configurationManager.RemoveAccount(account.Id);
                        LoadAccounts();
                        UpdateAutoLoginStatus($"Account '{account.Username}' đã được xóa");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing account from context menu");
                MessageBox.Show($"Lỗi khi xóa account: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartGameForAccount(Models.GameAccount account)
        {
            try
            {
                if (_configurationManager == null || _gameProcessManager == null)
                    return;

                var gamePath = _configurationManager.GetGamePath();
                if (string.IsNullOrEmpty(gamePath))
                {
                    MessageBox.Show("Vui lòng chọn đường dẫn game trước khi khởi động.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(gamePath))
                {
                    MessageBox.Show("File game không tồn tại. Vui lòng kiểm tra lại đường dẫn game.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var process = _gameProcessManager.StartGame(account, gamePath);
                if (process != null)
                {
                    UpdateAutoLoginStatus($"Đã khởi động game cho account '{account.Username}'");
                }
                else
                {
                    UpdateAutoLoginStatus($"Không thể khởi động game cho account '{account.Username}'");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting game for account: {Username}", account.Username);
                MessageBox.Show($"Lỗi khi khởi động game: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateAutoLoginStatus(string message)
        {
            try
            {
                if (AutoLoginStatusTextBlock != null)
                {
                    AutoLoginStatusTextBlock.Text = message;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating auto login status");
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            _gameAutomation?.Dispose();
            _gameProcessManager?.Dispose();
            base.OnClosed(e);
        }
    }
}
