using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryForOOPaP_6_7
{
    public class MedicalRecord
    {
        public int RecordID { get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public string Diagnosis { get; set; }
        public DateTime ExaminationDate { get; set; }
                
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
    }
}
