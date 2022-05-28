
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Saro.Net.Http
{
    public class HttpHelper
    {
        public static void UploadFiles(string url, IList<string> localFiles, IList<string> remoteFiles, Action<string, int, int> onProgress = null)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = false;
            //request.AllowWriteStreamBuffering = false;
            request.Credentials = CredentialCache.DefaultCredentials;

            using (var requestStream = request.GetRequestStream())
            {
                const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                byte[] buffer = new byte[4096];
                for (int i = 0; i < localFiles.Count; i++)
                {
                    string filePath = localFiles[i];
                    string fileName = remoteFiles[i];

                    onProgress?.Invoke(filePath, i, localFiles.Count);

                    requestStream.Write(boundarybytes, 0, boundarybytes.Length);

                    string header = string.Format(headerTemplate, $"file-{i}", fileName, "*/*");
                    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    requestStream.Write(headerbytes, 0, headerbytes.Length);

                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        int count = 0;
                        long total = 0;
                        while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            requestStream.Write(buffer, 0, count);
                            total += count;
                        }
                    }
                }

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                requestStream.Write(trailer, 0, trailer.Length);
            }

            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var stringReader = new StreamReader(responseStream))
                    {
                        var retString = stringReader.ReadToEnd();
#if UNITY_EDITOR
                        Log.INFO("HTTP", retString);
#else
                        System.Console.WriteLine(retString);
#endif
                    }
                }
            }
        }
    }
}