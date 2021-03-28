using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlugAllocator.Core
{
    // alternative to memory chunk - works the same, but has more features and supports deallocation
    public unsafe class MemoryChunk
    {
        // properties
        public uint Offset { get; private set; }            // the base address of the allocation
        public uint Size { get; private set; }              // the size of the allocation
        public int Index { get; private set; }              // the index in the memory manager list
        public bool Allocated { get; set; }                 // flag indicating whether the chunk is currently allocated
        public bool ReadOnly { get; private set; }          // flag indicating whether the chunk is read only
        public string UID { get; private set; }             // optional flag - used if you need unique access to the chunk

        // pointer
        private byte* ptr;                                  // local pointer to the base address offset

        // memory usage - not very accurate, just checking if value is 0 then free++
        private uint free, used;

        // blank constructor - no properties will be set until it has been allocated
        public MemoryChunk() { }

        // constructor used for creating chunk in memory manager
        public MemoryChunk(uint offset, uint size, bool ro, string uid = "")
        {
            // set properties
            this.Offset = offset;
            this.Size = size;
            this.Allocated = false;
            this.ReadOnly = false;
            this.UID = uid;
            ptr = (byte*)Offset;
        }

        // externally set offset - should only be used by memory manager
        public void SetOffset(uint offset) { this.Offset = offset; ptr = (byte*)Offset; }

        // externally set index - should only be used by memory manager
        public void SetIndex(int index) { this.Index = index; ptr = (byte*)Offset; }

        // zero entire memory chunk
        public void Clear() { ptr = (byte*)Offset; Fill(0x00); }

        // fill chunk with repetitive 8-bit value
        public void Fill(byte data) { for (int i = 0; i < Size; i++) { ptr[i] = data; } }

        // write character to chunk
        public bool WriteChar(uint offset, char data)
        {
            // out of range
            if (offset >= Size) { return false; }

            // write
            ptr[offset] = (byte)data;
            return true;
        }

        // write boolean value to chunk
        public bool WriteBoolean(uint offset, bool data)
        {
            // out of range
            if (offset >= Size) { return false; }

            // write
            ptr[offset] = (byte)(data ? 1 : 0);
            return true;
        }

        // write 8-bit value to chunk
        public bool WriteInt8(uint offset, byte data)
        {
            // out of range
            if (offset >= Size) { return false; }

            // write
            ptr[offset] = data;
            return true;
        }

        // write 16-bit value to chunk
        public bool WriteInt16(uint offset, ushort data)
        {
            // out of range
            if (offset + 1 >= Size) { return false; }

            // write
            ptr[offset] = (byte)((data & 0xFF00) >> 8);
            ptr[offset + 1] = (byte)(data & 0x00FF);
            return true;
        }

        // write 32-bit value to chunk
        public bool WriteInt32(uint offset, uint data)
        {
            // out of range
            if (offset + 3 >= Size) { return false; }

            // write
            ptr[offset] = (byte)((data & 0xFF000000) >> 24);
            ptr[offset + 1] = (byte)((data & 0x00FF0000) >> 16);
            ptr[offset + 2] = (byte)((data & 0x0000FF00) >> 8);
            ptr[offset + 3] = (byte)(data & 0x000000FF);
            return true;
        }

        // write string to chunk
        public bool WriteString(uint offset, string data)
        {
            // out of range
            if (offset + data.Length >= Size) { return false; }

            // write
            for (uint i = 0; i < data.Length; i++) { ptr[offset + i] = (byte)data[(int)i]; }
            return true;
        }

        // read 8-bit value from chunk
        public byte ReadInt8(uint offset)
        {
            // out of range
            if (offset >= Size) { return 0; }

            // return
            byte data = ptr[offset];
            return data;
        }

        // read 16-bit value from chunk
        public ushort ReadInt16(uint offset)
        {
            // out of range
            if (offset + 1 >= Size) {return 0; }

            // return
            ushort data = (ushort)((ptr[offset] << 8) | ptr[offset + 1]);
            return data;
        }

        // read 32-bit value from chunk
        public uint ReadInt32(uint offset)
        {
            // out of range
            if (offset + 3 >= Size) {  return 0; }

            // return
            uint data = (uint)((ptr[offset] << 24) | (ptr[offset + 1] << 16) | ptr[offset + 2] << 8 | ptr[offset + 3]);
            return data;
        }

        // read character from chunk
        public char ReadChar(uint offset) { return (char)ReadInt8(offset); }

        // read string from chunk
        public string ReadString(uint offset, uint len)
        {
            // out of range
            if (offset + len >= Size) { return string.Empty; }

            // return
            string text = "";
            for (uint i = 0; i < len; i++) { text += (char)ptr[offset + i]; }
            return text;
        }

        // calculate memory usage
        public void CalculateUsage()
        {
            ptr = (byte*)Offset;

            // reset values
            free = 0; used = 0;
            
            // calculate
            for (uint i = 0; i < Size; i++) { if (ptr[i] == 0) { free++; } else { used++; } }
        }

        // get amount of memory used
        public uint GetUsed() { return used; }

        // get amount of memory free
        public uint GetFree() { return free; }
    }
}
