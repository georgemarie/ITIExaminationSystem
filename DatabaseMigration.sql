-- ============================================================
-- ITI Examination System — Database Migration Script
-- Run this script ONCE on your existing database to add
-- the new columns required by the enhanced features.
-- ============================================================

USE ITIExaminationSystem;
GO

-- ── 1. Add IsSubmitted column to Student_Exam ──────────────
-- Tracks whether a student has submitted the exam.
-- Prevents re-taking after submission.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Student_Exam') AND name = 'IsSubmitted'
)
BEGIN
    ALTER TABLE Student_Exam ADD IsSubmitted BIT NOT NULL DEFAULT 0;
    PRINT 'Added: Student_Exam.IsSubmitted';
END
ELSE
    PRINT 'Already exists: Student_Exam.IsSubmitted';
GO

-- ── 2. Add StartedAt column to Student_Exam ────────────────
-- Records when the student began the exam (for audit/analytics).
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Student_Exam') AND name = 'StartedAt'
)
BEGIN
    ALTER TABLE Student_Exam ADD StartedAt DATETIME NULL;
    PRINT 'Added: Student_Exam.StartedAt';
END
ELSE
    PRINT 'Already exists: Student_Exam.StartedAt';
GO

-- ── 3. Add SubmittedAt column to Student_Exam ──────────────
-- Records when the student submitted the exam.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Student_Exam') AND name = 'SubmittedAt'
)
BEGIN
    ALTER TABLE Student_Exam ADD SubmittedAt DATETIME NULL;
    PRINT 'Added: Student_Exam.SubmittedAt';
END
ELSE
    PRINT 'Already exists: Student_Exam.SubmittedAt';
GO

-- ── 4. Mark existing submitted records ─────────────────────
-- Any existing Student_Exam row that already has a Grade
-- is treated as already submitted (data migration).
UPDATE Student_Exam
SET IsSubmitted = 1
WHERE Grade IS NOT NULL AND IsSubmitted = 0;
PRINT 'Migrated: existing graded attempts marked as submitted.';
GO

-- ── 5. Verify the changes ───────────────────────────────────
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Student_Exam'
ORDER BY ORDINAL_POSITION;
GO

PRINT '✅ Migration complete. You can now run the enhanced project.';
GO
