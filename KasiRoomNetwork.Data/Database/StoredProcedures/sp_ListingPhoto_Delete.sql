ALTER PROCEDURE [dbo].[sp_ListingPhoto_Delete]
    @PhotoId INT,
    @ListingId INT,
    @LandlordUserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE lp
    SET IsActive = 0,
        IsPrimary = 0
    FROM ListingPhoto lp
    INNER JOIN Listing l ON l.ListingId = lp.ListingId
    WHERE lp.PhotoId = @PhotoId
      AND lp.ListingId = @ListingId
      AND lp.IsActive = 1
      AND l.LandlordUserId = @LandlordUserId
      AND l.IsActive = 1;

    SELECT CAST(CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END AS INT) AS Success;
END
