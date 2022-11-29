using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;

namespace FileChecker
{
    public class Checker
    {
        /// <summary>
        /// CheckerList includes results of the MD5 hash codes of the folder
        /// </summary>
        public List<CheckerMember> CheckerList { get; set; }

        /// <summary>
        /// Selected folder for generate MD5
        /// </summary>
        public string SelectedFolderPath { get; set; }

        /// <summary>
        /// MD5 generating progree as a percentage
        /// </summary>
        public double WorkingProgress { get; set; }

        /// <summary>
        /// Total checked files
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Last MD5 generated file path
        /// </summary>
        public string LastWorkFilePath { get; set; }

        /// <summary>
        /// Background worker to update UI
        /// </summary>
        BackgroundWorker BackgroundWorker { get; set; }

        bool _jobDone;
        /// <summary>
        /// True when MD5 completely generated for the selected folder
        /// </summary>
        public bool JobDone
        {
            get
            {
                return _jobDone;
            }
            set
            {
                _jobDone = value;
            }
        }

        /// <summary>
        /// Default constructor for xml serialization
        /// </summary>
        public Checker()
        {
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Checker(BackgroundWorker backgroundWorker)
        {
            CheckerList = new List<CheckerMember>();
            BackgroundWorker = backgroundWorker;
        }

        /// <summary>
        ///  Generate hash codes and add them to CheckerList
        /// </summary>
        public void GenerateHashList()
        {
            _jobDone = false;
            CheckerList.Clear();
            WorkingProgress = 0;
            double i = 1;

            TotalFiles = Directory.GetFiles(SelectedFolderPath, "*.*", SearchOption.AllDirectories).Count();

            foreach (var file in Directory.GetFiles(SelectedFolderPath, "*.*", SearchOption.AllDirectories))
            {
                string hashCode;
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        hashCode = BytesToHash(md5.ComputeHash(stream));
                        stream.Close();
                    }
                }
                if (hashCode != null)
                {
                    // The documentation of property FileVersion is misleading. It makes you think it is a concatenation of major, minor, build and private numbers.
                    // Actually, FileVersion is extracted using a call to a system API function (VerQueryValue), which can returns something different.
                    //var version = FileVersionInfo.GetVersionInfo(file).FileVersion;
                    //version = string.IsNullOrEmpty(version) ? "" : version;

                    var versionInfo = FileVersionInfo.GetVersionInfo(file);
                    string version = string.Format("{0}.{1}.{2}.{3}", versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
                    version = version == "0.0.0.0" ? "" : version;

                    var relativeFilePath = file.Split(new string[] { $"{SelectedFolderPath}\\" }, StringSplitOptions.None).Last();
                    CheckerMember checker = new CheckerMember(relativeFilePath, version, hashCode);

                    CheckerList.Add(checker);
                    WorkingProgress = i / TotalFiles * 100;
                    LastWorkFilePath = file;

                    //Calling background worker to update the UI
                    BackgroundWorker.ReportProgress((int)WorkingProgress);
                    _jobDone = i == TotalFiles ? true : false;
                    i++;
                }
            }
        }

        string BytesToHash(byte[] byteArray)
        {
            string hash = "";
            foreach (byte b in byteArray)
            {
                hash = hash + String.Format("{0:X2} ", b);
            }
            hash = hash.Replace(" ", "");
            return hash;
        }
    }
}
