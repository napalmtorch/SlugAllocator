using System;
using System.Collections.Generic;
using System.Text;

namespace SlugAllocator.Core
{
    // dynamic memory management class designed for cosmos
    public static unsafe class MemoryManager
    {
        // chunks
        public static List<MemoryChunk> Chunks { get; private set; }            // list of memory chunks
        public static int TotalChunks { get { return Chunks.Count; } }          // amount allocated memory chunks

        // properties
        public static uint OffsetBottom { get; private set; }                   // lowest address usable to manager
        public static uint OffsetTop { get; private set; }                      // highest address usable to manageer
        public static uint Size { get; private set; }                           // total size in bytes of all usable space
        public static uint ChunkPointer { get; private set; }                   // variable storing the pointer for the allocation stack
        public static bool LogToConsole { get; set; }                           // flag indicating whether to print operations to console

        // initialization
        public static void Initialize(uint bottom, uint top)
        {
            // create list
            Chunks = new List<MemoryChunk>();

            // set pointer to top of stack
            ChunkPointer = bottom;

            // log to console
            if (LogToConsole) { Console.WriteLine("[BOTTOM] " + UnsignedIntegerToHex(bottom, true) + "   [TOP] " + UnsignedIntegerToHex(top, true)); }
        }

        // swape range of memory
        public static void Swap(byte* dest, byte* source, uint len)
        {
            for (uint i = 0; i < len; i++)
                if (*(dest + i) != *(source + i))
                    *(dest + i) = *(source + i);
        }

        // swap range of memory using blocks
        public static void Swap(MemoryChunk dest, MemoryChunk src, uint len) { Swap((byte*)dest.Offset, (byte*)src.Offset, len); }

        // copy range of memory
        public static void Copy(byte* src, byte* dest, uint len)
        {
            for (uint i = 0; i < len; i++) { *(dest + i) = *(src + i); }
        }

        // allocate chunk of memory
        public static MemoryChunk Allocate(uint size, bool ro, string uid = "")
        {
            // not enough space to allocate
            if (ChunkPointer - size < OffsetBottom)
            {
                if (LogToConsole) { Console.WriteLine("Allocation overflow"); }
                return null; 
            }
            // decrement pointer
            else { ChunkPointer -= size; }

            // create chunk
            MemoryChunk chunk = new MemoryChunk(ChunkPointer, size, ro, uid);
            chunk.Allocated = true;

            // zero chunk
            chunk.Clear();

            // add chunk to list
            Chunks.Add(chunk);
            Size = (uint)Math.Abs(OffsetTop - OffsetBottom);

            // update index values
            UpdateIndexValues();

            // log message to console
            if (LogToConsole) 
            {
                Console.WriteLine("Allocated " + size.ToString() + " bytes at offset " + UnsignedIntegerToHex(chunk.Offset, true));
                Console.WriteLine("Chunk Pointer: " + UnsignedIntegerToHex(ChunkPointer, true));
            }

            // return memory chunk
            return chunk;
        }

        // free chunk of memory
        public static bool Free(MemoryChunk chunk)
        {
            // check if chunk is actually able to free
            if (chunk == null) { return false; }
            if (!chunk.Allocated) { return false; }

            /* this is normally where you would notify the user a memory is being free'd
               in my operating system, I re-draw'd the screen but changed the mouse cursor
               to a a loading wheel/hour glass */
            Console.WriteLine("Collecting garbage, this could take some time...");

            // clear data from chunk
            chunk.Clear();

            // unallocate chunk
            PerformShift(chunk);
            chunk.Allocated = false;

            // remove chunk
            Chunks.RemoveAt(chunk.Index);
            Size = (uint)Math.Abs(OffsetTop - OffsetBottom);

            // update index values
            UpdateIndexValues();

            // update pointers
            UpdatePointers(chunk.Offset, chunk.Size);
            ChunkPointer += chunk.Size;

            // log to console
            if (LogToConsole)
            {
                Console.WriteLine("Un-allocated " + chunk.Size.ToString() + " bytes at offset " + UnsignedIntegerToHex(chunk.Offset, true));
                Console.WriteLine("Chunk Pointer: " + UnsignedIntegerToHex(ChunkPointer, true));
            }

            // return
            return true;
        }

        // free based on offset - will only work if offset is base of chunk
        public static bool Free(uint offset)
        {
            return false;
        }

        // perform unallocation method - clear data and shift other chunks close to top
        private static void PerformShift(MemoryChunk chunk)
        {
            // get bottom address of chunks to relocate
            uint bottom = chunk.Offset;
            for (int i = 0; i < Chunks.Count; i++) { bottom -= Chunks[i].Size; }

            // save last value - this may not be necessary but you know what they say
            // don't fix it if it ain't broke xD
            byte* savePtr = (byte*)(chunk.Offset + chunk.Size);
            byte saved = savePtr[0];

            // shift all data towards top of stack
            for (uint i = 0; i < chunk.Size; i++) { ShiftRight(chunk.Offset + i, bottom); }

            // return saved value
            savePtr[1] = saved;
        }

        // shift all data right by 1 bytes
        private static void ShiftRight(uint top, uint bottom)
        {
            byte* ptr = (byte*)top;
            byte saved = ptr[1];
            for (uint i = top; i > bottom; i--)
            {
                ptr = (byte*)i;
                ptr[1] = ptr[0];
            }
            ptr = (byte*)top;
            ptr[1] = saved;
        }

        // update pointer values of all chunks in list
        private static void UpdatePointers(uint offset, uint size)
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                if (Chunks[i].Offset <= offset)
                {
                    Chunks[i].SetOffset(Chunks[i].Offset + size);
                }
            }
        }

        // update index values of all chunks in list
        private static void UpdateIndexValues()
        {
            for (int i = 0; i < Chunks.Count; i++) { Chunks[i].SetIndex(i); }
        }

        // convert integer to hex string
        public static string IntegerToHex(int n, bool prefix)
        {
            string output = "";
            int q, dn = 0, m, l;
            int tmp;
            int s;
            q = n;
            for (l = q; l > 0; l = l / 16)
            {
                tmp = l % 16;
                if (tmp < 10)
                    tmp = tmp + 48;
                else
                    tmp = tmp + 55;
                dn = dn * 100 + tmp;
            }
            for (m = dn; m > 0; m = m / 100)
            {
                s = m % 100;
                output += ((char)s).ToString();
            }
            if (output == "") { output = "0"; }
            if (output.Length == 1) { output = "0" + output; }
            return output;
            if (prefix) { return "0x" + output; }
            else { return output; }
        }

        // convert 32-bit integer to hex string
        public static string UnsignedIntegerToHex(uint decn, bool prefix)
        {
            string left1 =  IntegerToHex((byte)((decn & 0xFF000000) >> 24), false);
            string left2 =  IntegerToHex((byte)((decn & 0x00FF0000) >> 16), false);
            string right1 = IntegerToHex((byte)((decn & 0x0000FF00) >> 8), false);
            string right2 = IntegerToHex((byte) (decn & 0x000000FF), false);
            if (prefix) { return "0x" + left1 + left2 + right1 + right2; }
            else { return left1 + left2 + right1 + right2; }
        }
    }
}
