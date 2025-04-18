-- Morning Shifts
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T06:00:00', '2025-04-19T14:00:00', 0, 0, 'Terminal A - Gate 1', 'Shift open for assignment.', NULL, 'Morning');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T07:00:00', '2025-04-19T15:00:00', 0, 0, 'Terminal B - Baggage', 'Shift open for assignment.', NULL, 'Morning');

-- Afternoon Shifts
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T14:00:00', '2025-04-19T22:00:00', 0, 0, 'Terminal A - Gate 7', 'Afternoon shift available.', NULL, 'Afternoon');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T13:00:00', '2025-04-19T21:00:00', 0, 0, 'Terminal B - Lounge', 'Afternoon shift available.', NULL, 'Afternoon');

-- Evening Shifts
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T17:00:00', '2025-04-19T00:00:00', 0, 0, 'Cargo Zone', 'Evening shift available.', NULL, 'Evening');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T18:00:00', '2025-04-19T01:00:00', 0, 0, 'Parking Area - Level 2', 'Evening shift available.', NULL, 'Evening');

-- Night Shifts
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T22:00:00', '2025-04-20T06:00:00', 0, 0, 'Terminal A - Security', 'Night shift available.', NULL, 'Night');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-19T23:00:00', '2025-04-20T07:00:00', 0, 0, 'Terminal C - Check-In', 'Night shift available.', NULL, 'Night');

-- More Morning
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T05:00:00', '2025-04-20T13:00:00', 0, 0, 'Terminal B - Customs', 'Morning shift available.', NULL, 'Morning');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T06:30:00', '2025-04-20T14:30:00', 0, 0, 'Control Tower', 'Morning shift available.', NULL, 'Morning');

-- More Afternoon
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T12:00:00', '2025-04-20T20:00:00', 0, 0, 'Terminal A - Arrival', 'Afternoon shift open.', NULL, 'Afternoon');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T13:30:00', '2025-04-20T21:30:00', 0, 0, 'Terminal B - Departure', 'Afternoon shift open.', NULL, 'Afternoon');

-- More Evening
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T17:00:00', '2025-04-20T23:00:00', 0, 0, 'Runway Operations', 'Evening shift.', NULL, 'Evening');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T19:00:00', '2025-04-21T01:00:00', 0, 0, 'Fire Station 1', 'Evening shift.', NULL, 'Evening');

-- More Night
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T22:30:00', '2025-04-21T06:30:00', 0, 0, 'Cargo Terminal', 'Night shift.', NULL, 'Night');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-20T23:30:00', '2025-04-21T07:30:00', 0, 0, 'Fueling Station', 'Night shift.', NULL, 'Night');

-- Random
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-21T08:00:00', '2025-04-21T16:00:00', 0, 0, 'Maintenance Facility', 'Morning shift.', NULL, 'Morning');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-21T14:00:00', '2025-04-21T22:00:00', 0, 0, 'Admin Building', 'Afternoon shift.', NULL, 'Afternoon');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-21T18:00:00', '2025-04-22T00:00:00', 0, 0, 'Technical Center', 'Evening shift.', NULL, 'Evening');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-21T22:00:00', '2025-04-22T06:00:00', 0, 0, 'Terminal C - Loading Bay', 'Night shift.', NULL, 'Night');


-- December 2024 Shifts
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2024-12-28T06:00:00', '2024-12-28T14:00:00', 0, 0, 'Terminal A - Gate 1', 'Morning shift available.', NULL, 'Morning');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2024-12-28T14:00:00', '2024-12-28T22:00:00', 0, 0, 'Terminal B - Lounge', 'Afternoon shift available.', NULL, 'Afternoon');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2024-12-29T22:00:00', '2024-12-30T06:00:00', 0, 0, 'Terminal C - Security', 'Night shift available.', NULL, 'Night');

-- January 2025 Shifts
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-01-03T06:30:00', '2025-01-03T14:30:00', 0, 0, 'Cargo Zone', 'Morning shift.', NULL, 'Morning');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-01-04T17:00:00', '2025-01-04T23:00:00', 0, 0, 'Fire Station', 'Evening shift.', NULL, 'Evening');

-- February 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-02-10T06:00:00', '2025-02-10T14:00:00', 0, 0, 'Terminal A - Arrival', 'Morning shift available.', NULL, 'Morning');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-02-11T14:00:00', '2025-02-11T22:00:00', 0, 0, 'Terminal B - Departure', 'Afternoon shift available.', NULL, 'Afternoon');

-- March 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-03-05T22:00:00', '2025-03-06T06:00:00', 0, 0, 'Runway Operations', 'Night shift.', NULL, 'Night');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-03-06T18:00:00', '2025-03-07T01:00:00', 0, 0, 'Technical Center', 'Evening shift.', NULL, 'Evening');

-- April 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-04-10T06:00:00', '2025-04-10T14:00:00', 0, 0, 'Terminal B - Customs', 'Morning shift.', NULL, 'Morning');

-- May 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-05-05T14:00:00', '2025-05-05T22:00:00', 0, 0, 'Terminal C - Loading Bay', 'Afternoon shift.', NULL, 'Afternoon');

-- June 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-06-15T22:00:00', '2025-06-16T06:00:00', 0, 0, 'Parking Area', 'Night shift.', NULL, 'Night');

-- July 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-07-01T06:00:00', '2025-07-01T14:00:00', 0, 0, 'Maintenance Facility', 'Morning shift.', NULL, 'Morning');

-- August 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-08-20T14:00:00', '2025-08-20T22:00:00', 0, 0, 'Control Tower', 'Afternoon shift.', NULL, 'Afternoon');

-- September 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-09-15T22:00:00', '2025-09-16T06:00:00', 0, 0, 'Cargo Terminal', 'Night shift.', NULL, 'Night');

-- October 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-10-10T06:00:00', '2025-10-10T14:00:00', 0, 0, 'Terminal A - Gate 5', 'Morning shift.', NULL, 'Morning');

-- November 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-11-12T18:00:00', '2025-11-13T01:00:00', 0, 0, 'Admin Building', 'Evening shift.', NULL, 'Evening');

-- December 2025
INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-12-05T14:00:00', '2025-12-05T22:00:00', 0, 0, 'Fueling Station', 'Afternoon shift.', NULL, 'Afternoon');

INSERT INTO [dbo].[Shifts] (SupervisorId, SupervisorName, StartTime, EndTime, IsClaimed, IsClosed, Location, Notes, TotalManpower, ShiftType)
VALUES ('Unassigned', 'Unassigned', '2025-12-15T22:00:00', '2025-12-16T06:00:00', 0, 0, 'Terminal A - Baggage Claim', 'Night shift.', NULL, 'Night');


UPDATE Shifts
SET Department = 'Terminal A Operations'
WHERE Location LIKE 'Terminal A%';

UPDATE Shifts
SET Department = 'Terminal B Services'
WHERE Location LIKE 'Terminal B%';

UPDATE Shifts
SET Department = 'Terminal C Management'
WHERE Location LIKE 'Terminal C%';

UPDATE Shifts
SET Department = 'Cargo Handling'
WHERE Location LIKE '%Cargo%';

UPDATE Shifts
SET Department = 'Runway Operations'
WHERE Location LIKE '%Runway%';

UPDATE Shifts
SET Department = 'Fire and Safety'
WHERE Location LIKE '%Fire Station%';

UPDATE Shifts
SET Department = 'Parking and Transport'
WHERE Location LIKE '%Parking%';

UPDATE Shifts
SET Department = 'Admin Department'
WHERE Location LIKE '%Admin Building%';

