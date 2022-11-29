using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileChecker.Utils;

namespace FileChecker
{
    public class CompareMember : ICloneable
    {
        // Since we need a copy for csv we implement from ICloneable
        public object Clone()
        {
            return new CompareMember()
            {
                File1Path = this.File1Path,
                File1Version = this.File1Version,
                File1Hash = this.File1Hash,
                File2Path = this.File2Path,
                File2Version = this.File2Version,
                File2Hash = this.File2Hash,
                Result = this.Result
            };
        }
        

        /// <summary>
        /// File1 path of the member
        /// </summary>
        public string File1Path { get; set; }

        /// <summary>
        /// File1 version
        /// </summary>
        public string File1Version { get; set; }

        /// <summary>
        /// File1 Hash code from the MD5
        /// </summary>
        public string File1Hash { get; set; }

        /// <summary>
        /// File1 path of the member
        /// </summary>
        public string File2Path { get; set; }

        /// <summary>
        /// File2 Version
        /// </summary>
        public string File2Version { get; set; }

        /// <summary>
        /// File2 Hash code from the MD5
        /// </summary>
        public string File2Hash { get; set; }

        /// <summary>
        /// Result between the comparison members
        /// </summary>
        public ComparedResults Result { get; set; }


        public CompareMember() { }

        public CompareMember(string path1, string hash1, string version1, string path2, string hash2, string version2, ComparedResults result)
        {
            File1Path = path1;
            File2Path = path2;
            File1Version = version1;
            File2Version = version2;
            File1Hash = hash1;
            File2Hash = hash2;
            Result = result;
        }
    }
}
