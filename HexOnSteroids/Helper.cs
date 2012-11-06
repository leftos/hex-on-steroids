using System;
using System.IO;
using System.Linq;

namespace HexOnSteroids
{
    public static class Helper
    {
        private static readonly byte[] negativeZeroArr = new byte[]{0,0,0,128};

        public static bool IsValidFilename(string filename)
        {
            return filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }

        public static string GetFolderName(string directory)
        {
            return directory.Split('\\').Last();
        }
    }
}