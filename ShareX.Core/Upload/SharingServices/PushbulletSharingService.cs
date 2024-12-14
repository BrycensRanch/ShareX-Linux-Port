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
using ShareX.Core.Upload.File;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Upload.SharingServices;

public class PushbulletSharingService : URLSharingService
{
    public override URLSharingServices EnumValue => URLSharingServices.Pushbullet;

    public override bool CheckConfig(UploadersConfig config)
    {
        var pushbulletSettings = config.PushbulletSettings;

        return pushbulletSettings != null && !string.IsNullOrEmpty(pushbulletSettings.UserAPIKey) && pushbulletSettings.DeviceList != null &&
            pushbulletSettings.DeviceList.IsValidIndex(pushbulletSettings.SelectedDevice);
    }

    public override URLSharer CreateSharer(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new PushbulletSharer(config.PushbulletSettings);
    }
}

public sealed class PushbulletSharer : URLSharer
{
    public PushbulletSettings Settings { get; private set; }

    public PushbulletSharer(PushbulletSettings settings)
    {
        Settings = settings;
    }

    public override UploadResult ShareURL(string url)
    {
        var result = new UploadResult { URL = url, IsURLExpected = false };

        new Pushbullet(Settings).PushLink(url, "ShareX: URL share");

        return result;
    }
}

