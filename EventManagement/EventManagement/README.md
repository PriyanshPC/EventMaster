# EventMaster – Event Ticket Management System

EventMaster is a web-based Event Ticket Management System that allows:

- **Customers** to discover events, book tickets, manage bookings, and leave reviews.
- **Organizers** to create and manage events, view bookings, and respond to customer feedback.

The system uses:

- A **RESTful JSON API** implemented in **ASP.NET** (backend – in progress)
- A **Bootstrap-based UI** (frontend – in progress)
- A **MySQL database** running inside **Docker** for consistent development and testing

This repository currently contains the **database and Docker setup** that all team members can use as a common foundation.

---

## 1. Project Scope (from SRS)

Core features planned (as per SRS):

- User accounts with roles: **CUSTOMER** and **ORGANIZER**
- Event browsing and event details (with filters and date/venue information)
- Booking flow:
  - select event occurrence (date/time)
  - choose ticket quantity (and seating if enabled)
  - proceed to payment (payment is emulated, not a real gateway)
- Booking management for customers (view upcoming/past bookings, edit/cancel according to rules)
- Reviews and ratings for completed events
- Media uploads (photos/videos) for completed events by organizers and eligible customers
- Organizer replies to reviews
- Optional: coupon codes and basic analytics

Non-functional requirements include: containerized deployment with Docker, MySQL for persistence, RESTful HTTP+JSON API design, Bootstrap-based UI, and performance testing with JMeter.

---

## 2. Current Repository Contents

At this stage the repository contains:

- `docker-compose.yml`  
  - Defines a **MySQL 8.0** container for the project.
- `db/init/01_schema.sql`  
  - Creates the `eventtickets` database and all core tables:
    - `users`
    - `organizer_profiles`
    - `event_series`
    - `event_occurrences`
    - `bookings`
    - `payments`
    - `reviews`
    - `review_replies`
    - `media`
- `.gitignore`  
  - Ensures local Docker data, IDE settings, and other generated files are not committed.

Backend and frontend code will be added on top of this foundation.

---

## 3. Tech Stack (planned)

- **Backend:** ASP.NET (Web API)
- **Database:** MySQL 8.x
- **Containerization:** Docker + docker-compose
- **Frontend:** HTML, CSS, JavaScript, Bootstrap
- **Performance Testing:** JMeter (for load and performance scenarios)

---

## 4. Running the Database with Docker

### 4.1 Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- Git installed (to clone the repository)