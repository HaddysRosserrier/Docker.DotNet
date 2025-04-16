using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Docker.DotNet.HR.Extended.Utils
{
    internal class ReadWriteStreamUtils(Stream read, Stream write) : Stream
    {
        public override bool CanRead => read.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => write.CanWrite;

        public override void Flush()
        {
            read.Flush();
            write.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return read.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return read.ReadByte();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await read.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(true);
        }

        

        public override void Write(byte[] buffer, int offset, int count)
        {
            write.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            write.WriteByte(value);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await write.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(true);
        }

        public override void Close()
        {
            CloseWrite();
            CloseRead();
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
                DisposeWrite();
                DisposeRead();
            }

            base.Dispose(disposing);
        }

        #region Not Supported 

        public override long Length => throw new System.NotSupportedException();

        public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        #endregion

        private void CloseRead()
        {
            try
            {
                read.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void CloseWrite()
        {
            try
            {
                write.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void DisposeRead()
        {
            try
            {
                read.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void DisposeWrite()
        {
            try
            {
                write.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
