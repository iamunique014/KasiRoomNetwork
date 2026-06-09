CREATE PROCEDURE [dbo].[sp_Admin_Get_Listing_For_Verification]
    @ListingId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        l.ListingId,
        l.Title,
        l.Description,
        l.Price,
        l.IsVerified,
        l.CreatedAt,
        u.UserName AS LandlordName,
        u.PhoneNumber,
        a.Province,
        a.City,
        a.Suburb,
        a.Street
    FROM Listing l
    INNER JOIN AspNetUsers u ON l.LandlordUserId = u.Id
    INNER JOIN Property pr ON l.PropertyId = pr.PropertyId
	INNER JOIN Address a ON pr.AddressId = a.AddressId
    WHERE l.ListingId = @ListingId;
END