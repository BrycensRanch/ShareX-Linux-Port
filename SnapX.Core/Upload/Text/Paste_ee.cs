
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Collections.Specialized;
using System.Text.Json;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.Text;

public class Paste_eeTextUploaderService : TextUploaderService
{
    public override TextDestination EnumValue => TextDestination.Paste_ee;
    public override bool CheckConfig(UploadersConfig config) => true;

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        string apiKey;

        if (!string.IsNullOrEmpty(config.Paste_eeUserKey))
        {
            apiKey = config.Paste_eeUserKey;
        }
        else
        {
            apiKey = APIKeys.Paste_eeApplicationKey;
        }

        return new Paste_ee(apiKey)
        {
            EncryptPaste = config.Paste_eeEncryptPaste
        };
    }
}

public sealed class Paste_ee : TextUploader
{
    public string APIKey { get; private set; }
    public bool EncryptPaste { get; set; }

    public Paste_ee(string apiKey)
    {
        APIKey = apiKey;
    }

    public override UploadResult UploadText(string text, string fileName)
    {
        if (string.IsNullOrEmpty(APIKey))
        {
            throw new Exception("API key is missing.");
        }

        var ur = new UploadResult();

        if (string.IsNullOrEmpty(text)) return ur;

        var requestBody = new Paste_eeSubmitRequestBody
        {
            encrypted = EncryptPaste,
            description = string.Empty,
            expiration = "never",
            sections = new[]
            {
                new Paste_eeSubmitRequestBodySection
                {
                    name = string.Empty,
                    syntax = "autodetect",
                    contents = text
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        var headers = new NameValueCollection() { { "X-Auth-Token", APIKey } };

        ur.Response = SendRequest(HttpMethod.Post, "https://api.paste.ee/v1/pastes", json, RequestHelpers.ContentTypeJSON, null, headers);

        if (string.IsNullOrEmpty(ur.Response)) return ur;

        var response = JsonSerializer.Deserialize<Paste_eeSubmitResponse>(ur.Response);

        ur.URL = response?.link;

        return ur;
    }

}

public class Paste_eeSubmitRequestBody
{
    public bool encrypted { get; set; }
    public string description { get; set; }
    public string expiration { get; set; }
    public Paste_eeSubmitRequestBodySection[] sections { get; set; }
}

public class Paste_eeSubmitRequestBodySection
{
    public string name { get; set; }
    public string syntax { get; set; }
    public string contents { get; set; }
}

public class Paste_eeSubmitResponse
{
    public string id { get; set; }
    public string link { get; set; }
}
