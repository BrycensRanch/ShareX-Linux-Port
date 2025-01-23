using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace SnapX.Core.Utils;

public static partial class OsInfo
{
    public static string GetFancyOSNameAndVersion()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetWindowsVersion();

        }
        else if (OperatingSystem.IsLinux())
        {
            return GetLinuxVersion();
        }
        else if (OperatingSystem.IsMacOS())
        {
            return GetmacOSVersion();
        }
        else
        {
            return $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
        }
    }
    [SupportedOSPlatform("windows")]
    static string GetWindowsVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            if (key == null)
                return $"Windows {Environment.OSVersion.Version}";

            var productName = key.GetValue("ProductName")?.ToString() ?? "Unknown Windows";
            var releaseId = key.GetValue("ReleaseId")?.ToString() ?? "Unknown Release";
            var currentBuild = key.GetValue("CurrentBuild")?.ToString() ?? "Unknown Version";

            if (Helpers.IsWindows11OrGreater())
                productName = productName.Replace("10", "11");

            return $"{productName} {releaseId} {currentBuild}";

        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine($"Error getting Windows version, hmm.{Environment.NewLine}{ex.ToString}");
            return $"Windows {Environment.OSVersion.Version}";
        }
    }

    static string GetLinuxVersion()
    {
        try
        {
            var osReleaseFile = "/etc/os-release";
            if (File.Exists(osReleaseFile))
            {
                var lines = File.ReadAllLines(osReleaseFile);

                var prettyName = lines.FirstOrDefault(line => line.StartsWith("PRETTY_NAME"))?.Split('=')[1]?.Trim('"');

                if (string.IsNullOrEmpty(prettyName))
                {
                    prettyName = lines.FirstOrDefault(line => line.StartsWith("NAME"))?.Split('=')[1]?.Trim('"');
                    if (string.IsNullOrEmpty(prettyName))
                    {
                        return $"Linux {Environment.OSVersion.Version}";
                    }

                    return prettyName + " " + lines.FirstOrDefault(line => line.StartsWith("VERSION"))?.Split('=')[1]?.Trim('"');
                }

                return prettyName;
            }

            return $"Linux {Environment.OSVersion.Version}";
        }
        catch
        {
            return $"Linux {Environment.OSVersion.Version}";
        }
    }


    static string GetmacOSVersion()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "sw_vers",
                Arguments = "-productName",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            if (process == null) return RuntimeInformation.OSDescription; // Gracefully fail
            var osName = process.StandardOutput.ReadLine().Trim();
            process.WaitForExit();

            processStartInfo.Arguments = "-productVersion";
            process = Process.Start(processStartInfo);
            var version = process.StandardOutput.ReadLine().Trim();
            process.WaitForExit();

            return $"{osName} {version}";
        }
        catch
        {
            return $"macOS {Environment.OSVersion.Version}";
        }
    }

    public static string GetProcessorName()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetProcessorNameWindows();
        }
        else if (OperatingSystem.IsLinux())
        {
            return GetProcessorNameLinux();
        }
        else if (OperatingSystem.IsMacOS())
        {
            return GetProcessorNameMacOS();
        }
        else
        {
            throw new PlatformNotSupportedException("This platform is not supported.");
        }
    }
    [SupportedOSPlatform("windows")]
    private static string GetProcessorNameWindows()
    {
        try
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
            {
                if (key != null)
                {
                    var processorName = key.GetValue("ProcessorNameString")?.ToString();
                    return processorName ?? "Unknown Processor";
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading registry: " + ex.Message);
        }

        return "Unknown Processor";
    }


    private static string GetProcessorNameLinux()
    {
        try
        {
            var cpuInfoPath = "/proc/cpuinfo";
            var lines = File.ReadAllLines(cpuInfoPath);
            foreach (var line in lines)
            {
                if (line.StartsWith("model name"))
                {
                    var processorName = line.Substring(line.IndexOf(":") + 2).Trim();
                    return processorName;
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading /proc/cpuinfo: " + ex.Message);
        }

        return "Unknown Processor";
    }

    private static string GetProcessorNameMacOS()
    {
        try
        {
            var process = new Process();
            process.StartInfo.FileName = "sysctl";
            process.StartInfo.Arguments = "machdep.cpu.brand_string";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            var cpuName = process.StandardOutput.ReadLine()!.Replace("machdep.cpu.brand_string: ", "");
            process.WaitForExit();

            return cpuName?.Trim() ?? "Unknown Processor";
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading sysctl: " + ex.Message);
        }

        return "Unknown Processor";
    }
    public static (long totalMemory, long usedMemory) GetMemoryInfo()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetMemoryInfoWindows();
        }
        if (OperatingSystem.IsLinux())
        {
            return GetMemoryInfoLinux();
        }
        if (OperatingSystem.IsMacOS())
        {
            return GetMemoryInfoMacOS();
        }
        throw new PlatformNotSupportedException("This platform is not supported.");
    }
    [SupportedOSPlatform("windows")]
    private static (long totalMemory, long usedMemory) GetMemoryInfoWindows()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            if (key != null)
            {
                // Get total physical memory using the registry
                var totalMemory = (long)key.GetValue("TotalPhysicalMemory", 0);
                // Let's assume a fixed value for used memory (this is an approximation)
                var usedMemory = totalMemory - GetAvailableMemoryWindows();

                return (totalMemory / (1024 * 1024), usedMemory / (1024 * 1024)); // Return in MiB
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading memory info on Windows (Registry): " + ex.Message);
        }

        return (0, 0);
    }
    [SupportedOSPlatform("windows")]
    private static long GetAvailableMemoryWindows()
    {
        try
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management");
            if (key != null)
            {
                var availableMemory = (long)key.GetValue("AvailablePhysicalMemory", 0);
                return availableMemory;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error accessing registry for available memory: " + ex.Message);
        }

        return 0;
    }
    [SupportedOSPlatform("linux")]
    private static (long totalMemory, long usedMemory) GetMemoryInfoLinux()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");

            long totalMemory = 0;
            long availableMemory = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal"))
                {
                    totalMemory = ParseMemInfo(line);
                }
                else if (line.StartsWith("MemAvailable"))
                {
                    availableMemory = ParseMemInfo(line);
                }
            }

            long usedMemory = totalMemory - availableMemory;

            return (totalMemory / 1024, usedMemory / 1024);
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading memory info on Linux: " + ex.Message);
        }

        return (0, 0);
    }

    private static long ParseMemInfo(string line)
    {
        var parts = line.Split(':');
        var value = parts[1].Trim().Split(' ')[0];
        return long.TryParse(value, out long result) ? result : 0;
    }

    private static (long totalMemory, long usedMemory) GetMemoryInfoMacOS()
    {
        try
        {
            var totalMemory = GetSysctl("hw.memsize");
            var freeMemory = GetSysctl("vm.page_free_count");
            var pageSize = GetSysctl("hw.pagesize");

            long freeMemoryBytes = freeMemory * pageSize;
            long usedMemoryBytes = totalMemory - freeMemoryBytes;

            return (totalMemory / (1024 * 1024), usedMemoryBytes / (1024 * 1024));
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading memory info on macOS: " + ex.Message);
        }

        return (0, 0);
    }

    private static long GetSysctl(string key)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "sysctl";
        process.StartInfo.Arguments = key;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        var output = process.StandardOutput.ReadLine();
        process.WaitForExit();

        if (long.TryParse(output.Split(':')[1].Trim(), out long value))
        {
            return value;
        }

        return 0;
    }
    public static void PrintGraphicsInfo()
    {
        if (OperatingSystem.IsWindows())
        {
            PrintGraphicsInfoWindows();
        }
        else if (OperatingSystem.IsLinux())
        {
            PrintGraphicsInfoLinux();
        }
        else if (OperatingSystem.IsMacOS())
        {
            PrintGraphicsInfoMacOS();
        }
        else
        {
            DebugHelper.WriteLine("This platform is not supported for printing graphics information.");
        }
    }
    [SupportedOSPlatform("windows")]
    private static void PrintGraphicsInfoWindows()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\PCI");
            if (key != null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var deviceKey = key.OpenSubKey(subKeyName);
                    var deviceDesc = deviceKey?.GetValue("DeviceDesc")?.ToString();
                    var driverVersion = deviceKey?.OpenSubKey("Device Parameters")?.GetValue("DriverVersion")?.ToString();
                    if (deviceDesc != null && driverVersion != null)
                    {
                        DebugHelper.WriteLine($"GPU: {deviceDesc}");
                        DebugHelper.WriteLine($"Driver Version: {driverVersion}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading GPU info on Windows: " + ex.Message);
        }

        try
        {
            // Did you know Powershell is extremely powerful?
            var monitorInfo = RunPowerShellCommand("Get-WmiObject Win32_DesktopMonitor | Select-Object -Property Name");
            DebugHelper.WriteLine($"Monitor: {monitorInfo}");
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading monitor info on Windows: " + ex.Message);
        }
    }
    private static string RunPowerShellCommand(string command)
    {
        var process = new Process();
        process.StartInfo.FileName = "powershell";
        process.StartInfo.Arguments = $"-Command \"{command}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
    private static void PrintGraphicsInfoLinux()
    {
        try
        {
            var GpuInfo = RunShellCommand("lspci | grep -i 'vga'");
            if (!string.IsNullOrWhiteSpace(GpuInfo))
            {
                DebugHelper.WriteLine("GPU: " + GpuInfo);
                var driverVersion = RunShellCommand("glxinfo | grep 'OpenGL version'");
                DebugHelper.WriteLine("Driver Version: " + driverVersion.Split(':')[1].Trim());
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading GPU info on Linux: " + ex.Message);
        }

        try
        {
            var gpuInfo = File.ReadAllText("/proc/driver/nvidia/version");
            DebugHelper.WriteLine("NVIDIA GPU Driver Version: " + gpuInfo);
        }
        catch
        {
            // I acknowledge I am swallowing errors.
        }

        try
        {
            var output = RunShellCommand("xrandr --listmonitors");
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("connected"))
                {
                    var parts = line.Split(' ');
                    DebugHelper.WriteLine($"Monitor: {parts[0]}");
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading X11 monitor info on Linux: " + ex.Message);
        }
    }

    private static void PrintGraphicsInfoMacOS()
    {
        try
        {
            var output = RunShellCommand("system_profiler SPDisplaysDataType");
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("Chipset Model"))
                {
                    var gpu = line.Split(":")[1].Trim();
                    DebugHelper.WriteLine($"GPU: {gpu}");
                }
                if (line.Contains("Driver Version"))
                {
                    var driverVersion = line.Split(":")[1].Trim();
                    DebugHelper.WriteLine($"Driver Version: {driverVersion}");
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading GPU info on macOS: " + ex.Message);
        }

        try
        {
            var output = RunShellCommand("system_profiler SPDisplaysDataType");
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("Resolution"))
                {
                    var resolution = line.Split(":")[1].Trim();
                    DebugHelper.WriteLine($"Monitor Resolution: {resolution}");
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.WriteLine("Error reading monitor info on macOS: " + ex.Message);
        }
    }

    private static string RunShellCommand(string command)
    {
        var process = new Process();
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"{command}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
    public static bool IsHdrSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            return CheckWindowsHdr();
        }
        if (OperatingSystem.IsMacOS())
        {
            return CheckMacOSHdr();
        }
        // Detection of HDR on Linux is way too work.
        // If they're on Linux, they should know they're using things like HDR.
        return false;
    }

    [SupportedOSPlatform("linux")]
    public static bool IsWSL()
    {
        if (!OperatingSystem.IsLinux()) return false;
        try
        {
            string procVersion = File.ReadAllText("/proc/version");

            return procVersion.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        catch (Exception)
        {
            // If we can't read /proc/version, it's likely not running in WSL or there is another error
            return false;
        }
    }
    [SupportedOSPlatform("windows")]
    private static bool CheckWindowsHdr()
    {
        var hdc = GetDC(IntPtr.Zero);
        var bpp = GetDeviceCaps(hdc, BITSPIXEL);
        return bpp >= 30;
    }

    private static bool CheckMacOSHdr()
    {
        string displayInfo = GetMacOSDisplayInfo();
        return displayInfo.Contains("High Dynamic Range");
    }

    private static string GetMacOSDisplayInfo()
    {
        var process = new Process();
        process.StartInfo.FileName = "system_profiler";
        process.StartInfo.Arguments = "SPDisplaysDataType";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    private const int BITSPIXEL = 12;
}
