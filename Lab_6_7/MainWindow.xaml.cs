using System;
using System.Data.SqlClient;
using System.Windows;
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

            var diagnoses = _medicalRecordRepository.GetDiagnoses("SELECT DISTINCT Diagnosis FROM MedicalRecords");

            DoctorsComboBox.ItemsSource = doctors;
            DoctorsComboBox.DisplayMemberPath = "FullName";
            DoctorsComboBox.SelectedValuePath = "DoctorID";

            DiagnosesComboBox.ItemsSource = diagnoses;
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

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var doctor = DoctorsComboBox.SelectedItem as Doctor;
            var diagnosis = DiagnosesComboBox.SelectedItem as string;

            if (doctor == null || string.IsNullOrEmpty(diagnosis) ||
                string.IsNullOrEmpty(PatientNameTextBox.Text) ||
                string.IsNullOrEmpty(BirthYearTextBox.Text) ||
                string.IsNullOrEmpty(HeightTextBox.Text) ||
                string.IsNullOrEmpty(WeightTextBox.Text) ||
                string.IsNullOrEmpty(BloodPressureTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            var patient = new Patient
            {
                FullName = PatientNameTextBox.Text,
                BirthYear = int.Parse(BirthYearTextBox.Text),
                Height = int.Parse(HeightTextBox.Text),
                Weight = int.Parse(WeightTextBox.Text),
                BloodPressure = BloodPressureTextBox.Text
            };

            int patientID;
            using (var connection = new SqlConnection("Data Source=DESKTOP-2GTDQ2V\\SQLEXPRESS;Initial" +
                " Catalog=OOPaP_67;Integrated Security=True"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert new patient
                        var insertPatientCommand = new SqlCommand(
                            "INSERT INTO Patients (FullName, BirthYear, Height, Weight, BloodPressure) " +
                            "VALUES (@FullName, @BirthYear, @Height, @Weight, @BloodPressure); " +
                            "SELECT CAST(scope_identity() AS int)", connection, transaction);
                        insertPatientCommand.Parameters.AddWithValue("@FullName", patient.FullName);
                        insertPatientCommand.Parameters.AddWithValue("@BirthYear", patient.BirthYear);
                        insertPatientCommand.Parameters.AddWithValue("@Height", patient.Height);
                        insertPatientCommand.Parameters.AddWithValue("@Weight", patient.Weight);
                        insertPatientCommand.Parameters.AddWithValue("@BloodPressure", patient.BloodPressure);

                        patientID = (int)insertPatientCommand.ExecuteScalar();

                        // Insert new medical record
                        var insertMedicalRecordCommand = new SqlCommand(
                            "INSERT INTO MedicalRecords (DoctorID, PatientID, Diagnosis, ExaminationDate) " +
                            "VALUES (@DoctorID, @PatientID, @Diagnosis, @ExaminationDate)", connection, transaction);
                        insertMedicalRecordCommand.Parameters.AddWithValue("@DoctorID", doctor.DoctorID);
                        insertMedicalRecordCommand.Parameters.AddWithValue("@PatientID", patientID);
                        insertMedicalRecordCommand.Parameters.AddWithValue("@Diagnosis", diagnosis);
                        insertMedicalRecordCommand.Parameters.AddWithValue("@ExaminationDate", DateTime.Now);

                        insertMedicalRecordCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Error: {ex.Message}");
                        return;
                    }
                }
            }

            LoadData_Click(sender, e);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalRecordsDataGrid.SelectedItem is MedicalRecord selectedRecord)
            {
                var doctor = DoctorsComboBox.SelectedItem as Doctor;
                var diagnosis = DiagnosesComboBox.SelectedItem as string;

                if (doctor == null || string.IsNullOrEmpty(diagnosis) ||
                    string.IsNullOrEmpty(PatientNameTextBox.Text) ||
                    string.IsNullOrEmpty(BirthYearTextBox.Text) ||
                    string.IsNullOrEmpty(HeightTextBox.Text) ||
                    string.IsNullOrEmpty(WeightTextBox.Text) ||
                    string.IsNullOrEmpty(BloodPressureTextBox.Text))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                using (var connection = new SqlConnection("Data Source=DESKTOP-2GTDQ2V\\SQLEXPRESS;Initial" +
                    " Catalog=OOPaP_67;Integrated Security=True"))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Update patient
                            var updatePatientCommand = new SqlCommand(
                                "UPDATE Patients SET FullName = @FullName, BirthYear = @BirthYear, Height = @Height, " +
                                "Weight = @Weight, BloodPressure = @BloodPressure WHERE PatientID = @PatientID", connection, transaction);
                            updatePatientCommand.Parameters.AddWithValue("@FullName", PatientNameTextBox.Text);
                            updatePatientCommand.Parameters.AddWithValue("@BirthYear", int.Parse(BirthYearTextBox.Text));
                            updatePatientCommand.Parameters.AddWithValue("@Height", int.Parse(HeightTextBox.Text));
                            updatePatientCommand.Parameters.AddWithValue("@Weight", int.Parse(WeightTextBox.Text));
                            updatePatientCommand.Parameters.AddWithValue("@BloodPressure", BloodPressureTextBox.Text);
                            updatePatientCommand.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);

                            updatePatientCommand.ExecuteNonQuery();

                            // Update medical record
                            var updateMedicalRecordCommand = new SqlCommand(
                                "UPDATE MedicalRecords SET DoctorID = @DoctorID, Diagnosis = @Diagnosis, ExaminationDate = @ExaminationDate " +
                                "WHERE DoctorID = @DoctorID AND PatientID = @PatientID", connection, transaction);
                            updateMedicalRecordCommand.Parameters.AddWithValue("@DoctorID", doctor.DoctorID);
                            updateMedicalRecordCommand.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);
                            updateMedicalRecordCommand.Parameters.AddWithValue("@Diagnosis", diagnosis);
                            updateMedicalRecordCommand.Parameters.AddWithValue("@ExaminationDate", DateTime.Now);

                            updateMedicalRecordCommand.ExecuteNonQuery();

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error: {ex.Message}");
                            return;
                        }
                    }
                }

                LoadData_Click(sender, e);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalRecordsDataGrid.SelectedItem is MedicalRecord selectedRecord)
            {
                using (var connection = new SqlConnection("Data Source=DESKTOP-2GTDQ2V\\SQLEXPRESS;Initial " +
                    "Catalog=OOPaP_67;Integrated Security=True"))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete medical record
                            var deleteMedicalRecordCommand = new SqlCommand(
                                "DELETE FROM MedicalRecords WHERE DoctorID = @DoctorID AND PatientID = @PatientID", connection, transaction);
                            deleteMedicalRecordCommand.Parameters.AddWithValue("@DoctorID", selectedRecord.DoctorID);
                            deleteMedicalRecordCommand.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);

                            deleteMedicalRecordCommand.ExecuteNonQuery();

                            // Optionally, you can delete the patient if needed
                            var deletePatientCommand = new SqlCommand(
                                 "DELETE FROM Patients WHERE PatientID = @PatientID", connection, transaction);
                            deletePatientCommand.Parameters.AddWithValue("@PatientID", selectedRecord.PatientID);
                            deletePatientCommand.ExecuteNonQuery();

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Error: {ex.Message}");
                            return;
                        }
                    }
                }

                LoadData_Click(sender, e);
            }
        }
    }
}
