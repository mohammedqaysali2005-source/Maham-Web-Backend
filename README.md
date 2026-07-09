# 🚀 مشروع منصة مهام — Maham Full-Stack Enterprise System

### 🏛️ جامعة الحكمة — قسم تكنولوجيا المعلومات (Al-Hikma University - IT Department)
* **مادة التدريب الميداني (Field Training Course - W9 GitHub)**
* **المشرف على المادة:** أ.د. إبراهيم أحمد البلطة (Prof. Ibrahim Ahmed Al-Baltah)
* **العام الأكاديمي:** 2026-2027

---

## 📌 نبذة عن المشروع (Project Overview)
**منصة مهام (Maham)** هي نظام متكامل لإدارة المشاريع والمهام التشاركية المبنّي باتباع أحدث المعايير الهندسية (Clean Architecture). يتيح النظام لفرق العمل والمؤسسات إنشاء المشاريع، إدارة اللوحات الكانبان، متابعة المهام والبطاقات، وتلقي الدعوات والتحديثات اللحظية عبر الويب والهواتف الذكية.

---

## 🏗️ البنية الهندسية والتقنيات (Architecture & Tech Stack)

### 1️⃣ الخادم والباك إند (Backend Web API)
* **التقنية:** `.NET 6 C# Web API`
* **المعمارية:** Clean Architecture (Domain, Application, Infrastructure, API)
* **أنماط التصميم:** Repository Pattern, Unit of Work, DTO Pattern, Middleware Exception Handling
* **قاعدة البيانات:** PostgreSQL عبر Entity Framework Core 6
* **الأمان والتشفير:** JWT Bearer Tokens, BCrypt Password Hashing
* **الاتصال اللحظي:** SignalR Hubs (`BoardHub`)

### 2️⃣ تطبيق الويب (ASP.NET Core MVC)
* **التقنية:** `ASP.NET Core MVC 6`
* **التصميم والواجهات:** Bootstrap 5, Cairo Font, CSS Variables, Bootstrap Modals, DataTables.js
* **التوافقية:** كامل الدعم للغة العربية والاتجاه من اليمين لليسار (RTL)

### 3️⃣ تطبيق الموبايل (Flutter Mobile App)
* **التقنية:** `Flutter 3.x (Dart)`
* **إدارة الحالة:** BLoC Pattern (`flutter_bloc`)
* **حقن التبعيات والتوجيه:** GetIt Service Locator, GoRouter (with Auth Guards)
* **الشبكة:** Dio Client with Auth Interceptors
* **التصميم:** Material 3, Custom Cairo Typography, Drawer, BottomNavigationBar, SheetModal, Full RTL

---

## 🌿 فروع المشروع (Git Branches & Workflow)

تم بناء وتنظيم مستودع المشروع باتباع أفضل الممارسات المعتمدة في هندسة البرمجيات عبر الفروع المخصصة التالية:

* `main`: الفرع الرئيسي للنسخة المستقرة والتكامل النهائى (Production-ready).
* `feature/backend-api`: يضم كود الباك إند، الطبقات النظيفة (Clean Architecture)، والـ controllers.
* `feature/web-mvc`: يضم كود تطبيق الويب MVC، الواجهات الشجرية، والبطاقات الشبكية.
* `feature/mobile-flutter`: يضم كود تطبيق الموبايل Flutter الشامل لإدارة الواجهات والـ BLoCs.

---

## ⚙️ طريقة تشغيل المشروع (How to Run)

### 1️⃣ تشغيل الباك إند (Backend API)
```bash
dotnet run --project maham_backend/Maham.API/Maham.API.csproj
```
*(يعمل الخادم على: `http://localhost:5233`)*

### 2️⃣ تشغيل تطبيق الويب (ASP.NET MVC Web)
```bash
dotnet run --project maham_web/Maham.Web/Maham.Web.csproj
```
*(افتح المتصفح على: `http://localhost:5000`)*

### 3️⃣ تشغيل تطبيق الموبايل (Flutter App)
```bash
cd maham_app
flutter run
```

---

## 🔑 بيانات الدخول الجاهزة للاختبار (Test Credentials)
* **حساب مدير المشروع (Owner / Dev Lead):** `m.qahtan@maham.com` | كلمة المرور: `12345678`
* **حساب مدير النظام (Admin):** `admin@maham.com` | كلمة المرور: `12345678`
