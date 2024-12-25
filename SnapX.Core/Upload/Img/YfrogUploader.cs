
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using System.Xml.Linq;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.Upload.Img
{
    public enum YfrogThumbnailType
    {
        [Description("Mini Thumbnail")]
        Mini,
        [Description("Normal Thumbnail")]
        Thumb
    }

    public enum YfrogUploadType
    {
        [Description("Upload Image")]
        UPLOAD_IMAGE_ONLY,
        [Description("Upload Image and update Twitter Status")]
        UPLOAD_IMAGE_AND_TWITTER
    }

    public class YfrogOptions : AccountInfo
    {
        public string DeveloperKey { get; set; }

        public string Source { get; set; }

        public YfrogUploadType UploadType { get; set; }

        public bool ShowFull { get; set; }

        public YfrogThumbnailType ThumbnailMode { get; set; }

        public YfrogOptions(string devKey)
        {
            DeveloperKey = devKey;
        }
    }

    public sealed class YfrogUploader : ImageUploader
    {
        private YfrogOptions Options;

        private const string UploadLink = "https://yfrog.com/api/upload";
        private const string UploadAndPostLink = "https://yfrog.com/api/uploadAndPost";

        public YfrogUploader(YfrogOptions options)
        {
            Options = options;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            switch (Options.UploadType)
            {
                case YfrogUploadType.UPLOAD_IMAGE_ONLY:
                    return Upload(stream, fileName, "");
                case YfrogUploadType.UPLOAD_IMAGE_AND_TWITTER: throw new NotImplementedException("YfrogUploadType UPLOAD_IMAGE_AND_TWITTER is not implemented.");
            }

            return null;
        }

        private UploadResult Upload(Stream stream, string fileName, string msg)
        {
            string url;

            Dictionary<string, string> arguments = new Dictionary<string, string>();

            arguments.Add("username", Options.Username);
            arguments.Add("password", Options.Password);

            if (!string.IsNullOrEmpty(msg))
            {
                arguments.Add("message", msg);
                url = UploadAndPostLink;
            }
            else
            {
                url = UploadLink;
            }
            if (!string.IsNullOrEmpty(Options.Source))
            {
                arguments.Add("source", Options.Source);
            }

            arguments.Add("key", Options.DeveloperKey);

            UploadResult result = SendRequestFile(url, stream, fileName, "media", arguments);

            return ParseResult(result);
        }

        private UploadResult ParseResult(UploadResult result)
        {
            if (result.IsSuccess)
            {
                XDocument xdoc = XDocument.Parse(result.Response);
                XElement xele = xdoc.Element("rsp");

                if (xele != null)
                {
                    switch (xele.GetAttributeFirstValue("status", "stat"))
                    {
                        case "ok":
                            //string statusid = xele.GetElementValue("statusid");
                            //string userid = xele.GetElementValue("userid");
                            //string mediaid = xele.GetElementValue("mediaid");
                            string mediaurl = xele.GetElementValue("mediaurl");
                            if (Options.ShowFull) mediaurl += "/full";
                            result.URL = mediaurl;
                            result.ThumbnailURL = mediaurl + ".th.jpg";
                            break;
                        case "fail":
                            //string code = xele.Element("err").Attribute("code").Value;
                            string msg = xele.Element("err").Attribute("msg").Value;
                            Errors.Add(msg);
                            break;
                    }
                }
            }

            return result;
        }
    }
}