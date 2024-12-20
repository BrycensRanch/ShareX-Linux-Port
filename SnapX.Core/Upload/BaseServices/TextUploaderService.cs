
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.BaseServices
{
    public abstract class TextUploaderService : UploaderService<TextDestination>, IGenericUploaderService
    {
        public abstract GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo);
    }
}
