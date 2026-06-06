ALTER PROCEDURE [dbo].[sp_Landlord_Get_All_Listings]
    @LandlordId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.ListingId,
        l.LandlordUserId AS LandlordId,
        l.Title,
        l.Price,
        l.IsVerified,
        l.IsAvailable,
        a.Province,
        a.City,
        a.Suburb,
        p.PhotoPath AS PrimaryPhoto
    FROM Listing l
    INNER JOIN AspNetUsers u ON l.LandlordUserId = u.Id
    INNER JOIN Property pr ON l.PropertyId = pr.PropertyId
    INNER JOIN Address a ON pr.AddressId = a.AddressId
    LEFT JOIN ListingPhoto p
        ON p.ListingId = l.ListingId
       AND p.IsPrimary = 1
       AND p.IsActive = 1
    WHERE l.LandlordUserId = @LandlordId
      AND l.IsActive = 1
    ORDER BY l.CreatedAt DESC;
END
