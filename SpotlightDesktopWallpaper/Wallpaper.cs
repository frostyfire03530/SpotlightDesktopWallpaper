/*
 * Created by SharpDevelop.
 * User: Frosty
 * Date: 2/23/2016
 * Time: 10:35 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32; 

namespace SpotlightDesktopWallpaper{
	/// <summary>
	/// Used to set wallpapers.
	/// </summary>
	public sealed class Wallpaper{
	    Wallpaper() { }
	    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
   		[return: MarshalAs(UnmanagedType.Bool)]
   		private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
	    private const uint SPI_SETDESKWALLPAPER = 20;
   		private const uint SPIF_UPDATEINIFILE = 0x01;
   		private const uint SPIF_SENDWININICHANGE = 0x02;
	    public enum WallpaperStyle{
            Tile,
	        Center,
	        Stretch,
	        Fit,
	        Fill
	    }
	    public static void Set(string path, WallpaperStyle style){
	        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
	        switch (style){
		        case WallpaperStyle.Tile:
		            key.SetValue(@"WallpaperStyle", "0");
		            key.SetValue(@"TileWallpaper", "1");
		            break;
		        case WallpaperStyle.Center:
		            key.SetValue(@"WallpaperStyle", "0");
		            key.SetValue(@"TileWallpaper", "0");
		            break;
		        case WallpaperStyle.Stretch:
		            key.SetValue(@"WallpaperStyle", "2");
		            key.SetValue(@"TileWallpaper", "0");
		            break;
		        case WallpaperStyle.Fit: // (Windows 7 and later)
		            key.SetValue(@"WallpaperStyle", "6");
		            key.SetValue(@"TileWallpaper", "0");
		            break;
		        case WallpaperStyle.Fill: // (Windows 7 and later)
		            key.SetValue(@"WallpaperStyle", "10");
		            key.SetValue(@"TileWallpaper", "0");
		            break;
		    }
		    key.Close();
	        string newpath = String.Format(@"{0}\Microsoft\Windows\Themes\{1}.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.GetFileNameWithoutExtension(path));
		    if(!System.IO.File.Exists(newpath)){
				System.IO.File.Copy(path, newpath);
		    }
		    if(!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, newpath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE)){ 
                throw new Win32Exception();
            }
	    }
	}
}