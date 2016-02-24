/*
 * Created by SharpDevelop.
 * User: Frosty
 * Date: 2/23/2016
 * Time: 9:51 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SpotlightDesktopWallpaper
{
	public sealed class NotificationIcon
	{
		private NotifyIcon notifyIcon;
		private ContextMenu notificationMenu;
		private RegistryUtils.RegistryMonitor notificationMonitor;
		
		#region Initialize icon and menu
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			notificationMonitor = new RegistryUtils.RegistryMonitor(RegistryHive.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative\\");
			notificationMonitor.RegChanged += OnRegChanged;
			notificationMonitor.Start();
			notifyIcon.DoubleClick += IconDoubleClick;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
			notifyIcon.ContextMenu = notificationMenu;
		}
		
		private MenuItem[] InitializeMenu()
		{
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("About", menuAboutClick),
				new MenuItem("Exit", menuExitClick)
			};
			return menu;
		}
		#endregion
		
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			bool isFirstInstance;
			// Please use a unique name for the mutex to prevent conflicts with other programs
			using (Mutex mtxLockscreenToWallpaper = new Mutex(true, "Lockscreen_to_Wallpaper", out isFirstInstance)) {
				if (isFirstInstance) {
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					Application.Run(new wallpaperApplicationContext());
					notificationIcon.notifyIcon.Dispose();
				} else {
					MessageBox.Show("Lockscreen to Wallpaper is already running!");
				}
			} // releases the Mutex
		}
		#endregion
		
		#region Event Handlers
		private void menuAboutClick(object sender, EventArgs e)
		{
			const string aboutBoxCaption = " About SpotlightDesktopWallpaper";
			const string aboutBoxMessage = "Copyright 2016 Nathan Waters (frostyfire03530)\n\nLicensed under the Apache License, Version 2.0 (the \"License\");\nyou may not use this file except in compliance with the License.\nYou may obtain a copy of the License at\n\nhttp://www.apache.org/licenses/LICENSE-2.0\n\nUnless required by applicable law or agreed to in writing, software\ndistributed under the License is distributed on an \"AS IS\" BASIS,\nWITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.\nSee the License for the specific language governing permissions and\nlimitations under the License.Copyright 2016\n\nHome Page: http://nathanjwaters.com\n\nOriginal sources: https://code.msdn.microsoft.com/windowsapps/CSSetDesktopWallpaper-2107409c/sourcecode?fileId=21700&pathId=734742078\nhttp://www.codeproject.com/Articles/4502/RegistryMonitor-a-NET-wrapper-class-for-RegNotifyC\nhttp://stackoverflow.com/questions/18232972/how-to-read-value-of-a-registry-key-c-sharp\n\nWritten in C# using SharpDevelop 5.1.0\nBuild: 1.00";
			MessageBox.Show(aboutBoxMessage,aboutBoxCaption);
		}
		
		private void menuExitClick(object sender, EventArgs e)
		{
			notificationMonitor.Stop();
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e)
		{
			MessageBox.Show("Windows Spotlight is being monitored for a change in the background to load into the desktop","SpotlightDesktopWallpaper");
		}
		
		private void OnRegChanged(object sender, EventArgs e)
		{
			//var timeStamp = DateTime.Now;
			//String output = timeStamp + ": OnRegChanged fired!";
			//System.Diagnostics.Debug.Write(output);
			ApplicationContext wallpaperApplicationContext = new wallpaperApplicationContext();
		}
		#endregion
	}
	public class wallpaperApplicationContext : ApplicationContext
	{
		public wallpaperApplicationContext()
		{
			try
			{
			    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative"))
			    {
			        if (key != null)
			        {
		        	    Object o = key.GetValue("LandscapeAssetPath");
						string landscapeAssetPath;
						landscapeAssetPath = o.ToString();
		                if (o != null && landscapeAssetPath!="")
		                {
		                	Wallpaper.Set(landscapeAssetPath, Wallpaper.WallpaperStyle.Fill);
		                }
			        }
			    }
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.ToString());
			}
		}
	}
}
