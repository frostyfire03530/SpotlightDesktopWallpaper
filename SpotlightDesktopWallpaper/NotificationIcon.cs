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

namespace SpotlightDesktopWallpaper{
	public sealed class NotificationIcon{
		private NotifyIcon notifyIcon;
		private ContextMenu notificationMenu;
		private RegistryUtils.RegistryMonitor notificationMonitor;
		#region Initialize icon and menu
		public NotificationIcon(){
			
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			RegistryKey autoStart = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if(autoStart.GetValue("SpotlightDesktopWallpaper") != null){
			   	(notificationMenu.MenuItems[2]).Checked = true;
			}
			RegistryKey saveWallpaper = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpotlightDesktopWallpaper",true);
			if(saveWallpaper == null){
				Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpotlightDesktopWallpaper");
			}
			else if(saveWallpaper.GetValue("SaveWallpaper") != null){
			   	(notificationMenu.MenuItems[1]).Checked = true;
			}
			String registryPath = getRegistryPath();//"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative\\"
			if (registryPath.Contains("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative\\")){
				notificationMonitor = new RegistryUtils.RegistryMonitor(RegistryHive.CurrentUser, registryPath);
			}
			else{
				notificationMonitor = new RegistryUtils.RegistryMonitor(RegistryHive.LocalMachine, registryPath);
			}
			notificationMonitor.RegChanged += OnRegChanged;
			notificationMonitor.Start();
			notifyIcon.DoubleClick += IconDoubleClick;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
			notifyIcon.ContextMenu = notificationMenu;
		}
		private MenuItem[] InitializeMenu(){
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("About", menuAboutClick),
				new MenuItem("Save to Pictures", menuSaveClick),
				new MenuItem("Start when Windows starts", menuStartClick),
				new MenuItem("Exit", menuExitClick)
			};
			return menu;
		}
		#endregion
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static string getLandscapePath(){
			string landscapeAssetPath = "";
			string landscapeAssetPathNew = "";
			string landscapeAssetPathNewSub = "";
			string subKeyNamePath = "";
			try {
				using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				using (RegistryKey key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative", true)){
			       	if (key != null){
						foreach(string subKey in key.GetSubKeyNames()){
							RegistryKey subKeyNames = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative\\" + subKey + "\\", true);
							if(subKeyNames.ValueCount > 4){
								subKeyNamePath = subKey;
								Object o = subKeyNames.GetValue("LandscapeAssetPath");
								landscapeAssetPathNew = o.ToString();
								break;
							}
						}
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.Write(ex.ToString());
			}
			try {
				using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				using (RegistryKey key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative\\" + subKeyNamePath + "\\", true)){
					if (key != null){
						String[] subKeyNamesArray = key.GetSubKeyNames();
						int subKeyNamesArrayLastIndex = subKeyNamesArray.Length - 1;
						RegistryKey subKeyNames = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative\\" + subKeyNamePath + "\\" + subKeyNamesArray[subKeyNamesArrayLastIndex] + "\\", true);
						if(subKeyNames.ValueCount > 3){
							Object o = subKeyNames.GetValue("landscapeImage");
							landscapeAssetPathNewSub = o.ToString();
							return landscapeAssetPathNewSub;
						}
						else{
							return landscapeAssetPathNew;
						}
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.Write(ex.ToString());
			}
			try{
			    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative")){
			        if (key != null){
		        	    Object o = key.GetValue("LandscapeAssetPath");
						landscapeAssetPath = o.ToString();
						return landscapeAssetPath;
			        }
			    }
			}
			catch (Exception ex){
				System.Diagnostics.Debug.Write(ex.ToString());
			}
			return "";
		}
		public static string getRegistryPath(){
			string registryPath = "";
			string registryPathNew = "";
			try {
				using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				using (RegistryKey key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative", true)){
			       	if (key != null){
						foreach(string subKey in key.GetSubKeyNames()){
							RegistryKey subKeyNames = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative\\" + subKey + "\\", true);
							if(subKeyNames.ValueCount > 4){
								registryPathNew = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\Creative\\" + subKey + "\\";
								return registryPathNew;
							}
						}
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.Write(ex.ToString());
			}
			try{
			    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative")){
			        if (key != null){
						registryPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Lock Screen\\Creative";
						return registryPath;
			        }
			    }
			}
			catch (Exception ex){
				System.Diagnostics.Debug.Write(ex.ToString());
			}
			return "";
		}
		public static void Main(string[] args){
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			bool isFirstInstance;
			// Please use a unique name for the mutex to prevent conflicts with other programs
			using (Mutex mtxSpotlightDesktopWallpaper = new Mutex(true, "SpotlightDesktopWallpaper", out isFirstInstance)) {
				if(isFirstInstance){
					MenuItem saveWallpaperItem = new MenuItem();
					saveWallpaperItem.Checked = false;
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					RegistryKey saveWallpaper = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpotlightDesktopWallpaper",true);
					if(saveWallpaper.GetValue("SaveWallpaper") != null){
						saveWallpaperItem.Checked = true;
					}
					ApplicationContext wallpaperApplicationContext = new wallpaperApplicationContext(saveWallpaperItem);
					Application.Run();
					notificationIcon.notifyIcon.Dispose();
				} 
				else {
					MessageBox.Show("SpotlightDesktopWallpaper is already running!","SpotlightDesktopWallpaper");
				}
			} // releases the Mutex
		}
		#endregion
		#region Event Handlers
		private void menuAboutClick(object sender, EventArgs e){
			const string aboutBoxCaption = " About SpotlightDesktopWallpaper";
			const string aboutBoxMessage = "Copyright 2016 Nathan Waters (frostyfire03530)\n\nLicensed under the Apache License, Version 2.0 (the \"License\");\nyou may not use this file except in compliance with the License.\nYou may obtain a copy of the License at\n\nhttp://www.apache.org/licenses/LICENSE-2.0\n\nUnless required by applicable law or agreed to in writing, software\ndistributed under the License is distributed on an \"AS IS\" BASIS,\nWITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.\nSee the License for the specific language governing permissions and\nlimitations under the License.Copyright 2016\n\nHome Page: http://nathanjwaters.com\n\nOriginal sources: https://code.msdn.microsoft.com/windowsapps/CSSetDesktopWallpaper-2107409c/sourcecode?fileId=21700&pathId=734742078\nhttp://www.codeproject.com/Articles/4502/RegistryMonitor-a-NET-wrapper-class-for-RegNotifyC\nhttp://stackoverflow.com/questions/18232972/how-to-read-value-of-a-registry-key-c-sharp\nhttp://stackoverflow.com/questions/674628/how-do-i-set-a-program-to-launch-at-startup\n\nWritten in C# using SharpDevelop 5.1.0\nBuild: 1.0.6";
			MessageBox.Show(aboutBoxMessage,aboutBoxCaption);
		}
		private void menuSaveClick(object sender, EventArgs e){
			if((notificationMenu.MenuItems[1]).Checked == false){
				(notificationMenu.MenuItems[1]).Checked = true;
				SetSave();
			}
			else{
				(notificationMenu.MenuItems[1]).Checked = false;
				SetSave();
			}
		}
		private void menuStartClick(object sender, EventArgs e){
			if((notificationMenu.MenuItems[2]).Checked == false){
				(notificationMenu.MenuItems[2]).Checked = true;
				SetStartup();
			}
			else{
				(notificationMenu.MenuItems[2]).Checked = false;
				SetStartup();
			}
		}
		private void menuExitClick(object sender, EventArgs e){
			notificationMonitor.Stop();
			Application.Exit();
		}
		private void IconDoubleClick(object sender, EventArgs e){
			MessageBox.Show("Windows Spotlight is being monitored for a change in the background to load into the desktop","SpotlightDesktopWallpaper");
		}
		private void OnRegChanged(object sender, EventArgs e){
			ApplicationContext wallpaperApplicationContext = new wallpaperApplicationContext((notificationMenu.MenuItems[1]));
		}
		private void SetSave(){
	        RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpotlightDesktopWallpaper", true);
	        if((notificationMenu.MenuItems[1]).Checked){
	        	rk.SetValue("SaveWallpaper", "true");
	        }
	        else{
	        	rk.DeleteValue("SaveWallpaper",false);
	        }
	    }
		private void SetStartup(){
	        RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
	        if((notificationMenu.MenuItems[2]).Checked){
	        	rk.SetValue("SpotlightDesktopWallpaper", Application.ExecutablePath.ToString());
	        }
	        else{
	        	rk.DeleteValue("SpotlightDesktopWallpaper",false);
	        }
	    }
		#endregion
	}

	public class wallpaperApplicationContext : ApplicationContext{
		public wallpaperApplicationContext(MenuItem saveWallpaper){
			try{
				//getLandscapePath newPath = new getLandscapePath();
				String landscapeAssetPath = SpotlightDesktopWallpaper.NotificationIcon.getLandscapePath();//newPath.ToString();
                if (landscapeAssetPath!=""){
                	Wallpaper.Set(landscapeAssetPath, Wallpaper.WallpaperStyle.Fill);
                	if(saveWallpaper.Checked == true){
                	   	Wallpaper.Save(landscapeAssetPath);
                	}
                }
			}
			catch (Exception ex){
				System.Diagnostics.Debug.Write(ex.ToString());
			}
		}
	}
}