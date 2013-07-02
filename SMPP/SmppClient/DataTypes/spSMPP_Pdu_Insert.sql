USE [Database]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spSMPP_Pdu_Insert]
    @SMSCServiceID      INT,
    @LogKey             NVARCHAR(128),
    @Direction          SMALLINT,
    @CommandLength      BIGINT,
    @CommandId          NVARCHAR(30),
    @CommandStatusID    NVARCHAR(30),
    @SequenceNumber     BIGINT,
    @PduPropertyDetails SMPPPduPropertyDetailType READONLY,
    @PduDataBlocks      SMPPPduDataBlockType READONLY,
    @PduHeaderID        UNIQUEIDENTIFIER OUTPUT
AS
BEGIN

    SET NOCOUNT ON;

    -- Capture the current time
    DECLARE @ReceivedSentDate DATETIME = GETUTCDATE()

    SET @PduHeaderId = NEWID()

    BEGIN TRY
    
        BEGIN TRANSACTION

            -- Insert the header record
            INSERT SMPPPduHeader
                (PduHeaderID, SMSCServiceID, LogKey, Direction, ReceivedSentDate, CommandLength, CommandId, CommandStatusId, SequenceNumber)
            VALUES
                (@PduHeaderId, @SMSCServiceID, @LogKey, @Direction, @ReceivedSentDate, @CommandLength, @CommandId, @CommandStatusId, @SequenceNumber)

            -- Check for low severity errors
            IF (@@ERROR != 0)
            BEGIN
                
                RAISERROR ('Error Inserting Header Record', 16, 1)

            END

            -- Insert the detail records
            INSERT
                SMPPPduDetails
            SELECT
                @PduHeaderId,
                PPD.StartingBytePosition,
                PPD.PduDataTypeId,
                PPD.DataBlock,
                PPD.PropertyName,
                PPD.Value
            FROM
                @PduPropertyDetails PPD

            -- Check for low severity errors
            IF (@@ERROR != 0)
            BEGIN

                RAISERROR('Error Inserting Detail Records', 16, 1)

            END

            -- Insert the Pdu Data
            INSERT
                SMPPPduData
            SELECT
                @PduHeaderId,
                PDB.SequenceId,
                PDB.DataBlock
            FROM
                @PduDataBlocks PDB

            -- Check for low severity errors
            IF (@@ERROR != 0)
            BEGIN

                RAISERROR('Error Inserting Data Records', 16, 1)

            END

            -- Update the header with phone number
            UPDATE
                SMPPPduHeader
            SET
                DestAddr = PPD.Value
            FROM
                @PduPropertyDetails PPD
            WHERE
                PropertyName = 'DestAddr' AND
                PduHeaderId = @PduHeaderId

            -- Update the header with phone number
            UPDATE
                SMPPPduHeader
            SET
                SourceAddr = PPD.Value
            FROM
                @PduPropertyDetails PPD
            WHERE
                PropertyName = 'SourceAddr' AND
                PduHeaderId = @PduHeaderId

            -- Update the header with message
            UPDATE
                SMPPPduHeader
            SET
                ShortMessage = PPD.Value
            FROM
                @PduPropertyDetails PPD
            WHERE
                PropertyName = 'ShortMessage' AND
                PduHeaderId = @PduHeaderId

        COMMIT TRANSACTION
    
    END TRY
  
    BEGIN CATCH

		DECLARE @ErrorSeverity INT		    = ERROR_SEVERITY()
        DECLARE @ErrorState    INT	        = ERROR_STATE()
        DECLARE @ErrorMessage  VARCHAR(MAX) = ERROR_MESSAGE()
        
        IF (XACT_STATE() != 0)
        BEGIN
            ROLLBACK TRANSACTION
        END

        RAISERROR(@ErrorMessage, @ErrorState, @ErrorSeverity)
        
    END CATCH

END
