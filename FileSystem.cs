using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher
{
    public class FileSystem
    {
        public List<string> List(string folder)
        {
            int startLength = folder.Length;
            var list = new List<string>();
            foreach (var filePath in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                list.Add(filePath.Substring(startLength + 1));
            }
            return list;
        }
    }
}
