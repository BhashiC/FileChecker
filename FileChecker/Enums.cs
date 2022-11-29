using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChecker.Utils
{
    public enum ComparedResults
    {
        None,
        Identical,
        PathOrNameMismatch,
        ContentMismatch,
        Location1Only,
        Location2Only
    }
}
