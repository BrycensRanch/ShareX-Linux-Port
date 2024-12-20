
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.BaseServices
{
    public abstract class URLShortenerService : UploaderService<UrlShortenerType>
    {
        public abstract URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo);
    }
}
