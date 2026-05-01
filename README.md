# HireFlow ATS API

## Overview

HireFlow is a simplified Applicant Tracking System (ATS) 

This API allows recruiters and hiring managers to:

* Create and manage job postings
* Accept candidate applications
* Track candidates through hiring stages
* Add notes and feedback
* Score candidates across multiple dimensions

---

## Tech Stack

* **.NET 8 (ASP.NET Core Web API)**
* **Entity Framework Core**
* **PostgreSQL (Npgsql)**
* **Swagger (OpenAPI)**
* **Postman (for testing)**

---

## How to Run Locally

### 1. Clone the repository

```bash
git clone <your-repo-link>
cd HireFlow
```

---

### 2. Configure Database

Ensure PostgreSQL is installed and running.

Create database manually:

```sql
CREATE DATABASE hireflow_db;
```

Update connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=hireflow_db;Username=hireflow;Password=hireflow"
}
```

---

### 3. Run Migrations

```bash
dotnet ef database update
```

---

### 4. Run the API

```bash
dotnet run
```

API will be available at:

```
http://localhost:5274
```

Swagger:

```
http://localhost:5274/swagger
```

---
### Running with Docker

```bash
docker compose up --build
```

API will be available at:

```
http://localhost:8080/swagger
```

---

## Seeded Team Members

(Used via `X-Team-Member-Id` header)

| Id | Name           | Role           |
| -- | -------------- | -------------- |
| 1  | John Recruiter | Recruiter      |
| 2  | Sarah Manager  | Hiring Manager |

---

## Authentication Approach

This project does **not use real authentication**.

Instead, it uses:

```
X-Team-Member-Id header
```

Example:

```
X-Team-Member-Id: 1
```

Used for:

* Stage updates
* Adding notes
* Updating scores

---

## API Endpoints

### Jobs

* `POST /api/jobs` → Create job
* `GET /api/jobs` → List jobs
* `GET /api/jobs/{id}` → Get single job

---

### Applications

* `POST /api/jobs/{jobId}/applications` → Apply to job
* `GET /api/jobs/{jobId}/applications` → List applications
* `GET /api/applications/{id}` → Full profile

---

### Stage Management

* `PATCH /api/applications/{id}/stage`

---

### Notes

* `POST /api/applications/{id}/notes`
* `GET /api/applications/{id}/notes`

---

### Scores

* `PUT /api/applications/{id}/scores/culture-fit`
* `PUT /api/applications/{id}/scores/interview`
* `PUT /api/applications/{id}/scores/assessment`

---

## Validation Rules

* Duplicate applications (same email + job) are rejected
* Score must be between **1 and 5**
* Invalid stage transitions return **400 Bad Request**
* Missing `X-Team-Member-Id` returns error

---

### Background Processing
When an application moves to either Hired or Rejected, a background job is triggered.

This job:
* Logs a notification event
* Inserts a record into the Notifications table
*  Runs asynchronously to avoid blocking the API response

This ensures that the ```PATCH /stage``` endpoint returns immediately while side effects are handled in the background.

---

## Testing

The project includes unit tests covering:

* Application creation
* Duplicate application validation
* Job existence validation
* Stage transitions
* Notes functionality

Run tests using:
```
dotnet test
```

Example flow:

1. Create job
2. Apply to job
3. Add note
4. Move stage
5. Add scores
6. Fetch full profile

---

### Limitations and Future Improvements
* No real authentication (JWT or OAuth)
* No pagination for large datasets
* No caching layer implemented
* Limited validation and error standardization
* No CI/CD pipeline included
---





