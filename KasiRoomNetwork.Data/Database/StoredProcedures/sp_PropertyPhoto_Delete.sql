-- CREATE PROCEDURE sp_PropertyPhoto_Delete
--     @PhotoId INT,
--     @PropertyId INT
-- AS
-- BEGIN
--     SET NOCOUNT ON;

--     UPDATE PropertyPhoto
--     SET IsActive = 0
--     WHERE PhotoId = @PhotoId
--       AND PropertyId = @PropertyId;

--     -- Handle primary photo edge case:
--     -- If the deleted photo was the primary photo, find the oldest remaining active photo for the property
--     -- and set it as the new primary. If no photos remain, no primary photo will exist.
--     IF EXISTS (SELECT 1 FROM PropertyPhoto WHERE PhotoId = @PhotoId AND IsPrimary = 1 AND IsActive = 0)
--     BEGIN
--         DECLARE @NewPrimaryPhotoId INT;

--         SELECT TOP 1 @NewPrimaryPhotoId = PhotoId
--         FROM PropertyPhoto
--         WHERE PropertyId = @PropertyId
--           AND IsActive = 1
--         ORDER BY CreatedAt ASC;

--         IF @NewPrimaryPhotoId IS NOT NULL
--         BEGIN
--             UPDATE PropertyPhoto
--             SET IsPrimary = 1
--             WHERE PhotoId = @NewPrimaryPhotoId;
--         END
--     END
-- END;
