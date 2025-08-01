using Godot;
using Netisu;
using System;
using System.Threading.Tasks;

namespace Netisu.Client.CacheBuilders
{
    public partial class Builder : Node
    {
        public static Builder Instance { get; private set; } = null!;
        
        public const string AssetsBaseCdn = "https://cdn.netisu.com/uploads/";

        public override void _Ready()
        {
            Instance = this;
        }

        public async Task<bool> DownloadAndCache(string hash, string extension = "obj")
        {
            bool wasSuccess = false;

            if (Utilities.CacheExists(hash, extension))
                return true;

            HttpRequest httpRequest = new()
            {
                UseThreads = true,
                DownloadFile = $"{AssetsBaseCdn}{hash}.{extension}",
            };

            AddChild(httpRequest);

            httpRequest.RequestCompleted += (result, statusCode, responseHeaders, responseBody) =>
            {
                if (statusCode == 200)
                    wasSuccess = true;
                else
                    return;

                if (extension == "png")
                {
                    var correspondingImage = new Image();
                    Error error = correspondingImage.LoadPngFromBuffer(responseBody);
                    if (error != Error.Ok)
                    {
                        GD.PrintErr("Tried caching an unknown type as a png?");
                        return;
                    }


                    error = correspondingImage.SavePng($"user://Cache/{hash}.png");
                    if (error != Error.Ok)
                    {
                        GD.PrintErr("An error occured while saving a downloaded png.");
                        return;
                    }

                    return;
                }

                // all other extensions are saved the same way!
                FileAccess fileAccess = FileAccess.Open($"user://Cache/{hash}.{extension}", FileAccess.ModeFlags.WriteRead);
                fileAccess.StoreString(responseBody.GetStringFromUtf8());
            };

            await ToSignal(httpRequest, "requested_completed");
            return wasSuccess;
        }
    }
}