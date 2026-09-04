# Masareef — Personal & Business Finance Management API

A full-stack finance management system for tracking household expenses, small-business income and expenses, product inventory and sales, and debts with payment schedules. The **backend is the core of this project**: a layered ASP.NET Core Web API built with a focus on clean architecture, security, and query performance.

---

## ✨ Highlights

- **11 entities + 1 database view**, **~92 RESTful endpoints**, **38 DTOs**, and **19 targeted database indexes** (composite & filtered).
- Layered architecture (**API → Business → Data Access**) with the **Repository** and **Service** patterns and **Dependency Injection** throughout.
- **JWT authentication** with **BCrypt** password hashing and controller-level authorization.
- **Performance-tuned EF Core**: N+1 elimination via SQL-side aggregation, DTO projection, `AsNoTracking()`, and optimistic concurrency (`rowversion`).
- Interactive **Swagger / OpenAPI** documentation with JWT support.

---

## 🛠️ Tech Stack

**Backend**
- C# · .NET 8
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- JWT Bearer Authentication · BCrypt
- Swagger / OpenAPI

**Frontend**
- React 18 · Vite · React Router · Recharts

**Architecture & Patterns**
- Layered / N-Tier Architecture
- Repository Pattern · Service Layer Pattern
- Dependency Injection · Result Pattern
- SOLID principles (SRP, DIP)

---

## 🏗️ Architecture

```
┌─────────────────┐
│   API Layer     │  Controllers, JWT auth, HTTP mapping
├─────────────────┤
│  Business Layer │  Services, validation, business rules (Result<T>)
├─────────────────┤
│    Data Layer   │  Repositories, EF Core DbContext
├─────────────────┤
│    Entities     │  Domain models + DTOs
└─────────────────┘
```

Each layer depends only on the abstraction (interface) of the layer below it, keeping controllers free of data-access code and business rules out of the persistence layer.

---

## 🔑 Key Features

- **Household finance** — track home income and expenses by category with filtering and search.
- **Business management** — manage multiple businesses, each with its own income, expenses, products, and sales.
- **Inventory & sales** — product stock, cost/selling price, and per-sale profit tracking with soft-delete for auditability.
- **Debt management** — create debts, record payments, auto-update status, and write off debts with reasons.
- **Reports & dashboard** — aggregated financial summaries computed at the database level.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express is fine)
- Node.js 18+ (for the frontend)

### Backend setup

```bash
# 1. Clone the repo
git clone https://github.com/<your-username>/masareef.git
cd masareef

# 2. Configure secrets (do NOT commit these — see note below)
dotnet user-secrets init --project Masareef.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=MasareefDB;Trusted_Connection=True;TrustServerCertificate=True;" --project Masareef.Api
dotnet user-secrets set "Jwt:Key" "<your-random-32+-char-secret>" --project Masareef.Api

# 3. Apply the database (if migrations are included)
dotnet ef database update --project Masareef.Api

# 4. Run the API
dotnet run --project Masareef.Api
```

The API and Swagger UI will be available at `https://localhost:<port>/swagger`.

### Frontend setup

```bash
cd frontend
npm install
npm run dev
```

> ⚠️ **Security note:** The JWT signing key and database connection string are **never committed** to source control. They are stored via .NET **User Secrets** in development and **environment variables** in production.

---

## 📖 API Documentation

Once running, open `/swagger` for interactive documentation. Protected endpoints require a JWT — log in via `POST /api/auth/login`, then click **Authorize** in Swagger and paste the token.

---

## 🎨 A Note on the Frontend

This project's focus and my contribution is the **backend** — the API, architecture, database design, and performance work described above. The **React frontend was not built by me**; it was added as a ready-made client to demonstrate the API end-to-end. All backend design decisions, code, and optimizations are my own work.

---

## 📊 Project Stats

| Metric | Count |
|---|---|
| Entities (tables) | 11 (+1 view) |
| REST endpoints | ~92 |
| Controllers | 7 |
| Services | 8 |
| Repositories | 7 |
| DTOs | 38 |
| Database indexes | 19 |


# مصاريف — الواجهة الأمامية (Frontend)

واجهة React عصرية لنظام مصاريف، مربوطة بالـ API بتاعك عبر JWT.

## المتطلّبات
- Node.js (النسخة 18 أو أحدث) — نزّله من https://nodejs.org

## التشغيل (٣ خطوات)

1. افتح Terminal داخل مجلد `masareef-frontend` ونفّذ:
```
npm install
```
(بيحمّل المكتبات — مرة واحدة بس)

2. شغّل الباكند بتاعك (مشروع Masareef.Api) على `http://localhost:5238`

3. شغّل الواجهة:
```
npm run dev
```

هيفتح على `http://localhost:5173` — سجّل دخول بحساب موجود، أو اعمل حساب جديد.

## لو الباكند على بورت مختلف
غيّر العنوان في ملف `vite.config.js` (السطر `target`).

## الصفحات
- **تسجيل الدخول / إنشاء حساب** — أول شاشة
- **اللوحة** — نظرة عامة على الوضع المالي
- **البيت** — المصاريف والدخل + إضافة
- **البيزنس** — المحلات وأرباحها + إضافة
- **الديون** — اللي عليك واللي ليك + دفعات + تفاصيل
- **التقارير** — رسوم بيانية للإنفاق والاتجاه

## ملاحظات
- التوكن بيتخزّن في المتصفح، فمش هتحتاج تسجّل دخول كل مرة
- لو الجلسة انتهت (بعد ساعتين)، بيرجّعك لصفحة الدخول تلقائياً
