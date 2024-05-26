using System;
using System.Data.SqlClient;
using System.Windows;
using ClassLibraryForOOPaP_6_7;

namespace Lab_6_7
{
    public partial class EditRecordsWindow : Window
    {
        private readonly Repository<Doctor> _doctorRepository = new Repository<Doctor>();
        private readonly Repository<MedicalRecord> _medicalRecordRepository = new Repository<MedicalRecord>();
        private MedicalRecord _selectedRecord;

        public EditRecordsWindow(MedicalRecord selectedRecord)
        {
            InitializeComponent();
            _selectedRecord = selectedRecord;
            LoadComboBoxes();

            if (_selectedRecord != null)
            {
                // Заполнение полей при редактировании записи
                DoctorsComboBox.SelectedValue = _selectedRecord.DoctorID;
                PatientNameTextBox.Text = _selectedRecord.PatientName;
                BirthYearTextBox.Text = _selectedRecord.BirthYear.ToString();
                HeightTextBox.Text = _selectedRecord.Height.ToString();
                WeightTextBox.Text = _selectedRecord.Weight.ToString();
                BloodPressureTextBox.Text = _selectedRecord.BloodPressure;
                DiagnosisComboBox.SelectedItem = _selectedRecord.Diagnosis;
            }
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

            DiagnosisComboBox.ItemsSource = diagnoses;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var doctor = DoctorsComboBox.SelectedItem as Doctor;
            var diagnosis = DiagnosisComboBox.SelectedItem as string;

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

            Patient patient = new Patient
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
                        // Добавление нового пациента
                        SqlCommand insertPatientCommand = new SqlCommand(
                            "INSERT INTO Patients (FullName, BirthYear, Height, Weight, BloodPressure) " +
                            "VALUES (@FullName, @BirthYear, @Height, @Weight, @BloodPressure); " +
                            "SELECT CAST(scope_identity() AS int)", connection, transaction);
                        insertPatientCommand.Parameters.AddWithValue("@FullName", patient.FullName);
                        insertPatientCommand.Parameters.AddWithValue("@BirthYear", patient.BirthYear);
                        insertPatientCommand.Parameters.AddWithValue("@Height", patient.Height);
                        insertPatientCommand.Parameters.AddWithValue("@Weight", patient.Weight);
                        insertPatientCommand.Parameters.AddWithValue("@BloodPressure", patient.BloodPressure);

                        patientID = (int)insertPatientCommand.ExecuteScalar();

                        // Добавление новой записи
                        SqlCommand insertMedicalRecordCommand = new SqlCommand(
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

            this.Close();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRecord == null)
            {
                MessageBox.Show("No record selected for updating.");
                return;
            }

            var doctor = DoctorsComboBox.SelectedItem as Doctor;
            var diagnosis = DiagnosisComboBox.SelectedItem as string;

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
                        // Обновление информации о пациенте
                        SqlCommand updatePatientCommand = new SqlCommand(
                            "UPDATE Patients SET FullName = @FullName, BirthYear = @BirthYear, Height = @Height, Weight = @Weight, BloodPressure = @BloodPressure " +
                            "WHERE PatientID = @PatientID", connection, transaction);
                        updatePatientCommand.Parameters.AddWithValue("@FullName", PatientNameTextBox.Text);
                        updatePatientCommand.Parameters.AddWithValue("@BirthYear", int.Parse(BirthYearTextBox.Text));
                        updatePatientCommand.Parameters.AddWithValue("@Height", int.Parse(HeightTextBox.Text));
                        updatePatientCommand.Parameters.AddWithValue("@Weight", int.Parse(WeightTextBox.Text));
                        updatePatientCommand.Parameters.AddWithValue("@BloodPressure", BloodPressureTextBox.Text);
                        updatePatientCommand.Parameters.AddWithValue("@PatientID", _selectedRecord.PatientID);

                        updatePatientCommand.ExecuteNonQuery();

                        // Обновление медицинской записи
                        SqlCommand updateMedicalRecordCommand = new SqlCommand(
                            "UPDATE MedicalRecords SET DoctorID = @DoctorID, Diagnosis = @Diagnosis, ExaminationDate = @ExaminationDate " +
                            "WHERE DoctorID = @OldDoctorID AND PatientID = @PatientID", connection, transaction);
                        updateMedicalRecordCommand.Parameters.AddWithValue("@DoctorID", doctor.DoctorID);
                        updateMedicalRecordCommand.Parameters.AddWithValue("@Diagnosis", diagnosis);
                        updateMedicalRecordCommand.Parameters.AddWithValue("@ExaminationDate", DateTime.Now);
                        updateMedicalRecordCommand.Parameters.AddWithValue("@OldDoctorID", _selectedRecord.DoctorID);
                        updateMedicalRecordCommand.Parameters.AddWithValue("@PatientID", _selectedRecord.PatientID);

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

            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRecord == null)
            {
                MessageBox.Show("No record selected for deletion.");
                return;
            }

            using (var connection = new SqlConnection("Data Source=DESKTOP-2GTDQ2V\\SQLEXPRESS;Initial " +
                "Catalog=OOPaP_67;Integrated Security=True"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Удаление записи
                        SqlCommand deleteMedicalRecordCommand = new SqlCommand(
                            "DELETE FROM MedicalRecords WHERE DoctorID = @DoctorID AND PatientID = @PatientID", connection, transaction);
                        deleteMedicalRecordCommand.Parameters.AddWithValue("@DoctorID", _selectedRecord.DoctorID);
                        deleteMedicalRecordCommand.Parameters.AddWithValue("@PatientID", _selectedRecord.PatientID);

                        deleteMedicalRecordCommand.ExecuteNonQuery();

                        // Удаление пациента
                        SqlCommand deletePatientCommand = new SqlCommand(
                             "DELETE FROM Patients WHERE PatientID = @PatientID", connection, transaction);
                        deletePatientCommand.Parameters.AddWithValue("@PatientID", _selectedRecord.PatientID);
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

            this.Close();
        }
    }
}
