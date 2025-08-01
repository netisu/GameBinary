using Godot;

namespace Netisu.Client.CacheBuilders
{
    public sealed class Utilities
    {
        public static bool CacheExists(string hash, string extension)
        {
            DirAccess dirAccess = DirAccess.Open("user://");
            if (dirAccess == null)
                return false;

            bool cacheDir = dirAccess.DirExists("Cache");
            if (!cacheDir)
                return false;


            return FileAccess.FileExists($"user://Cache/{hash}.{extension}");
        }
    }
}