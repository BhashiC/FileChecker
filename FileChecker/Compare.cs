using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileChecker.Utils;

namespace FileChecker
{
    public class Compare
    {
        /// <summary>
        /// From the folder lacation1
        /// </summary>
        Checker Checker1 { get; set; }

        /// <summary>
        /// From the folder location2
        /// </summary>
        Checker Checker2 { get; set; }

        /// <summary>
        /// Checker1 Selected Folder Path
        /// </summary>
        public string MD5File1SelectedFolderPath { get; set; }

        /// <summary>
        /// Checker2 Selected Folder Path
        /// </summary>
        public string MD5File2SelectedFolderPath { get; set; }

        List<CompareMember> _fullResult;
        /// <summary>
        /// Final results list
        /// </summary>
        public List<CompareMember> FullResult
        {
            get
            {
                return _fullResult;
            }
        }

        /// <summary>
        /// Default constructor for xml serialization
        /// </summary>
        public Compare() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checker1"></param>
        /// <param name="checker2"></param>
        public Compare(Checker checker1, Checker checker2)
        {
            Checker1 = checker1;
            Checker2 = checker2;
            _fullResult = new List<CompareMember>();
        }

        /// <summary>
        /// Compare checker1 and checker2 then generate results
        /// </summary>
        public void DoCompare()
        {
            _fullResult.Clear();
            MD5File1SelectedFolderPath = Checker1.SelectedFolderPath;
            MD5File2SelectedFolderPath = Checker2.SelectedFolderPath;

            var file1Identicals = Checker1.CheckerList.Where(c1 => Checker2.CheckerList.Any(c2 => c2.RelativeFilePath == c1.RelativeFilePath && c2.HashCode == c1.HashCode)).ToList<CheckerMember>();
            foreach (var c1 in file1Identicals)
            {
                var file2member = Checker2.CheckerList.First(c2 => c2.HashCode == c1.HashCode);
                _fullResult.Add(new CompareMember(
                    c1.RelativeFilePath,
                    c1.HashCode,
                    c1.FileVersion,
                    file2member.RelativeFilePath,
                    file2member.HashCode,
                    file2member.FileVersion, 
                    ComparedResults.Identical));
            }

            var file1PathMismatches = Checker1.CheckerList.Where(c1 => Checker2.CheckerList.Any(c2 => c2.HashCode == c1.HashCode && c2.RelativeFilePath != c1.RelativeFilePath)).ToList<CheckerMember>();
            foreach (var c1 in file1PathMismatches)
            {
                var file2member = Checker2.CheckerList.First(c2 => c2.HashCode == c1.HashCode && c2.RelativeFilePath != c1.RelativeFilePath);
                _fullResult.Add(new CompareMember(
                    c1.RelativeFilePath,
                    c1.HashCode,
                    c1.FileVersion,
                    file2member.RelativeFilePath,
                    file2member.HashCode,
                    file2member.FileVersion,
                    ComparedResults.PathOrNameMismatch));
            }

            var file1ContentMismatches = Checker1.CheckerList.Where(c1 => Checker2.CheckerList.Any(c2 => c2.RelativeFilePath == c1.RelativeFilePath && c2.HashCode != c1.HashCode)).ToList<CheckerMember>();
            foreach (var c1 in file1ContentMismatches)
            {
                var file2member = Checker2.CheckerList.First(c2 => c2.RelativeFilePath == c1.RelativeFilePath);
                _fullResult.Add(new CompareMember(
                    c1.RelativeFilePath,
                    c1.HashCode,
                    c1.FileVersion,
                    file2member.RelativeFilePath,
                    file2member.HashCode,
                    file2member.FileVersion,
                    ComparedResults.ContentMismatch));
            }

            Checker1.CheckerList.Where(c1 => Checker2.CheckerList.All(c2 => c2.HashCode != c1.HashCode && c2.RelativeFilePath != c1.RelativeFilePath)).ToList<CheckerMember>().ForEach(x => 
                _fullResult.Add(new CompareMember(                   
                    x.RelativeFilePath,
                    x.HashCode,
                    x.FileVersion,
                    "",
                    "",
                    "",
                    ComparedResults.Location1Only)));

            Checker2.CheckerList.Where(c2 => Checker1.CheckerList.All(c1 => c1.HashCode != c2.HashCode && c1.RelativeFilePath != c2.RelativeFilePath)).ToList<CheckerMember>().ForEach(x=> 
                _fullResult.Add(new CompareMember(
                    "",
                    "",
                    "", 
                    x.RelativeFilePath,
                    x.HashCode,
                    x.FileVersion,
                    ComparedResults.Location2Only)));
        }
    }
}
