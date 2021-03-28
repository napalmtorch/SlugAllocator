using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using SlugAllocator.Core;

namespace SlugAllocator
{
    public class Kernel : Sys.Kernel
    {
        // boot sequence
        protected override void BeforeRun()
        {
            // prepare screen and print message
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Slug Memory Allocator");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("version 1.4");
            Console.ForegroundColor = ConsoleColor.White;

            // initialize memory manager
            MemoryManager.LogToConsole = true;                      // set before init so bottom and top address print to console
            MemoryManager.Initialize(0x1000000, 0x100000);          // perform mandatory initialization

            // test allocation
            MemoryChunk chk = MemoryManager.Allocate(512, false, "poop_chunk");
            MemoryManager.Free(chk);
        }

        // main
        protected override void Run()
        {
            // print input caret to screen
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("shell");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("> ");

            // get input from user
            string input = Console.ReadLine();

            // parse command
            ParseCommand(input);
        }

        // parse command
        private void ParseCommand(string input)
        {
            // split command into arguments
            string[] args = input.Split(' ');

            // clear the screen
            if (args[0].ToUpper() == "CLS")
            {
                Console.Clear();
            }
            // allocate chunk of memory
            else if (args[0].ToUpper() == "ALLOC")
            {
                if (args.Length == 2)
                {
                    uint size;
                    if (!uint.TryParse(args[1], out size))
                    {
                        // invalid size
                        ThrowError("Invalid size");
                    }

                    // valid size
                    MemoryManager.Allocate(size, false);
                }
                else { ThrowError("Invalid arguments"); }
            }
            // free chunk of memory
            else if (args[0].ToUpper() == "FREE")
            {
                if (args.Length == 2)
                {
                    uint offset;
                    // invalid offset
                    if (!uint.TryParse(args[1], out offset)) { ThrowError("Invalid offset"); }
                    // offset valid, locate chunk
                    else
                    {
                        for (int i = 0; i < MemoryManager.TotalChunks; i++)
                        {
                            if (MemoryManager.Chunks[i].Offset == offset) { MemoryManager.Free(MemoryManager.Chunks[i]); return; }
                        }
                        ThrowError("Could not locate chunk with specified offset");
                    }
                }
                else { ThrowError("Invalid arguments"); }
            }
            // invalid command
            else
            {
                Console.ForegroundColor = ConsoleColor.Red; 
                Console.WriteLine("Invalid command"); 
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void ThrowError(string msg)
        {
            // invalid size
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
