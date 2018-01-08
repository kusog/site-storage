using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kusog.SiteStorage
{
    public interface ISiteStorage
    {
        Guid Id { get; }
        List<FileDir> GetItems(string path, GetItemsOptions options = 0);
        string GetFilePublicUrl(string filePath);

        FileUploadModel PrepareMetaData(int blocksCount, int blockSize, string fileName, long fileSize);
        FileUploadStatus UploadBlock(int id, byte[] chunk, FileUploadModel modelObj);

        Binary GetFile(string fileName);
        void WriteFile(string fileName, Binary data, bool asText=false);
        bool doesExist(string name);
        void DeleteFile(string fileName);

        void MoveToStorage(string name, ISiteStorage destination, string destName);
    }
}
