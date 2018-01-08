using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Kusog.SiteStorage
{

    public class AzureFileUploadModel : FileUploadModel
    {
        public CloudBlockBlob BlockBlob { get; set; }
    }

    public class AzureBlobStorage : ISiteStorage
    {
        protected static string s_defaultAccountName, s_defaultAccountKey;

        protected string m_accountName;
        protected string m_accountKey;
        protected string m_containerName;
        protected Guid m_id;

        public static void SetupDefaultAccountInfo(string name, string key)
        {
            s_defaultAccountName = name;
            s_defaultAccountKey = key;
        }

        public AzureBlobStorage(string accountName, string accountKey)
        {
            m_accountName = accountName;
            m_accountKey = accountKey;
            m_id = Guid.NewGuid();
        }

        public AzureBlobStorage(Guid id, string accessInfo)
        {
            m_id = id;
            string[] pieces = accessInfo.Split(',');
            if (pieces.Length == 3)
            {
                m_containerName = pieces[0];
                m_accountName = pieces[1];
                m_accountKey = pieces[2];
            }
            else
            {
                m_containerName = pieces[0];
                m_accountName = s_defaultAccountName;
                m_accountKey = s_defaultAccountKey;
            }
        }

        public bool CreateContainer(string containerName)
        {
            m_containerName = containerName;
            CloudStorageAccount storageAccount = CreateStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            try
            {
                container.FetchAttributes();
                return false;
            }
            catch (StorageException ex)
            {
                //Bad, but we expect this exception when the container does not exists.
                container.Create();
                return true;
            }
        }

        public Guid Id { get { return m_id; } }

        public bool doesExist(string name)
        {
            try
            {
                CloudBlobContainer container = GetContainer();
                return container.Exists();
            }
            catch (StorageException e)
            {
                return false;
            }
        }

        public void DeleteFile(string fileName)
        {
            CloudBlobContainer container = GetContainer();
            CloudBlockBlob imageBlob = container.GetBlockBlobReference(fileName);
            imageBlob.Delete();
        }

        public void MoveToStorage(string name, ISiteStorage destination, string destName)
        {

        }


        public string GetFilePublicUrl(string url)
        {
            var readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(30)
            };

            CloudBlobContainer container = GetContainer();
            CloudBlockBlob imageBlob = container.GetBlockBlobReference(url);

            Uri u = new Uri(imageBlob.Uri.AbsoluteUri + imageBlob.GetSharedAccessSignature(readPolicy));

            return u.ToString();
        }



        public List<FileDir> GetItems(string path, GetItemsOptions options=0)
        {
            path = path.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(path))
                path = "/";
            path = path.Replace("//", "/");
            List<FileDir> list = new List<FileDir>();
            CloudBlobContainer container = GetContainer(true);
            if (path.EndsWith("/"))
            {
                IEnumerable<IListBlobItem> items = null;
                if(!string.IsNullOrWhiteSpace(path) && path != "/")
                { 
                    CloudBlobDirectory dir = container.GetDirectoryReference(path);
                    items = dir.ListBlobs(false, BlobListingDetails.Metadata);// container.ListBlobs(path/*, new BlobRequestOptions() { UseFlatBlobListing = true }*/);
                }
                else
                    items = container.ListBlobs(null, false, BlobListingDetails.Metadata);

                //IEnumerable<IListBlobItem> x = client.ListBlobsWithPrefix(getPath(path));
                if (items != null)
                    GetChildItems(list, items, options,null);
            }
            else
            {
                try
                {
                    var items = container.ListBlobs(path);
                    if (items != null)
                        foreach(var i in items)
                            if(i is CloudBlockBlob)
                                list.Add(FromBlob((CloudBlockBlob)i));


                    //CloudBlockBlob blob = container.GetBlockBlobReference(path);
                    //blob.FetchAttributes();
                    //list.Add(FromBlob(blob));
                }
                catch (Exception)
                {
                    //Intentionally left blank.  FetchAttributes will throw exception if file doesn't exist.
                }
            }

            return list;
        }

        protected FileDir FromBlob(CloudBlockBlob blob)
        {
            string pname = blob.Parent == null ? "" : blob.Parent.Prefix;
            return new FileDir() { Name = blob.Name.Substring(pname.Length), CreationTimeUtc = blob.Properties.LastModified.Value.Date, IsDirectory = false, Length = blob.Properties.Length, Path = pname };
        }

        protected void GetChildItems(List<FileDir> list, IEnumerable<IListBlobItem> items, GetItemsOptions options, FileDir parentDir)
        {
            foreach (IListBlobItem item in items)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    if(options.HasFlag(GetItemsOptions.IncludeFiles))
                        list.Add(FromBlob((CloudBlockBlob)item));

                    if (parentDir != null)
                        parentDir.Length++;
                }
                else if (options.HasFlag(GetItemsOptions.IncludeDirectories) && item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory bdir = (CloudBlobDirectory)item;
                    string pname = bdir.Parent == null?"":bdir.Parent.Prefix;
                    FileDir childDir = new FileDir() { Name = bdir.Prefix.Substring(pname.Length, bdir.Prefix.Length - pname.Length - 1), IsDirectory = true, Path = pname };
                    list.Add(childDir);
                    if (options.HasFlag(GetItemsOptions.Deep))
                    {
                        GetChildItems(list, bdir.ListBlobs(), options, childDir);
                    }
                }
            }
        }


        protected string getPath(string name)
        {
            int i = name.LastIndexOf('/');
            if (i > -1)
                return name.Substring(0, i - 1);
            return "";
        }

        public Binary GetFile(string fileName)
        {
            CloudBlobContainer container = GetContainer(true);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            try
            {
                //return new Binary(blob.OpenRead(), false);
                var b = new Binary();
                blob.DownloadToStream(b.DataStream);
                b.DataStream.Position = 0;
                return b;
            }
            catch (Exception)
            {
            }
            return null;
        }

        public void WriteFile(string fileName, Binary data, bool asText = false)
        {
            CloudBlobContainer container = GetContainer(true);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            string mimeType = MediaStream.calcMimeType(Path.GetExtension(fileName));
            blob.Properties.ContentType = mimeType;
            blob.Properties.ContentEncoding = "";
            if (!asText)
                blob.UploadFromStream(data.DataStream);
            else
            {
                using(var strReader = new StreamReader(data.DataStream))
                    blob.UploadText(strReader.ReadToEnd(), Encoding.ASCII);
            }
        }

        public void WriteFile(string fileName, MediaStream data, bool asText = false)
        {
            CloudBlobContainer container = GetContainer(true);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = data.MimeType;
            if (!asText)
                blob.UploadFromStream(data.DataStream);
            else
            {
                using (var strReader = new StreamReader(data.DataStream))
                    blob.UploadText(strReader.ReadToEnd(), Encoding.ASCII);
            }
        }

        public FileUploadModel PrepareMetaData(int blocksCount, int blockSize, string fileName, long fileSize)
        {
            CloudBlobContainer container = GetContainer(true);
            var fileToUpload = new AzureFileUploadModel()
            {
                BlockCount = blocksCount,
                FileName = fileName,
                FileSize = fileSize,
                BlockBlob = container.GetBlockBlobReference(fileName),
                StartTime = DateTime.Now,
                IsUploadCompleted = false,
                UploadStatusMessage = string.Empty
            };
            return fileToUpload;
        }

        public FileUploadStatus UploadBlock(int id, byte[] chunk, FileUploadModel modelObj)
        {
            AzureFileUploadModel model = modelObj as AzureFileUploadModel;
            bool errorInOperation = false;
            if (model != null && chunk.Length != 0)
            {
                using (var chunkStream = new MemoryStream(chunk))
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", id)));
                    try
                    {
                        model.BlockBlob.PutBlock(blockId, chunkStream, null);
                    }
                    catch (StorageException e)
                    {
                        //...
                        return new FileUploadStatus() { Error = true, IsLastBlock = false, Message = model.UploadStatusMessage };
                    }
                }
            }

            if (id == model.BlockCount)
            {
                //...
                try
                {
                    var blockList = Enumerable.Range(0, (int)model.BlockCount).ToList<int>().ConvertAll(rangeElement => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));
                    model.BlockBlob.PutBlockList(blockList);
                    //...
                    return new FileUploadStatus() { IsLastBlock = true };
                }
                catch (StorageException e)
                {
                    errorInOperation = true;
                    //...
                }
                finally
                {
                    //Session.Clear();
                }

                return new FileUploadStatus() { Error = errorInOperation, IsLastBlock = model.IsUploadCompleted, Message = model.UploadStatusMessage };
            }

            return new FileUploadStatus() { Error = false, IsLastBlock = false }; //new { error = true, isLastBlock = false, message = "Error" /*string.Format(Resources.FailedToUploadFileMessage, Resources.SessonExpired)*/ };
        }



        protected virtual CloudStorageAccount CreateStorageAccount()
        {
            return CloudStorageAccount.Parse(string.Format("DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1}", m_accountName, m_accountKey));
        }

        protected CloudBlobContainer m_container = null;
        protected virtual CloudBlobContainer GetContainer(bool createIfNotExist = false)
        {        
            if(m_container == null)
                lock(this)
                    if (m_container == null)
                    {
                        CloudStorageAccount storageAccount = CreateStorageAccount();
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference(m_containerName);
                        if (createIfNotExist)
                            container.CreateIfNotExists();
                        return m_container = container;
                    }
            return m_container;
        }

    }
}
