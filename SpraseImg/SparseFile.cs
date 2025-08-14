namespace SpraseImg;

/// <summary>
/// Sparse块数据结构
/// </summary>
public class SparseChunk(ChunkHeader header)
{
    public ChunkHeader Header { get; set; } = header;
    public byte[]? Data { get; set; }
    public uint FillValue { get; set; }
}

/// <summary>
/// Sparse文件结构
/// </summary>
public class SparseFile
{
    /// <summary>
    /// 只读取sparse文件头部信息（不解析 chunk）
    /// </summary>
    public static SparseHeader PeekHeader(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var headerData = new byte[SparseFormat.SPARSE_HEADER_SIZE];
        return stream.Read(headerData, 0, headerData.Length) != headerData.Length
            ? throw new InvalidDataException("无法读取 sparse 文件头")
            : SparseHeader.FromBytes(headerData);
    }

    public SparseHeader Header { get; set; }
    public List<SparseChunk> Chunks { get; set; } = [];

    public SparseFile() { }

    public SparseFile(uint blockSize, long totalSize)
    {
        var totalBlocks = (uint)((totalSize + blockSize - 1) / blockSize);
        Header = new SparseHeader
        {
            Magic = SparseFormat.SPARSE_HEADER_MAGIC,
            MajorVersion = 1,
            MinorVersion = 0,
            FileHeaderSize = SparseFormat.SPARSE_HEADER_SIZE,
            ChunkHeaderSize = SparseFormat.CHUNK_HEADER_SIZE,
            BlockSize = blockSize,
            TotalBlocks = totalBlocks,
            TotalChunks = 0,
            ImageChecksum = 0
        };
    }

    /// <summary>
    /// 从文件流读取sparse文件
    /// </summary>
    public static SparseFile FromStream(Stream stream)
    {
        var sparseFile = new SparseFile();
        var headerData = new byte[SparseFormat.SPARSE_HEADER_SIZE];
        if (stream.Read(headerData, 0, headerData.Length) != headerData.Length)
        {
            throw new InvalidDataException("无法读取 sparse 文件头");
        }

        sparseFile.Header = SparseHeader.FromBytes(headerData);

        if (!sparseFile.Header.IsValid())
        {
            throw new InvalidDataException("无效的 sparse 文件头");
        }
        for (uint i = 0; i < sparseFile.Header.TotalChunks; i++)
        {
            var chunkHeaderData = new byte[SparseFormat.CHUNK_HEADER_SIZE];
            if (stream.Read(chunkHeaderData, 0, chunkHeaderData.Length) != chunkHeaderData.Length)
            {
                throw new InvalidDataException($"无法读取第 {i} 个 chunk 头");
            }

            var chunkHeader = ChunkHeader.FromBytes(chunkHeaderData);
            var chunk = new SparseChunk(chunkHeader);

            if (!chunkHeader.IsValid())
            {
                throw new InvalidDataException($"第 {i} 个 chunk 头无效");
            }

            var dataSize = (int)(chunkHeader.TotalSize - SparseFormat.CHUNK_HEADER_SIZE);

            switch (chunkHeader.ChunkType)
            {
                case SparseFormat.CHUNK_TYPE_RAW:
                    chunk.Data = new byte[dataSize];
                    if (stream.Read(chunk.Data, 0, dataSize) != dataSize)
                    {
                        throw new InvalidDataException($"无法读取第 {i} 个 chunk 的原始数据");
                    }

                    break;

                case SparseFormat.CHUNK_TYPE_FILL:
                    if (dataSize >= 4)
                    {
                        var fillData = new byte[4];
                        if (stream.Read(fillData, 0, 4) != 4)
                        {
                            throw new InvalidDataException($"无法读取第 {i} 个 chunk 的填充值");
                        }

                        chunk.FillValue = BitConverter.ToUInt32(fillData, 0);

                        if (dataSize > 4)
                        {
                            stream.Seek(dataSize - 4, SeekOrigin.Current);
                        }
                    }
                    break;

                case SparseFormat.CHUNK_TYPE_DONT_CARE:
                    if (dataSize > 0)
                    {
                        stream.Seek(dataSize, SeekOrigin.Current);
                    }
                    break;

                case SparseFormat.CHUNK_TYPE_CRC32:
                    //应要求不验证CRC32，贼说会有解析报错问题。推测是某些镜像工具生成的sprase文件中块存在异常CRC32值
                    if (dataSize > 0)
                    {
                        stream.Seek(dataSize, SeekOrigin.Current);
                    }
                    break;

                default:
                    throw new InvalidDataException($"第 {i} 个 chunk 类型未知: 0x{chunkHeader.ChunkType:X4}");
            }

            sparseFile.Chunks.Add(chunk);
        }

        return sparseFile;
    }

    /// <summary>
    /// 将sparse文件写入流
    /// </summary>
    public void WriteToStream(Stream stream)
    {
        var header = Header;
        header.TotalChunks = (uint)Chunks.Count;
        Header = header;
        var headerData = Header.ToBytes();
        stream.Write(headerData, 0, headerData.Length);
        foreach (var chunk in Chunks)
        {
            var chunkHeaderData = chunk.Header.ToBytes();
            stream.Write(chunkHeaderData, 0, chunkHeaderData.Length);

            switch (chunk.Header.ChunkType)
            {
                case SparseFormat.CHUNK_TYPE_RAW:
                    if (chunk.Data != null)
                    {
                        stream.Write(chunk.Data, 0, chunk.Data.Length);
                    }

                    break;

                case SparseFormat.CHUNK_TYPE_FILL:
                    var fillData = BitConverter.GetBytes(chunk.FillValue);
                    stream.Write(fillData, 0, fillData.Length);
                    break;

                case SparseFormat.CHUNK_TYPE_DONT_CARE:
                    break;

                case SparseFormat.CHUNK_TYPE_CRC32:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 添加原始数据块
    /// </summary>
    public void AddRawChunk(byte[] data)
    {
        var dataSize = (uint)data.Length;
        var blockSize = Header.BlockSize;
        var chunkBlocks = (dataSize + blockSize - 1) / blockSize;

        var chunkHeader = new ChunkHeader
        {
            ChunkType = SparseFormat.CHUNK_TYPE_RAW,
            Reserved = 0,
            ChunkSize = chunkBlocks,
            TotalSize = SparseFormat.CHUNK_HEADER_SIZE + dataSize
        };

        var chunk = new SparseChunk(chunkHeader)
        {
            Data = data
        };

        Chunks.Add(chunk);
    }

    /// <summary>
    /// 添加填充块
    /// </summary>
    public void AddFillChunk(uint fillValue, uint size)
    {
        var blockSize = Header.BlockSize;
        var chunkBlocks = (size + blockSize - 1) / blockSize;

        var chunkHeader = new ChunkHeader
        {
            ChunkType = SparseFormat.CHUNK_TYPE_FILL,
            Reserved = 0,
            ChunkSize = chunkBlocks,
            TotalSize = SparseFormat.CHUNK_HEADER_SIZE + 4
        };

        var chunk = new SparseChunk(chunkHeader)
        {
            FillValue = fillValue
        };

        Chunks.Add(chunk);
    }

    /// <summary>
    /// 添加空数据块
    /// </summary>
    public void AddDontCareChunk(uint size)
    {
        var blockSize = Header.BlockSize;
        var chunkBlocks = (size + blockSize - 1) / blockSize;

        var chunkHeader = new ChunkHeader
        {
            ChunkType = SparseFormat.CHUNK_TYPE_DONT_CARE,
            Reserved = 0,
            ChunkSize = chunkBlocks,
            TotalSize = SparseFormat.CHUNK_HEADER_SIZE
        };

        var chunk = new SparseChunk(chunkHeader);
        Chunks.Add(chunk);
    }
}
