## Summary 
This is a *sandbox* if you will to learn new concepts and generally broaden my own horizon.

## TODO 
- [ ] Create a protocol for messages
- [ ] Use a thread safe collection instead of the SemaphoreSlim: ConcurrenDictionary
- [ ] Create a client application for communication  
   - [ ] Param Name and PortNum?
- [ ] Create script to add to PATH for terminal invocation


## Things Learned
- ```TcpListener``` is a configurable helper class that opens up a port for use of TCP Connections.
   - Using a ```while``` loop to accept incoming client connections. 
- ```TcpClient``` is an encapsulation of a TCP connetion.
   - Using a ```while``` loop to read incoming stream.
- ```SemaphoreSlim``` used to prevent multiple threads from accessing a single resource. (This was used before I understood 
the concept of the Concurrent Collections)
- ```Memory<T>``` arbitrary contiguous region of memory. Async friendly version of ```Span<T>```. A view into the OG buffer
- ```Buffer.BlockCopy()``` Copies one buffer to another 
