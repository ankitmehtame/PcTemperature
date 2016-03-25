using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CpuTemperature.Helpers
{
    public static class FileReader
    {
        public static async Task<string[]> ReadAllLinesAsync(string filePath)
        {
            await Task.Yield();
            var lines = new List<string>();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096, true))
            {
                using (TextReader tr = new StreamReader(fs))
                {
                    do
                    {
                        var line = await tr.ReadLineAsync().ConfigureAwait(false);
                        if (line == null)
                        {
                            break;
                        }
                        lines.Add(line);
                    }
                    while (true);
                }
            }
            return lines.ToArray();
        }
    }
}
