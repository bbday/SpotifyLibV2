using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [ResolvedSpClientEndpoint]
    public interface IPlaybackLicense
    {
        /// <summary>
        /// Gets a PlayReady license for this user
        /// </summary>
        /// <param name="sdkVersion">The sdk version to report to the server</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Get("/melody/v1/license_url?keysystem=com.microsoft.playready&mediatype=audio&sdk_name=harmony&sdk_version={sdkVersion}")]
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        Task<PlayReadyLicenseUriData> GetPlayReadyLicense([AliasAs("sdkVersion")] string sdkVersion = "4.4.0");
    }
}

