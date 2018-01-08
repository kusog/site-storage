using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kusog.SiteStorage
{
    public class FileDir
    {
        public bool IsDirectory { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Length { get; set; }
        public List<FileDir> Children { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public string Extension
        {
            get
            {
                int i = Name.LastIndexOf('.');
                if (i > -1)
                    return Name.Substring(i + 1);
                return "";
            }
        }
        public string FullName
        {
            get
            {
                return Path + Name;
            }
        }
    }
}
