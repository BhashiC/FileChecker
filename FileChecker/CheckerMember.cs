using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChecker
{
    public class CheckerMember
    {
        /// <summary>
        /// Relative file path of the member
        /// </summary>
        public string RelativeFilePath { get; set; }

        /// <summary>
        /// Hash code from the MD5
        /// </summary>
        public string HashCode { get; set; }

        /// <summary>
        /// File version
        /// </summary>
        public string FileVersion { get; set; }

        /// <summary>
        /// Default constructor for xml serialization
        /// </summary>
        public CheckerMember() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <param name="hashCode"></param>
        public CheckerMember(string relativeFilePath, string version, string hashCode)
        {
            RelativeFilePath = relativeFilePath;
            FileVersion = version;
            HashCode = hashCode;
        }
    }
}
