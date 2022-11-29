using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileChecker.Utils
{
    public static class ImportExportUtils
    {
        /// <summary>
        /// Check and create the given directory if not exist
        /// </summary>
        /// <param name="dir"></param>
        public static void EnsureDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// Serialize a given object and write to xml file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param> object to be serialize
        /// <param name="fileName"></param> xml file name
        /// <param name="directory"></param> xnl file directory
        public static void WriteToXml<T>(T obj, string fileName, string directory)
        {
            var serializer = new XmlSerializer(typeof(T));
            EnsureDirectory(directory);
            var path = directory + "\\" + fileName + ".xml";

            using (var streamer = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var writer = new StreamWriter(streamer))
                {
                    serializer.Serialize(writer, obj);
                    writer.Close();
                    writer.Dispose();
                }
                streamer.Close();
            }
        }

        /// <summary>
        /// Dserialize to a given object type by reading froma xml file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param> xml file path
        /// <returns></returns>
        public static T ReadFromXml<T>(string filePath)
        {
            T obj;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var streamer = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    obj = (T)serializer.Deserialize(streamer);
                    streamer.Close();
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            return obj;
        }

        public static void WriteToCSV<T>(List<T> objList, string fileName, string directory)
        {
            // Replace any property value that contains ',' by '.' in order to avoid coloumn value conflict when openning from Excel  
            // we make a copy of the list and modify the properties before save as a csv
            foreach (var obj in objList)
            {
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (prop.GetValue(obj).ToString().Contains(','))
                    {
                        prop.SetValue(obj, prop.GetValue(obj).ToString().Replace(',', '.'));
                    }
                }
            }

            var lines = new List<string>();
            IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();

            var header = string.Join(",", props.ToList().Select(x => x.Name));
            lines.Add(header);
            var valueLines = objList.Select(row => string.Join(",", header.Split(',').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
            lines.AddRange(valueLines);
            var path = directory + "\\" + fileName + ".csv";
            File.WriteAllLines(path, lines.ToArray());
        }
    }
}
