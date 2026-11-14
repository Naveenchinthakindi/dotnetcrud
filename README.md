# Student CRUD API (.NET 8 & MySQL - Beginner Friendly)

A simple, straightforward RESTful Web API built with **ASP.NET Core (.NET 8)** and **MySQL** demonstrating basic CRUD (Create, Read, Update, Delete) operations on student records.

This version is designed specifically for **learning the fundamentals** before moving on to complex multi-layered architectures:
- **Direct Controller Implementation**: All SQL queries and HTTP actions are written directly inside the controller so you can clearly see the flow of data.
- **One DB Connection File**: A simple `DbConnection` helper provides connections to your MySQL database.
- **Single Model File Per Controller/Entity**: All request and response classes related to an entity/controller reside in one single file (`Models/Student.cs`), keeping everything organized and clean without nested folders.

---

## 🛠️ Tech Stack

- **Framework**: .NET 8 Web API
- **Database**: MySQL (using `MySql.Data` ADO.NET)
- **API Documentation**: Swagger / OpenAPI (Swashbuckle)

---

## 📋 Prerequisites

Before running the project locally:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
   - Verify with: `dotnet --version`
2. **[MySQL Server](https://dev.mysql.com/downloads/mysql/)** (v8.0+ or MariaDB)
3. **Git**

---

## 🚀 Getting Started

### 1. Database Setup

Open MySQL (Workbench, terminal CLI, or phpMyAdmin) and run:

```sql
CREATE DATABASE IF NOT EXISTS studentdb;
USE studentdb;

CREATE TABLE IF NOT EXISTS students (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL,
    age INT NOT NULL
);
```

*(Optional) Seed sample data:*
```sql
INSERT INTO students (name, email, age) VALUES
('John Doe', 'john.doe@example.com', 21),
('Jane Smith', 'jane.smith@example.com', 23);
```

---

### 2. Configure Database Connection

Open `appsettings.json` and set your MySQL username and password:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=studentdb;Uid=YOUR_USER;Pwd=YOUR_PASSWORD;"
  }
}
```

---

### 3. Build & Run

```bash
dotnet restore
dotnet build
dotnet run
```

---

## 📖 API Documentation & Testing

### Swagger UI
Open your browser to:
- **HTTP**: [http://localhost:5095/swagger](http://localhost:5095/swagger)
- **HTTPS**: [https://localhost:7011/swagger](https://localhost:7011/swagger)

---

## 🛣️ API Endpoints

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/Student` | Get all students |
| **GET** | `/api/Student/{id}` | Get student by ID |
| **POST** | `/api/Student` | Add a new student |
| **PUT** | `/api/Student/{id}` | Update student by ID |
| **DELETE** | `/api/Student/{id}` | Delete student by ID |

---

## 📁 Project Structure (One Model File Per Feature)

```
StudentApi/
├── Controllers/
│   ├── StudentController.cs      # All CRUD queries & logic for Student
│   └── (Future: TeacherController.cs, CourseController.cs, etc.)
│
├── Data/
│   └── DbConnection.cs           # Single helper file for MySQL connection
│
├── Models/
│   ├── Student.cs                # Contains Student, AddStudentRequest, UpdateStudentRequest, StudentResponse
│   └── (Future: Teacher.cs, Course.cs, etc.)
│
├── Properties/
│   └── launchSettings.json       # Local server launch configuration
├── appsettings.json              # MySQL connection string
├── Program.cs                    # Registers DbConnection & Swagger
├── StudentApi.csproj             # Dependencies
├── StudentApi.http               # Quick HTTP test requests
└── README.md                     # Setup instructions
```
