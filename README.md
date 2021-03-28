# SlugAllocator
Dynamic memory management system for Cosmos OS projects
The method I'm using for deallocation is quite simple to understand, but
is slow when deallocating large amounts of memory. When the memory manager
gets a request to deallocate memory, it clears the region of the chunk,
then shifts the memory after the deleted chunk to line up with the other chunks,
and then updates the chunk offsets. I've included a Kernel.cs for a basic
demonstation for how it works

# Usage
Add MemoryChunk.cs and MemoryManager.cs to your project. Once they are added,
you can allocate memory by using MemoryManager.Allocate() and MemoryManager.Free(),
and I also recommend you read the code
