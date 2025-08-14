namespace SpraseImg;

/// <summary>
/// Sparse 镜像验证器
/// </summary>
public static class SparseImageValidator
{
    /// <summary>
    /// 验证sparse镜像文件
    /// </summary>
    public static ValidationResult ValidateSparseImage(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var sparseFile = SparseFile.FromStream(stream);

            var result = new ValidationResult
            {
                Success = true,
                FilePath = filePath,
                Header = new HeaderInfo
                {
                    Magic = sparseFile.Header.Magic,
                    Version = $"{sparseFile.Header.MajorVersion}.{sparseFile.Header.MinorVersion}",
                    BlockSize = sparseFile.Header.BlockSize,
                    TotalBlocks = sparseFile.Header.TotalBlocks,
                    TotalChunks = sparseFile.Header.TotalChunks
                }
            };
            if (!sparseFile.Header.IsValid())
            {
                result.Success = false;
                result.ErrorMessage = "无效的文件头";
                return result;
            }
            uint totalBlocks = 0;
            var chunkInfos = new List<ChunkInfo>();

            for (uint i = 0; i < sparseFile.Header.TotalChunks; i++)
            {
                var chunk = sparseFile.Chunks[(int)i];

                if (!chunk.Header.IsValid())
                {
                    result.Success = false;
                    result.ErrorMessage = $"第 {i} 个 chunk 头无效";
                    return result;
                }

                var chunkInfo = new ChunkInfo
                {
                    Index = i,
                    ChunkType = chunk.Header.ChunkType,
                    ChunkSize = chunk.Header.ChunkSize,
                    TotalSize = chunk.Header.TotalSize
                };
                chunkInfos.Add(chunkInfo);

                totalBlocks += chunk.Header.ChunkSize;
            }

            result.Chunks = chunkInfos;
            if (totalBlocks > sparseFile.Header.TotalBlocks)
            {
                result.Success = false;
                result.ErrorMessage = $"chunk 总块数 ({totalBlocks}) 超过了文件头中的总块数 ({sparseFile.Header.TotalBlocks})";
                return result;
            }

            result.CalculatedTotalBlocks = totalBlocks;
            return result;
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public HeaderInfo? Header { get; set; }
        public List<ChunkInfo>? Chunks { get; set; }
        public uint CalculatedTotalBlocks { get; set; }
    }

    /// <summary>
    /// 文件头信息
    /// </summary>
    public class HeaderInfo
    {
        public uint Magic { get; set; }
        public string Version { get; set; } = "";
        public uint BlockSize { get; set; }
        public uint TotalBlocks { get; set; }
        public uint TotalChunks { get; set; }
    }

    /// <summary>
    /// Chunk信息
    /// </summary>
    public class ChunkInfo
    {
        public uint Index { get; set; }
        public ushort ChunkType { get; set; }
        public uint ChunkSize { get; set; }
        public uint TotalSize { get; set; }
    }

    /// <summary>
    /// 检查文件是否为sparse镜像
    /// </summary>
    public static bool IsSparseImage(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var magicBytes = new byte[4];
            if (stream.Read(magicBytes, 0, 4) != 4)
            {
                return false;
            }

            var magic = BitConverter.ToUInt32(magicBytes, 0);
            return magic == SparseFormat.SPARSE_HEADER_MAGIC;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取sparse镜像文件的详细信息
    /// </summary>
    public static SparseImageInfo GetSparseImageInfo(string filePath)
    {
        try
        {
            if (!IsSparseImage(filePath))
            {
                return new SparseImageInfo
                {
                    Success = false,
                    ErrorMessage = "不是有效的 sparse 镜像文件"
                };
            }

            var header = SparseFile.PeekHeader(filePath);
            var fileInfo = new FileInfo(filePath);
            var uncompressedSize = (long)header.TotalBlocks * header.BlockSize;
            var compressionRatio = 100.0 - ((double)fileInfo.Length / uncompressedSize * 100.0);

            return new SparseImageInfo
            {
                Success = true,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                UncompressedSize = uncompressedSize,
                CompressionRatio = compressionRatio,
                Version = $"{header.MajorVersion}.{header.MinorVersion}",
                BlockSize = header.BlockSize,
                TotalBlocks = header.TotalBlocks,
                TotalChunks = header.TotalChunks
            };
        }
        catch (Exception ex)
        {
            return new SparseImageInfo
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Sparse镜像详细信息
    /// </summary>
    public class SparseImageInfo
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public long UncompressedSize { get; set; }
        public double CompressionRatio { get; set; }
        public string Version { get; set; } = "";
        public uint BlockSize { get; set; }
        public uint TotalBlocks { get; set; }
        public uint TotalChunks { get; set; }
    }

    /// <summary>
    /// 打印sparse镜像信息（为兼容性保留，但不推荐在类库中使用）
    /// </summary>
    [Obsolete("此方法包含控制台输出，不推荐在类库中使用。请使用GetSparseImageInfo方法。")]
    public static void PrintSparseImageInfo(string filePath)
    {
        var info = GetSparseImageInfo(filePath);
        if (info.Success)
        {
            Console.WriteLine($"{filePath}: Android sparse image, " +
                              $"version {info.Version}, " +
                              $"blocks {info.TotalBlocks}, " +
                              $"chunks {info.TotalChunks}");
            Console.WriteLine($"文件大小: {info.FileSize:N0} 字节");
            Console.WriteLine($"解压后大小: {info.UncompressedSize:N0} 字节");
            Console.WriteLine($"压缩率: {info.CompressionRatio:F1}%");
        }
        else
        {
            Console.WriteLine($"{filePath}: 不是有效的 sparse 镜像文件");
        }
    }
}
