# SkillSnap

SkillSnap is a full-stack web application built with ASP.NET Core and Blazor, designed to manage user skills and projects.

## 📌 Overview

The application allows users to manage their skills and projects, providing a simple platform to showcase competencies and track development.

The system is built using a client-server architecture with a Blazor WebAssembly frontend and an ASP.NET Core Web API backend.

---

## 🏗️ Architecture

The project follows a **client-server architecture**:

### 🔹 Client (Blazor WebAssembly)
- Handles UI rendering  
- Calls backend APIs using `HttpClient`  
- Manages authentication state  
- Uses scoped services for state management (e.g., user session)

### 🔹 Server (ASP.NET Core API)
- Exposes REST endpoints  
- Handles business logic  
- Secures endpoints using authentication and authorization  
- Implements caching for performance optimization  

### 🔹 Data Layer
- Entity Framework Core  
- SQLite database  
- DbContext for data access  

---

## 🔐 Authentication & Authorization

- Custom authentication implementation  
- `AuthenticationStateProvider` used in Blazor  
- JWT stored in browser (via LocalStorage service)  
- Role-based or authenticated access to API endpoints  

---

## ⚙️ Features

### 👤 User Management
- User login  
- Persistent session using client-side state  

### 🧠 Skills
- Retrieve list of skills from API  
- Display skills dynamically in UI  
- Cached on server to improve performance  

### 📁 Projects
- Retrieve projects from API  
- Display project data in Blazor components  

---

## 🚀 Performance Optimization

- **IMemoryCache (Server-side)**
  - Caches frequently requested data (e.g., skills)  
  - Reduces database queries  

- **Basic performance measurement**
  - `Stopwatch` used to measure request duration  
  - Logging for cache hits and misses  

---

## 🧪 Testing and Validation

- Manual testing via browser and API endpoints  
- Validation of data flow between client and server  

> Note: Automated testing is not implemented yet.

---

## 🔧 Technologies Used

- .NET 8  
- ASP.NET Core Web API  
- Blazor WebAssembly  
- Entity Framework Core  
- SQLite  
- IMemoryCache  

---

## 📂 Project Structure
SkillSnap/
│
├── Client/ # Blazor WebAssembly frontend
├── Server/ # ASP.NET Core Web API
├── Shared/ # Shared models between client and server


---

## ⚡ State Management (Blazor)
---

## ⚡ State Management (Blazor)

- Scoped services used to persist user session data  
- Avoids unnecessary reloads between components  

---

## 📈 Future Improvements

- Implement full authentication with refresh tokens  
- Add CRUD operations for skills and projects  
- Introduce automated testing  
- Improve UI/UX  
- Add global error handling  
- Implement CI/CD pipelines  

