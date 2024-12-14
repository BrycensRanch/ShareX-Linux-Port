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

using System.Collections.Specialized;
using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.Text;

public class OneTimeSecretTextUploaderService : TextUploaderService
{
    public override TextDestination EnumValue => TextDestination.OneTimeSecret;

    public override bool CheckConfig(UploadersConfig config) => true;

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new OneTimeSecret()
        {
            API_KEY = config.OneTimeSecretAPIKey,
            API_USERNAME = config.OneTimeSecretAPIUsername
        };
    }
}

public sealed class OneTimeSecret : TextUploader
{
    private const string API_ENDPOINT = "https://onetimesecret.com/api/v1/share";

    public string API_KEY { get; set; }
    public string API_USERNAME { get; set; }

    public override UploadResult UploadText(string text, string fileName)
    {
        var ur = new UploadResult();
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(fileName)) return ur;

        var args = new Dictionary<string, string>() { { "text", text } };

        NameValueCollection headers = null;

        if (!string.IsNullOrEmpty(API_USERNAME) && !string.IsNullOrEmpty(API_KEY))
        {
            headers = RequestHelpers.CreateAuthenticationHeader(API_USERNAME, API_KEY);
        }

        ur.Response = SendRequestMultiPart(API_ENDPOINT, args, headers);
        if (string.IsNullOrEmpty(ur.Response)) return ur;

        var jsonResponse = JsonSerializer.Deserialize<OneTimeSecretResponse>(ur.Response);

        if (jsonResponse != null)
        {
            ur.URL = URLHelpers.CombineURL("https://onetimesecret.com/secret/", jsonResponse.secret_key);
        }

        return ur;
    }

    public class OneTimeSecretResponse
    {
        public string custid { get; set; }
        public string metadata_key { get; set; }
        public string secret_key { get; set; }
        public string ttl { get; set; }
        public string updated { get; set; }
        public string created { get; set; }
    }
}

