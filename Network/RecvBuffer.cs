using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public class RecvBuffer
    {
        // [r][][w][][]..
        ArraySegment<byte> _buffer;

        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize]);
        }

        public int DataSize => _writePos - _readPos;
        public int FreeSize => _buffer.Count - _writePos;
        public ArraySegment<byte> ReadSegment => 
            new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);
        public ArraySegment<byte> WriteSegment => 
            new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
        
        public void Clean()
        {
            // Copy and reset
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                _readPos = _writePos = 0;
                return;
            }

            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, 0, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;
            _writePos += numOfBytes;
            return true;
        }
    }
}
