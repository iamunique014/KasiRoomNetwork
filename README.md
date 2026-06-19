# Kasi Room Network (KRN)

Kasi Room Network (KRN) is a room rental and accommodation discovery platform designed to connect tenants with landlords across South Africa.

The platform focuses on helping people find affordable accommodation while giving landlords a simple way to advertise available rooms and manage inquiries.

---

## Mission

To make room hunting safer, simpler, and more accessible for communities across South Africa.

---

## Vision

To become the most trusted room rental platform for townships, mining towns, industrial hubs, and growing communities across South Africa.

---

## Key Features

### Tenant Features

- Search available rooms
- Filter listings by location and price
- View listing details
- Browse property photos
- Contact landlords directly
- Manage tenant profile
- Secure messaging with landlords

### Landlord Features

- Create landlord profile
- Add properties
- Upload property photos
- Create room listings
- Manage listings
- Receive tenant inquiries
- Secure messaging with tenants

### Admin Features

- Verify listings
- Review platform activity
- Manage users
- Moderate reported content
- Review verification logs

---

## Technology Stack

### Backend

- ASP.NET MVC
- ASP.NET Identity
- Dapper
- SQL Server
- Stored Procedures

### Frontend

- Razor Views
- Bootstrap
- JavaScript

### Database

- Microsoft SQL Server

---

## Architecture

The project follows a layered architecture.

```text
KRN.Common
│
├── Models
├── ViewModels
└── DTOs

KRN.Data
│
├── Interfaces
├── Repositories
└── Data Access

KRN.UI
│
├── Controllers
├── Views
├── ViewComponents
└── Static Assets

SQL Server
│
├── Tables
├── Relationships
└── Stored Procedures
```

Development workflow:

```text
Stored Procedure
    ↓
ViewModel
    ↓
Repository
    ↓
Controller
    ↓
UI
```

---

## Current MVP Scope

The MVP focuses on solving the core accommodation discovery problem.

Included:

- User registration and login
- Tenant profiles
- Landlord profiles
- Property management
- Property photos
- Room listings
- Listing photos
- Listing search
- Listing verification
- Internal messaging
- Admin moderation

Future releases may include:

- Advanced search filters
- Occupancy verification
- Tenant reviews
- Landlord reviews
- Roommate matching
- Agent-assisted listings
- Payment integrations
- Mobile applications

---

## User Roles

### Tenant

Can:

- Search rooms
- Contact landlords
- Manage profile
- Send and receive messages

### Landlord

Can:

- Create properties
- Create listings
- Upload photos
- Manage listings
- Communicate with tenants

### Admin

Can:

- Verify listings
- Review reports
- Moderate content
- Manage users

---

## Security Principles

KRN is designed with security in mind.

Examples include:

- ASP.NET Identity authentication
- Role-based authorization
- Ownership validation
- Server-side validation
- Stored procedure data access
- Verification workflows
- Moderation controls

---

## Project Status

Current Status:

**MVP Development**

Primary focus areas:

- Verified room listings
- Safe landlord-tenant communication
- Property management
- Listing management
- Platform moderation

---

## Roadmap

### Phase 1 — MVP

- User accounts
- Profiles
- Properties
- Listings
- Search
- Messaging
- Listing verification

### Phase 2

- Landlord verification
- Property verification
- Reporting system
- Spam prevention
- Enhanced moderation

### Phase 3

- Occupancy verification
- Tenant trust scoring
- Review system
- Agent-assisted listings
- Mobile application

---

## Documentation

Platform policies:

- Terms and Conditions
- Privacy Policy
- Acceptable Use Policy
- Community Guidelines
- Reporting and Appeals Policy
- Account Deletion Policy
- Data Retention Policy

---

## Contributing

At this stage, KRN is under active development.

Contribution guidelines may be published in future releases.

---

## Disclaimer

Kasi Room Network provides a platform that enables landlords and tenants to connect.

KRN does not own, manage, inspect, or guarantee accommodation listed by users.

Users are responsible for conducting their own due diligence before entering into rental agreements or making payments.

---

## Contact

**Kasi Room Network (KRN)**

Email: alungilemvenyeza@gmail.com

Website: NONE

Location: South Africa
