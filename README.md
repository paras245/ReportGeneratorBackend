# 🔙 Report Generator Backend

This backend is built with **.NET 8** and provides API endpoints for creating reports, a background service for processing them, and SignalR for real-time updates.

## 🏗️ Architecture Overview

The system follows a producer-consumer pattern using a background worker.

1.  **API (Producer)**: Receives a report request (`POST /reports`) and saves it to the database as `Pending`.
2.  **Background Service (Consumer)**: Continuously checks the database for `Pending` items.
3.  **SignalR (Broadcaster)**: Notifies the frontend whenever a report's status changes.

---

## 📂 Key Components

### 1. `Program.cs` (Entry Point)
- Configures **DI (Dependency Injection)**.
- Sets up **SignalR**, **Database**, and **Background Services**.
- Defines **Minimal API endpoints** (`CreateReport`, `GetReports`).

### 2. `BackgroundService/ReportProcessorService.cs`
- This is a long-running service (`IHostedService`).
- It uses a `while` loop to poll the database every 5 seconds.
- **Critical Concept**: It creates a **Service Scope** manually because the Background Service is a Singleton, but the Database Context is Scoped.

### 3. `ReportHub.cs` (SignalR)
- The communication bridge.
- We use `IHubContext<ReportHub>` in the API and Background Service to push messages to clients.

---

## 🚀 How to Run

1.  **Update Connection String** in `appsettings.json`.
2.  **Run Migrations**:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```
3.  **Start the Server**:
    ```bash
    dotnet run
    ```
