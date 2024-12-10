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

using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.File
{
    public class LobFileFileUploaderService : FileUploaderService
    {
        public override FileDestination EnumValue { get; } = FileDestination.Lithiio;


        public override bool CheckConfig(UploadersConfig config)
        {
            return config.LithiioSettings != null && !string.IsNullOrEmpty(config.LithiioSettings.UserAPIKey);
        }

        public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            return new LobFile(config.LithiioSettings);
        }
    }

    public sealed class LobFile : FileUploader
    {
        public LobFileSettings Config { get; private set; }

        public LobFile()
        {
        }

        public LobFile(LobFileSettings config)
        {
            Config = config;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("api_key", Config.UserAPIKey);

            UploadResult result = SendRequestFile("https://lobfile.com/api/v3/upload", stream, fileName, "file", args);

            if (result.IsSuccess)
            {
                LobFileUploadResponse uploadResponse = JsonSerializer.Deserialize<LobFileUploadResponse>(result.Response);

                if (uploadResponse.Success)
                {
                    result.URL = uploadResponse.URL;
                }
                else
                {
                    Errors.Add(uploadResponse.Error);
                }
            }

            return result;
        }

        public string FetchAPIKey(string email, string password)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("email", email);
            args.Add("password", password);

            string response = SendRequestMultiPart("https://lobfile.com/api/v3/fetch-api-key", args);

            if (!string.IsNullOrEmpty(response))
            {
                LobFileFetchAPIKeyResponse apiKeyResponse = JsonSerializer.Deserialize<LobFileFetchAPIKeyResponse>(response);

                if (apiKeyResponse.Success)
                {
                    return apiKeyResponse.API_Key;
                }
                else
                {
                    throw new Exception(apiKeyResponse.Error);
                }
            }

            return null;
        }

        private class LobFileResponse
        {
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private class LobFileUploadResponse : LobFileResponse
        {
            public string URL { get; set; }
        }

        private class LobFileFetchAPIKeyResponse : LobFileResponse
        {
            public string API_Key { get; set; }
        }
    }

    public class LobFileSettings
    {
        public string UserAPIKey { get; set; } = "";
    }
}
