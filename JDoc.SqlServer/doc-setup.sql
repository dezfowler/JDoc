
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
		DECLARE @RevTable TABLE (RevisionEtag uniqueidentifier, Modified datetime2, Content xml)

		IF @DocumentId <> @EmptyGuid OR @FriendlyId IS NOT NULL 
		BEGIN 
			INSERT INTO @DocTable
			SELECT 
				DocumentId,
				CalculatedId,
				Created
			FROM Document
			WHERE (DocumentId = @DocumentId OR @DocumentId = @EmptyGuid) 
			AND (@FriendlyId IS NULL OR @FriendlyId = CalculatedId)
			
			IF @@ROWCOUNT > 1
			BEGIN
				-- The identifiers supplied were for different documents
				SELECT 'IdentifierMismatch'
				RETURN 3
			END

			SELECT 
				@DbDocId = DocumentId,
				@DbFriendlyId = CalculatedId,
				@Created = Created
			FROM @DocTable

			IF @DocumentId <> @EmptyGuid AND @DbDocId IS NULL 
			BEGIN
				SELECT 'NotFound'
				RETURN 1
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
			WHERE DocumentId = @DbDocId
			ORDER BY RevisionEtag DESC
		END
		
		IF @DbDocRev IS NOT NULL AND @RevisionEtag <> @DbDocRev
		BEGIN
			SELECT 'RevisionMismatch', @DbDocRev AS Expected, @RevisionEtag AS Actual
			RETURN 2
		END
		
		INSERT INTO DocumentRevision (DocumentId, Modified, Content)
		OUTPUT inserted.RevisionEtag, inserted.Modified, inserted.Content INTO @RevTable
		SELECT DocumentId, SYSUTCDATETIME(), @Content
		FROM @DocTable
		
		SELECT 'Document', DocumentId, CalculatedId, RevisionEtag, Created, Modified, Content
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
	@DocumentId uniqueidentifier = NULL,
	@FriendlyId varchar(256) = NULL,
	@Revision uniqueidentifier = NULL
AS
BEGIN

SET TRANSACTION ISOLATION LEVEL READ COMMITTED
SET XACT_ABORT ON
SET NOCOUNT ON

BEGIN TRY
	BEGIN TRAN	

		DECLARE @DbDocId uniqueidentifier
		DECLARE @DbFriendlyId varchar(256)
		DECLARE @Created datetime2

		IF @DocumentId IS NULL AND @FriendlyId IS NULL
		BEGIN
			-- This is an error, should catch this in code
			RAISERROR ('Document ID or Friendly ID must be provided', 11, 1)
		END
		
		SELECT 
			@DbDocId = DocumentId,
			@DbFriendlyId = CalculatedId,
			@Created = Created
		FROM Document
		WHERE (@DocumentId IS NULL OR DocumentId = @DocumentId) 
		AND (@FriendlyId IS NULL OR @FriendlyId = CalculatedId)

		IF @DbDocId IS NULL
		BEGIN
			SELECT 'NotFound'
			RETURN 1
		END

		IF @@ROWCOUNT > 1
		BEGIN
			-- The identifiers supplied were for different documents
			SELECT 'IdentifierMismatch'
			RETURN 3
		END
		
		SELECT TOP 1 'Document', @DbDocId DocumentId, @DbFriendlyId FriendlyId, RevisionEtag, @Created Created, Modified, Content 
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

