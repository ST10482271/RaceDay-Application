CREATE DATABASE RaceDayDB;

USE RaceDayDB


CREATE TABLE Category(
	CategoryCode INT PRIMARY KEY,
	CategoryType VARCHAR (50) NOT NULL,
    CategoryAge VARCHAR (30) NOT NULL,
    CategoryDistance DECIMAL(5, 2) NOT NULL);


CREATE TABLE Organiser (
    OrganiserID INT PRIMARY KEY,
    OrganiserName VARCHAR (25) NOT NULL,
    OrganiserSurname VARCHAR (25) NOT NULL,
    OrganiserUsername VARCHAR (50) NOT NULL UNIQUE,
    OrganiserPassword VARCHAR (255) NOT NULL,
    OrganiserCellNumber VARCHAR (20) NULL,
    OrganiserEmail VARCHAR (100) NOT NULL UNIQUE,);

CREATE TABLE Participant (
    ParticipantID INT PRIMARY KEY,
    ParticipantName VARCHAR (25) NOT NULL,
    ParticipantSurname VARCHAR (25) NOT NULL,
    ParticipantAge INT NOT NULL,
    ParticipantUsername VARCHAR (50) NOT NULL UNIQUE,
    ParticipantPassword VARCHAR (255) NOT NULL,
    ParticipantCellNumber VARCHAR (20) NULL,
    ParticipantEmail VARCHAR (100) NOT NULL UNIQUE,);
    
CREATE TABLE Event (
    EventCode INT PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATETIME NOT NULL,
    EventDescription VARCHAR(255) NULL,
    EventLocation VARCHAR(100) NOT NULL,
    EventDistance DECIMAL(5, 2) NOT NULL,
    CategoryCode INT NOT NULL,
    OrganiserID INT NOT NULL,
    FOREIGN KEY (CategoryCode) REFERENCES Category (CategoryCode),
    FOREIGN KEY (OrganiserID) REFERENCES Organiser (OrganiserID));

CREATE TABLE Enrollments ( --assignment table between participant and Event
    EnrollmentId INT PRIMARY KEY,
    EventCode INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrollmentStatus VARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (EventCode) REFERENCES Event (EventCode),
    FOREIGN KEY (ParticipantID) REFERENCES Participant (ParticipantID));

 CREATE TABLE Results (
    ResultsNumber INT PRIMARY KEY,
    EnrollmentId INT NOT NULL UNIQUE, -- 1:1 relationship with Enrollment
    Position_Number INT NULL,
    Finishing_Time TIME NULL
    FOREIGN KEY (EnrollmentId) REFERENCES Enrollments (EnrollmentId));

    SELECT * FROM AspNetUsers

    DELETE FROM Enrollments
    WHERE Id = 2;


    



