using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using ClassLibraryForOOPaP_6_7;

namespace Lab_6_7
{
    public partial class MainWindow : Window
    {
        private readonly Repository<Doctor> _doctorRepository = new Repository<Doctor>();
        private readonly Repository<Patient> _patientRepository = new Repository<Patient>();
        private readonly Repository<MedicalRecord> _medicalRecordRepository = new Repository<MedicalRecord>();

        public MainWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            var doctors = _doctorRepository.GetAll("SELECT * FROM Doctors", reader =>
            {
                return new Doctor
                {
                    DoctorID = (int)reader["DoctorID"],
                    FullName = reader["FullName"].ToString()
                };
            });

            var patients = _patientRepository.GetAll("SELECT * FROM Patients", reader =>
            {
                return new Patient
                {
                    PatientID = (int)reader["PatientID"],
                    FullName = reader["FullName"].ToString(),
                    BirthYear = (int)reader["BirthYear"],
                    Height = (decimal)reader["Height"],
                    Weight = (decimal)reader["Weight"],
                    BloodPressure = reader["BloodPressure"].ToString()
                };
            });

            DoctorsComboBox.ItemsSource = doctors;
            DoctorsComboBox.DisplayMemberPath = "FullName";
            DoctorsComboBox.SelectedValuePath = "DoctorID";

            PatientsComboBox.ItemsSource = patients;
            PatientsComboBox.DisplayMemberPath = "FullName";
            PatientsComboBox.SelectedValuePath = "PatientID";
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            var query = @"
            SELECT 
                m.DoctorID, 
                d.FullName AS DoctorName, 
                m.PatientID, 
                p.FullName AS PatientName, 
                p.BirthYear, 
                p.Height, 
                p.Weight, 
                p.BloodPressure, 
                m.Diagnosis, 
                m.ExaminationDate
            FROM 
                MedicalRecords m
            JOIN 
                Doctors d ON m.DoctorID = d.DoctorID
            JOIN 
                Patients p ON m.PatientID = p.PatientID";

            var medicalRecords = _medicalRecordRepository.GetAll(query, reader =>
            {
                return new MedicalRecord
                {
                    DoctorID = (int)reader["DoctorID"],
                    DoctorName = reader["DoctorName"].ToString(),
                    PatientID = (int)reader["PatientID"],
                    PatientName = reader["PatientName"].ToString(),
                    BirthYear = (int)reader["BirthYear"],
                    Height = (decimal)reader["Height"],
                    Weight = (decimal)reader["Weight"],
                    BloodPressure = reader["BloodPressure"].ToString(),
                    Diagnosis = reader["Diagnosis"].ToString(),
                    ExaminationDate = (DateTime)reader["ExaminationDate"]
                };
            });

            MedicalRecordsDataGrid.ItemsSource = medicalRecords;
        }


        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var doctor = DoctorsComboBox.SelectedItem as Doctor;
            var patient = PatientsComboBox.SelectedItem as Patient;

            if (doctor == null || patient == null)
            {
                MessageBox.Show("Please select both a doctor and a patient.");
                return;
            }

            var newRecord = new MedicalRecord
            {
                DoctorID = doctor.DoctorID,
                PatientID = patient.PatientID,
                DoctorName = doctor.FullName,
                PatientName = patient.FullName,
                Diagnosis = "New Diagnosis",
                ExaminationDate = DateTime.Now
            };

            _medicalRecordRepository.Create("INSERT INTO MedicalRecords (DoctorID, PatientID, Diagnosis, ExaminationDate) VALUES (@DoctorID, @PatientID, @Diagnosis, @ExaminationDate)", cmd =>
            {
                cmd.Parameters.AddWithValue("@DoctorID", newRecord.DoctorID);
                cmd.Parameters.AddWithValue("@PatientID", newRecord.PatientID);
                cmd.Parameters.AddWithValue("@Diagnosis", newRecord.Diagnosis);
                cmd.Parameters.AddWithValue("@ExaminationDate", newRecord.ExaminationDate);
            });

            LoadData_Click(sender, e);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalRecordsDataGrid.SelectedItem is MedicalRecord selectedRecord)
            {
                var doctor = DoctorsComboBox.SelectedItem as Doctor;
                var patient = PatientsComboBox.SelectedItem as Patient;

                if (doctor == null || patient == null)
                {
                    MessageBox.Show("Please select both a doctor and a patient.");
                    return;
                }

                selectedRecord.DoctorID = doctor.DoctorID;
                selectedRecord.PatientID = patient.PatientID;
                selectedRecord.DoctorName = doctor.FullName;
                selectedRecord.PatientName = patient.FullName;
                selectedRecord.Diagnosis = "Updated Diagnosis";
                selectedRecord.ExaminationDate = DateTime.Now;

                _medicalRecordRepository.Update("UPDATE MedicalRecords SET DoctorID = @DoctorID, PatientID = @PatientID, Diagnosis = @Diagnosis, ExaminationDate = @ExaminationDate WHERE DoctorID = @DoctorID AND PatientID = @PatientID", cmd =>
                {
                    cmd.Parameters.AddWithValue("@DoctorID", selectedRecord.DoctorID);
                    cmd.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);
                    cmd.Parameters.AddWithValue("@Diagnosis", selectedRecord.Diagnosis);
                    cmd.Parameters.AddWithValue("@ExaminationDate", selectedRecord.ExaminationDate);
                });

                LoadData_Click(sender, e);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalRecordsDataGrid.SelectedItem is MedicalRecord selectedRecord)
            {
                _medicalRecordRepository.Delete("DELETE FROM MedicalRecords WHERE DoctorID = @DoctorID AND PatientID = @PatientID", cmd =>
                {
                    cmd.Parameters.AddWithValue("@DoctorID", selectedRecord.DoctorID);
                    cmd.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);
                });

                LoadData_Click(sender, e);
            }
        }
    }
}
