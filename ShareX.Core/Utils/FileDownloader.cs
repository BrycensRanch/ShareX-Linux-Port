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

using System.Diagnostics;
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Utils;
public class FileDownloader
{
    public event Action FileSizeReceived;
    public event Action ProgressChanged;

    public string URL { get; set; }
    public string DownloadLocation { get; set; }
    public string AcceptHeader { get; set; }

    public bool IsDownloading { get; private set; }
    public bool IsCanceled { get; private set; }
    public long FileSize { get; private set; } = -1;
    public long DownloadedSize { get; private set; }
    public double DownloadSpeed { get; private set; }

    public double DownloadPercentage
    {
        get
        {
            if (FileSize > 0)
            {
                return (double)DownloadedSize / FileSize * 100;
            }

            return 0;
        }
    }

    private const int bufferSize = 32768;

    public FileDownloader()
    {
    }

    public FileDownloader(string url, string downloadLocation)
    {
        URL = url;
        DownloadLocation = downloadLocation;
    }

    public async Task<bool> StartDownload()
    {
        if (!IsDownloading && !string.IsNullOrEmpty(URL))
        {
            IsDownloading = true;
            IsCanceled = false;
            FileSize = -1;
            DownloadedSize = 0;
            DownloadSpeed = 0;

            return await DoWork();
        }

        return false;
    }

    public void StopDownload()
    {
        IsCanceled = true;
    }

    private async Task<bool> DoWork()
    {
        try
        {
            var client = HttpClientFactory.Create();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, URL);
            if (!string.IsNullOrEmpty(AcceptHeader))
            {
                requestMessage.Headers.Accept.ParseAdd(AcceptHeader);
            }

            using var responseMessage =
                await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();

            FileSize = responseMessage.Content.Headers.ContentLength ?? -1;
            FileSizeReceived?.Invoke();

            if (FileSize <= 0) return false; // Early return if file size is not valid

            var timer = new Stopwatch();
            var progressEventTimer = new Stopwatch();
            long speedTest = 0;

            var buffer = new byte[(int)Math.Min(bufferSize, FileSize)];
            int bytesRead;

            await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(DownloadLocation, FileMode.Create, FileAccess.Write, FileShare.Read);

            while (DownloadedSize < FileSize && !IsCanceled)
            {
                // Start timers if they haven't started yet
                if (!timer.IsRunning) timer.Start();
                if (!progressEventTimer.IsRunning) progressEventTimer.Start();

                bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Exit if no more data is read

                await fileStream.WriteAsync(buffer, 0, bytesRead);
                DownloadedSize += bytesRead;
                speedTest += bytesRead;

                // Update speed every 500ms
                if (timer.ElapsedMilliseconds > 500)
                {
                    DownloadSpeed = (double)speedTest / timer.ElapsedMilliseconds * 1000;
                    speedTest = 0;
                    timer.Reset();
                }

                // Trigger progress event every 100ms
                if (progressEventTimer.ElapsedMilliseconds < 100) continue;
                ProgressChanged?.Invoke();
                progressEventTimer.Reset();
            }

            // Final progress event after loop
            ProgressChanged?.Invoke();
            return true;
        }
        catch (Exception)
        {
            if (!IsCanceled)
            {
                throw;
            }
        }
        finally
        {
            // Handle cleanup if canceled
            if (IsCanceled)
            {
                try
                {
                    if (System.IO.File.Exists(DownloadLocation))
                    {
                        System.IO.File.Delete(DownloadLocation);
                    }
                }
                catch
                {
                    // Swallow exceptions during cleanup
                }
            }

            IsDownloading = false;
        }

        return false;
    }
}

