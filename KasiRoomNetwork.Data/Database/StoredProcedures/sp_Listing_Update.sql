ALTER PROCEDURE [dbo].[sp_Listing_Update]
    @ListingId INT,
    @LandlordUserId NVARCHAR(450),
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @Price DECIMAL(10,2),
    @IsAvailable BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Listing
    SET
        Title = @Title,
        Description = @Description,
        Price = @Price,
        IsAvailable = @IsAvailable,
        UpdatedAt = GETDATE(),

        -- Re-verification required after edits
        IsVerified = 0,
        VerifiedBy = NULL,
        VerifiedOn = NULL,
        VerificationNotes = NULL

    WHERE ListingId = @ListingId
      AND LandlordUserId = @LandlordUserId
      AND IsActive = 1;
END
