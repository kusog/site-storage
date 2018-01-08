using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kusog.SiteStorage
{
    public class StorageFactory
    {
        public delegate ISiteStorage Creator(Guid id, string storageConfig);

        protected static Dictionary<string, StorageFactory.Creator> s_storageFactories = new Dictionary<string, StorageFactory.Creator>();

        static StorageFactory()
        {
            RegisterStorageType("filesys", delegate(Guid id, string storageConfig)
            {
                return new FileSysStorage(storageConfig, id);
            });
            RegisterStorageType("azure", delegate(Guid id, string storageConfig)
            {
                return new AzureBlobStorage(id, storageConfig);
            });
        }

        public static ISiteStorage GetStorage(string storageType, Guid instanceId, string storageConfig)
        {
            ISiteStorage storage = null;
            storageType = storageType.ToLower();

            if (s_storageFactories.ContainsKey(storageType))
                storage = s_storageFactories[storageType](instanceId, storageConfig);

            return storage;
        }

        public static void RegisterStorageType(string storageType, StorageFactory.Creator creator)
        {
            lock (s_storageFactories)
            {
                s_storageFactories[storageType] = creator;
            }
        }
    }
}
