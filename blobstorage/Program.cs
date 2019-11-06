using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace blobstorage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Azure Blob Storage - Exemplo em .NET Core\n");
            
            ProcessAsync().GetAwaiter().GetResult();

            Console.WriteLine("Pressione qualquer tecla para sair.");
            Console.ReadLine();
        }

        private static async Task ProcessAsync()
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("CONNECT_STR_BLOB");

            // Verifique se a string de conexão é válida 
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // Se a string de conexão for válida, prossegue com as operações.

                #region Criar contêiner
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                //Criar um container chamado 'blobstorage' e contatena um GUID para garantir o nome unico.
                //Em um ambiente de produção pode utilizar o método CreateIfNotExistsAsync.
                CloudBlobContainer cloudBlobContainer = 
                    cloudBlobClient.GetContainerReference("blobstorage" + 
                        Guid.NewGuid().ToString());
                await cloudBlobContainer.CreateAsync();
                #endregion

                #region Definir permissão em um contêiner
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);
                #endregion

                #region Upload blobs
                // Cria um arquivo na diretório 'documentos' para realizar o upload 
                string diretorio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string nomeDocumento = "BlobStorage_" + Guid.NewGuid().ToString() + ".txt";
                string caminhoDoc = Path.Combine(diretorio, nomeDocumento);

                // Escreve um texto no arquivo.
                File.WriteAllText(caminhoDoc, "Teste upload blob!");

                // Get caminho do blob e faz o upload.
                // Usa o valor da variavel nomeDocumento para nomear o blob.
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(nomeDocumento);
                await cloudBlockBlob.UploadFromFileAsync(caminhoDoc);
                #endregion

                #region Download blobs                
                string diretorioDownload = caminhoDoc.Replace(".txt", "_DOWNLOADED.txt");
                Console.WriteLine("Downloading blob {0}", diretorioDownload);
                await cloudBlockBlob.DownloadToFileAsync(diretorioDownload, FileMode.Create);
                #endregion

            }
            else
            {    
                Console.WriteLine("Variavel de ambiente não definida ou inválida");
                Console.ReadLine();
            }
        }
    }
}
