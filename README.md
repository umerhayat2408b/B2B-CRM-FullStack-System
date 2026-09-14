# 🚀 B2B CRM & Lead Management System

A high-performance, full-stack enterprise **B2B CRM (Customer Relationship Management) & Lead Management System** designed to streamline business operations, automate lead tracking, and manage role-based user access.

---

## 📹 Project Demo & Walkthrough

https://github.com/user-attachments/assets/0bab84ce-01b0-4197-9082-175fda33e94e

https://github.com/user-attachments/assets/a1f9c605-7a6a-4716-aceb-c255af694f73

---

## 🌟 Key Features

### 🔐 1. Authentication & Security
* **JWT Token Authentication:** Secure user sessions and API endpoint authorization.
* **Role-Based Access Control (RBAC):** Custom permissions for Admin, Sales Executives, and Managers.

### 💼 2. Lead & Sales Pipeline Management
* Dynamic status updates for leads (New, In Progress, Contacted, Closed/Won, Lost).
* Real-time lead tracking and client communication updates.

### ⏱️ 3. Background Job Processing
* Integrated **Hangfire** for automated background job scheduling, notification dispatches, and system cleanups without blocking the main thread.

### 📊 4. Data Auditing & Operations
* Real-time search, sorting, and pagination across lead records.
* Integrated audit logging for tracking key system actions.

---

## 🛠️ Tech Stack & Architecture

### **Backend (.NET Core API)**
* **Framework:** ASP.NET Core 8 Web API
* **Language:** C#
* **Database:** Microsoft SQL Server
* **ORM:** Entity Framework Core (Code-First Migration)
* **Background Jobs:** Hangfire
* **Security:** JSON Web Tokens (JWT) & ASP.NET Identity

### **Frontend (Angular)**
* **Framework:** Angular 17+
* **Styling:** Bootstrap 5 & Custom CSS
* **HTTP Client:** REST API integration with RxJS Observables

---

## 📂 Project Structure

```text
B2B-CRM-FullStack-System/
├── B2B_Backend/      # ASP.NET Core 8 Web API Solution
└── B2B_frontend/     # Angular Frontend Application
