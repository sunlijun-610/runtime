// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Reflection.Internal
{
    internal sealed unsafe class ReadOnlyUnmanagedMemoryStream : Stream
    {
        private readonly byte* _data;
        private readonly int _length;
        private int _position;

        public ReadOnlyUnmanagedMemoryStream(byte* data, int length)
        {
            _data = data;
            _length = length;
        }

        public override unsafe int ReadByte()
        {
            if (_position >= _length)
            {
                return -1;
            }

            return _data[_position++];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = Math.Min(count, _length - _position);
            Marshal.Copy((IntPtr)(_data + _position), buffer, offset, bytesRead);
            _position += bytesRead;
            return bytesRead;
        }

#if NET
        // Duplicate the Read(byte[]) logic here instead of refactoring both to use Spans
        // so we don't affect perf on .NET Framework.
        public override int Read(Span<byte> buffer)
        {
            int bytesRead = Math.Min(buffer.Length, _length - _position);
            new Span<byte>(_data + _position, bytesRead).CopyTo(buffer);
            _position += bytesRead;
            return bytesRead;
        }
#endif

        public override void Flush()
        {
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long target;
            try
            {
                target = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => checked(offset + _position),
                    SeekOrigin.End => checked(offset + _length),
                    _ => throw new ArgumentOutOfRangeException(nameof(origin)),
                };
            }
            catch (OverflowException)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (target < 0 || target > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            _position = (int)target;
            return target;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
