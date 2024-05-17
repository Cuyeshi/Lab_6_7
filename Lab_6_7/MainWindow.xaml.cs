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
                    FullName = reader["FullName"].ToString()
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
            var medicalRecords = _medicalRecordRepository.GetAll("SELECT * FROM MedicalRecords", reader =>
            {
                return new MedicalRecord
                {
                    RecordID = (int)reader["RecordID"],
                    DoctorID = (int)reader["DoctorID"],
                    PatientID = (int)reader["PatientID"],
                    Diagnosis = reader["Diagnosis"].ToString(),
                    ExaminationDate = (DateTime)reader["ExaminationDate"]
                };
            });

            MedicalRecordsDataGrid.ItemsSource = medicalRecords;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var newRecord = new MedicalRecord
            {
                DoctorID = (int)DoctorsComboBox.SelectedValue,
                PatientID = (int)PatientsComboBox.SelectedValue,
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
                selectedRecord.DoctorID = (int)DoctorsComboBox.SelectedValue;
                selectedRecord.PatientID = (int)PatientsComboBox.SelectedValue;
                selectedRecord.Diagnosis = "Updated Diagnosis";
                selectedRecord.ExaminationDate = DateTime.Now;

                _medicalRecordRepository.Update("UPDATE MedicalRecords SET DoctorID = @DoctorID, PatientID = @PatientID, Diagnosis = @Diagnosis, ExaminationDate = @ExaminationDate WHERE RecordID = @RecordID", cmd =>
                {
                    cmd.Parameters.AddWithValue("@DoctorID", selectedRecord.DoctorID);
                    cmd.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);
                    cmd.Parameters.AddWithValue("@Diagnosis", selectedRecord.Diagnosis);
                    cmd.Parameters.AddWithValue("@ExaminationDate", selectedRecord.ExaminationDate);
                    cmd.Parameters.AddWithValue("@RecordID", selectedRecord.RecordID);
                });

                LoadData_Click(sender, e);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalRecordsDataGrid.SelectedItem is MedicalRecord selectedRecord)
            {
                _medicalRecordRepository.Delete("DELETE FROM MedicalRecords WHERE RecordID = @RecordID", cmd =>
                {
                    cmd.Parameters.AddWithValue("@RecordID", selectedRecord.RecordID);
                });

                LoadData_Click(sender, e);
            }
        }
    }
}
