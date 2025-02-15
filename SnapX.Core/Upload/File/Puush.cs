
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.Upload.File;

public class PuushFileUploaderService : FileUploaderService
{
    public override FileDestination EnumValue { get; } = FileDestination.Puush;

    public override bool CheckConfig(UploadersConfig config)
    {
        return !string.IsNullOrEmpty(config.PuushAPIKey);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new Puush(config.PuushAPIKey);
    }
}

public class Puush : FileUploader
{
    public const string PuushURL = "https://puush.me";
    public const string PuushRegisterURL = PuushURL + "/register";
    public const string PuushResetPasswordURL = PuushURL + "/reset_password";

    private const string PuushAPIURL = PuushURL + "/api";
    private const string PuushAPIAuthenticationURL = PuushAPIURL + "/auth";
    private const string PuushAPIUploadURL = PuushAPIURL + "/up";
    private const string PuushAPIDeletionURL = PuushAPIURL + "/del";
    private const string PuushAPIHistoryURL = PuushAPIURL + "/hist";
    private const string PuushAPIThumbnailURL = PuushAPIURL + "/thumb";

    public string APIKey { get; set; }

    public Puush()
    {
    }

    public Puush(string apiKey)
    {
        APIKey = apiKey;
    }

    public string Login(string email, string password)
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>
        {
            { "e", email },
            { "p", password },
            { "z", SnapXResources.UserAgent }
        };

        // Successful: status,apikey,expire,usage
        // Failed: status
        string response = SendRequestMultiPart(PuushAPIAuthenticationURL, arguments);

        if (!string.IsNullOrEmpty(response))
        {
            string[] values = response.Split(',');

            if (values.Length > 1 && int.TryParse(values[0], out int status) && status >= 0)
            {
                return values[1];
            }
        }

        return null;
    }

    public bool DeleteFile(string id)
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>
        {
            { "k", APIKey },
            { "i", id },
            { "z", SnapXResources.UserAgent }
        };

        // Successful: status\nlist of history items
        // Failed: status
        string response = SendRequestMultiPart(PuushAPIDeletionURL, arguments);

        if (!string.IsNullOrEmpty(response))
        {
            string[] lines = response.Lines();

            if (lines.Length > 0)
            {
                return int.TryParse(lines[0], out int status) && status >= 0;
            }
        }

        return false;
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        var args = new Dictionary<string, string>
        {
            { "k", APIKey },
            { "z", SnapXResources.UserAgent }
        };

        // Successful: status,url,id,usage
        // Failed: status
        var result = SendRequestFile(PuushAPIUploadURL, stream, fileName, "f", args);

        if (result.IsSuccess)
        {
            var values = result.Response.Split(',');

            if (values.Length > 0)
            {
                if (!int.TryParse(values[0], out int status))
                {
                    status = -2;
                }

                if (status < 0)
                {
                    switch (status)
                    {
                        case -1:
                            Errors.Add("Authentication failure.");
                            break;
                        default:
                        case -2:
                            Errors.Add("Connection error.");
                            break;
                        case -3:
                            Errors.Add("Checksum error.");
                            break;
                        case -4:
                            Errors.Add("Insufficient account storage remaining.");
                            break;
                    }
                }
                else if (values.Length > 1)
                {
                    result.URL = values[1];
                }
            }
        }

        return result;
    }
}
