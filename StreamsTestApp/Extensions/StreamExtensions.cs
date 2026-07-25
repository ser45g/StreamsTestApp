using System.Buffers;

namespace StreamsTestApp.Extensions
{
    public static class StreamExtensions
    {
        public static async Task CopyToStreamWithProgressAsync(this Stream source, Stream destination, Action<long>? bytesReadProgressCallback=null, int bufferSize = 8096, CancellationToken cancellationToken = default) {
            
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);//8kb
            
            int bytesRead;
            try
            {
                while ((bytesRead = await source.ReadAsync(new Memory<byte>(buffer), cancellationToken)) != 0)
                {
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken);
                    bytesReadProgressCallback?.Invoke(bytesRead);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
