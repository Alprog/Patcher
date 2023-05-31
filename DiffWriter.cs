
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

        public void Write(Byte[] bytes)
        {
            Writer.Write(bytes.Length);
            Writer.Write(bytes);
        }

        public void Write(BlockMatch blockMatch)
        {
            Writer.Write(-blockMatch.Length);
            Writer.Write(blockMatch.SrcPosition);
            WriteFileIndex(blockMatch.SrcFile);
        }
    }
}
