using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kusog.SiteStorage
{
    public class FileUploadModel
    {
        public int BlockCount { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsUploadCompleted { get; set; }
        public string UploadStatusMessage { get; set; }
        public int BlockSize { get; set; }

        public string ExtraData { get; set; }
        public string OriginalFileName { get; set; }    
    }
}
