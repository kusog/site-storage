
//Copyright 1997-2009 Syrinx Development, Inc.
//This file is part of the Syrinx Web Application Framework (SWAF).
// == BEGIN LICENSE ==
//
// Licensed under the terms of any of the following licenses at your
// choice:
//
//  - GNU General Public License Version 3 or later (the "GPL")
//    http://www.gnu.org/licenses/gpl.html
//
//  - GNU Lesser General Public License Version 3 or later (the "LGPL")
//    http://www.gnu.org/licenses/lgpl.html
//
//  - Mozilla Public License Version 1.1 or later (the "MPL")
//    http://www.mozilla.org/MPL/MPL-1.1.html
//
// == END LICENSE ==
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;

namespace Kusog.SiteStorage
{
	/// <summary>
	/// Used to manage the data for an image in either a byte array format or as a 
	/// generic System.IO.Stream.  This class keeps track of the mime type of the image
	/// to make it easier to "remember" what kind of image it is.
	/// </summary>
	[Serializable]
	public class MediaStream : Binary, ISerializable
	{
		protected string m_type;
		protected string m_name;

		protected static IDictionary s_contentMappings;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="stream"></param>
		/// <param name="name"></param>
		public MediaStream(string type, Stream stream, string name)
			:base(stream,false)
		{
			m_type = calcMimeType(type);
			m_name = name;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="imageData"></param>
        /// <param name="name"></param>
		public MediaStream(string type, byte[] imageData, string name)
			:base(imageData)
		{
			m_type = calcMimeType(type);
			m_name = name;
		}

        public MediaStream(string type)
        {
            m_type = calcMimeType(type);
        }

        /// <summary>
        /// 
        /// </summary>
		private static char[] s_typeTrims = new char[]{'.'};
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
		public static string calcMimeType(string baseType)
		{
			string type = (string)s_contentMappings[baseType.ToLower()];
			if(type == null)
			{
				//Perhaps we should throw an exception but right now this
				//code lets the "unknown" type of be used to build a mime
				//type directly.  If a new type is slipped in somehow, via 
				//some new .NET library things might still work.  However, if 
				//a bad type name is used an error with the mime type will be 
				//created too.  Ultimately this means a failure later in the image
				//processing that is using this MediaStream.
				if(!baseType.StartsWith("image/"))
					type = "image/" + baseType.Trim(s_typeTrims);
			}
			return type;
		}

		/// <summary>
		/// Builds a static map of mime types using well known image
		/// file extensions as the key.  This makes it easy to do a map lookup
		/// on a file extension and get the mime type ready to use, rather then
		/// trying to build a string manually every time an image request comes in.
		/// This helps to improve performance.
		/// </summary>
		static MediaStream()
		{
			s_contentMappings = new Hashtable();
			s_contentMappings[".jpg"] = "image/jpeg";
			s_contentMappings[".jpeg"] = "image/jpeg";
			s_contentMappings[".tif"] = "image/tiff";
			s_contentMappings[".tiff"] = "image/tiff";
			s_contentMappings[".gif"] = "image/gif";
			s_contentMappings[".png"] = "image/png";
			s_contentMappings[".bmp"] = "image/bmp";

			//These entries as the same as the ones above, minus the
			//prefixed period in the key to help clients creating a Media
			//Stream that don't have a "file extension", but just a simple
			//type name.  This allows successful map searches.
			s_contentMappings["jpg"] = "image/jpeg";
			s_contentMappings["jpeg"] = "image/jpeg";
			s_contentMappings["tif"] = "image/tiff";
			s_contentMappings["tiff"] = "image/tiff";
			s_contentMappings["gif"] = "image/gif";
			s_contentMappings["png"] = "image/png";
			s_contentMappings["bmp"] = "image/bmp";

			s_contentMappings["image/jpeg"] = "image/jpeg";
			s_contentMappings["image/tiff"] = "image/tiff";
			s_contentMappings["image/gif"] = "image/gif";
			s_contentMappings["image/png"] = "image/png";
			s_contentMappings["image/bmp"] = "image/bmp";

			s_contentMappings[".txt"] = "text/plain";
			s_contentMappings[".mov"] = "video/quicktime";
			s_contentMappings[".pdf"] = "application/pdf";
			s_contentMappings[".wmv"] = "video/x-ms-wmv";


            s_contentMappings["wav"] = "audio/wav";
            s_contentMappings[".wav"] = "audio/wav";
            s_contentMappings["audio/wav"] = "audio/wav";

            s_contentMappings["js"] = "application/javascript; charset=UTF8";
            s_contentMappings[".js"] = "application/javascript; charset=UTF8";
            s_contentMappings["application/javascript"] = "application/javascript";
        }
        /// <summary>
        /// 
        /// </summary>
		public string Name {get{return m_name;}}
        /// <summary>
        /// 
        /// </summary>
		public string MimeType {get{return m_type;}}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public MediaStream(System.Runtime.Serialization.SerializationInfo info, StreamingContext context)
            :base(info,context)
		{
            m_type = info.GetString("ty");
            m_name = info.GetString("tn");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public new void GetObjectData(System.Runtime.Serialization.SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ty", m_type);
            info.AddValue("tn", m_name);
        }
	}
}
