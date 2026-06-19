ALTER PROCEDURE [dbo].[sp_Listing_Get_For_Edit]
    @ListingId INT,
    @LandlordUserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.ListingId,
        l.PropertyId,
        l.Title,
        l.Description,
        l.Price,
        l.IsAvailable,

        p.PropertyName,

        a.City,
        a.Suburb

    FROM Listing l

    INNER JOIN Property p
        ON l.PropertyId = p.PropertyId

    INNER JOIN Address a
        ON p.AddressId = a.AddressId

    WHERE l.ListingId = @ListingId
      AND l.LandlordUserId = @LandlordUserId
      AND l.IsActive = 1;
END