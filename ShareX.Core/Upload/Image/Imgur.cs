
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Collections.Specialized;
using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.OAuth;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.Image;

public enum ImgurThumbnailType // Localized
{
    Small_Square,
    Big_Square,
    Small_Thumbnail,
    Medium_Thumbnail,
    Large_Thumbnail,
    Huge_Thumbnail
}

public class ImgurImageUploaderService : ImageUploaderService
{
    public override ImageDestination EnumValue => ImageDestination.Imgur;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.ImgurAccountType == AccountType.Anonymous || OAuth2Info.CheckOAuth(config.ImgurOAuth2Info);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        if (config.ImgurOAuth2Info == null)
        {
            config.ImgurOAuth2Info = new OAuth2Info(APIKeys.ImgurClientID, APIKeys.ImgurClientSecret);
        }

        string albumID = null;

        if (config.ImgurUploadSelectedAlbum && config.ImgurSelectedAlbum != null)
        {
            albumID = config.ImgurSelectedAlbum.id;
        }

        return new Imgur(config.ImgurOAuth2Info)
        {
            UploadMethod = config.ImgurAccountType,
            DirectLink = config.ImgurDirectLink,
            ThumbnailType = config.ImgurThumbnailType,
            UseGIFV = config.ImgurUseGIFV,
            UploadAlbumID = albumID
        };
    }
}

public sealed class Imgur : ImageUploader, IOAuth2
{
    public AccountType UploadMethod { get; set; }
    public OAuth2Info AuthInfo { get; set; }
    public ImgurThumbnailType ThumbnailType { get; set; }
    public string UploadAlbumID { get; set; }
    public bool DirectLink { get; set; }
    public bool UseGIFV { get; set; }

    public Imgur(OAuth2Info oauth)
    {
        AuthInfo = oauth;
    }

    public string GetAuthorizationURL()
    {
        var args = new Dictionary<string, string>
        {
            { "client_id", AuthInfo.Client_ID },
            { "response_type", "pin" }
        };

        return URLHelpers.CreateQueryString("https://api.imgur.com/oauth2/authorize", args);
    }

    public bool GetAccessToken(string pin)
    {
        var args = new Dictionary<string, string>
        {
            { "client_id", AuthInfo.Client_ID },
            { "client_secret", AuthInfo.Client_Secret },
            { "grant_type", "pin" },
            { "pin", pin }
        };

        var response = SendRequestMultiPart("https://api.imgur.com/oauth2/token", args);

        if (string.IsNullOrEmpty(response)) return false;

        var token = JsonSerializer.Deserialize<OAuth2Token>(response);
        if (token?.access_token == null) return false;

        token.UpdateExpireDate();
        AuthInfo.Token = token;
        return true;
    }


    public bool RefreshAccessToken()
    {
        if (!OAuth2Info.CheckOAuth(AuthInfo) || string.IsNullOrEmpty(AuthInfo.Token.refresh_token))
            return false;

        var args = new Dictionary<string, string>
        {
            { "refresh_token", AuthInfo.Token.refresh_token },
            { "client_id", AuthInfo.Client_ID },
            { "client_secret", AuthInfo.Client_Secret },
            { "grant_type", "refresh_token" }
        };

        string response = SendRequestMultiPart("https://api.imgur.com/oauth2/token", args);

        if (string.IsNullOrEmpty(response)) return false;

        var token = JsonSerializer.Deserialize<OAuth2Token>(response);
        if (token?.access_token == null) return false;

        token.UpdateExpireDate();
        AuthInfo.Token = token;
        return true;
    }


    private NameValueCollection GetAuthHeaders()
    {
        var headers = new NameValueCollection
        {
            { "Authorization", $"Bearer {AuthInfo.Token.access_token}" }
        };

        return headers;
    }

    public bool CheckAuthorization()
    {
        if (!OAuth2Info.CheckOAuth(AuthInfo))
        {
            Errors.Add("Imgur login is required.");
            return false;
        }

        if (AuthInfo.Token.IsExpired && !RefreshAccessToken())
        {
            Errors.Add("Refresh access token failed.");
            return false;
        }

        return true;
    }

    public List<ImgurAlbumData> GetAlbums(int maxPage = 10, int perPage = 100)
    {
        var albums = new List<ImgurAlbumData>();

        for (var i = 0; i < maxPage; i++)
        {
            var tempAlbums = GetAlbumsPage(i, perPage);

            if (tempAlbums?.Count == 0)
                break;

            albums.AddRange(tempAlbums);

            if (tempAlbums.Count < perPage)
                break;
        }

        return albums;
    }


    private List<ImgurAlbumData> GetAlbumsPage(int page, int perPage)
    {
        if (!CheckAuthorization())
            return null;

        var args = new Dictionary<string, string>
        {
            { "page", page.ToString() },
            { "perPage", perPage.ToString() }
        };

        var response = SendRequest(HttpMethod.Get, "https://api.imgur.com/3/account/me/albums", args, GetAuthHeaders());

        var imgurResponse = JsonSerializer.Deserialize<ImgurResponse>(response);

        if (imgurResponse?.success == true && imgurResponse.status == 200)
        {
            return JsonSerializer.Deserialize<List<ImgurAlbumData>>(imgurResponse.data.ToString());
        }

        HandleErrors(imgurResponse);
        return null;
    }


    public List<ImgurImageData> GetAlbumImages(string albumID)
    {
        if (!CheckAuthorization())
            return null;

        var response = SendRequest(HttpMethod.Get, $"https://api.imgur.com/3/album/{albumID}/images", headers: GetAuthHeaders());

        var imgurResponse = JsonSerializer.Deserialize<ImgurResponse>(response);

        if (imgurResponse?.success == true && imgurResponse.status == 200)
        {
            return JsonSerializer.Deserialize<List<ImgurImageData>>(imgurResponse.data.ToString());
        }

        HandleErrors(imgurResponse);
        return null;
    }


    public override UploadResult Upload(Stream stream, string fileName)
    {
        return InternalUpload(stream, fileName, true);
    }

    private UploadResult InternalUpload(Stream stream, string fileName, bool refreshTokenOnError)
    {
        Dictionary<string, string> args = new Dictionary<string, string>();
        NameValueCollection headers;

        if (UploadMethod == AccountType.User)
        {
            if (!CheckAuthorization()) return null;

            if (!string.IsNullOrEmpty(UploadAlbumID))
                args.Add("album", UploadAlbumID);

            headers = GetAuthHeaders();
        }
        else
        {
            headers = new NameValueCollection { { "Authorization", "Client-ID " + AuthInfo.Client_ID } };
        }

        ReturnResponseOnError = true;

        string fileFormName = FileHelpers.IsVideoFile(fileName) ? "video" : "image";

        UploadResult result = SendRequestFile("https://api.imgur.com/3/upload", stream, fileName, fileFormName, args, headers);

        if (string.IsNullOrEmpty(result.Response)) return result;

        var imgurResponse = JsonSerializer.Deserialize<ImgurResponse>(result.Response);

        if (imgurResponse?.success != true || imgurResponse.status != 200)
            return HandleUploadError(imgurResponse, stream, fileName, refreshTokenOnError);

        var imageData = JsonDocument.Parse(imgurResponse.data.ToString()).Deserialize<ImgurImageData>();
        if (imageData == null || string.IsNullOrEmpty(imageData.link)) return result;

        result.URL = DirectLink
            ? GetDirectLink(imageData)
            : $"https://imgur.com/{imageData.id}";

        result.ThumbnailURL = GetThumbnailURL(imageData);
        result.DeletionURL = $"https://imgur.com/delete/{imageData.deletehash}";

        return result;
    }

    private string GetDirectLink(ImgurImageData imageData)
    {
        if (UseGIFV && !string.IsNullOrEmpty(imageData.gifv))
            return imageData.gifv;

        return imageData.link.TrimEnd('.');
    }

    private string GetThumbnailURL(ImgurImageData imageData)
    {
        string thumbnail = ThumbnailType switch
        {
            ImgurThumbnailType.Small_Square => "s",
            ImgurThumbnailType.Big_Square => "b",
            ImgurThumbnailType.Small_Thumbnail => "t",
            ImgurThumbnailType.Medium_Thumbnail => "m",
            ImgurThumbnailType.Large_Thumbnail => "l",
            ImgurThumbnailType.Huge_Thumbnail => "h",
            _ => throw new ArgumentOutOfRangeException(nameof(imageData))
        };

        return $"https://i.imgur.com/{imageData.id}{thumbnail}.jpg";
    }

    private UploadResult HandleUploadError(ImgurResponse imgurResponse, Stream stream, string fileName, bool refreshTokenOnError)
    {
        var errorData = ParseError(imgurResponse);

        if (errorData != null && UploadMethod == AccountType.User && refreshTokenOnError &&
            ((string)errorData.error).Equals("The access token provided is invalid.", StringComparison.OrdinalIgnoreCase) &&
            RefreshAccessToken())
        {
            DebugHelper.WriteLine("Imgur access token refreshed, reuploading image.");
            return InternalUpload(stream, fileName, false);
        }

        Errors.AddFirst($"Imgur upload failed: ({imgurResponse.status}) {errorData?.error}");
        return new UploadResult();
    }


    private void HandleErrors(ImgurResponse response)
    {
        ImgurErrorData errorData = ParseError(response);

        if (errorData != null)
        {
            Errors.Add($"Status: {response.status}, Request: {errorData.request}, Error: {errorData.error}");
        }
    }

    private ImgurErrorData ParseError(ImgurResponse response)
    {
        ImgurErrorData errorData = JsonSerializer.Deserialize<ImgurErrorData>(response.data.ToString());

        if (errorData != null && !(errorData.error is string))
        {
            errorData.error = JsonSerializer.Deserialize<ImgurErrorData>(errorData.error.ToString());
        }

        return errorData;
    }
}

internal class ImgurResponse
{
    public object data { get; set; }
    public bool success { get; set; }
    public int status { get; set; }
}

internal class ImgurErrorData
{
    public object error { get; set; }
    public string request { get; set; }
    public string method { get; set; }
}

internal class ImgurError
{
    public int code { get; set; }
    public string message { get; set; }
    public string type { get; set; }
    //public string[] exception { get; set; }
}

public class ImgurImageData
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public int datetime { get; set; }
    public string type { get; set; }
    public bool animated { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int size { get; set; }
    public int views { get; set; }
    public long bandwidth { get; set; }
    public string deletehash { get; set; }
    public string name { get; set; }
    public string section { get; set; }
    public string link { get; set; }
    public string gifv { get; set; }
    public string mp4 { get; set; }
    public string webm { get; set; }
    public bool looping { get; set; }
    public bool favorite { get; set; }
    public bool? nsfw { get; set; }
    public string vote { get; set; }
    public string comment_preview { get; set; }
}

public class ImgurAlbumData
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public int datetime { get; set; }
    public string cover { get; set; }
    public string cover_width { get; set; }
    public string cover_height { get; set; }
    public string account_url { get; set; }
    public long? account_id { get; set; }
    public string privacy { get; set; }
    public string layout { get; set; }
    public int views { get; set; }
    public string link { get; set; }
    public bool favorite { get; set; }
    public bool? nsfw { get; set; }
    public string section { get; set; }
    public int order { get; set; }
    public string deletehash { get; set; }
    public int images_count { get; set; }
    public ImgurImageData[] images { get; set; }
}

