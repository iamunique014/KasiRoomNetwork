ALTER PROCEDURE [dbo].[sp_Listing_Add_Listing_Photo]
    @ListingId INT,
    @LandlordUserId NVARCHAR(450),
    @PhotoPath NVARCHAR(500),
    @IsPrimary BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM Listing
        WHERE ListingId = @ListingId
          AND LandlordUserId = @LandlordUserId
          AND IsActive = 1
    )
    BEGIN
        SELECT CAST(0 AS INT) AS Success;
        RETURN;
    END

    IF @IsPrimary = 1
    BEGIN
        UPDATE ListingPhoto
        SET IsPrimary = 0
        WHERE ListingId = @ListingId
          AND IsActive = 1;
    END

    INSERT INTO ListingPhoto
    (
        ListingId,
        PhotoPath,
        IsPrimary
    )
    VALUES
    (
        @ListingId,
        @PhotoPath,
        @IsPrimary
    );

    SELECT CAST(CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END AS INT) AS Success;
END
