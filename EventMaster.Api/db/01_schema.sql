-- =========================================================
-- EventMaster DB Schema (MySQL 8)
-- Matches: DB Details.txt
-- Database: eventmasterdb
-- =========================================================

CREATE DATABASE IF NOT EXISTS eventmasterdb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE eventmasterdb;

-- Clean re-run safety (dev only)
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS replies;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS payments;
DROP TABLE IF EXISTS bookings;
DROP TABLE IF EXISTS event_occurrences;
DROP TABLE IF EXISTS venues;
DROP TABLE IF EXISTS events;
DROP TABLE IF EXISTS users;

SET FOREIGN_KEY_CHECKS = 1;

-- =========================================================
-- 1) Users
-- =========================================================
CREATE TABLE users (
  user_id        INT AUTO_INCREMENT PRIMARY KEY,
  role           ENUM('CUSTOMER','ORGANIZER') NOT NULL DEFAULT 'CUSTOMER',
  name           VARCHAR(120) NOT NULL,
  age            INT NOT NULL,
  phone          VARCHAR(30) NULL,
  email          VARCHAR(190) NOT NULL,
  username       VARCHAR(60) NOT NULL,
  password		 VARCHAR(255) NOT NULL,
  status		 ENUM('Active','Deactivated') NOT NULL DEFAULT 'Active',
  created_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  UNIQUE KEY uq_users_email (email),
  UNIQUE KEY uq_users_username (username),
  UNIQUE KEY uq_users_phone (phone),
  KEY idx_users_role (role),

  CHECK (age >= 18)
) ENGINE=InnoDB;

-- =========================================================
-- 2) Events (base definition)
-- =========================================================
CREATE TABLE events (
  event_id     INT AUTO_INCREMENT PRIMARY KEY,
  org_id       INT NOT NULL, -- organizer user_id FK
  name         VARCHAR(180) NOT NULL,
  category     VARCHAR(80) NOT NULL,
  description  TEXT NULL,
  image        VARCHAR(500) NULL, -- image metadata/path/filename (backend can use event_id naming)
  created_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT fk_events_org
    FOREIGN KEY (org_id) REFERENCES users(user_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  KEY idx_events_org (org_id),
  KEY idx_events_category (category),
  KEY idx_events_name (name)
) ENGINE=InnoDB;

-- =========================================================
-- 3) Venues
-- =========================================================
CREATE TABLE venues (
  venue_id     INT AUTO_INCREMENT PRIMARY KEY,
  name         VARCHAR(180) NOT NULL,
  address      VARCHAR(255) NOT NULL,
  city         VARCHAR(120) NOT NULL,
  province     VARCHAR(80) NOT NULL,
  postal_code  VARCHAR(20) NOT NULL,
  capacity     INT NOT NULL,
  seating      BOOLEAN NOT NULL DEFAULT true,
  created_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CHECK (capacity >= 0),

  KEY idx_venues_city (city),
  KEY idx_venues_seating (seating)
) ENGINE=InnoDB;

-- =========================================================
-- 4) Event Occurrences
-- (app logic: after creation only allow cancel/complete)
-- =========================================================
CREATE TABLE event_occurrences (
  occurrence_id        INT AUTO_INCREMENT PRIMARY KEY,
  event_id             INT NOT NULL,
  date                 DATE NOT NULL,
  time                 TIME NOT NULL,
  venue_id             INT NOT NULL,
  price                DECIMAL(10,2) NOT NULL,
  remaining_capacity   INT NOT NULL,
  seats_occupied       TEXT NULL, -- comma-separated seat labels (A1,A2,B1...)
  status               ENUM('Scheduled','Cancelled','Completed') NOT NULL DEFAULT 'Scheduled',
  created_at           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT fk_occ_event
    FOREIGN KEY (event_id) REFERENCES events(event_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  CONSTRAINT fk_occ_venue
    FOREIGN KEY (venue_id) REFERENCES venues(venue_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  CHECK (remaining_capacity >= 0),

  KEY idx_occ_event (event_id),
  KEY idx_occ_venue (venue_id),
  KEY idx_occ_datetime (date, time),
  KEY idx_occ_status (status)
) ENGINE=InnoDB;

-- =========================================================
-- 5) Bookings
-- =========================================================
CREATE TABLE bookings (
  booking_id      INT AUTO_INCREMENT PRIMARY KEY,
  occurrence_id   INT NOT NULL,
  customer_id     INT NOT NULL,
  quantity        INT NOT NULL,
  seats_occupied  TEXT NULL, -- comma-separated seats selected in this booking
  status          ENUM('Confirmed','Cancelled') NOT NULL DEFAULT 'Confirmed',
  total_amount    DECIMAL(10,2) NOT NULL,
  ticket_number   VARCHAR(64) NOT NULL, -- backend generated (your file had ticker_number, fixed to ticket_number)
  created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT fk_booking_occ
    FOREIGN KEY (occurrence_id) REFERENCES event_occurrences(occurrence_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  CONSTRAINT fk_booking_customer
    FOREIGN KEY (customer_id) REFERENCES users(user_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  CHECK (quantity > 0),
  CHECK (total_amount >= 0),

  UNIQUE KEY uq_booking_ticket (ticket_number),
  KEY idx_booking_customer (customer_id),
  KEY idx_booking_occurrence (occurrence_id),
  KEY idx_booking_status (status)
) ENGINE=InnoDB;

-- =========================================================
-- 6) Payments (log ALL attempts)
-- =========================================================
CREATE TABLE payments (
  payment_id     INT AUTO_INCREMENT PRIMARY KEY,
  booking_id     INT NOT NULL,
  amount         DECIMAL(10,2) NOT NULL,
  card           VARCHAR(120) NULL, -- last4 + exp (hashed/obfuscated)
  status         ENUM('Success','Failed','Refunded') NOT NULL,
  details        VARCHAR(255) NULL, -- Approved / Insufficient balance / Expired / etc.
  created_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT fk_payment_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(booking_id)
    ON DELETE CASCADE ON UPDATE CASCADE,

 
  KEY idx_payment_booking (booking_id),
  KEY idx_payment_status (status),
  KEY idx_payment_created (created_at)
) ENGINE=InnoDB;

-- =========================================================
-- 7) Reviews + Replies
-- =========================================================
CREATE TABLE reviews (
  review_id       INT AUTO_INCREMENT PRIMARY KEY,
  occurrence_id   INT NOT NULL,
  customer_id     INT NOT NULL,
  rating          INT NOT NULL,
  comment         TEXT NULL,
  created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT fk_review_occ
    FOREIGN KEY (occurrence_id) REFERENCES event_occurrences(occurrence_id)
    ON DELETE CASCADE ON UPDATE CASCADE,

  CONSTRAINT fk_review_customer
    FOREIGN KEY (customer_id) REFERENCES users(user_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  CHECK (rating >= 1 AND rating <= 5),

  KEY idx_review_occ (occurrence_id),
  KEY idx_review_customer (customer_id)
) ENGINE=InnoDB;

CREATE TABLE replies (
  reply_id        INT AUTO_INCREMENT PRIMARY KEY,
  review_id       INT NOT NULL,
  organizer_id    INT NOT NULL,
  reply_text      TEXT NOT NULL,
  created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT fk_reply_review
    FOREIGN KEY (review_id) REFERENCES reviews(review_id)
    ON DELETE CASCADE ON UPDATE CASCADE,

  CONSTRAINT fk_reply_org
    FOREIGN KEY (organizer_id) REFERENCES users(user_id)
    ON DELETE RESTRICT ON UPDATE CASCADE,

  KEY idx_reply_review (review_id),
  KEY idx_reply_org (organizer_id)
) ENGINE=InnoDB;