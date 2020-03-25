using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhelfondService
{
    public class ReportModel
    {
        public string message_id { get; set; }
        public string accession_number { get; set; }
        public string study_date { get; set; }
        public string study_description { get; set; }
        public string patient_id { get; set; }
        public string last_name { get; set; }
        public string birth_date { get; set; }
        public string sex { get; set; }
        public string requested_procedure_id { get; set; }
        public string requested_procedure_name { get; set; }
        public string last_name_request_doctor { get; set; }
        public string code_request_doctor { get; set; }
        public string first_name_read_doctor { get; set; }
        public string last_name_read_doctor { get; set; }
        public string code_read_doctor { get; set; }
        public string report_sign_datetime { get; set; }
        public string report { get; set; }

        public static ReportModel Create(IDataRecord record)
        {
            return new ReportModel
            {
                message_id = record["message_id"].ToString(),
                accession_number = record["accession_number"].ToString(),
                study_date = record["study_date"].ToString(),
                study_description = record["study_description"].ToString(),
                patient_id = record["patient_id"].ToString(),
                last_name = record["last_name"].ToString(),
                birth_date = record["birth_date"].ToString(),
                sex = record["sex"].ToString(),
                requested_procedure_id = record["requested_procedure_id"].ToString(),
                requested_procedure_name = record["requested_procedure_name"].ToString(),
                last_name_request_doctor = record["last_name_request_doctor"].ToString(),
                code_request_doctor = record["code_request_doctor"].ToString(),
                first_name_read_doctor = record["first_name_read_doctor"].ToString(),
                last_name_read_doctor = record["last_name_read_doctor"].ToString(),
                code_read_doctor = record["code_read_doctor"].ToString(),
                report_sign_datetime = record["report_sign_datetime"].ToString(),
                report = record["report"].ToString()
            };
        }
    }
}
