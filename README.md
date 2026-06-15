<div align="center">

# 🍔 HungryHub
### A Full-Stack Food Delivery Web Application

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

**HungryHub** is a complete food delivery platform built with ASP.NET Core 8 MVC.
It features a customer-facing dashboard, a full admin panel, and a dedicated
restaurant owner portal — all in one application.

[Features](#-features) • [Tech Stack](#-tech-stack) • [Getting Started](#-getting-started) • [Screenshots](#-project-structure) • [API Integrations](#-api-integrations)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Database Setup](#-database-setup)
- [API Integrations](#-api-integrations)
- [User Roles](#-user-roles)
- [Default Credentials](#-default-credentials)

---

## 🌟 Overview

HungryHub is a production-ready food delivery web application
tailored for the Bangladeshi market (Chattogram).
It supports three separate portals —
**Customer**, **Admin**, and **Restaurant Owner** —
each with their own authentication, dashboard, and feature set.

Customers can browse restaurants, place orders, apply coupon codes,
split bills with friends, pre-order for events, and subscribe to
weekly meal plans. Restaurant owners can manage their menus in
real time. Admins monitor everything from a business intelligence dashboard.

---

## ✨ Features

### 👤 Customer Portal

| Feature | Description |
|---------|-------------|
| 🔐 Authentication | Register & login with email domain validation and BCrypt password hashing |
| 🏠 Dashboard | Personalized greeting, order stats, weather widget, food suggestions |
| 🏪 Restaurants | Browse all restaurants with real photos, ratings, delivery info |
| 🔍 Search | Search restaurants by name, cuisine, or address |
| ❤️ Favourites | Save and manage favourite restaurants |
| 🍽️ Menu | Browse menu with categories, food photos, live cart |
| 🛒 Cart | Add/remove items, apply coupon codes, live discount calculation |
| 💳 Payment | bKash, Nagad, Rocket, Visa, Mastercard, Cash on Delivery |
| 📦 Orders | Track orders with status bar, rate food, cancel orders |
| 🏷️ Coupons | Apply discount codes at checkout, copy codes from dashboard |
| 💰 Split Order | Split food bills equally or custom among friends |
| 🎉 Pre-Order | Book catering for events (weddings, parties) with minimum 10 guests |
| 🍱 Meal Plans | Subscribe to weekly meal plans with auto-delivery |
| 🔔 Notifications | Real-time order and payment notifications with badge count |
| 👤 Profile | Edit personal info, change password |
| ☀️ Weather | Live weather widget with food suggestions based on weather |

### 🔧 Admin Portal

| Feature | Description |
|---------|-------------|
| 📊 BI Dashboard | Revenue charts, order trends, donut charts, KPI cards, peak hours |
| 🏪 Restaurants | Full CRUD — create, edit, delete, open/close, activate/deactivate |
| 🧾 Menu Items | Add/remove/toggle menu items per restaurant |
| 📦 Orders | View all orders, update status, delete orders |
| 👥 Users | View, search, delete user accounts |
| ⭐ Ratings | View and delete customer ratings |
| 🏷️ Coupons | Create, edit, enable/disable, delete discount coupons with usage tracking |
| 🎉 Pre-Orders | Confirm or reject event catering pre-orders with admin notes |
| 🍱 Subscriptions | Create/manage meal plans, activate/cancel user subscriptions |
| 💰 Split Orders | Monitor all bill splits across users |
| 🏪 Restaurant Owners | Create owner accounts, activate/deactivate, view activity logs |
| 📧 Weekly Reports | Auto-send weekly business reports via email every Monday 8AM |
| 📊 Excel Export | One-click export of all data to Excel (.xlsx) |

### 🍽️ Restaurant Owner Portal

| Feature | Description |
|---------|-------------|
| 📊 Dashboard | Stats — total orders, revenue, rating, today's orders |
| 🧾 Menu Management | Add/edit/delete/enable/disable menu items with food photos |
| 📦 Orders | View restaurant-specific orders, mark as delivered or cancel |
| ⭐ Reviews | View customer reviews with star rating breakdown |
| 🏪 Profile | Update restaurant description, phone, address, photo |
| 🕐 Opening Hours | Set opening/closing times per day of week |
| 🔐 Password | Change account password |
| 📋 Activity Log | All actions logged and visible to admin |

---

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 8 MVC** — Web framework
- **C# 12** — Primary language
- **Entity Framework Core 8** — ORM and database access
- **BCrypt.Net** — Password hashing
- **MailKit** — Email service (weekly reports)
- **EPPlus 8** — Excel report generation
- **SixLabors.ImageSharp** — Image resizing and cropping

### Frontend
- **Bootstrap 5.3** — Responsive UI framework
- **Vanilla JavaScript** — Cart, payments, emoji picker, coupon validation
- **html2pdf.js** — PDF order memo generation
- **Google Fonts (Poppins)** — Typography

### Database
- **SQL Server 2022** — Primary database
- **SSMS 22** — Database management

### APIs
- **OpenWeatherMap API** — Live weather data and food suggestions

---

## 📁 Project Structure

HungryHub/

│

├── Controllers/

│   ├── AccountController.cs        ← Registration & login

│   ├── DashboardController.cs      ← Customer dashboard

│   ├── OrderController.cs          ← Menu, cart, orders, payment

│   ├── CartController.cs           ← Cart page

│   ├── AdminController.cs          ← Full admin panel

│   ├── RestaurantOwnerController.cs← Restaurant owner portal

│   ├── FeaturesController.cs       ← Split, pre-order, subscription

│   └── CouponController.cs         ← Coupon validation API

│

├── Models/

│   ├── User.cs / Admin.cs

│   ├── RestaurantDb.cs / MenuItem.cs

│   ├── Order.cs / OrderItem.cs

│   ├── CartItem.cs / CartViewModel.cs

│   ├── Payment.cs / PaymentViewModel.cs

│   ├── Coupon.cs / CouponUsage.cs

│   ├── SplitOrder.cs / SplitParticipant.cs

│   ├── PreOrder.cs / PreOrderItem.cs

│   ├── MealPlan.cs / UserSubscription.cs

│   ├── RestaurantOwner.cs

│   ├── WeatherData.cs

│   └── ... (ViewModels)

│

├── Services/

│   ├── WeatherService.cs           ← OpenWeatherMap API

│   ├── EmailReportService.cs       ← Weekly email reports

│   ├── WeeklyReportHostedService.cs← Background scheduler

│   ├── ImageService.cs             ← Image resize & save

│   ├── PaymentService.cs           ← Payment processing

│   └── ExcelService.cs             ← Excel export

│

├── Data/

│   └── ApplicationDbContext.cs     ← EF Core DbContext

│

├── Views/

│   ├── Account/                    ← Login, Register

│   ├── Dashboard/                  ← Customer dashboard views

│   ├── Order/                      ← Menu, Cart, Payment, Confirmation

│   ├── Cart/                       ← Cart & checkout

│   ├── Features/                   ← Split, PreOrder, Subscription

│   ├── Admin/                      ← All admin views

│   ├── RestaurantOwner/            ← Owner portal views

│   └── Shared/

│       ├── _DashboardLayout.cshtml ← Customer layout

│       ├── _AdminLayout.cshtml     ← Admin layout

│       └── _OwnerLayout.cshtml     ← Owner layout

│

├── wwwroot/

│   ├── images/

│   │   ├── restaurants/            ← Restaurant logos

│   │   └── menu/                   ← Food photos

│   └── ...

│

├── appsettings.json                ← Config (DB, Email, Weather)

└── Program.cs                      ← App startup & DI


---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server 2022](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SSMS 22](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/YOUR_USERNAME/HungryHub.git
cd HungryHub
```

**2. Install NuGet packages**
```bash
dotnet restore
```

Or install manually in Visual Studio Package Manager Console:
```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package BCrypt.Net-Next
Install-Package EPPlus
Install-Package MailKit
Install-Package SixLabors.ImageSharp
```

**3. Configure `appsettings.json`**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=HungryHubDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AdminSetup": {
    "FirstName":     "Super",
    "LastName":      "Admin",
    "Email":         "admin@hungryhub.com",
    "PlainPassword": "HH@Admin#2026!xZ9",
    "PhoneNumber":   "+8801700000000",
    "Gender":        "Male",
    "DateOfBirth":   "1990-01-01"
  },
  "EmailSettings": {
    "SmtpHost":    "smtp.gmail.com",
    "SmtpPort":    587,
    "SenderEmail": "YOUR_GMAIL@gmail.com",
    "SenderName":  "HungryHub Reports",
    "Password":    "YOUR_GMAIL_APP_PASSWORD",
    "AdminEmail":  "admin@hungryhub.com"
  },
  "WeatherApi": {
    "ApiKey":  "YOUR_OPENWEATHERMAP_API_KEY",
    "City":    "Chattogram",
    "Country": "BD"
  }
}
```

> Replace `YOUR_SERVER_NAME` with your SQL Server instance name
> (find it in SSMS Object Explorer top panel)

**4. Create the database**

Open SSMS and run the full SQL setup script from
`Database/HungryHubDB_Setup.sql`
(create all tables and seed data)

**5. Create image folders**

Make sure these folders exist in `wwwroot/`:

---

## 🌐 API Integrations

### OpenWeatherMap API
- **Purpose:** Live weather data on customer dashboard
- **Features:** Temperature, humidity, wind speed, weather emoji, food suggestions
- **Free tier:** 1,000 calls/day
- **Sign up:** https://openweathermap.org/api

### Payment Methods (Simulated)
The payment system simulates the following Bangladeshi and international methods:

| Method | Type | Prefix |
|--------|------|--------|
| bKash | Mobile Banking | BK |
| Nagad | Mobile Banking | NG |
| Rocket (DBBL) | Mobile Banking | RK |
| Visa | Card | VS |
| Mastercard | Card | MC |
| Cash on Delivery | Physical | — |

> To integrate real payment gateways, replace `PaymentService.cs`
> with the respective SDK (bKash API, Nagad API, Stripe for cards).

### Email Reports (Gmail SMTP)
- **Purpose:** Weekly automated business report every Monday 8AM
- **Setup:** Enable 2-Step Verification on Gmail →
  Security → App Passwords → Generate
- **Config:** Add the 16-character app password to `appsettings.json`

---

## 👥 User Roles

### 1. Customer
- Register at `/Account/Register`
- Login at `/Account/Login`
- Access dashboard at `/Dashboard`

### 2. Admin
- First run: Visit `/Admin/Setup` to create account
- Login at `/Admin/Login`
- Full platform control

### 3. Restaurant Owner
- Account created by Admin at `/Admin/RestaurantOwners`
- Login at `/RestaurantOwner/Login`
- Manages only their own restaurant

---

## 🔑 Default Credentials

### Admin Account
> ⚠️ Create this first by visiting `/Admin/Setup`

---

## 🏷️ Sample Coupon Codes

These coupon codes are seeded automatically:

| Code | Discount | Min Order | Valid |
|------|----------|-----------|-------|
| `HUNGRY1ST` | 20% off (max ৳150) | ৳100 | 3 months |
| `SAVE50` | ৳50 flat off | ৳300 | 1 month |
| `WEEKEND20` | 20% off (max ৳100) | ৳200 | 2 months |
| `BIRYANI30` | ৳30 flat off | ৳150 | 2 months |

---

## 📱 Portal URLs

| Portal | URL | Who uses it |
|--------|-----|-------------|
| Customer App | `/` or `/Account/Login` | Customers |
| Admin Panel | `/Admin/Login` | Admins |
| Owner Portal | `/RestaurantOwner/Login` | Restaurant owners |

---

## 🤝 Contributing

Contributions, issues and feature requests are welcome.

1. Fork the project
2. Create your feature branch
   (`git checkout -b feature/AmazingFeature`)
3. Commit your changes
   (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch
   (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.
See the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

Built with ❤️ using ASP.NET Core 8 MVC

> **HungryHub** — Delivering happiness, one meal at a time 🍔

---

<div align="center">

⭐ **If you found this project helpful, please give it a star!** ⭐

</div>
