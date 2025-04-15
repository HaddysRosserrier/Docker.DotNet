using System;
using System.IO;

namespace Docker.DotNet.HR.Extended.Utils
{
    internal class ReadWriteStreamUtils(Stream read, Stream write) : Stream
    {
        ~ReadWriteStreamUtils()
        {
            Dispose();
        }

        public override bool CanRead => read.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => write.CanWrite;

        #region Not implemented methods

        public override long Length => throw new System.NotImplementedException();

        public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        public override void Flush()
        {
            read.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return read.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            write.Write(buffer, offset, count);
        }

        public override void Close()
        {
            try
            {
                write.Close();
            }
            finally
            {
                read.Close();
            }

            base.Close();
        }

        public override int GetHashCode()
        {
            return read.GetHashCode() ^ write.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    write.Dispose();
                }
                finally
                {
                    read.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
