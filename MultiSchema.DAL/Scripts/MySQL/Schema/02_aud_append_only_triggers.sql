-- MySQL: block UPDATE/DELETE on aud tables for everyone (Database mode names shown).
-- Run AFTER `dotnet ef database update`.
DELIMITER $$
CREATE TRIGGER aud.trg_AuditTrail_no_update BEFORE UPDATE ON aud.AuditTrail FOR EACH ROW
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'aud.AuditTrail is append-only'$$
CREATE TRIGGER aud.trg_AuditTrail_no_delete BEFORE DELETE ON aud.AuditTrail FOR EACH ROW
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'aud.AuditTrail is append-only'$$
CREATE TRIGGER aud.trg_WorkflowHistory_no_update BEFORE UPDATE ON aud.WorkflowHistory FOR EACH ROW
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'aud.WorkflowHistory is append-only'$$
CREATE TRIGGER aud.trg_WorkflowHistory_no_delete BEFORE DELETE ON aud.WorkflowHistory FOR EACH ROW
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'aud.WorkflowHistory is append-only'$$
CREATE TRIGGER aud.trg_LicenceEvidence_no_update BEFORE UPDATE ON aud.LicenceEvidence FOR EACH ROW
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'aud.LicenceEvidence is append-only'$$
CREATE TRIGGER aud.trg_LicenceEvidence_no_delete BEFORE DELETE ON aud.LicenceEvidence FOR EACH ROW
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'aud.LicenceEvidence is append-only'$$
DELIMITER ;
