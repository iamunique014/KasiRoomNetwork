ALTER PROCEDURE [dbo].[sp_ListingPhoto_Set_Primary]
    @ListingId INT,
    @PhotoId INT,
    @LandlordUserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM ListingPhoto lp
        INNER JOIN Listing l ON l.ListingId = lp.ListingId
        WHERE lp.PhotoId = @PhotoId
          AND lp.ListingId = @ListingId
          AND lp.IsActive = 1
          AND l.LandlordUserId = @LandlordUserId
          AND l.IsActive = 1
    )
    BEGIN
        SELECT CAST(0 AS INT) AS Success;
        RETURN;
    END

    UPDATE ListingPhoto
    SET IsPrimary = 0
    WHERE ListingId = @ListingId
      AND IsActive = 1;

    UPDATE ListingPhoto
    SET IsPrimary = 1
    WHERE PhotoId = @PhotoId
      AND ListingId = @ListingId
      AND IsActive = 1;

    SELECT CAST(CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END AS INT) AS Success;
END
