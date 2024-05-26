using System;
using System.Data.SqlClient;
using System.Windows;
using ClassLibraryForOOPaP_6_7;

namespace Lab_6_7
{
    public partial class MainWindow : Window
    {
        private readonly Repository<MedicalRecord> _medicalRecordRepository = new Repository<MedicalRecord>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            var query = @"
                SELECT 
                    MR.DoctorID, 
                    D.FullName AS DoctorName, 
                    MR.PatientID, 
                    P.FullName AS PatientName, 
                    P.BirthYear, 
                    P.Height, 
                    P.Weight, 
                    P.BloodPressure, 
                    MR.Diagnosis, 
                    MR.ExaminationDate
                FROM 
                    MedicalRecords MR
                JOIN 
                    Doctors D ON MR.DoctorID = D.DoctorID
                JOIN 
                    Patients P ON MR.PatientID = P.PatientID";

            var medicalRecords = _medicalRecordRepository.GetAll(query, reader =>
            {
                int? GetNullableInt(object value) => value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
                string GetNullableString(object value) => value == DBNull.Value ? null : value.ToString();

                return new MedicalRecord
                {
                    DoctorID = GetNullableInt(reader["DoctorID"]).GetValueOrDefault(),
                    DoctorName = GetNullableString(reader["DoctorName"]),
                    PatientID = GetNullableInt(reader["PatientID"]).GetValueOrDefault(),
                    PatientName = GetNullableString(reader["PatientName"]),
                    BirthYear = GetNullableInt(reader["BirthYear"]).GetValueOrDefault(),
                    Height = GetNullableInt(reader["Height"]).GetValueOrDefault(),
                    Weight = GetNullableInt(reader["Weight"]).GetValueOrDefault(),
                    BloodPressure = GetNullableString(reader["BloodPressure"]),
                    Diagnosis = GetNullableString(reader["Diagnosis"]),
                    ExaminationDate = reader["ExaminationDate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["ExaminationDate"]
                };
            });

            MedicalRecordsDataGrid.ItemsSource = medicalRecords;
        }

        private void EditTable_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecord = MedicalRecordsDataGrid.SelectedItem as MedicalRecord;
            var editWindow = new EditRecordsWindow(selectedRecord);
            editWindow.ShowDialog();
            LoadData_Click(sender, e);
        }
    }
}
