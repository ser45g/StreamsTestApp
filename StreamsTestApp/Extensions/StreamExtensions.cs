using System.Buffers;

namespace StreamsTestApp.Extensions
{
    public static class StreamExtensions
    {
        public static async Task CopyToStreamWithProgressAsync(this Stream source, Stream destination, Action<long>? bytesReadProgressCallback=null, CancellationToken cancellationToken = default) {
            const int bufferSize = 8 * 1024 * 8;
            byte[] buffer = new byte[bufferSize];//8kb
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
                
            }
        }
    }
}
