using System;
using System.Collections;
using System.Collections.Generic;

namespace GMaster.Core.Camera.Panasonic
{
    public class Slice : IEnumerable<byte>
    {
        private readonly byte[] array;
        private readonly int offset;

        public Slice(byte[] array, int offset = 0)
        {
            this.array = array;
            this.offset = offset;
            Length = array.Length - offset;
        }

        public Slice(Slice slice, int offset = 0, int length = -1)
        {
            array = slice.array;
            this.offset = offset + slice.offset;
            Length = slice.Length - offset;
            if (length > -1)
            {
                if (length > Length)
                {
                    throw new Exception("Slice length is wrong");
                }

                Length = length;
            }
        }

        public int Length { get; }

        public byte this[int index] => array[offset + index];

        public IEnumerator<byte> GetEnumerator()
        {
            for (var i = offset; i < offset + Length; i++)
            {
                yield return array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = offset; i < offset + Length; i++)
            {
                yield return array[i];
            }
        }

        public short ToShort(int i)
        {
            return (short)((this[i] << 8) + this[i + 1]);
        }

        public ushort ToUShort(int i)
        {
            return (ushort)((this[i] << 8) + this[i + 1]);
        }

        public object ToInt(int i)
        {
            return (short)((this[i] << 24) + (this[i + 1] << 16) + (this[i + 2] << 8) + this[i + 3]);
        }
    }
}