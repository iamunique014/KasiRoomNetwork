# Edit Listing and Listing Photo Management Audit

## Scope

Audited the Edit Listing flow from **My Rooms → Listing Details → Edit Listing → Save Changes → Listing Details** and the Listing Photo Management operations for upload, delete, and set-primary.

## Findings Addressed

### Ownership Checks

- `AddListingPhotos` did not validate listing ownership before showing or accepting photo uploads.
- `UploadListingPhoto`, `DeleteListingPhoto`, and `SetPrimaryListingPhoto` validated the listing owner in the controller, but non-owner attempts were redirected instead of being rejected with `Forbid`.
- Repository photo methods accepted only listing/photo identifiers, so a controller regression could allow ID tampering to reach stored procedures.

### Update Verification

- `UpdateListing` did not return a success indicator to the controller.
- `sp_Listing_Update` could update zero rows while the controller still displayed success.

### Public Visibility

- Listing details did not distinguish public users from owners.
- Public users could request inactive, unavailable, or unverified listing details if the repository returned them.
- Search and detail data needed explicit `Listing.IsVerified = 1` enforcement in addition to property verification.

### Verification Reset

- `sp_Listing_Update` intentionally resets listing verification by setting `IsVerified = 0` and clearing reviewer metadata.
- The UX warning has been made explicit so landlords understand that edits remove listings from public search until re-approved.

### Photo Tampering

- Photo delete and set-primary stored procedures needed to prove the photo belongs to the submitted listing and that the listing belongs to the current landlord.
- Upload stored procedure needed to prove the target listing belongs to the current landlord before inserting a photo.

### UX

- My Rooms needed direct Manage Photos actions and landlord status badges.
- Listing Details needed to hide Contact Landlord when an owner views their own listing.
- Listing Details needed owner-only status badges for verification and availability.

### Tests

- Controller tests were missing for Edit Listing owner/non-owner/missing-listing/failed-update scenarios.
- Controller tests were missing for listing photo upload/delete/set-primary owner and non-owner scenarios.
- Controller tests were missing for public hidden listing visibility versus owner access.
