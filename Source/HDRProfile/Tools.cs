﻿using CodectoryCore.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AutoHDR
{
    enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

	public static class Tools
	{

		public static int GlobalRefreshInterval = 500;

		public static void SetAutoStart(string applicationName, string filePath, bool autostart)
		{
			RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
			object existing = rk.GetValue(applicationName);
			if (filePath.Equals(existing) && autostart)
				return;
			if (rk.GetValue(applicationName) == null && !autostart)
				return;

			if (autostart)
				rk.SetValue(applicationName, filePath);
			else
				rk.DeleteValue(applicationName, false);
		}

		public static IDictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
		(Dictionary<TKey, TValue> original) where TValue : ICloneable
		{
			Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
			foreach (KeyValuePair<TKey, TValue> entry in original)
			{
				ret.Add(entry.Key, (TValue)entry.Value.Clone());
			}
			return ret;
		}
		public static Version ApplicationVersion
		{
			get
			{
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				string versionString = assembly.GetName().Version.ToString();
				Version version = new Version(versionString.Substring(0, versionString.LastIndexOf('.')));
				return version;
			}
		}

		public static Logs Logs = new Logs($"{System.AppDomain.CurrentDomain.BaseDirectory}AutoHDR.log", "AutoHDR", Assembly.GetExecutingAssembly().GetName().Version.ToString(), false);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int SetForegroundWindow(IntPtr hwnd);


		public static void BringMainWindowToFront(string processName)
		{
			// get the process
			Process bProcess = Process.GetProcessesByName(processName).FirstOrDefault();

			// check if the process is running
			if (bProcess != null)
			{
				// check if the window is hidden / minimized
				if (bProcess.MainWindowHandle == IntPtr.Zero)
				{
					// the window is hidden so try to restore it before setting focus.
					ShowWindow(bProcess.Handle, ShowWindowEnum.Restore);
				}

				// set user the focus to the window
				SetForegroundWindow(bProcess.MainWindowHandle);
			}
			else
			{
				// the process is not running, so start it
				Process.Start(processName);
			}
		}




		/// <summary>
		/// Returns an icon for a given file - indicated by the name parameter.
		/// </summary>
		/// <param name="name">Pathname for file.</param>
		/// <param name="size">Large or small</param>
		/// <param name="linkOverlay">Whether to include the link icon</param>
		/// <returns>System.Drawing.Icon</returns>
		public static Bitmap GetFileIcon(string name)
		{
			return CodectoryCore.Windows.Icons.IconHelper.GetIconImage(name, CodectoryCore.Windows.Icons.IconHelper.IconSize.jumbo);
		}

		private static Bitmap GetBitmap(BitmapSource source)
		{
			Bitmap bmp = new Bitmap(
			  source.PixelWidth,
			  source.PixelHeight,
			  PixelFormat.Format32bppPArgb);
			BitmapData data = bmp.LockBits(
			  new Rectangle(System.Drawing.Point.Empty, bmp.Size),
			  ImageLockMode.WriteOnly,
			  PixelFormat.Format32bppPArgb);
			source.CopyPixels(
			  Int32Rect.Empty,
			  data.Scan0,
			  data.Height * data.Stride,
			  data.Stride);
			bmp.UnlockBits(data);
			return bmp;
		}

	}

}

