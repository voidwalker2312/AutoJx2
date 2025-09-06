# AutoJx2 – Kiến trúc và Luồng Hoạt Động

Tài liệu mô tả cấu trúc thành phần, dữ liệu, và các luồng chính (khởi động, Auto Login, attach + automation) của ứng dụng.

## 1) Tổng Quan Kiến Trúc

```mermaid
graph LR
  U[Người dùng] -->|WPF UI| MW[MainWindow]
  subgraph Services
    CFG[IConfigurationManager<br/>ConfigurationManager]
    CRE[ICredentialManager<br/>CredentialManager]
    GPM[IGameProcessManager<br/>GameProcessManager]
    AUT[IGameAutomation<br/>GameAutomation (FlaUI)]
  end
  MW --> CFG
  MW --> CRE
  MW --> GPM
  MW --> AUT

  subgraph External
    FL[UIA3 / FlaUI]
    PROC[so2game.exe]
    WCM[Windows Credential Manager]
    DPAPI[Windows DPAPI]
    LOGS[Serilog -> logs/*.log]
  end
  AUT --> FL
  AUT --> PROC
  CRE --> WCM
  CRE --> DPAPI
  MW --> LOGS
```

Thành phần chính và vai trò:
- MainWindow: Điều phối UI, gọi services, cập nhật trạng thái.
- ConfigurationManager: Lưu/đọc `config.json` (đường dẫn game, accounts, settings).
- CredentialManager: Lưu thông tin đăng nhập an toàn (WCM + DPAPI).
- GameProcessManager: Mở/đóng, theo dõi process game theo từng account.
- GameAutomation: Attach vào process (FlaUI), chạy vòng lặp automation.

Tập tin liên quan:
- AutoJx2/App.xaml.cs:17 – Startup, DI, Serilog.
- AutoJx2/MainWindow.xaml(.cs) – UI chính và event handlers.
- AutoJx2/Services/* – Nghiệp vụ.
- AutoJx2/Models/* – Model dữ liệu.

## 2) Khởi Động Ứng Dụng

```mermaid
sequenceDiagram
  participant App as App.xaml.cs
  participant DI as ServiceCollection
  participant MW as MainWindow
  App->>App: OnStartup()
  App->>App: Configure Serilog (logs/autojx2-.log)
  App->>DI: ConfigureServices(AddSingleton services)
  App->>MW: new MainWindow(services...)
  MW->>MW: InitializeUI()
  MW->>MW: LoadCredentials/LoadSettings/LoadGamePath/LoadAccounts
  MW->>MW: RefreshProcesses()
```

## 3) Auto Login – Bắt Đầu Game Theo Account

```mermaid
sequenceDiagram
  participant UI as MainWindow
  participant CFG as ConfigurationManager
  participant GPM as GameProcessManager
  participant P as Process(so2game.exe)

  UI->>CFG: GetGamePath()
  alt đường dẫn hợp lệ
    UI->>GPM: StartGame(account, gamePath)
    GPM->>GPM: Kiểm tra tồn tại file, tránh trùng process
    GPM->>P: Process.Start(gamePath)
    GPM-->>UI: Trả về Process + cập nhật account(ProcessId/IsRunning)
    P-->>GPM: Exited (sự kiện)
    GPM-->>UI: GameStopped event (dọn trạng thái)
  else không hợp lệ
    UI-->>UI: Thông báo lỗi/nhắc chọn lại đường dẫn
  end
```

Điểm móc:
- Start: AutoJx2/MainWindow.xaml.cs:716 → `StartGameForAccount`.
- Logic: AutoJx2/Services/GameProcessManager.cs:30 → `StartGame`.

## 4) Attach + Automation Loop (FlaUI)

```mermaid
flowchart TD
  A[Chọn process trong ComboBox] --> B[AttachProcessButton_Click]
  B --> C[IGameAutomation.AttachToProcess(process)]
  C --> D[Application.Attach + GetMainWindow(UIA3)]
  D --> E[StartAutomationButton_Click]
  E --> F[StartAutomation]
  F --> G{_isRunning}
  G -->|true| H[Task.Run RunAutomationLoop]
  H --> I[PerformAutomationTasks theo flags]
  I --> J[Delay theo _delay]
  J --> H
  G -->|stop| K[StopAutomation - Cancel token]
```

Trích dẫn:
- Attach: AutoJx2/MainWindow.xaml.cs:140 → `_gameAutomation.AttachToProcess(process)`.
- Vòng lặp: AutoJx2/Services/GameAutomation.cs:122 → `RunAutomationLoop`.

## 5) Mô Hình Dữ Liệu

```mermaid
classDiagram
  class AppConfiguration {
    string GamePath
    List~GameAccount~ Accounts
    AutomationSettings Automation
    UISettings UI
  }
  class GameAccount {
    string Id
    string Username
    string Password  // base64 từ DPAPI
    string Server
    DateTime CreatedAt
    DateTime LastUpdated
    bool IsHidden
    string? Notes
    int? ProcessId
    bool IsRunning
  }
  class GameCredentials {
    string Username
    string Password  // DPAPI bảo vệ
    string Server
    DateTime CreatedAt
    DateTime LastUpdated
    string? Notes
  }
  class AutomationSettings {
    int Delay
    int MaxHpThreshold
    int MaxMpThreshold
    bool AutoFarming
    bool AutoQuest
    bool AutoCombat
    bool AutoHeal
  }
  class UISettings {
    string LogLevel
    string Hotkey
    bool AutoStartWithWindows
    bool MinimizeToTray
  }
  AppConfiguration --> GameAccount
  AppConfiguration --> AutomationSettings
  AppConfiguration --> UISettings
```

Nguồn:
- AutoJx2/Models/AppConfiguration.cs
- AutoJx2/Models/GameAccount.cs
- AutoJx2/Models/GameCredentials.cs

## 6) Bảo Mật & Lưu Trữ
- Mật khẩu account (trong config) được mã hóa bằng DPAPI ở phía UI trước khi lưu.
- Thông tin đăng nhập chung (tab Credentials) lưu trong Windows Credential Manager, blob JSON, DPAPI bảo vệ.
- Log ghi bằng Serilog: `logs/autojx2-*.log` (theo ngày, giữ tối đa 7 file).

## 7) Gợi Ý Render Diagram
- GitHub hiện hỗ trợ Mermaid trực tiếp trong Markdown.
- Trong VS Code, cài “Markdown Preview Mermaid Support” để xem sơ đồ ngay trong IDE.
- Nếu cần ảnh PNG/SVG, có thể dùng `mmdc` (Mermaid CLI) để xuất ảnh từ các khối Mermaid ở trên.

