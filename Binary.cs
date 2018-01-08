
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
using System.IO;
using System.Runtime.Serialization;

namespace Kusog.SiteStorage
{
	[Serializable]
	public class Binary : IDisposable, IConvertible, ISerializable
	{
		protected Stream m_dataStream = null;
		protected byte[] m_data = null;

		public static explicit operator byte[](Binary b)
		{
			return b.Bytes;
		}

		public Binary(Binary b)
		{
			if(b.Bytes != null)
				m_data = (byte[])b.Bytes.Clone();
		}

		public Binary(Stream str, bool pullBytesFromStreamNow)
		{
			m_dataStream = str;
			if (pullBytesFromStreamNow)
			{
				byte[] b = Bytes;
				m_dataStream = null;
			}
		}

        public Binary(string d)
        {
            m_data = new byte[d.Length * sizeof(char)];
            System.Buffer.BlockCopy(d.ToCharArray(), 0, m_data, 0, m_data.Length);
        }

		public Binary(byte[] d)
		{
			m_data = d;
		}

		public Binary()
		{
		}

		public bool HasDataStream { get { return m_dataStream != null; } }

		public byte[] Bytes
		{
			get
			{
				if(m_data == null && m_dataStream != null)
			{
					if(m_dataStream.CanSeek)
					{
						int len = (int)m_dataStream.Length;
						m_data = new byte[len];
						m_dataStream.Read(m_data, 0, len);
					}
					else
					{
						MemoryStream ms = new MemoryStream();
						int bt;
						while((bt = m_dataStream.ReadByte()) != -1)
							ms.WriteByte((byte)bt);
						m_data = ms.ToArray();
						ms.Position = 0;      
					}
				}
				return m_data;
			}
		}

        public string GetString()
        {
            char[] chars = new char[Bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(Bytes, 0, chars, 0, Bytes.Length);
            return new string(chars);
        }

		public Stream DataStream 
		{
			get
			{
				if(m_dataStream == null)
					m_dataStream = (m_data == null?new MemoryStream():new MemoryStream(m_data));
                if(m_dataStream.CanSeek)
				    m_dataStream.Position = 0;

				return m_dataStream;
			}
		}
		#region IDisposable Members

		public void Dispose()
		{
			cleanUp();
			GC.SuppressFinalize(this);
		}
		private void cleanUp()
		{
            try
            {
                if (m_dataStream != null)
                {
                    m_dataStream.Close();
                    m_dataStream.Dispose();
                }
            }
            finally
            {
                m_dataStream = null;
                m_data = null;
            }
		}
		~ Binary()
		{
			cleanUp();
		}

		#endregion

		#region IConvertible Members 

		public ulong ToUInt64(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToUInt64 implementation
			return 0;
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToSByte implementation
			return 0;
		}

		public double ToDouble(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToDouble implementation
			return 0;
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToDateTime implementation
			return new DateTime ();
		}

		public float ToSingle(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToSingle implementation
			return 0;
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToBoolean implementation
			return false;
		}

		public int ToInt32(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToInt32 implementation
			return 0;
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToUInt16 implementation
			return 0;
		}

		public short ToInt16(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToInt16 implementation
			return 0;
		}

		public string ToString(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToString implementation
			return null;
		}

		public byte ToByte(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToByte implementation
			return 0;
		}

		public char ToChar(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToChar implementation
			return '\0';
		}

		public long ToInt64(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToInt64 implementation
			return 0;
		}

		public System.TypeCode GetTypeCode()
		{
			// TODO:  Add Binary.GetTypeCode implementation
			return new System.TypeCode ();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToDecimal implementation
			return 0;
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			if(conversionType.Equals(typeof(byte[])))
				return this.Bytes;
			return null;
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			// TODO:  Add Binary.ToUInt32 implementation
			return 0;
		}

		#endregion

        #region ISerializable Members

		public Binary(System.Runtime.Serialization.SerializationInfo info, StreamingContext context)
		{
            m_data = (byte[])info.GetValue("d", typeof(byte[]));
        }
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, StreamingContext context)
        {
            info.AddValue("d", Bytes);
        }

        #endregion
    }
}
