# Fermetta 🛒🌱

Fermetta is a feature-rich, collaborative e-commerce web application built with **ASP.NET Core MVC**. It goes beyond a traditional online store by introducing a community-driven catalog management system, allowing authorized "Contributors" to propose new products or edits, which are then reviewed by Administrators. 

## ✨ Key Features

### 👤 User Roles & Authentication
The platform uses ASP.NET Core Identity with three primary roles:
* **Admin:** Full access to manage users, orders, products, categories, and review catalog change requests.
* **Contributor:** Can shop like a normal user but has the added ability to propose new products and categories or suggest updates to existing ones.
* **User:** Standard customer who can browse, add to cart, maintain a wishlist, place orders, and leave reviews.

### 🛍️ Shopping Experience
* **Dynamic Catalog:** Browse products by category, sort by price/rating, and search by keyword.
* **Shopping Cart & Checkout:** Intuitive cart management with quantity adjustments and stock validation. Secure checkout process with delivery details and payment methods (Cash/Card).
* **Wishlist:** Users can save their favorite products for later.
* **Order Tracking:** Customers can view their past orders and track their status (*New, In Process, Shipped, Cancelled*).

### 🤝 Community & Engagement
* **Product Reviews:** Customers can leave 1-5 star ratings and comments on products they've purchased (or viewed).
* **AI Product Assistant:** An integrated AI chat interface on product pages that answers specific customer queries about a product.

### 📝 Collaborative Catalog (Change Requests)
* **Proposals:** Contributors can fill out forms to suggest a new product/category or update an existing one.
* **Admin Review Pipeline:** Admins have an "Inbox" of pending requests. They can view the Contributor's proposal, edit it in an "Admin Draft" sandbox, and finally *Accept* or *Decline* the change, which automatically updates the live database.

## 🛠️ Tech Stack

* **Framework:** .NET (C#) / ASP.NET Core MVC
* **Database ORM:** Entity Framework Core
* **Database:** SQL Server
* **Authentication:** ASP.NET Core Identity
* **Frontend UI:** Bootstrap 5, HTML5, CSS3, JavaScript
* **Icons:** Bootstrap Icons
