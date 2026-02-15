# 🔙 Report Generator Backend

This backend is built with **.NET 8** and provides API endpoints for creating reports, a background service for processing them, and SignalR for real-time updates.

## 🏗️ Architecture Overview

The system follows a producer-consumer pattern using a background worker.

1.  **API (Producer)**: Receives a report request (`POST /reports`) and saves it to the database as `Pending`.
2.  **Background Service (Consumer)**: Continuously checks the database for `Pending` items.
3.  **SignalR (Broadcaster)**: Notifies the frontend whenever a report's status changes.

---

## 📂 Key Components & Concepts

### 1. API Endpoints (`Program.cs`)
The application exposes two main Minimal API endpoints:
- **`POST /reports`**:
    - Accepts a report request (Date Range, Type).
    - Validates input and saves a new job to the database with status `Pending`.
    - **Crucial Step**: It immediately triggers a SignalR notification so all connected clients see the new job instantly without refreshing.
- **`GET /reports`**:
    - Fetches the history of all report jobs from the database, sorted by newest first.

### 2. SignalR & `ReportHub.cs`
**What is SignalR?**
SignalR is a library that enables real-time web functionality. It allows the server to push content to connected clients instantly, rather than the client having to request updates (polling).

**Role of `ReportHub`**:
- It acts as the "meeting point" for websocket connections.
- In this app, we use it to broadcast `ReceiveUpdate` messages to all connected clients whenever a report status changes (Pending → Processing → Completed).

### 3. Background Worker (`ReportProcessorService.cs`)
- A simpler alternative to external job queues (like Hangfire).
- Runs continuously in the background using a `while` loop.
- **Process**:
    1. Checks DB for oldest `Pending` job.
    2. Updates status to `Processing` → Notifies Client via SignalR.
    3. Simulates work (5-second delay).
    4. Updates status to `Completed` → Notifies Client via SignalR.

---

## 🚀 How to Run

1.  **Configure Database**:
    Update `DefaultConnection` in `appsettings.json` with your SQL Server connection string.

2.  **Setup Database (Migrations)**:
    > **⚠️ IMPORTANT**: If a `Migrations` folder already exists in the project, **DELETE IT** first. This ensures you start with a clean slate and avoid definition conflicts.

    Open your terminal in the project folder and run:
    ```bash
    add-migration initial
    update-database
    ```

3.  **Start the Server**:
    ```bash
    dotnet run
    ```
