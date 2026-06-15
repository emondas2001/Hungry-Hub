<div align="center">

# 🍔 HungryHub
### A Full-Stack Food Delivery Web Application

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
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
