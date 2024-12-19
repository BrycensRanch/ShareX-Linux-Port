// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.Core.Upload.BaseUploaders;

public abstract class FileUploader : GenericUploader
{
    public UploadResult UploadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
        if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Upload(stream, Path.GetFileName(filePath));
    }
}
