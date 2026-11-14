# Student CRUD API (.NET 8 & MySQL)

A RESTful Web API built with **ASP.NET Core (.NET 8)** and **MySQL** demonstrating CRUD (Create, Read, Update, Delete) operations on student records using a clean layered architecture (Controller -> Service -> Data Access Layer -> MySQL Database).

---

## 🛠️ Tech Stack & Features

- **Framework**: .NET 8 Web API
- **Database**: MySQL (using `MySql.Data` client)
- **Architecture**: 3-Tier Layered Architecture (Controllers, Services, DAL)
- **API Documentation**: Swagger / OpenAPI (Swashbuckle)
- **Asynchronous**: Fully `async`/`await` database access

---

## 📋 Prerequisites

Before running the project locally, make sure you have installed:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
   - Check with: `dotnet --version` (should show `8.x.x`)
2. **[MySQL Server](https://dev.mysql.com/downloads/mysql/)** (v8.0 or above / MariaDB)
3. **Git**
4. Optional: Visual Studio 2022 / VS Code / Rider / Postman

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Naveenchinthakindi/dotnetcrud.git
cd dotnetcrud
```

*(If you cloned into a folder containing `StudentApi`, navigate into `StudentApi`)*:
```bash
cd StudentApi
```

---

### 2. Database Setup

1. Open your MySQL client (MySQL Workbench, phpMyAdmin, or terminal CLI).
2. Execute the following script to create the database and `students` table:

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

*(Optional) Insert sample data:*
```sql
INSERT INTO students (name, email, age) VALUES
('John Doe', 'john.doe@example.com', 21),
('Jane Smith', 'jane.smith@example.com', 23);
```

---

### 3. Configure Database Connection

Open `appsettings.json` (or `appsettings.Development.json`) and update the connection string with your local MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=studentdb;Uid=YOUR_USER;Pwd=YOUR_PASSWORD;"
  }
}
```

> **Note**: Replace `YOUR_USER` (e.g., `root`) and `YOUR_PASSWORD` with your actual MySQL database credentials.

---

### 4. Restore Dependencies & Build

```bash
dotnet restore
dotnet build
```

---

### 5. Run the Application

```bash
dotnet run
```

When the application starts, you will see output similar to:
```
Now listening on: http://localhost:5095
Now listening on: https://localhost:7011
```

---

## 📖 API Documentation & Testing

### Swagger UI
Open your browser and navigate to:
- **HTTP**: [http://localhost:5095/swagger](http://localhost:5095/swagger)
- **HTTPS**: [https://localhost:7011/swagger](https://localhost:7011/swagger)

You can view all endpoints and execute requests directly from the Swagger interactive interface.

---

## 🛣️ API Endpoints

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/Student` | Retrieve all students |
| **GET** | `/api/Student/{id}` | Retrieve a student by ID |
| **POST** | `/api/Student` | Add a new student |
| **PUT** | `/api/Student/{id}` | Update an existing student |
| **DELETE** | `/api/Student/{id}` | Delete a student by ID |

### Sample Request Payloads

#### Create Student (`POST /api/Student`)
```json
{
  "name": "Alex Johnson",
  "email": "alex.johnson@example.com",
  "age": 22
}
```

#### Update Student (`PUT /api/Student/{id}`)
```json
{
  "name": "Alex J. Updated",
  "email": "alex.updated@example.com",
  "age": 23
}
```

You can also use the included **`StudentApi.http`** file inside VS Code (with the REST Client extension) or Visual Studio to test endpoints with one click.

---

## 📁 Project Architecture

```
StudentApi/
├── Controllers/
│   └── StudentController.cs      # API Endpoints & HTTP handling
├── DAL/
│   ├── Implementations/
│   │   ├── MySqlConnectionFactory.cs # DB connection provider
│   │   └── StudentDataAccess.cs      # Direct MySQL queries & mapping
│   └── Interfaces/
│       └── IStudentDataAccess.cs
├── Models/
│   ├── Entities/
│   │   └── Student.cs            # Core Student entity
│   ├── Request/
│   │   ├── AddStudentRequest.cs  # Payload for creating student
│   │   └── UpdateStudentRequest.cs# Payload for updating student
│   └── Response/
│       └── StudentResponse.cs    # Output DTO
├── Properties/
│   └── launchSettings.json       # Local runtime launch profiles
├── Services/
│   ├── Implementations/
│   │   └── StudentService.cs     # Business logic & DTO transformations
│   └── Interfaces/
│       └── IStudentService.cs
├── appsettings.json              # Connection strings & configurations
├── Program.cs                    # App bootstrap & DI container registration
├── StudentApi.csproj             # Project dependencies & metadata
└── StudentApi.http               # HTTP test requests
```

---

## 🤝 Contributing
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request
