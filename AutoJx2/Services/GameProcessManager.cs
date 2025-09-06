using AutoJx2.Interfaces;
using AutoJx2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoJx2.Services
{
    /// <summary>
    /// Service quản lý process game
    /// </summary>
    public class GameProcessManager : IGameProcessManager, IDisposable
    {
        private readonly ILogger<GameProcessManager> _logger;
        private readonly Dictionary<string, Process> _accountProcesses;
        private readonly object _lockObject = new object();
        private bool _disposed = false;

        public event EventHandler<GameProcessEventArgs>? GameStarted;
        public event EventHandler<GameProcessEventArgs>? GameStopped;
        public event EventHandler<string>? ErrorOccurred;

        public GameProcessManager(ILogger<GameProcessManager> logger)
        {
            _logger = logger;
            _accountProcesses = new Dictionary<string, Process>();
        }

        public Process? StartGame(Models.GameAccount account, string gamePath)
        {
            try
            {
                lock (_lockObject)
                {
                    // Kiểm tra xem game đã chạy cho account này chưa
                    if (_accountProcesses.ContainsKey(account.Id))
                    {
                        var existingProcess = _accountProcesses[account.Id];
                        if (!existingProcess.HasExited)
                        {
                            _logger.LogWarning("Game is already running for account: {Username}", account.Username);
                            return existingProcess;
                        }
                        else
                        {
                            // Process đã thoát, xóa khỏi dictionary
                            _accountProcesses.Remove(account.Id);
                        }
                    }

                    // Kiểm tra file game có tồn tại không
                    if (!File.Exists(gamePath))
                    {
                        var error = $"Game file not found: {gamePath}";
                        _logger.LogError(error);
                        OnErrorOccurred(error);
                        return null;
                    }

                    // Khởi động process
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = gamePath,
                        WorkingDirectory = Path.GetDirectoryName(gamePath),
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    var process = Process.Start(startInfo);
                    if (process == null)
                    {
                        var error = "Failed to start game process";
                        _logger.LogError(error);
                        OnErrorOccurred(error);
                        return null;
                    }

                    // Lưu process vào dictionary
                    _accountProcesses[account.Id] = process;

                    // Cập nhật thông tin account
                    account.ProcessId = process.Id;
                    account.IsRunning = true;

                    // Đăng ký sự kiện process exit
                    process.EnableRaisingEvents = true;
                    process.Exited += (sender, e) => OnProcessExited(account, process);

                    _logger.LogInformation("Game started for account: {Username} (PID: {ProcessId})", 
                        account.Username, process.Id);

                    OnGameStarted(account, process);
                    return process;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game for account: {Username}", account.Username);
                OnErrorOccurred($"Failed to start game: {ex.Message}");
                return null;
            }
        }

        public void StopGame(int processId)
        {
            try
            {
                lock (_lockObject)
                {
                    var accountEntry = _accountProcesses.FirstOrDefault(kvp => kvp.Value.Id == processId);
                    if (accountEntry.Key == null)
                    {
                        _logger.LogWarning("Process not found: {ProcessId}", processId);
                        return;
                    }

                    var process = accountEntry.Value;
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000); // Đợi tối đa 5 giây
                    }

                    _accountProcesses.Remove(accountEntry.Key);
                    _logger.LogInformation("Game stopped: PID {ProcessId}", processId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping game: PID {ProcessId}", processId);
                OnErrorOccurred($"Failed to stop game: {ex.Message}");
            }
        }

        public void StopAllGames()
        {
            try
            {
                lock (_lockObject)
                {
                    var processesToStop = _accountProcesses.Values.ToList();
                    foreach (var process in processesToStop)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                process.WaitForExit(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error stopping process: PID {ProcessId}", process.Id);
                        }
                    }

                    _accountProcesses.Clear();
                    _logger.LogInformation("All games stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping all games");
                OnErrorOccurred($"Failed to stop all games: {ex.Message}");
            }
        }

        public List<Process> GetRunningGameProcesses()
        {
            lock (_lockObject)
            {
                return _accountProcesses.Values.Where(p => !p.HasExited).ToList();
            }
        }

        public bool IsGameRunning(int processId)
        {
            lock (_lockObject)
            {
                var process = _accountProcesses.Values.FirstOrDefault(p => p.Id == processId);
                return process != null && !process.HasExited;
            }
        }

        public Process? GetProcessByAccount(string accountId)
        {
            lock (_lockObject)
            {
                if (_accountProcesses.TryGetValue(accountId, out var process) && !process.HasExited)
                {
                    return process;
                }
                return null;
            }
        }

        private void OnProcessExited(Models.GameAccount account, Process process)
        {
            try
            {
                lock (_lockObject)
                {
                    _accountProcesses.Remove(account.Id);
                    account.ProcessId = null;
                    account.IsRunning = false;
                }

                _logger.LogInformation("Game process exited for account: {Username} (PID: {ProcessId})", 
                    account.Username, process.Id);

                OnGameStopped(account, process);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling process exit");
            }
        }

        private void OnGameStarted(Models.GameAccount account, Process process)
        {
            GameStarted?.Invoke(this, new GameProcessEventArgs
            {
                Account = account,
                Process = process
            });
        }

        private void OnGameStopped(Models.GameAccount account, Process process)
        {
            GameStopped?.Invoke(this, new GameProcessEventArgs
            {
                Account = account,
                Process = process
            });
        }

        private void OnErrorOccurred(string error)
        {
            ErrorOccurred?.Invoke(this, error);
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
                StopAllGames();
                _disposed = true;
            }
        }
    }
}
