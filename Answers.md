--- 
## SCHEMA DESIGN

### APPLICATION

```sql
CREATE TABLE Applications (
Id SERIAL PRIMARY KEY,
JobId INT NOT NULL,
CandidateName TEXT NOT NULL,
CandidateEmail TEXT NOT NULL,
CurrentStage TEXT NOT NULL,
CreatedAt TIMESTAMP DEFAULT NOW(),
OrganizationId INT NOT NULL
);

CREATE UNIQUE INDEX idx_app_unique_email_job
ON Applications (CandidateEmail, JobId);

CREATE INDEX idx_app_job
ON Applications (JobId);

CREATE INDEX idx_app_org
ON Applications (OrganizationId);
```

### ApplicationNotes
```sql
CREATE TABLE ApplicationNotes (
Id SERIAL PRIMARY KEY,
ApplicationId INT NOT NULL,
Type TEXT NOT NULL,
Description TEXT NOT NULL,
CreatedBy INT NOT NULL,
CreatedAt TIMESTAMP DEFAULT NOW(),
OrganizationId INT NOT NULL
);

CREATE INDEX idx_notes_app
ON ApplicationNotes (ApplicationId);

CREATE INDEX idx_notes_org
ON ApplicationNotes (OrganizationId);
```

### StageHistory

```sql
CREATE TABLE StageHistory (
    Id SERIAL PRIMARY KEY,
    ApplicationId INT NOT NULL,
    FromStage TEXT NOT NULL,
    ToStage TEXT NOT NULL,
    ChangedBy INT NOT NULL,
    ChangedAt TIMESTAMP DEFAULT NOW(),
    Comment TEXT,
    OrganizationId INT NOT NULL
);

CREATE INDEX idx_stage_app 
ON StageHistory (ApplicationId);

CREATE INDEX idx_stage_org 
ON StageHistory (OrganizationId);

CREATE INDEX idx_stage_time 
ON StageHistory (ChangedAt);
```
### Indexing Rationale
* ```(CandidateEmail, JobId)``` ensures duplicate applications are prevented efficiently
* ApplicationId indexes support fast joins for notes, history, and scores
* OrganizationId ensures efficient tenant filtering
* ChangedAt supports sorting stage history chronologically

### Query for GET /api/applications/{id}

The endpoint retrieves:

* Application
* Notes
* Stage history
* Scores

Using Entity Framework Core:

* 1 query for Application
* 1 query for Notes
* 1 query for StageHistory
* 1 query for Scores

Total: 4 database round-trips

This approach keeps queries simple and maintainable while avoiding overly complex joins.

---

## 2. Scoring Design Trade-off
###   (a) Separate Endpoints vs Single Endpoint

Separate endpoints (one per score type):

* Provide clear separation of concerns
* Allow independent updates without affecting other scores
* Simplify validation per score type
* Reduce risk of accidental overwrites

Single endpoint (combined scores):

* More efficient when all scores are updated together
* Better suited for form-based UI submissions
* Ensures atomic updates across all dimensions

Choice depends on product requirements and UI behavior.

### (b) Supporting Score History

To track score changes over time, a new table can be introduced:

```sql

CREATE TABLE ApplicationScoreHistory (
Id SERIAL PRIMARY KEY,
ApplicationId INT NOT NULL,
Type TEXT NOT NULL,
OldScore INT,
NewScore INT NOT NULL,
ChangedBy INT NOT NULL,
ChangedAt TIMESTAMP DEFAULT NOW(),
Comment TEXT,
OrganizationId INT NOT NULL
);
```

Each score update would:

* Insert a new history record
* Optionally update the current score table

Endpoints would remain unchanged, but the service layer would include history tracking logic.

---

## 3. Debugging Scenario

Issue: A recruiter reports that a candidate moved to "Interview" but still appears in "Screening".

Investigation steps:

* Verify the API request and response for the stage update
* Check application logs for the update operation
* Confirm that SaveChangesAsync() executed successfully
* Query the database directly to verify the stored stage
* Check for any subsequent updates that may have overwritten the stage
* Validate stage transition rules to ensure the update was allowed
* Confirm that the correct application ID was used
* Inspect transaction handling and rollback scenarios
* Compare API response data with actual database state

---

## 4. Self Assessment
###   C# – 3/5

Comfortable with core ASP.NET Core concepts and service implementation, with room for improvement in deeper architectural patterns.

### SQL – 4/5

Strong understanding of schema design, indexing, and query structure.

### Git – 4/5

Confident with version control workflows including branching, merging, and resolving conflicts.

### REST API Design – 3/5

Able to design functional APIs, with ongoing improvement in best practices and consistency.

### Writing Tests – 1/5

Basic exposure to unit testing, with significant room for growth in test coverage and strategy.

---


