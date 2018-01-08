using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kusog.SiteStorage
{
    public class FileSysStorage : ISiteStorage
    {
        protected string m_baseDir;
        protected Guid m_id;

        public FileSysStorage(string baseDir, Guid? id = null)
        {
            m_baseDir = baseDir;
            m_id = id.HasValue ? id.Value : Guid.NewGuid();
        }

        public Guid Id
        {
            get { return m_id; }
        }

        public List<FileDir> GetItems(string path, GetItemsOptions options=0)
        {
            List<FileDir> items = new List<FileDir>();
            path = path.Replace('/', '\\').Replace("\\\\","\\");


            if (path.EndsWith("\\"))
            {
                DirectoryInfo di = new DirectoryInfo(getFullPath(path));
                if (di.Exists)
                {
                    GetChildItems(items, di, options, m_baseDir.Length);
                }
            }
            else
            {
                
                FileInfo fi = new FileInfo(getFullPath(path));
                if (fi.Exists)
                {
                    items.Add(FromFI(fi, m_baseDir.Length));
                }
                else
                {
                    int i = path.LastIndexOf('\\');
                    if (i != -1)
                        path = path.Substring(0, i);

                    DirectoryInfo di = new DirectoryInfo(getFullPath(path));
                    if (di.Exists)
                    {
                        foreach (FileInfo fi2 in di.GetFiles(path.Substring(i + 1) + "*"))
                        {
                            items.Add(FromFI(fi2, m_baseDir.Length));
                        }
                    }
                }
            }
            return items;
        }

        protected FileDir FromFI(FileInfo fi, int prefixLength)
        {
            string pname = fi.Directory.FullName;
            if (pname.Length > prefixLength)
                pname = pname.Substring(prefixLength);
            pname += "\\";
            return new FileDir() { Name = fi.Name, CreationTimeUtc = fi.CreationTimeUtc, IsDirectory = false, Length = fi.Length, Path = pname };
        }

        protected void GetChildItems(List<FileDir> items, DirectoryInfo di, GetItemsOptions options, int prefixLength)
        {
            if (options.HasFlag(GetItemsOptions.IncludeFiles))
            {
                foreach (FileInfo fi in di.GetFiles())
                {
                    items.Add(FromFI(fi, prefixLength));
                }
            }
            if (options.HasFlag(GetItemsOptions.IncludeDirectories))
            {
                foreach (DirectoryInfo cdi in di.GetDirectories())
                {
                    items.Add(new FileDir() { Name = cdi.Name, IsDirectory = true, Path = cdi.Parent.FullName.Length <= prefixLength?"": (cdi.Parent.FullName.Substring(prefixLength) + "\\") });
                    if (options.HasFlag(GetItemsOptions.Deep))
                        GetChildItems(items, cdi, options, prefixLength);
                }
            }
            
        }


        public string GetFilePublicUrl(string filePath)
        {
            return "/img" + (filePath.StartsWith("/") ? "" : "/") + filePath;
        }

        public FileUploadModel PrepareMetaData(int blocksCount, int blockSize, string fileName, long fileSize)
        {
            FileInfo fi = new FileInfo(getFullPath(fileName));
            Directory.CreateDirectory(fi.DirectoryName);
            FileStream fs = fi.Create();
            var fileToUpload = new FileSysFileUploadModel()
            {
                BlockCount = blocksCount,
                FileName = fileName,
                FileSize = fileSize,
                StartTime = DateTime.Now,
                IsUploadCompleted = false,
                UploadStatusMessage = string.Empty,
                BlockSize = blockSize,
                File = fs
            };
            return fileToUpload;
        }

        public FileUploadStatus UploadBlock(int id, byte[] chunk, FileUploadModel modelObj)
        {
            FileSysFileUploadModel model = modelObj as FileSysFileUploadModel;
            if (model != null && model.File != null)
            {
                if (chunk.Length > 0)
                {
                    model.File.Seek(id * model.BlockSize, SeekOrigin.Begin);
                    model.File.Write(chunk, 0, chunk.Length);
                }

                if (id == model.BlockCount)
                {
                    model.File.Dispose();
                    return new FileUploadStatus() { Error = false, IsLastBlock = true };
                }

            }

            return new FileUploadStatus() { Error = false, IsLastBlock = false };
        }

        public Binary GetFile(string fileName)
        {
            string fullName = getFullPath(fileName).Replace('/', '\\');           
            Binary bin = null;
            if (File.Exists(fullName))
            {
                bin = new Binary(File.OpenRead(fullName), false);
            }
            return bin;
        }

        public void WriteFile(string fileName, Binary data, bool asText = false)
        {
            FileInfo fi = new FileInfo(getFullPath(fileName));
            Directory.CreateDirectory(fi.DirectoryName);
            data.DataStream.Flush();
            File.WriteAllBytes(fi.FullName, data.Bytes);
        }

        public bool doesExist(string name)
        {
            return Directory.Exists(getFullPath(name)) || File.Exists(getFullPath(name));
        }

        public void DeleteFile(string fileName)
        {
            string fname = getFullPath(fileName);
            if (File.Exists(fname))
                File.Delete(fname);
            else
                throw new FileNotFoundException();
        }
        public void MoveToStorage(string name, ISiteStorage destination, string destName)
        {

        }

        protected string getFullPath(string relativePath)
        {
            string fpath = m_baseDir;
            if (!fpath.EndsWith("\\"))
                fpath += "\\";
            fpath+=relativePath;
            return fpath;
        }


    }

    public class FileSysFileUploadModel : FileUploadModel
    {
        public FileStream File { get; set; }
    }
}
