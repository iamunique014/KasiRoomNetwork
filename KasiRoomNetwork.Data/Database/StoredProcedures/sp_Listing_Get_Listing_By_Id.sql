ALTER PROCEDURE [dbo].[sp_Listing_Get_Listing_By_Id]
    @ListingId INT
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
        l.IsVerified,
        l.CreatedAt,
        pr.PropertyName,
        pr.PropertyType,
        pr.IsVerified AS PropertyVerified,
        a.Province,
        a.City,
        a.Suburb,
        a.Street,
        l.LandlordUserId,
        up.PhoneNumber,
        up.FullName
    FROM Listing l
    INNER JOIN Property pr
        ON l.PropertyId = pr.PropertyId
    INNER JOIN Address a
        ON pr.AddressId = a.AddressId
    INNER JOIN UserProfiles up
        ON l.LandlordUserId = up.UserId
    WHERE l.ListingId = @ListingId
      AND l.IsActive = 1;
END
