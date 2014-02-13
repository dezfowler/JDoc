
USE master
GO

DROP DATABASE DocumentDatabase
GO

CREATE DATABASE DocumentDatabase
GO

USE DocumentDatabase
GO

CREATE TABLE Document
(
	DocumentId uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWSEQUENTIALID() PRIMARY KEY NONCLUSTERED,
	FriendlyId varchar(256) NULL,
	Created datetime2 NOT NULL,
	CalculatedId AS ISNULL(FriendlyId, LOWER(CAST(DocumentId AS varchar(36)))) PERSISTED NOT NULL,
)

CREATE UNIQUE CLUSTERED INDEX UCL_Document ON Document(CalculatedId)

CREATE TABLE DocumentRevision
(
	DocumentId uniqueidentifier NOT NULL,
	RevisionEtag uniqueidentifier NOT NULL DEFAULT NEWSEQUENTIALID(),
	Modified datetime2 NOT NULL,
	Content xml NULL,
	CONSTRAINT PK_DocumentRevision PRIMARY KEY CLUSTERED (DocumentId, RevisionEtag)
)

-- document references
CREATE TABLE DocumentReference
(
	DocumentId uniqueidentifier NOT NULL,
	RevisionEtag uniqueidentifier NOT NULL,
	[Path] varchar(256) NOT NULL,
	ForeignDocumentId uniqueidentifier NOT NULL,
	ForeignRevisionEtag uniqueidentifier NULL
)


GO 



CREATE PROC StoreDocument
	@DocumentId uniqueidentifier,
	@RevisionEtag uniqueidentifier,
	@FriendlyId varchar(256) = NULL,
	@Content xml
AS
BEGIN

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
SET XACT_ABORT ON
SET NOCOUNT ON

BEGIN TRY

	BEGIN TRAN 
		
		DECLARE @EmptyGuid uniqueidentifier = CAST('00000000-0000-0000-0000-000000000000' as uniqueidentifier)
		DECLARE @DocTable TABLE (DocumentId uniqueidentifier, CalculatedId varchar(256), Created datetime2)
		DECLARE @MyTempTable TABLE (DocumentId uniqueidentifier)
		DECLARE @DbDocId uniqueidentifier = NULL
		DECLARE @DbFriendlyId varchar(256)
		DECLARE @DbDocRev uniqueidentifier = NULL
		DECLARE @HasHistory bit
		DECLARE @Created datetime2
		DECLARE @RevTable TABLE (RevisionEtag uniqueidentifier, Modified datetime2)

		IF @DocumentId <> @EmptyGuid OR @FriendlyId IS NOT NULL
		BEGIN 
			INSERT INTO @DocTable
			SELECT 
				DocumentId,
				CalculatedId,
				Created
			FROM Document
			WHERE (DocumentId = @DocumentId OR @DocumentId = @EmptyGuid) 
			AND (@FriendlyId IS NULL OR @FriendlyId = FriendlyId)
						
			SELECT 
				@DbDocId = DocumentId,
				@DbFriendlyId = CalculatedId,
				@Created = Created
			FROM @DocTable

			IF @DbDocId IS NULL 
			BEGIN
				RAISERROR ('Document not found', 11, 1)
			END 
		END

		IF @DbDocId IS NULL 
		BEGIN
			INSERT INTO Document (FriendlyId, Created)
			OUTPUT inserted.DocumentId, inserted.CalculatedId, inserted.Created INTO @DocTable
			VALUES (@FriendlyId, SYSUTCDATETIME())
		END

		IF @DbDocId IS NOT NULL 
		BEGIN
			SELECT TOP 1 @DbDocRev = RevisionEtag
			FROM DocumentRevision 
			WHERE DocumentId = @DocumentId
			ORDER BY RevisionEtag DESC
		END
		
		IF @DbDocRev IS NOT NULL AND @RevisionEtag <> @DbDocRev
		BEGIN
			RAISERROR ('Document changed - revision etag mismatch', 11, 1)
		END
		
		INSERT INTO DocumentRevision (DocumentId, Modified, Content)
		OUTPUT inserted.RevisionEtag, inserted.Modified INTO @RevTable
		SELECT DocumentId, SYSUTCDATETIME(), @Content
		FROM @DocTable
		
		SET NOCOUNT OFF

		SELECT DocumentId, CalculatedId, RevisionEtag, Created, Modified
		FROM @DocTable, @RevTable

	COMMIT TRAN

END TRY
BEGIN CATCH

	ROLLBACK TRAN
	
	DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    -- Use RAISERROR inside the CATCH block to return error
    -- information about the original error that caused
    -- execution to jump to the CATCH block.
    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );

	
END CATCH

END
GO




CREATE PROC LoadDocument
	@DocumentId uniqueidentifier,
	@Revision uniqueidentifier = NULL
AS
BEGIN

SET TRANSACTION ISOLATION LEVEL READ COMMITTED
SET XACT_ABORT ON

BEGIN TRY
	BEGIN TRAN	

		SET NOCOUNT ON

		DECLARE @DbDocId uniqueidentifier
		DECLARE @DbFriendlyId varchar(256)
		DECLARE @Created datetime2

		SELECT 
			@DbDocId = DocumentId,
			@DbFriendlyId = CalculatedId,
			@Created = Created
		FROM Document
		WHERE DocumentId = @DocumentId

		SET NOCOUNT OFF

		IF @DbDocId IS NULL
		BEGIN
			RAISERROR ('Document missing', 11, 1)
		END

		SELECT TOP 1 @DbDocId DocumentId, @DbFriendlyId FriendlyId, RevisionEtag, @Created Created, Modified, Content 
		FROM DocumentRevision 
		WHERE DocumentId = @DbDocId AND (@Revision IS NULL OR @Revision = RevisionEtag)
		ORDER BY RevisionEtag DESC
		
	COMMIT TRAN

END TRY
BEGIN CATCH
	
	ROLLBACK TRAN
	
	DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    -- Use RAISERROR inside the CATCH block to return error
    -- information about the original error that caused
    -- execution to jump to the CATCH block.
    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
	
END CATCH

END
GO

