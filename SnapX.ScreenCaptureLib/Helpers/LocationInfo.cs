
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.ScreenCaptureLib
{
    public class LocationInfo
    {
        public long Location { get; set; }
        public long Length { get; set; }

        public LocationInfo(long location, long length)
        {
            Location = location;
            Length = length;
        }
    }
}