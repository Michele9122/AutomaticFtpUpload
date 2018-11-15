using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace AutomaticFtpUpload
{
    class Program
    {
        public static Dictionary<FileSystemWatcher, UploadConfig> dict;
        public static Dictionary<FileSystemWatcher, UploadConfig> queueUpload;
        static void Main(string[] args)
        {
            dict = new Dictionary<FileSystemWatcher, UploadConfig>();
            queueUpload = new Dictionary<FileSystemWatcher, UploadConfig>();
            string json = File.ReadAllText("config.json");
            List <UploadConfig> configs2 = JsonConvert.DeserializeObject<List<UploadConfig>>(json);
            Console.WriteLine(configs2);
            configs2.ForEach(i => Console.WriteLine("{0}\r\n", i));
            //all'inizializzazione ci potrebbero essere file non sincronizzati
            //ma anche in un momento arbitrario...
            //esempio...connection loss, impossibilità di inviare l'allarme
            foreach (var item in configs2)
            {
                FileSystemWatcher f = new FileSystemWatcher(item.LocalDir);
                f.Created += FileCreated;
                dict.Add(f, item);
                f.EnableRaisingEvents = true;
            }

            //Console.ReadKey(); //serve un modo migliore per tenere in vita il processo...
            Console.ReadLine();
        }
        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File Creato: " + e.FullPath);
            UploadConfig u = dict[(FileSystemWatcher)sender];
            bool connectionActive = true;
            Thread.Sleep(300);//prevent using a locked file
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(u.FTPUsername, u.FTPPassword);
                try
                {
                    client.UploadFile("ftp://" + u.FTPHost + "/" + u.RemoteDir + e.Name, WebRequestMethods.Ftp.UploadFile, e.FullPath);
                    File.Delete(e.FullPath);
                    while(connectionActive == false)
                    {
                        

                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ConnectFailure)
                    {
                        Console.WriteLine("ERRORE CONNESSIONE");
                        connectionActive = false;
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine("Eccezione ArgumentNullException");
                    Console.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Eccezione non prevista");
                    Console.WriteLine(ex);
                }
            }
        }
        private static void UploadFile(UploadConfig u, string filepath)
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(u.FTPUsername, u.FTPPassword);
                try
                {
                    //client.UploadFile("ftp://" + u.FTPHost + "/" + u.RemoteDir + e.Name, WebRequestMethods.Ftp.UploadFile, e.FullPath);
                    //rimozione del file su successo...
                    //gestire l'insuccesso
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Eccezione WebException");
                    Console.WriteLine(ex);
                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine("Eccezione ArgumentNullException");
                    Console.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Eccezione non prevista");
                    Console.WriteLine(ex);
                }
            }
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
    }
    class UploadConfig
    {
        public string LocalDir { get; set; }
        public string RemoteDir { get; set; }
        public string FTPHost { get; set; }
        public string FTPUsername { get; set; }
        public string FTPPassword { get; set; }
        public override string ToString() {
            return "LocalDir: " + LocalDir + "\r\nRemoteDir: "+RemoteDir+ "\r\nFTPHost: " + FTPHost+ "\r\nFTPUsername: " + FTPUsername+ "\r\nFTPPassword: " + FTPPassword+"";
        }
    }
}
