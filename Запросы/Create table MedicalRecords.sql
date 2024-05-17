-- Создание таблицы MedicalRecords
CREATE TABLE MedicalRecords (
    RecordID INT PRIMARY KEY IDENTITY(1,1),
    DoctorID INT NOT NULL,
    PatientID INT NOT NULL,
    Diagnosis NVARCHAR(255) NOT NULL,
    ExaminationDate DATE NOT NULL,
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);