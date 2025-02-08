using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace InjectData
{
    internal class ApiToUpload : IDisposable
    {
        Database db;

        public ApiToUpload()
        {
            db = new Database();
        }

        public void AddXml(int InstitutionID, int EduFormID, string XmlData, string FilePath)
        {
            db.ExecuteNonQueryProc(
                "add_xml_data",
                ("@Institution", InstitutionID),
                ("@EducationForm", EduFormID),
                ("@DataAll", XmlData),
                ("@FilePath", FilePath)
            );
        }

        public int? DublicateCheck(int InstitutionID, int EduFormID, string FilePath)
        {
            return (int?)db.ExecuteScalarProc(
                "dublicate_check",
                ("@Institution", InstitutionID),
                ("@EducationForm", EduFormID),
                ("@FilePath", FilePath)
            );
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
