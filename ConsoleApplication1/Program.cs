using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Oracle.ManagedDataAccess.Client;


#if DEBUG
//using NUnit.Framework;
#endif


namespace ConsoleApplication1
{

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (maxLength == 0)
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
    class Program
    {
        /* const string command = @"INSERT INTO MWLWL(MWL_KEY,TRIGGER_DTTM,SCHEDULED_AETITLE,SCHEDULED_DTTM,
                                               SCHEDULED_PROC_ID,SCHEDULED_PROC_DESC,
                                               REQUESTED_PROC_ID,REQUESTED_PROC_DESC,
                                               REQUESTED_PROC_CODES,REQUEST_DEPARTMENT,
                                               STUDY_INSTANCE_UID,ACCESSION_NO,REFER_DOCTOR,
                                               SCHEDULED_LOCATION,REQUEST_DOCTOR,
                                               PATIENT_ID,PATIENT_NAME,PATIENT_SEX,
                                               PATIENT_BIRTH_DATE,PATIENT_LOCATION,
                                               SCHEDULED_MODALITY) VALUES 
                                               (SPECTRA.SQ_MWLWL.NEXTVAL, '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',
                                                '{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}',
                                                '{16}','{17}','{18}','{19}')";                        */
        const string command = @"INSERT INTO MWLWL(MWL_KEY,
                                                     REPLICA_DTTM,
                                                     SCHEDULED_AETITLE,
                                                     CHARACTER_SET,
                                                     SCHEDULED_PROC_STATUS,
                                                     TRIGGER_DTTM,
                                                     SCHEDULED_DTTM,
                                                     SCHEDULED_PROC_ID,
                                                     REQUESTED_PROC_ID,
                                                     REQUESTED_PROC_DESC,
                                                     REQUESTED_PROC_CODES,
                                                     STUDY_INSTANCE_UID,
                                                     ACCESSION_NO,
                                                     REFER_DOCTOR,
                                                     PATIENT_ID,
                                                     PATIENT_NAME,
                                                     PATIENT_SEX,
                                                     PATIENT_BIRTH_DATE,
                                                     PATIENT_LOCATION,
                                                     SCHEDULED_MODALITY) 
                                                    VALUES 
                                                    (123, 
                                                    'ANY',
                                                    'ANY',
                                                    'ISO_IR 100',
                                                    '120',
                                                    '{0}',
                                                    '{1}',
                                                    '{2}',
                                                    '{3}',
                                                    '{4}',
                                                    '{5}',
                                                    '{6}',
                                                    '{7}',
                                                    '{8}',
                                                    '{9}',
                                                    '{10}',
                                                    '{11}',
                                                    '{12}',
                                                    '{13}',
                                                    '{14}')";
        static void Main(string[] args)
        {
            timer1_Tick();
        }
        private static void timer1_Tick()
        {
            string path = @"C:\Temp2";
            
            Dictionary<string, string> dic = new Dictionary<string, string>();
            

            if (Directory.Exists(path))
            {
                string query = string.Empty;
                string[] fileEntries = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly);
                XmlSanitizingStreamTest teste = new XmlSanitizingStreamTest();
                teste.ReadOnlyReturnsLegalXmlCharacters();
                foreach (string fileName in fileEntries)
                {
                    
                    using (XmlSanitizingStream fileSteam = new XmlSanitizingStream(fileName, Encoding.UTF8))
                    {
                        XmlReaderSettings settings;
                        
                        settings = new XmlReaderSettings();
                        settings.ConformanceLevel = ConformanceLevel.Document;

                        XmlReader reader = XmlReader.Create(fileSteam, settings);
                        XmlDocument document = new XmlDocument();
                        //
                        var xml = fileSteam.ReadToEnd();
                        document.Load(reader);
                        string dttm = GetString(document.SelectNodes("/MWL_ITEM/DATE")[0]) +
                                       GetString(document.SelectNodes("/MWL_ITEM/TIME")[0]);
                        query = string.Format(command, dttm,
                                                        dttm,
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_DESCRIPTION")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REFERRING_PHYSICIAN_IDENTIFICATION")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_NAME")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_SEX")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_BIRTHDATE")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_LOCATION")[0]),
                                                       GetModality(GetString(document.SelectNodes("/MWL_ITEM/MODALITY")[0])),
                                                       "CruzAzul",
                                                       GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/SCHEDULED_STATION_AE_TITLE")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/SCHEDULED_PERFORMING_PHYSICIAN")[0], 30),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_DESCRIPTION")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REFERRING_PHYSICIAN_IDENTIFICATION")[0]));
                        try
                        {

                            string constr = "DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=XE)));User ID=system;Password=noturno";

                            OracleConnection connection2 = new OracleConnection();
                            using (OracleConnection connection = new OracleConnection(constr))
                            {
                                OracleCommand command = new OracleCommand(query, connection);
                                connection.Open();
                                connection.BeginTransaction();
                                try
                                {
                                    command.ExecuteNonQuery();
                                    command.Transaction.Commit();
                                    //WriteLog("Sucesso " + DateTime.Now.ToString() + ": " + query);
                                    dic.Add(fileName, fileName);
                                }
                                catch (OracleException ex)
                                {
                                    command.Transaction.Rollback();
                                    //WriteLog("Erro " + DateTime.Now.ToString() + ": " + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException);
                                }
                                finally
                                {
                                    // always call Close when done reading.
                                    reader.Close();
                                    connection.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //WriteLog("Erro " + DateTime.Now.ToString() + ": " + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException);
                        }
                    }
                }

                foreach (KeyValuePair<string, string> entry in dic)
                {
                    File.Delete(entry.Value);
                }
            }
        }

        public static string GetModality(string modality)
        {
            switch (modality)
            {
                case "RX":
                    modality = "CR";
                    break;
            }
            return modality;
        }


        public static string GetString(XmlNode node, int maxLength = 0)
        {
            string column = string.Empty;
            if (node != null)
                column = node.InnerText;

            return column.Truncate(maxLength);
        }
    }


    /// <summary>
    /// A StreamReader that excludes XML-illegal characters while reading.
    /// </summary>
    public class XmlSanitizingStream : StreamReader
    {
        /// <summary>
        /// The charactet that denotes the end of a file has been reached.
        /// </summary>
        private const int EOF = -1;

        /// <summary>Create an instance of XmlSanitizingStream.</summary>
        /// <param name="streamToSanitize">
        /// The stream to sanitize of illegal XML characters.
        /// </param>
        public XmlSanitizingStream(Stream streamToSanitize)
            : base(streamToSanitize, true)
        { }

        public XmlSanitizingStream(string path, Encoding encoding)
    : base(path, encoding)
        { }

        /// <summary>
        /// Get whether an integer represents a legal XML 1.0 or 1.1 character. See
        /// the specification at w3.org for these characters.
        /// </summary>
        /// <param name="xmlVersion">
        /// The version number as a string. Use "1.0" for XML 1.0 character
        /// validation, and use "1.1" for XML 1.1 character validation.
        /// </param>
        public static bool IsLegalXmlChar(string xmlVersion, int character)
        {
            switch (xmlVersion)
            {
                case "1.1": // http://www.w3.org/TR/xml11/#charsets
                    {
                        return
                        !(
                             character <= 0x8 ||
                             character == 0xB ||
                             character == 0xC ||
                            (character >= 0xE && character <= 0x1F) ||
                            (character >= 0x7F && character <= 0x84) ||
                            (character >= 0x86 && character <= 0x9F) ||
                             character > 0x10FFFF
                        );
                    }
                case "1.0": // http://www.w3.org/TR/REC-xml/#charsets
                    {
                        return
                        (
                             character == 0x9 /* == '\t' == 9   */          ||
                             character == 0xA /* == '\n' == 10  */          ||
                             character == 0xD /* == '\r' == 13  */          ||
                            (character >= 0x20 && character <= 0xD7FF) ||
                            (character >= 0xE000 && character <= 0xFFFD) ||
                            (character >= 0x10000 && character <= 0x10FFFF)
                        );
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException
                            ("xmlVersion", string.Format("'{0}' is not a valid XML version."));
                    }
            }
        }

        /// <summary>
        /// Get whether an integer represents a legal XML 1.0 character. See the  
        /// specification at w3.org for these characters.
        /// </summary>
        public static bool IsLegalXmlChar(int character)
        {
            return XmlSanitizingStream.IsLegalXmlChar("1.1", character);
        }

        public override int Read()
        {
            // Read each character, skipping over characters that XML has prohibited

            int nextCharacter;

            do
            {
                // Read a character

                if ((nextCharacter = base.Read()) == EOF)
                {
                    // If the character denotes the end of the file, stop reading

                    break;
                }
            }

            // Skip the character if it's prohibited, and try the next

            while (!XmlSanitizingStream.IsLegalXmlChar(nextCharacter));

            return nextCharacter;
        }

        public override int Peek()
        {
            // Return the next legl XML character without reading it 

            int nextCharacter;

            do
            {
                // See what the next character is 

                nextCharacter = base.Peek();
            }
            while
            (
                // If it's prohibited XML, skip over the character in the stream
                // and try the next.

                !XmlSanitizingStream.IsLegalXmlChar(nextCharacter) &&
                (nextCharacter = base.Read()) != EOF
            );

            return nextCharacter;

        } // method

        #region Read*() method overrides

        // The following methods are exact copies of the methods in TextReader, 
        // extracting by disassembling it in Refelctor

        public override int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException();
            }
            int num = 0;
            do
            {
                int num2 = this.Read();
                if (num2 == -1)
                {
                    return num;
                }
                buffer[index + num++] = (char)num2;
            }
            while (num < count);
            return num;
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            int num;
            int num2 = 0;
            do
            {
                num2 += num = this.Read(buffer, index + num2, count - num2);
            }
            while ((num > 0) && (num2 < count));
            return num2;
        }

        public override string ReadLine()
        {
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                int num = this.Read();
                switch (num)
                {
                    case -1:
                        if (builder.Length > 0)
                        {
                            return builder.ToString();
                        }
                        return null;

                    case 13:
                    case 10:
                        if ((num == 13) && (this.Peek() == 10))
                        {
                            this.Read();
                        }
                        return builder.ToString();
                }
                builder.Append((char)num);
            }
        }

        public override string ReadToEnd()
        {
            int num;
            char[] buffer = new char[0x1000];
            StringBuilder builder = new StringBuilder(0x1000);
            while ((num = this.Read(buffer, 0, buffer.Length)) != 0)
            {
                builder.Append(buffer, 0, num);
            }
            return builder.ToString();
        }

        #endregion

    } // class



    //[TestFixture]
    public class XmlSanitizingStreamTest
    {

        //[Test]
        public void ReadOnlyReturnsLegalXmlCharacters()
        {
            // This should be stripped to "\t\r\n<>:"

            string xml = "\0\t\a\r\b\n<>:&#x1F;";

            // Load the XML as a Stream

            using (var buffer = new MemoryStream(System.Text.Encoding.Default.GetBytes(xml)))
            {
                using (var sanitizer = new XmlSanitizingStream(buffer))
                {
                    var t = sanitizer.Read();
                    t = sanitizer.Read();
                    t = sanitizer.Read();
                    t = sanitizer.Read();
                    t = sanitizer.Read();
                    t = sanitizer.Read();
                    t = sanitizer.Read();
                    t = sanitizer.Read();
                    var  y = sanitizer.EndOfStream;
                    /*Assert.AreEqual(sanitizer.Read(), '\t');
                    Assert.AreEqual(sanitizer.Read(), '\r');
                    Assert.AreEqual(sanitizer.Read(), '\n');
                    Assert.AreEqual(sanitizer.Read(), '<');
                    Assert.AreEqual(sanitizer.Read(), '>');
                    Assert.AreEqual(sanitizer.Read(), ':');
                    Assert.AreEqual(sanitizer.Read(), -1);
                    Assert.IsTrue(sanitizer.EndOfStream);*/
                }
            }

            using (var buffer = new MemoryStream(System.Text.Encoding.Default.GetBytes(xml)))
            {
                using (var sanitizer = new XmlSanitizingStream(buffer))
                {
                    //Assert.AreEqual(sanitizer.ReadToEnd(), "\t\r\n<>:");
                }
            }

        } // method

    } // class

}

