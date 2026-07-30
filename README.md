# Enterprise Library Management System

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap_5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

A robust, enterprise-grade Library Management System built with **ASP.NET Core MVC**. This application streamlines the borrowing process, automates inventory tracking, generates high-level analytics, and maintains strict administrative audit logs.

## Key Features

* **Role-Based Authentication**: Distinct, secure portals for `Librarians` (Administrators) and `Students`.
* **Visual Dashboard Analytics**: Real-time insights generated via `Chart.js`, tracking 7-day borrowing histories and live inventory statuses.
* **Transaction Engine**: Automatically detects late returns, tracks overdue metrics, and prevents duplicate book checkouts.
* **Reporting & Exporting**: 
  * Export transaction ledgers to native Excel (`.xlsx`) via **ClosedXML**.
  * Instantly generate high-resolution PDF reports using **html2pdf.js**.
* **System Audit Trails**: A strict Entity Framework Core pipeline interceptor that permanently logs all `Create`, `Edit`, and `Delete` actions performed by administrators.
* **Modern UI/UX**: Fully responsive Sidebar layout, dynamic Dark/Light modes, interactive DataTables, and Toastr/SweetAlert2 notifications.

## Architecture & Design Patterns

This project adheres to **SOLID** principles and modern software architecture guidelines:

* **MVC (Model-View-Controller)**: Strict separation of concerns between data models, HTTP routing logic, and Razor presentation views.
* **Repository Pattern (`IRepository<T>`)**: The Data Access Layer is entirely decoupled from the Controllers. Generic repositories inject directly into controllers via Dependency Injection, eliminating duplicate EF Core logic and enabling high testability.
* **Dependency Injection (DI)**: Services (like `IEmailService` and `IRepository`) are securely bound to the scoped lifecycle in `Program.cs`.
* **Performance Optimizations**: Heavy read operations (like loading the Student Directory) explicitly enforce `.AsNoTracking()` to bypass Entity Framework's change tracker, drastically reducing memory overhead.

## Tech Stack

* **Backend**: C#, .NET 10.0, ASP.NET Core MVC
* **Database**: Microsoft SQL Server
* **ORM**: Entity Framework Core 8.0 (Code-First Migrations)
* **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
* **Libraries**: Chart.js, ClosedXML, Toastr.js, SweetAlert2, html2pdf.js

## Local Setup & Installation

### Prerequisites
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* Microsoft SQL Server (Developer or Express/LocalDB)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/SimpleKartik/Library-Management-System.git
   cd Library-Management-System/LibraryManagement.Web
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Database Configuration**
   Ensure your SQL Server is running. If you are not using a standard local instance, update the `LibraryDbConnection` string in `appsettings.json`.

4. **Run Entity Framework Migrations**
   This will automatically build your database tables and seed them with default test accounts.
   ```bash
   dotnet ef database update --context LibraryDbContext
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```
   Navigate to `http://localhost:<port>` in your web browser.

## Default Credentials

The database is seeded with two default accounts for immediate testing:

**Librarian (Admin)**
* Username: `admin`
* Password: `admin123`

**Student**
* Username: `student1`
* Password: `pass123`
