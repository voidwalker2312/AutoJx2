using AutoJx2.Interfaces;
using AutoJx2.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace AutoJx2.Services
{
    /// <summary>
    /// Service quản lý cấu hình ứng dụng
    /// </summary>
    public class ConfigurationManager : IConfigurationManager, IDisposable
    {
        private readonly ILogger<ConfigurationManager> _logger;
        private readonly string _configFilePath;
        private Models.AppConfiguration _configuration;
        private bool _disposed = false;

        public ConfigurationManager(ILogger<ConfigurationManager> logger)
        {
            _logger = logger;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            _configuration = new Models.AppConfiguration();
            LoadConfiguration();
        }

        public void SaveGamePath(string gamePath)
        {
            try
            {
                _configuration.GamePath = gamePath;
                SaveConfiguration(_configuration);
                _logger.LogInformation("Game path saved: {GamePath}", gamePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game path");
                throw;
            }
        }

        public string? GetGamePath()
        {
            return string.IsNullOrEmpty(_configuration.GamePath) ? null : _configuration.GamePath;
        }

        public void SaveAccounts(List<Models.GameAccount> accounts)
        {
            try
            {
                _configuration.Accounts = accounts;
                SaveConfiguration(_configuration);
                _logger.LogInformation("Accounts saved. Count: {Count}", accounts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving accounts");
                throw;
            }
        }

        public List<Models.GameAccount> GetAccounts()
        {
            return _configuration.Accounts ?? new List<Models.GameAccount>();
        }

        public void AddAccount(Models.GameAccount account)
        {
            try
            {
                if (_configuration.Accounts == null)
                    _configuration.Accounts = new List<Models.GameAccount>();

                // Kiểm tra xem username đã tồn tại chưa
                if (_configuration.Accounts.Any(a => a.Username.Equals(account.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Account with username '{account.Username}' already exists");
                }

                _configuration.Accounts.Add(account);
                SaveConfiguration(_configuration);
                _logger.LogInformation("Account added: {Username}", account.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding account");
                throw;
            }
        }

        public void UpdateAccount(Models.GameAccount account)
        {
            try
            {
                if (_configuration.Accounts == null)
                    throw new InvalidOperationException("No accounts found");

                var existingAccount = _configuration.Accounts.FirstOrDefault(a => a.Id == account.Id);
                if (existingAccount == null)
                    throw new InvalidOperationException($"Account with ID '{account.Id}' not found");

                // Cập nhật thông tin
                existingAccount.Username = account.Username;
                existingAccount.Password = account.Password;
                existingAccount.Server = account.Server;
                existingAccount.LastUpdated = DateTime.Now;
                existingAccount.IsHidden = account.IsHidden;
                existingAccount.Notes = account.Notes;

                SaveConfiguration(_configuration);
                _logger.LogInformation("Account updated: {Username}", account.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account");
                throw;
            }
        }

        public void RemoveAccount(string accountId)
        {
            try
            {
                if (_configuration.Accounts == null)
                    return;

                var account = _configuration.Accounts.FirstOrDefault(a => a.Id == accountId);
                if (account == null)
                    return;

                _configuration.Accounts.Remove(account);
                SaveConfiguration(_configuration);
                _logger.LogInformation("Account removed: {Username}", account.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing account");
                throw;
            }
        }

        public void SaveConfiguration(Models.AppConfiguration config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configFilePath, json);
                _configuration = config;
                _logger.LogInformation("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
                throw;
            }
        }

        public Models.AppConfiguration GetConfiguration()
        {
            return _configuration;
        }

        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    _logger.LogInformation("Configuration file not found, using default configuration");
                    return;
                }

                var json = File.ReadAllText(_configFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var config = JsonSerializer.Deserialize<Models.AppConfiguration>(json, options);
                if (config != null)
                {
                    _configuration = config;
                    _logger.LogInformation("Configuration loaded successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration, using default configuration");
                _configuration = new Models.AppConfiguration();
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
}
