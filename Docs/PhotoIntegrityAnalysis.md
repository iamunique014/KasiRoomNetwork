# Data Integrity Analysis: Handling Photo Upload Failures

This document addresses the specific concern regarding data integrity when photo uploads fail or when database operations fail after photos have been uploaded.

## The Challenge: Distributed State
The primary challenge is that the system manages state across two different types of storage:
1.  **Relational Database (SQL Server)**: Supports ACID transactions (Atomicity, Consistency, Isolation, Durability).
2.  **Object Storage (Azure Blob Storage)**: Does not participate in SQL transactions.

## Current vs. Proposed Handling

### 1. Current Implementation (Manual Cleanup)
In the current `PropertyController` and `PostRoomWizardController`, cleanup is handled manually using `try-catch` blocks:

*   **PropertyController**: If `AddPropertyPhoto` (database) fails, it calls `_photoStorageService.DeletePhoto` to remove the blob.
*   **Wizard**: Uses `CleanupFailedSubmitAsync` to attempt to delete permanent photos if the submission fails.

**The Risk**: If the application crashes *between* the database failure and the cleanup call, the blob remains orphaned.

### 2. Proposed Implementation (Service-Level Orchestration)
The proposed `PropertyCreationService` uses a more robust approach called the **Compensation Pattern** combined with a strict execution sequence.

#### Execution Sequence:
1.  **Upload to Temporary Storage**: (Already done in the Wizard). Photos are in a "temp" state.
2.  **Begin SQL Transaction**: Start the atomic database operation.
3.  **Database Inserts**: Create Property, Address, Amenities, Listing.
4.  **Blob Operations (Copy/Move)**: Copy photos from temp to permanent containers.
5.  **Metadata Inserts**: Save the photo paths to the database.
6.  **Commit Transaction**: Finalize all database changes.
7.  **Cleanup**: Delete temporary photos.

## Failure Scenarios & Integrity Outcomes

| Failure Point | SQL Transaction State | Blob Storage State | Data Integrity Outcome |
| :--- | :--- | :--- | :--- |
| **During Blob Upload/Copy** | Not yet committed. | Partial blobs may exist in permanent storage. | **Consistent**: Transaction rolls back. No database records exist. Service triggers "Compensation" to delete any partially copied permanent blobs. |
| **During Metadata Insertion** | Active, not committed. | Blobs exist in permanent storage. | **Consistent**: Transaction rolls back. SQL records are wiped. Service triggers "Compensation" to delete the permanent blobs. |
| **During SQL Commit** | Fails to commit. | Blobs exist in permanent storage. | **Consistent**: Database is unchanged. Service triggers "Compensation" to delete the permanent blobs. |
| **App Crashes during Commit** | Rollback (automatic by SQL Server). | Blobs exist in permanent storage. | **Orphaned Blobs**: Database is consistent (no records), but blobs remain. This is a "Storage Leak" but **not** a "Data Integrity" issue (the app won't show broken images). |

## Ensuring 100% Integrity

To handle the "App Crash" scenario (the only one where a manual rollback might fail), we implement two additional safeguards:

### 1. Idempotent Storage Service
The `IPhotoStorageService` is designed so that deleting a non-existent blob doesn't throw an error. This allows the compensation logic to be "greedy" and attempt to delete everything associated with the failed request ID.

### 2. Background Scavenger (The "Final Safety Net")
The existing `WizardTempCleanupHostedService` can be extended to look for "Orphaned Permanent Blobs". 
*   **Logic**: Periodically scan the `property-images` container. If a blob exists but has no corresponding record in the `PropertyPhotos` table (and is older than, say, 24 hours), delete it.
*   **Result**: This ensures that even in catastrophic application failures, the storage is eventually consistent with the database.

## Summary
By moving the logic into a **Service Layer**, we ensure that the "Decision to Commit" happens only after both the storage and the database are ready. If either fails, the **SQL Transaction** ensures the database stays clean, and the **Compensation Logic** ensures the storage stays clean.
