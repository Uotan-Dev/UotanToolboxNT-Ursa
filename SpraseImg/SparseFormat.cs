namespace SpraseImg;

/// <summary>
/// Android sparse 镜像格式定义
/// </summary>
public static class SparseFormat
{
    public const uint SPARSE_HEADER_MAGIC = 0xed26ff3a;

    public const ushort CHUNK_TYPE_RAW = 0xCAC1;
    public const ushort CHUNK_TYPE_FILL = 0xCAC2;
    public const ushort CHUNK_TYPE_DONT_CARE = 0xCAC3;
    public const ushort CHUNK_TYPE_CRC32 = 0xCAC4;

    public const ushort SPARSE_HEADER_SIZE = 28;
    public const ushort CHUNK_HEADER_SIZE = 12;
}

/// <summary>
/// Sparse 文件头结构
/// </summary>
public struct SparseHeader
{
    //引用自libsprase的一些结构定义
    public uint Magic;           // 0xed26ff3a
    public ushort MajorVersion;  // (0x1) - reject images with higher major versions
    public ushort MinorVersion;  // (0x0) - allow images with higher minor versions
    public ushort FileHeaderSize; // 28 bytes for first revision of the file format
    public ushort ChunkHeaderSize; // 12 bytes for first revision of the file format
    public uint BlockSize;       // block size in bytes, must be a multiple of 4 (4096)
    public uint TotalBlocks;     // total blocks in the non-sparse output image
    public uint TotalChunks;     // total chunks in the sparse input image
    public uint ImageChecksum;   // CRC32 checksum of the original data

    public static SparseHeader FromBytes(byte[] data)
    {
        return data.Length < SparseFormat.SPARSE_HEADER_SIZE
            ? throw new ArgumentException("数据长度不足以构建 SparseHeader")
            : new SparseHeader
            {
                Magic = BitConverter.ToUInt32(data, 0),
                MajorVersion = BitConverter.ToUInt16(data, 4),
                MinorVersion = BitConverter.ToUInt16(data, 6),
                FileHeaderSize = BitConverter.ToUInt16(data, 8),
                ChunkHeaderSize = BitConverter.ToUInt16(data, 10),
                BlockSize = BitConverter.ToUInt32(data, 12),
                TotalBlocks = BitConverter.ToUInt32(data, 16),
                TotalChunks = BitConverter.ToUInt32(data, 20),
                ImageChecksum = BitConverter.ToUInt32(data, 24)
            };
    }

    public readonly byte[] ToBytes()
    {
        var data = new byte[SparseFormat.SPARSE_HEADER_SIZE];

        BitConverter.GetBytes(Magic).CopyTo(data, 0);
        BitConverter.GetBytes(MajorVersion).CopyTo(data, 4);
        BitConverter.GetBytes(MinorVersion).CopyTo(data, 6);
        BitConverter.GetBytes(FileHeaderSize).CopyTo(data, 8);
        BitConverter.GetBytes(ChunkHeaderSize).CopyTo(data, 10);
        BitConverter.GetBytes(BlockSize).CopyTo(data, 12);
        BitConverter.GetBytes(TotalBlocks).CopyTo(data, 16);
        BitConverter.GetBytes(TotalChunks).CopyTo(data, 20);
        BitConverter.GetBytes(ImageChecksum).CopyTo(data, 24);

        return data;
    }

    public readonly bool IsValid()
    {
        return Magic == SparseFormat.SPARSE_HEADER_MAGIC &&
               MajorVersion == 1 &&
               FileHeaderSize == SparseFormat.SPARSE_HEADER_SIZE &&
               ChunkHeaderSize == SparseFormat.CHUNK_HEADER_SIZE &&
               BlockSize > 0 && BlockSize % 4 == 0;
    }
}

/// <summary>
/// Chunk头结构
/// </summary>
public struct ChunkHeader
{
    public ushort ChunkType;     // 0xCAC1 -> raw; 0xCAC2 -> fill; 0xCAC3 -> don't care
    public ushort Reserved;      // Reserved field
    public uint ChunkSize;       // in blocks in output image
    public uint TotalSize;       // in bytes of chunk input file including chunk header and data

    public static ChunkHeader FromBytes(byte[] data)
    {
        return data.Length < SparseFormat.CHUNK_HEADER_SIZE
            ? throw new ArgumentException("数据长度不足以构建 ChunkHeader")
            : new ChunkHeader
            {
                ChunkType = BitConverter.ToUInt16(data, 0), // 需要为uint16，若为int，则会解析出负数
                Reserved = BitConverter.ToUInt16(data, 2),
                ChunkSize = BitConverter.ToUInt32(data, 4),
                TotalSize = BitConverter.ToUInt32(data, 8)
            };
    }

    public readonly byte[] ToBytes()
    {
        var data = new byte[SparseFormat.CHUNK_HEADER_SIZE];

        BitConverter.GetBytes(ChunkType).CopyTo(data, 0);
        BitConverter.GetBytes(Reserved).CopyTo(data, 2);
        BitConverter.GetBytes(ChunkSize).CopyTo(data, 4);
        BitConverter.GetBytes(TotalSize).CopyTo(data, 8);

        return data;
    }

    public readonly bool IsValid()
    {
        return ChunkType is SparseFormat.CHUNK_TYPE_RAW or
               SparseFormat.CHUNK_TYPE_FILL or
               SparseFormat.CHUNK_TYPE_DONT_CARE or
               SparseFormat.CHUNK_TYPE_CRC32;
    }
}
