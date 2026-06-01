CREATE PROCEDURE [dbo].[sp_PropertyPhoto_Set_Primary]
    @PhotoId INT,
    @PropertyId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Step 1: Clear existing primary photo for this property
    UPDATE PropertyPhoto
    SET IsPrimary = 0
    WHERE PropertyId = @PropertyId;

    -- Step 2: Set the selected photo as primary
    UPDATE PropertyPhoto
    SET IsPrimary = 1
    WHERE PhotoId = @PhotoId 
      AND PropertyId = @PropertyId
      AND IsActive = 1;
END
GO
