
namespace Patcher
{
    public class DiffWriter
    {
        BinaryWriter Writer;
        HashTable HashTable;

        public DiffWriter(string filePath, HashTable hashTable) 
        {
            var stream = File.OpenWrite(filePath);
            this.Writer = new BinaryWriter(stream);
            this.HashTable = hashTable;
        }

        public void WriteFileIndex(string filePath)
        {
            Int16 index = (Int16)HashTable.ToIndex(filePath);
            Writer.Write(index);
        }

        public void WriteHeader(string filePath)
        {
            Writer.Write((byte)BlockType.NewFile);
            WriteFileIndex(filePath);
        }

        public void Write(Byte[] bytes)
        {
            Writer.Write((byte)BlockType.RawBytes);
            Writer.Write(bytes.Length);
            Writer.Write(bytes);
        }

        public void Write(BlockMatch blockMatch)
        {
            Writer.Write((byte)BlockType.Indirect);
            WriteFileIndex(blockMatch.SrcFile);
            Writer.Write((int)blockMatch.SrcPosition);
            Writer.Write((int)blockMatch.Length);           
        }

        public void Close()
        {
            Writer.Close();
        }
    }
}
