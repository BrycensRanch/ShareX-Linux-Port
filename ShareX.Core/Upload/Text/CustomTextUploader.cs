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

using System.Text;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Custom;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.Text;

public class CustomTextUploaderService : TextUploaderService
{
    public override TextDestination EnumValue => TextDestination.CustomTextUploader;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.CustomUploadersList != null && config.CustomUploadersList.IsValidIndex(config.CustomTextUploaderSelected);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        int index;

        if (taskInfo.OverrideCustomUploader)
        {
            index = taskInfo.CustomUploaderIndex.BetweenOrDefault(0, config.CustomUploadersList.Count - 1);
        }
        else
        {
            index = config.CustomTextUploaderSelected;
        }

        var customUploader = config.CustomUploadersList.ReturnIfValidIndex(index);

        if (customUploader != null)
        {
            return new CustomTextUploader(customUploader);
        }

        return null;
    }
}

public sealed class CustomTextUploader : TextUploader
{
    private CustomUploaderItem uploader;

    public CustomTextUploader(CustomUploaderItem customUploaderItem)
    {
        uploader = customUploaderItem;
    }

    public override UploadResult UploadText(string text, string fileName)
    {
        var result = new UploadResult();
        var input = new CustomUploaderInput(fileName, text);

        switch (uploader.Body)
        {
            case CustomUploaderBody.None:
                result.Response = SendRequest(uploader.RequestMethod, uploader.GetRequestURL(input), null, uploader.GetHeaders(input));
                break;

            case CustomUploaderBody.MultipartFormData:
                if (string.IsNullOrEmpty(uploader.FileFormName))
                {
                    result.Response = SendRequestMultiPart(
                        uploader.GetRequestURL(input),
                        uploader.GetArguments(input),
                        uploader.GetHeaders(input),
                        null,
                        uploader.RequestMethod
                    );
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(text);
                    using (var stream = new MemoryStream(bytes))
                    {
                        result = SendRequestFile(
                            uploader.GetRequestURL(input),
                            stream,
                            fileName,
                            uploader.GetFileFormName(),
                            uploader.GetArguments(input),
                            uploader.GetHeaders(input),
                            null,
                            uploader.RequestMethod
                        );
                    }
                }
                break;

            case CustomUploaderBody.FormURLEncoded:
                result.Response = SendRequestURLEncoded(
                    uploader.RequestMethod,
                    uploader.GetRequestURL(input),
                    uploader.GetArguments(input),
                    uploader.GetHeaders(input)
                );
                break;

            case CustomUploaderBody.JSON:
            case CustomUploaderBody.XML:
                result.Response = SendRequest(
                    uploader.RequestMethod,
                    uploader.GetRequestURL(input),
                    uploader.GetData(input),
                    uploader.GetContentType(),
                    null,
                    uploader.GetHeaders(input)
                );
                break;

            case CustomUploaderBody.Binary:
                var binaryBytes = Encoding.UTF8.GetBytes(text);
                using (var binaryStream = new MemoryStream(binaryBytes))
                {
                    result.Response = SendRequest(
                        uploader.RequestMethod,
                        uploader.GetRequestURL(input),
                        binaryStream,
                        MimeTypes.GetMimeTypeFromFileName(fileName),
                        null,
                        uploader.GetHeaders(input)
                    );
                }
                break;

            default:
                throw new Exception("Unsupported request format: " + uploader.Body);
        }

        uploader.TryParseResponse(result, LastResponseInfo, Errors, input);
        return result;
    }
}

