CREATE PROCEDURE [dbo].[sp_PropertyPhoto_Set_Primary]
    @PhotoId INT,
    @PropertyId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        -- Validate that the photo exists, belongs to the property,
        -- and is active before changing anything.
        IF NOT EXISTS
        (
            SELECT 1
            FROM PropertyPhoto
            WHERE PhotoId = @PhotoId
              AND PropertyId = @PropertyId
              AND IsActive = 1
        )
        BEGIN
            RAISERROR('Invalid or inactive photo selected.', 16, 1);
            RETURN;
        END

        BEGIN TRANSACTION;

        -- Clear existing primary photo
        UPDATE PropertyPhoto
        SET IsPrimary = 0
        WHERE PropertyId = @PropertyId;

        -- Set selected photo as primary
        UPDATE PropertyPhoto
        SET IsPrimary = 1
        WHERE PhotoId = @PhotoId
          AND PropertyId = @PropertyId
          AND IsActive = 1;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;

    END CATCH
END

