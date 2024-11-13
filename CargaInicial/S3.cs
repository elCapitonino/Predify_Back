using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Configuration;
using System.IO;
using Amazon.S3.Transfer;
using System.Net;

namespace CargaInicial
{
    class S3
    {

        public S3()
        {


        }

        private IAmazonS3 CreateClient()
        {
            Console.WriteLine(string.Format("User: {0}", ConfigurationManager.AppSettings["predify_s3_user"]));
            Console.WriteLine(string.Format("Secret: {0}", ConfigurationManager.AppSettings["predify_s3_secret"]));
            return new AmazonS3Client(ConfigurationManager.AppSettings["predify_s3_user"], ConfigurationManager.AppSettings["predify_s3_secret"], RegionEndpoint.USWest2);
        }

        public async Task<List<S3Object>> ListFilesAsync(string bucketName)
        {
            //string bucketName = "doc-example-bucket";
            WebRequest.DefaultWebProxy = null; // here
            using (IAmazonS3 client = CreateClient())
            {
                Console.WriteLine($"Listing objects stored in the bucket {bucketName}.");
                var files = await ListingObjectsAsync(client, bucketName);
                Console.WriteLine($"Total de arquivos: {files.Count}");

                //Console.WriteLine();
                //Console.WriteLine($"Digite o index do arquivo: de {0} até {files.Count - 1} ou vazio para cancelar.");
                //var index = Console.ReadLine();
                //int selIndex = 0;
                //if (index.Trim() == "" || !int.TryParse(index, out selIndex) || selIndex < 0 || selIndex >= files.Count)
                //{
                //    Console.WriteLine("Cancelado");
                //    return files;
                //}

                //await Download(client, files[selIndex], bucketName);



                return files;
            }
        }

        public async Task<string> DownloadAsync(S3Object s3Object, string bucketName, IAmazonS3 s3Client = null)
        {
            if (s3Client != null)
                return await DownloadingAsync(s3Object, bucketName, s3Client);
            else
            {
                using (IAmazonS3 client = CreateClient())
                    return await DownloadingAsync(s3Object, bucketName, client);
            }
        }


        private async Task<string> DownloadingAsync(S3Object s3Object, string bucketName, IAmazonS3 s3Client = null)
        {
            string s3FileName = "";
            if (s3Object.Size > 0)
            {

                // We have a file (not a directory) --> download it
                GetObjectResponse objData = s3Client.GetObject(
                    new GetObjectRequest() { BucketName = bucketName, Key = s3Object.Key });
                s3FileName = new FileInfo(s3Object.Key).Name;

                int count = 4096;
                byte[] buffer;  //new char[count];
                int offset = 0;
                int read;
                long totalBytes = 0;
                Console.Write("Downloading: ");
                int linePosL = Console.CursorLeft;
                int linePosT = Console.CursorTop;



                //var fileTransferUtility = new TransferUtility(s3Client);
                //await fileTransferUtility.DownloadAsync("d:\\" + s3FileName, bucketName, objData.Key);

                using (var f = new FileStream(s3FileName, FileMode.Create))
                {
                    using (var sw = new BinaryWriter(f))
                    {

                        using (var sr = new BinaryReader(objData.ResponseStream))
                        {
                            do
                            {
                                buffer = sr.ReadBytes(count);
                                if (buffer.Length > 0)
                                {
                                    sw.Write(buffer);
                                    totalBytes += buffer.Length;
                                    Console.SetCursorPosition(linePosL, linePosT);
                                    Console.Write($"{(totalBytes * 100F / objData.ContentLength).ToString("0.00")}%");
                                }
                            } while (buffer.Length == count);
                        }
                    }
                }

                Console.SetCursorPosition(linePosL, linePosT);
                Console.WriteLine("100.0%");
            }

            return s3FileName;
        }

        
        
        private async Task<List<S3Object>> ListingObjectsAsync(IAmazonS3 client, string bucketName)
        {
            List<S3Object> result = new List<S3Object>();
            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request()
                {
                    BucketName = bucketName,
                    MaxKeys = 5,
                };

                var response = new ListObjectsV2Response();
                int index = 0;

                do
                {
                    response = await client.ListObjectsV2Async(request);

                    response.S3Objects
                        .ForEach(obj =>
                        {
                            Console.WriteLine($"{index++} - {obj.Key,-35}{obj.LastModified.ToShortDateString(),20}{obj.Size,20}");
                            result.Add(obj);
                        });

                    // If the response is truncated, set the request ContinuationToken
                    // from the NextContinuationToken property of the response.
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
            }

            return result;
        }



    }
}
