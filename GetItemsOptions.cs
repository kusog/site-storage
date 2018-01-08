using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kusog.SiteStorage
{
    [Flags]
    public enum GetItemsOptions
    {
        Deep = 1,
        IncludeFiles = 2,
        IncludeDirectories = 4
    }
}
