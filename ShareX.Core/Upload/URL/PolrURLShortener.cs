﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.URL;

public class PolrURLShortenerService : URLShortenerService
{
    public override UrlShortenerType EnumValue => UrlShortenerType.Polr;

    public override bool CheckConfig(UploadersConfig config)
    {
        return !string.IsNullOrEmpty(config.PolrAPIHostname) && !string.IsNullOrEmpty(config.PolrAPIKey);
    }

    public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new PolrURLShortener
        {
            Host = config.PolrAPIHostname,
            Key = config.PolrAPIKey,
            IsSecret = config.PolrIsSecret,
            UseAPIv1 = config.PolrUseAPIv1
        };
    }
}

public sealed class PolrURLShortener : URLShortener
{
    public string Host { get; set; }
    public string Key { get; set; }
    public bool IsSecret { get; set; }
    public bool UseAPIv1 { get; set; }

    public override UploadResult ShortenURL(string url)
    {
        var result = new UploadResult { URL = url };

        Host = URLHelpers.FixPrefix(Host);

        var args = new Dictionary<string, string>
        {
            { "url", url }
        };

        if (!string.IsNullOrEmpty(Key))
        {
            if (UseAPIv1)
            {
                args.Add("apikey", Key);
                args.Add("action", "shorten");
            }
            else
            {
                args.Add("key", Key);
                if (IsSecret)
                {
                    args.Add("is_secret", "true");
                }
            }
        }

        var response = SendRequest(HttpMethod.Get, Host, args);

        if (!string.IsNullOrEmpty(response))
        {
            result.ShortenedURL = response;
        }

        return result;
    }
}

