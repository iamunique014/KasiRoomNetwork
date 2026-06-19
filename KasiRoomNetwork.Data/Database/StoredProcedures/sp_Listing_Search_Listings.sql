ALTER PROCEDURE [dbo].[sp_Listing_Search_Listings]
    @Province NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @Suburb NVARCHAR(100) = NULL,
    @MinPrice DECIMAL(10,2) = NULL,
    @MaxPrice DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.ListingId,
        l.Title,
        l.Price,
        a.Province,
        a.City,
        a.Suburb,
        p.PhotoPath AS PrimaryPhoto,
        pr.PropertyId,
        pr.PropertyName,
        pr.PropertyType,
        pr.IsVerified
    FROM Listing l
    INNER JOIN Property pr
        ON l.PropertyId = pr.PropertyId
    INNER JOIN Address a
        ON pr.AddressId = a.AddressId
    LEFT JOIN ListingPhoto p
        ON p.ListingId = l.ListingId
       AND p.IsPrimary = 1
       AND p.IsActive = 1
    WHERE l.IsActive = 1
      AND l.IsAvailable = 1
      AND l.IsVerified = 1
      AND pr.IsVerified = 1
      AND (@Province IS NULL OR a.Province = @Province)
      AND (@City IS NULL OR a.City = @City)
      AND (@Suburb IS NULL OR a.Suburb = @Suburb)
      AND (@MinPrice IS NULL OR l.Price >= @MinPrice)
      AND (@MaxPrice IS NULL OR l.Price <= @MaxPrice)
    ORDER BY l.CreatedAt DESC;
END
