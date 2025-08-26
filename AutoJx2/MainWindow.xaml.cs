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

namespace AutoJx2
{
    public partial class MainWindow : Window
    {
        private ILogger<MainWindow>? _logger;
        private ICredentialManager? _credentialManager;
        private IGameAutomation? _gameAutomation;
        private readonly DispatcherTimer _timer;
        private Process? _attachedProcess;
        private bool _isAutomationRunning = false;
        private DateTime _startTime;

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
            }
            
            // Khởi tạo giao diện
            InitializeUI();
        }

        public MainWindow(ILogger<MainWindow> logger, ICredentialManager credentialManager, IGameAutomation gameAutomation)
        {
            InitializeComponent();
            
            _logger = logger;
            _credentialManager = credentialManager;
            _gameAutomation = gameAutomation;
            
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

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            _gameAutomation?.Dispose();
            base.OnClosed(e);
        }
    }
}
