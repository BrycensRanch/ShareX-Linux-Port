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

using System.ComponentModel;
using ShareX.Core.Upload.BaseUploaders;

namespace ShareX.Core.Upload.Text;

public sealed class Pastebin_ca : TextUploader
{
    private const string APIURL = "https://pastebin.ca/quiet-paste.php";

    private string APIKey;

    private PastebinCaSettings settings;

    public Pastebin_ca(string apiKey)
    {
        APIKey = apiKey;
        settings = new PastebinCaSettings();
    }

    public Pastebin_ca(string apiKey, PastebinCaSettings settings)
    {
        APIKey = apiKey;
        this.settings = settings;
    }

    public override UploadResult UploadText(string text, string fileName)
    {
        var ur = new UploadResult();

        if (string.IsNullOrEmpty(text))
            return ur;

        var arguments = new Dictionary<string, string>
        {
            { "api", APIKey },
            { "content", text },
            { "description", settings.Description },
            { "encryptpw", settings.EncryptPassword },
            { "expiry", settings.ExpireTime },
            { "name", settings.Author },
            { "s", "Submit Post" },
            { "tags", settings.Tags },
            { "type", settings.TextFormat }
        };

        if (settings.Encrypt)
        {
            arguments.Add("encrypt", "true");
        }

        ur.Response = SendRequestMultiPart(APIURL, arguments);

        if (string.IsNullOrEmpty(ur.Response))
            return ur;

        if (ur.Response.StartsWith("SUCCESS:"))
        {
            ur.URL = string.Concat("https://pastebin.ca/", ur.Response.AsSpan(8));
        }
        else if (ur.Response.StartsWith("FAIL:"))
        {
            Errors.Add(ur.Response.Substring(5));
        }

        return ur;
    }
}

public class PastebinCaSettings
{
    /// <summary>name</summary>
    [Description("Name / Title")]
    public string Author { get; set; }

    /// <summary>description</summary>
    [Description("Description / Question")]
    public string Description { get; set; }

    /// <summary>tags</summary>
    [Description("Tags (space separated, optional)")]
    public string Tags { get; set; }

    /// <summary>type</summary>
    [Description("Content Type"), DefaultValue("1")]
    public string TextFormat { get; set; }

    /// <summary>expiry</summary>
    [Description("Expire this post in ..."), DefaultValue("1 month")]
    public string ExpireTime { get; set; }

    /// <summary>encrypt</summary>
    [Description("Encrypt this paste")]
    public bool Encrypt { get; set; }

    /// <summary>encryptpw</summary>
    public string EncryptPassword { get; set; }

    public PastebinCaSettings()
    {
        Author = "";
        Description = "";
        Tags = "";
        TextFormat = "1";
        ExpireTime = "1 month";
        Encrypt = false;
        EncryptPassword = "";
    }
}
