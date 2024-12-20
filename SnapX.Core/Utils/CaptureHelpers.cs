
// SPDX-License-Identifier: GPL-3.0-or-later



using SixLabors.ImageSharp;


namespace SnapX.Core.Utils;

public static class CaptureHelpers
{
    public static Rectangle GetScreenBounds()
    {
        // TODO: Move all calls to SnapX.CommonUI.GetScreenBounds
        // Both Avalonia and GTK4 have functions to call for this that don't have us calling native APIs directly.
        // Any code relying on this should be refactored onto SnapX.CommonUI.GetScreenBounds
        throw new NotImplementedException("GetScreenBounds is not implemented");
    }

    public static Rectangle GetScreenWorkingArea()
    {
        throw new NotImplementedException("GetScreenWorkingArea is not implemented");
    }

    private static Rectangle GetScreenBounds2()
    {
        throw new NotImplementedException("GetScreenBounds2 is not implemented");
    }

    private static Rectangle GetScreenBounds3()
    {
        throw new NotImplementedException("GetScreenBounds3 is not implemented");
    }

    private static Rectangle GetScreenBounds4()
    {
        throw new NotImplementedException("GetScreenBounds4 is not implemented");
    }

    public static Rectangle GetActiveScreenBounds()
    {
        throw new NotImplementedException("GetActiveScreenBounds is not implemented");
    }

    public static Rectangle GetActiveScreenWorkingArea()
    {
        throw new NotImplementedException("GetActiveScreenWorkingArea is not implemented");
    }

    public static Rectangle GetPrimaryScreenBounds()
    {
        throw new NotImplementedException("GetPrimaryScreenBounds is not implemented");
    }

    public static Point ScreenToClient(Point p)
    {
        throw new NotImplementedException("ScreenToClient is not implemented");
    }

    public static Rectangle ScreenToClient(Rectangle r)
    {
        return new Rectangle(ScreenToClient(r.Location), r.Size);
    }

    public static Point ClientToScreen(Point p)
    {
        throw new NotImplementedException("ClientToScreen is not implemented");
    }

    public static Rectangle ClientToScreen(Rectangle r)
    {
        return new Rectangle(ClientToScreen(r.Location), r.Size);
    }

    public static Point GetCursorPosition()
    {
        return Point.Empty;
    }

    public static void SetCursorPosition(int x, int y)
    {
        throw new NotImplementedException("SetCursorPosition is not implemented");
    }

    public static void SetCursorPosition(Point position)
    {
        SetCursorPosition(position.X, position.Y);
    }

    public static Color GetPixelColor()
    {
        return GetPixelColor(GetCursorPosition());
    }

    public static Color GetPixelColor(int x, int y)
    {
        throw new NotImplementedException("GetPixelColor is not implemented");
    }

    public static Color GetPixelColor(Point position)
    {
        return GetPixelColor(position.X, position.Y);
    }

    public static bool CheckPixelColor(int x, int y, Color color)
    {
        Color targetColor = GetPixelColor(x, y);

        throw new NotImplementedException("CheckPixelColor is not implemented");
    }

    public static bool CheckPixelColor(int x, int y, Color color, byte variation)
    {
        throw new NotImplementedException("CheckPixelColor is not implemented");
    }

    public static Rectangle CreateRectangle(int x, int y, int x2, int y2)
    {
        int width, height;

        if (x <= x2)
        {
            width = x2 - x + 1;
        }
        else
        {
            width = x - x2 + 1;
            x = x2;
        }

        if (y <= y2)
        {
            height = y2 - y + 1;
        }
        else
        {
            height = y - y2 + 1;
            y = y2;
        }

        return new Rectangle(x, y, width, height);
    }

    public static Rectangle CreateRectangle(Point pos, Point pos2)
    {
        return CreateRectangle(pos.X, pos.Y, pos2.X, pos2.Y);
    }

    public static RectangleF CreateRectangle(float x, float y, float x2, float y2)
    {
        float width, height;

        if (x <= x2)
        {
            width = x2 - x + 1;
        }
        else
        {
            width = x - x2 + 1;
            x = x2;
        }

        if (y <= y2)
        {
            height = y2 - y + 1;
        }
        else
        {
            height = y - y2 + 1;
            y = y2;
        }

        return new RectangleF(x, y, width, height);
    }

    public static RectangleF CreateRectangle(PointF pos, PointF pos2)
    {
        return CreateRectangle(pos.X, pos.Y, pos2.X, pos2.Y);
    }

    public static Point ProportionalPosition(Point pos, Point pos2)
    {
        var newPosition = Point.Empty;
        int min;

        if (pos.X < pos2.X)
        {
            if (pos.Y < pos2.Y)
            {
                min = Math.Min(pos2.X - pos.X, pos2.Y - pos.Y);
                newPosition.X = pos.X + min;
                newPosition.Y = pos.Y + min;
            }
            else
            {
                min = Math.Min(pos2.X - pos.X, pos.Y - pos2.Y);
                newPosition.X = pos.X + min;
                newPosition.Y = pos.Y - min;
            }
        }
        else
        {
            if (pos.Y > pos2.Y)
            {
                min = Math.Min(pos.X - pos2.X, pos.Y - pos2.Y);
                newPosition.X = pos.X - min;
                newPosition.Y = pos.Y - min;
            }
            else
            {
                min = Math.Min(pos.X - pos2.X, pos2.Y - pos.Y);
                newPosition.X = pos.X - min;
                newPosition.Y = pos.Y + min;
            }
        }

        return newPosition;
    }

    public static PointF SnapPositionToDegree(PointF pos, PointF pos2, float degree, float startDegree)
    {
        var angle = Math.LookAtRadian(pos, pos2);
        var startAngle = Math.DegreeToRadian(startDegree);
        var snapAngle = Math.DegreeToRadian(degree);
        var newAngle = ((float)System.Math.Round((angle + startAngle) / snapAngle) * snapAngle) - startAngle;
        var distance = Math.Distance(pos, pos2);

        var newVector = (PointF)Math.RadianToVector2(newAngle, distance);

        return new PointF(pos.X + newVector.X, pos.Y + newVector.Y);
    }

    public static PointF CalculateNewPosition(PointF posOnClick, PointF posCurrent, Size size)
    {
        if (posCurrent.X > posOnClick.X)
        {
            if (posCurrent.Y > posOnClick.Y)
            {
                return new PointF(posOnClick.X + size.Width - 1, posOnClick.Y + size.Height - 1);
            }
            else
            {
                return new PointF(posOnClick.X + size.Width - 1, posOnClick.Y - size.Height + 1);
            }
        }
        else
        {
            if (posCurrent.Y > posOnClick.Y)
            {
                return new PointF(posOnClick.X - size.Width + 1, posOnClick.Y + size.Height - 1);
            }
            else
            {
                return new PointF(posOnClick.X - size.Width + 1, posOnClick.Y - size.Height + 1);
            }
        }
    }

    public static RectangleF CalculateNewRectangle(PointF posOnClick, PointF posCurrent, Size size)
    {
        var newPosition = CalculateNewPosition(posOnClick, posCurrent, size);
        return CreateRectangle(posOnClick, newPosition);
    }

    public static Rectangle GetWindowRectangle(IntPtr handle)
    {
        var rect = Rectangle.Empty;

        // if (NativeMethods.IsDWMEnabled() && NativeMethods.GetExtendedFrameBounds(handle, out Rectangle tempRect))
        // {
        //     rect = tempRect;
        // }
        //
        // if (rect.IsEmpty)
        // {
        //     rect = NativeMethods.GetWindowRect(handle);
        // }
        //
        // if (!Helpers.IsWindows10OrGreater() && NativeMethods.IsZoomed(handle))
        // {
        //     rect = NativeMethods.MaximizedWindowFix(handle, rect);
        // }

        return rect;
    }

    public static Rectangle GetActiveWindowRectangle()
    {
        // IntPtr handle = NativeMethods.GetForegroundWindow();
        // return GetWindowRectangle(handle);
        return Rectangle.Empty;
    }

    public static Rectangle GetActiveWindowClientRectangle()
    {
        // IntPtr handle = NativeMethods.GetForegroundWindow();
        // return NativeMethods.GetClientRect(handle);
        return Rectangle.Empty;
    }

    public static bool IsActiveWindowFullscreen()
    {
        // IntPtr handle = NativeMethods.GetForegroundWindow();
        //
        // if (handle.ToInt32() > 0)
        // {
        //     WindowInfo windowInfo = new WindowInfo(handle);
        //     string className = windowInfo.ClassName;
        //     string[] ignoreList = new string[] { "Progman", "WorkerW" };
        //
        //     if (ignoreList.All(ignore => !className.Equals(ignore, StringComparison.OrdinalIgnoreCase)))
        //     {
        //         Rectangle windowRectangle = windowInfo.Rectangle;
        //         Rectangle monitorRectangle = Screen.FromRectangle(windowRectangle).Bounds;
        //         return windowRectangle.Contains(monitorRectangle);
        //     }
        // }

        return false;
    }

    public static Rectangle EvenRectangleSize(Rectangle rect)
    {
        rect.Width -= rect.Width & 1;
        rect.Height -= rect.Height & 1;
        return rect;
    }
}

