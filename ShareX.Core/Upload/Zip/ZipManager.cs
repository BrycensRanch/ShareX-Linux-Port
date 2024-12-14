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

using System.IO.Compression;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.Zip;

public static class ZipManager
{
    public static void Extract(string archivePath, string destination, bool retainDirectoryStructure = true, Func<ZipArchiveEntry, bool> filter = null,
        long maxUncompressedSize = 0)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        if (maxUncompressedSize > 0)
        {
            var totalUncompressedSize = archive.Entries.Sum(entry => entry.Length);

            if (totalUncompressedSize > maxUncompressedSize)
            {
                throw new Exception("Uncompressed file size of this archive is bigger than the maximum allowed file size.\r\n\r\n" +
                    $"Archive uncompressed file size: {totalUncompressedSize.ToSizeString()}\r\n" +
                    $"Maximum allowed file size: {maxUncompressedSize.ToSizeString()}");
            }
        }

        var fullName = Directory.CreateDirectory(Path.GetFullPath(destination)).FullName;

        foreach (var entry in archive.Entries)
        {
            if (filter != null && !filter(entry))
            {
                continue;
            }

            var entryName = retainDirectoryStructure ? entry.FullName : entry.Name;

            var fullPath = Path.GetFullPath(Path.Combine(fullName, entryName));
            if (!fullPath.StartsWith(fullName, StringComparison.OrdinalIgnoreCase)) continue;

            if (Path.GetFileName(fullPath).Length == 0 && entry.Length == 0)
            {
                Directory.CreateDirectory(fullPath);
                return;
            }
            var directory = Path.GetDirectoryName(fullPath);
            if (directory == null) continue;
            Directory.CreateDirectory(directory);
            ExtractToFile(entry, fullPath, true);
        }
    }

    private static void ExtractToFile(ZipArchiveEntry source, string destinationFileName, bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationFileName);

        var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

        using var fs = new FileStream(destinationFileName, fileMode, FileAccess.Write, FileShare.None, bufferSize: 0x1000, useAsync: false);
        using var es = source.Open();
        using var maxLengthStream = new MaxLengthStream(es, source.Length);

        maxLengthStream.CopyTo(fs);

        System.IO.File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
    }



    public static void Compress(string source, string archivePath, CompressionLevel compression = CompressionLevel.Optimal)
    {
        if (System.IO.File.Exists(archivePath))
        {
            System.IO.File.Delete(archivePath);
        }

        ZipFile.CreateFromDirectory(source, archivePath, compression, false);
    }

    public static void Compress(string archivePath, List<ZipEntryInfo> entries, CompressionLevel compression = CompressionLevel.Optimal)
    {
        FileHelpers.CreateDirectoryFromFilePath(archivePath);

        if (System.IO.File.Exists(archivePath))
        {
            System.IO.File.Delete(archivePath);
        }

        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);

        foreach (var entry in entries)
        {
            archive.CreateEntry(entry, compression);
        }
    }


    private static ZipArchiveEntry CreateEntry(this ZipArchive archive, ZipEntryInfo entryInfo, CompressionLevel compressionLevel)
    {
        ArgumentNullException.ThrowIfNull(entryInfo);

        if (entryInfo.Data != null)
        {
            using var data = entryInfo.Data;
            return archive.CreateEntryFromStream(data, entryInfo.EntryName, compressionLevel);
        }

        if (!string.IsNullOrEmpty(entryInfo.SourcePath) && System.IO.File.Exists(entryInfo.SourcePath))
        {
            return archive.CreateEntryFromFile(entryInfo.SourcePath, entryInfo.EntryName, compressionLevel);
        }

        return null;
    }


    private static ZipArchiveEntry CreateEntryFromStream(this ZipArchive archive, Stream stream, string entryName, CompressionLevel compressionLevel)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(entryName);

        var entry = archive.CreateEntry(entryName, compressionLevel);
        entry.LastWriteTime = DateTime.Now;

        using var entryStream = entry.Open();
        stream.Position = 0;
        stream.CopyTo(entryStream);

        return entry;
    }

}

