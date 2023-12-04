using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.IO;

namespace ConfigCreater
{
    public class Sender
    {
        private string host = "";
        private string username = "root";
        private string password = "wirenboard";

        private string _remoteDirectory = "/root/wk/measure_module/Sensors/ConfigFolder";

        public string Host { get => host; set => host = value; }
        public string RemoteDirectory { get => _remoteDirectory; set => _remoteDirectory = value; }

        public bool Send(FileStream fileStream, string fileName)
        {
            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();
                    
                    sftp.UploadFile(fileStream, _remoteDirectory + "/" + fileName);

                    sftp.Disconnect();
                }
                catch (Exception er)
                {
                    Console.WriteLine("An exception has been caught " + er.ToString());
                    return false;
                }
                return true;
            }
        }
    }
}
