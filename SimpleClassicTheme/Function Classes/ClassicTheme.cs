/*
 *  Simple Classic Theme, a basic utility to bring back classic theme to 
 *  newer versions of the Windows operating system.
 *  Copyright (C) 2022 Anis Errais
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using Microsoft.Win32;
using NtApiDotNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace SimpleClassicTheme
{
    public static class ClassicTheme
    {
        //Enables Classic Theme
        public static void Enable()
        {
            if (Environment.OSVersion.Version.Major == 10)
            { 
                Process.Start("explorer.exe", "C:\\Windows\\System32\\ApplicationFrameHost.exe").WaitForExit();
                Thread.Sleep(200);
            }
            NtObject g = NtObject.OpenWithType("Section", $@"\Sessions\{Process.GetCurrentProcess().SessionId}\Windows\ThemeSection", null, GenericAccessRights.WriteDac);
            g.SetSecurityDescriptor(new SecurityDescriptor("O:BAG:SYD:(A;;RC;;;IU)(A;;DCSWRPSDRCWDWO;;;SY)"), SecurityInformation.Dacl);
            g.Close();
            Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Microsoft").CreateSubKey("Windows").CreateSubKey("CurrentVersion").CreateSubKey("Themes").CreateSubKey("DefaultColors");
            ExtraFunctions.RenameSubKey(Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("Microsoft").CreateSubKey("Windows").CreateSubKey("CurrentVersion").CreateSubKey("Themes"), "DefaultColors", "DefaultColorsOld");
        }

        //Disables Classic Theme
        public static void Disable()
        {
            NtObject g = NtObject.OpenWithType("Section", $@"\Sessions\{Process.GetCurrentProcess().SessionId}\Windows\ThemeSection", null, GenericAccessRights.WriteDac);
            g.SetSecurityDescriptor(new SecurityDescriptor("O:BAG:SYD:(A;;CCLCRC;;;IU)(A;;CCDCLCSWRPSDRCWDWO;;;SY)"), SecurityInformation.Dacl);
            g.Close();
        }

        //Enables Classic Theme and if specified Classic Taskbar.
        public static void MasterEnable(bool taskbar, bool commandLineError = false)
        {
            Process.Start($"{Configuration.Instance.InstallPath}EnableThemeScript.bat", "pre").WaitForExit();
            Configuration.Instance.Enabled = true;
            if (!taskbar)
            {
                // No taskbar
                Enable();
            }
            else if (!File.Exists($"{Configuration.Instance.InstallPath}SCT.exe"))
			{
                if (!commandLineError)
                    MessageBox.Show("You need to install Simple Classic Theme in order to enable it with Classic Taskbar enabled. Either disable Classic Taskbar from the options menu, or install SCT by pressing 'Run SCT on boot' in the main menu.", "Unsupported action");
                else
                    Console.WriteLine($"Enabling SCT with a taskbar requires SCT to be installed to \"{Configuration.Instance.InstallPath}SCT.exe\".");
                Configuration.Instance.Enabled = false;
                return;
            }
            else if (Configuration.Instance.TaskbarType == TaskbarType.SimpleClassicThemeTaskbar)
            {
                // Simple Classic Theme Taskbar
                Enable();
                Process.Start("cmd", "/c taskkill /im explorer.exe /f").WaitForExit();
                Process.Start("explorer.exe", @"C:\Windows\explorer.exe");
                ClassicTaskbar.EnableSCTT();
            }
            else if (Configuration.Instance.TaskbarType == TaskbarType.Windows81Vanilla)
			{
                // Windows 8.1 Vanilla taskbar with post-load patches
                Enable();
                Process.Start("cmd", "/c taskkill /im explorer.exe /f").WaitForExit();
                Process.Start("explorer.exe", @"C:\Windows\explorer.exe");
                Thread.Sleep(Configuration.Instance.TaskbarDelay);
                ClassicTaskbar.FixWin8_1();
            }
            else if (Configuration.Instance.TaskbarType == TaskbarType.RetroBar)
			{
                // RetroBar
                Enable();
                Process.Start("cmd", "/c taskkill /im explorer.exe /f").WaitForExit();
                Process.Start("explorer.exe", @"C:\Windows\explorer.exe");
                Process.Start($"{Configuration.Instance.InstallPath}RetroBar\\RetroBar.exe");
            }
            Process.Start($"{Configuration.Instance.InstallPath}EnableThemeScript.bat", "post").WaitForExit();
        }

        //Disables Classic Theme and if specified Classic Taskbar.
        public static void MasterDisable(bool taskbar)
        {
            Process.Start($"{Configuration.Instance.InstallPath}DisableThemeScript.bat", "pre").WaitForExit();
            Configuration.Instance.Enabled = false;
            if (!taskbar)
            {
                Disable();
            }
            else if (Configuration.Instance.TaskbarType == TaskbarType.SimpleClassicThemeTaskbar)
            {
                ClassicTaskbar.DisableSCTT();
                Disable();
                Process.Start("cmd", "/c taskkill /im explorer.exe /f").WaitForExit();
                Process.Start("explorer.exe", @"C:\Windows\explorer.exe");
            }
            else if (Configuration.Instance.TaskbarType == TaskbarType.Windows81Vanilla)
            {
                Disable();
                Process.Start("cmd", "/c taskkill /im explorer.exe /f").WaitForExit();
                Process.Start("explorer.exe", @"C:\Windows\explorer.exe");
            }
            else if (Configuration.Instance.TaskbarType == TaskbarType.RetroBar)
            {
                foreach (Process p in Process.GetProcessesByName("RetroBar"))
                    p.Kill();

                Disable();
                Process.Start("cmd", "/c taskkill /im explorer.exe /f").WaitForExit();
                Process.Start("explorer.exe", @"C:\Windows\explorer.exe");
            }
            Process.Start($"{Configuration.Instance.InstallPath}DisableThemeScript.bat", "post").WaitForExit();
        }
    }
}
