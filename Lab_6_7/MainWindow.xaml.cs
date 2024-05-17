using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }
}
