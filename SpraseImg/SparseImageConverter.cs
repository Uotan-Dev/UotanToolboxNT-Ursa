namespace SpraseImg;

/// <summary>
/// Sparse 镜像转换器
/// </summary>
public class SparseImageConverter
{

    /// <summary>
    /// 将sparse镜像转换为原始镜像
    /// </summary>
    public static void ConvertSparseToRaw(string[] inputFiles, string outputFile)
    {
        using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
        long maxFileSize = 0;
        foreach (var inputFile in inputFiles)
        {
            using var tempStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            var tempSparseFile = SparseFile.FromStream(tempStream);
            var fileSize = (long)tempSparseFile.Header.TotalBlocks * tempSparseFile.Header.BlockSize;
            maxFileSize = Math.Max(maxFileSize, fileSize);
        }
        outputStream.SetLength(maxFileSize);
        foreach (var inputFile in inputFiles)
        {
            using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            var sparseFile = SparseFile.FromStream(inputStream);
            WriteRawImageFromSparse(sparseFile, outputStream);
        }
    }

    /// <summary>
    /// 将原始镜像转换为sparse镜像
    /// </summary>
    public static void ConvertRawToSparse(string inputFile, string outputFile, uint blockSize = 4096)
    {
        using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
        using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

        var fileSize = inputStream.Length;

        var sparseFile = CreateSparseFromRaw(inputStream, blockSize, fileSize);
        sparseFile.WriteToStream(outputStream);
    }

    /// <summary>
    /// 从sparse文件写入原始镜像数据
    /// </summary>
    private static void WriteRawImageFromSparse(SparseFile sparseFile, Stream outputStream)
    {
        var blockSize = sparseFile.Header.BlockSize;
        var currentBlock = 0u;
        var blocksWritten = 0u;

        foreach (var chunk in sparseFile.Chunks)
        {
            var targetPosition = (long)currentBlock * blockSize;
            outputStream.Seek(targetPosition, SeekOrigin.Begin);

            switch (chunk.Header.ChunkType)
            {
                case SparseFormat.CHUNK_TYPE_RAW:
                    if (chunk.Data != null)
                    {
                        outputStream.Write(chunk.Data, 0, chunk.Data.Length);
                        blocksWritten += chunk.Header.ChunkSize;
                        var remainingBytes = (int)((chunk.Header.ChunkSize * blockSize) - chunk.Data.Length);
                        if (remainingBytes > 0)
                        {
                            var padding = new byte[remainingBytes];
                            outputStream.Write(padding, 0, padding.Length);
                        }
                    }
                    break;

                case SparseFormat.CHUNK_TYPE_FILL:
                    var fillBytes = BitConverter.GetBytes(chunk.FillValue);
                    var totalFillSize = chunk.Header.ChunkSize * blockSize;
                    blocksWritten += chunk.Header.ChunkSize;

                    for (uint i = 0; i < totalFillSize; i += 4)
                    {
                        var bytesToWrite = Math.Min(4, (int)(totalFillSize - i));
                        outputStream.Write(fillBytes, 0, bytesToWrite);
                    }
                    break;

                case SparseFormat.CHUNK_TYPE_DONT_CARE:
                    blocksWritten += chunk.Header.ChunkSize;
                    break;

                case SparseFormat.CHUNK_TYPE_CRC32:
                    break;

                default:
                    throw new InvalidDataException($"未知的 chunk 类型: 0x{chunk.Header.ChunkType:X4}");
            }

            currentBlock += chunk.Header.ChunkSize;
        }
    }

    /// <summary>
    /// 从原始文件创建sparse文件，等待优化。生成的稀疏文件大小略大于AOSP中的Cpp实现版本。原因待查
    /// </summary>
    private static SparseFile CreateSparseFromRaw(Stream inputStream, uint blockSize, long fileSize)
    {
        var sparseFile = new SparseFile(blockSize, fileSize);
        var buffer = new byte[blockSize];
        var currentBlock = 0u;

        while (inputStream.Position < fileSize)
        {
            var bytesRead = inputStream.Read(buffer, 0, (int)blockSize);
            if (bytesRead == 0)
            {
                break;
            }

            if (IsZeroBlock(buffer, bytesRead))
            {
                var zeroBlocks = CountConsecutiveZeroBlocks(inputStream, blockSize);

                if (zeroBlocks > 0)
                {
                    sparseFile.AddDontCareChunk(zeroBlocks * blockSize);
                    currentBlock += zeroBlocks;

                    for (uint i = 0; i < zeroBlocks; i++)
                    {
                        inputStream.ReadExactly(buffer, 0, (int)blockSize);
                    }
                    continue;
                }
            }

            if (bytesRead == blockSize && IsFillBlock(buffer, out var fillValue))
            {
                var fillBlocks = CountConsecutiveFillBlocks(inputStream, blockSize, fillValue);

                if (fillBlocks > 0)
                {
                    sparseFile.AddFillChunk(fillValue, fillBlocks * blockSize);
                    currentBlock += fillBlocks;

                    for (uint i = 0; i < fillBlocks; i++)
                    {
                        inputStream.ReadExactly(buffer, 0, (int)blockSize);
                    }
                    continue;
                }
            }

            var dataToAdd = new byte[bytesRead];
            Array.Copy(buffer, dataToAdd, bytesRead);
            sparseFile.AddRawChunk(dataToAdd);
            currentBlock++;
        }

        return sparseFile;
    }

    /// <summary>
    /// 检查是否为全零块
    /// </summary>
    private static bool IsZeroBlock(byte[] buffer, int length) => buffer.Take(length).All(b => b == 0);

    /// <summary>
    /// 检查是否为填充块（所有字节都相同）
    /// </summary>
    private static bool IsFillBlock(byte[] buffer, out uint fillValue)
    {
        fillValue = 0;

        if (buffer.Length < 4)
        {
            return false;
        }

        var pattern = BitConverter.ToUInt32(buffer, 0);

        for (var i = 4; i < buffer.Length; i += 4)
        {
            var remainingBytes = Math.Min(4, buffer.Length - i);
            var currentPattern = 0u;

            for (var j = 0; j < remainingBytes; j++)
            {
                currentPattern |= (uint)(buffer[i + j] << (j * 8));
            }

            if (remainingBytes == 4 && currentPattern != pattern)
            {
                return false;
            }
            else if (remainingBytes < 4)
            {
                var expectedPattern = pattern & ((1u << (remainingBytes * 8)) - 1);
                if (currentPattern != expectedPattern)
                {
                    return false;
                }
            }
        }

        fillValue = pattern;
        return true;
    }

    /// <summary>
    /// 计算连续的零块数量
    /// </summary>
    private static uint CountConsecutiveZeroBlocks(Stream stream, uint blockSize)
    {
        var originalPosition = stream.Position;
        var buffer = new byte[blockSize];
        uint zeroBlocks = 0;

        while (stream.Position < stream.Length)
        {
            var bytesRead = stream.Read(buffer, 0, (int)blockSize);
            if (bytesRead == 0 || !IsZeroBlock(buffer, bytesRead))
            {
                break;
            }
            zeroBlocks++;
        }
        stream.Position = originalPosition;
        return zeroBlocks;
    }

    /// <summary>
    /// 计算连续的相同填充块数量
    /// </summary>
    private static uint CountConsecutiveFillBlocks(Stream stream, uint blockSize, uint fillValue)
    {
        var originalPosition = stream.Position;
        var buffer = new byte[blockSize];
        uint fillBlocks = 0;

        while (stream.Position < stream.Length)
        {
            var bytesRead = stream.Read(buffer, 0, (int)blockSize);
            if (bytesRead != blockSize || !IsFillBlock(buffer, out var currentFillValue) || currentFillValue != fillValue)
            {
                break;
            }
            fillBlocks++;
        }
        stream.Position = originalPosition;
        return fillBlocks;
    }
}
