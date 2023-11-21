// See https://aka.ms/new-console-template for more information

using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Where should I look for files? Enter the full location of the directive");

var directory = Console.ReadLine();
if (directory == null)
    return;

Console.WriteLine("What is the name of the S3 bucket where you want the images uploaded to?");
var bucket = Console.ReadLine();
if(bucket == null) 
    return;

Console.WriteLine("file types? delimited by comma");
var fileTypesStr = Console.ReadLine();
if(string.IsNullOrEmpty(fileTypesStr)) 
    return;

Console.WriteLine("Public read access? y / n");
var ynPublicReadAccess = Console.ReadLine();
if (ynPublicReadAccess != "y" && ynPublicReadAccess != "n")
    return;
    
var isPublicReadAccess = ynPublicReadAccess == "y";
var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();

if (Directory.Exists(directory))
{
    var fileTypes = fileTypesStr.Split(',');
    var files = new List<string>();
    foreach(var fileType in fileTypes)
    {
        var filesForFileType = Directory.GetFiles(directory, $"*.{fileType}").ToHashSet();
        files.AddRange(filesForFileType);
    }

    var s3Client = serviceProvider.GetRequiredService<IAmazonS3>();
    var awsBlobStore = new AwsBlobStorage(s3Client, bucket, isPublicReadAccess);
    foreach (var file in files)
    {
        var fileStream = File.OpenRead(file);
        var fileName = Path.GetFileName(file);
        var memStream = new MemoryStream();
        fileStream.CopyTo(memStream);

        var fileKey = ProvideBlobImageKey(fileName);
        await awsBlobStore.Add(fileKey, memStream);

        fileStream.Dispose();
    }
}

void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IAmazonS3, AmazonS3Client>();
    services.AddScoped<ITransferUtility, TransferUtility>();
}

string ProvideBlobImageKey(string fileName) => $"{DateTime.UtcNow:yyyy\\/\\MM\\/dd}_{fileName}";

