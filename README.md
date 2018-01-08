This library allows you to write code to an abstract file system interface that can work with the local file storage or Azure blob storage. The code will eventually support SkyDrive, iCloud, and DropBox. 

If you have an interest to add support for a new storage system, please do. Extra hands working on this library would be nice.

I use this to have my ASP.NET sites use local storage when using IIS on my server, but use Azure blob storage when running on Azure.

The base code was written to support users of the site providing access to their own storage, such as their own Azure blob storage, so that when they upload content to the site it will store and access that content from the user's storage.