using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Decorator;

public class DataSourceDecorator : IDataSource
{
    protected IDataSource _dataSource { get; set; }

    public DataSourceDecorator(IDataSource dataSource)
    {
        _dataSource = dataSource;
    }


    public virtual void WriteData(string data)
    {
        _dataSource.WriteData(data);
    }

    public virtual string? ReadData()
    {
        return _dataSource.ReadData();
    }
}

public class DataSourceCompressionDecorator : DataSourceDecorator
{
    public DataSourceCompressionDecorator(IDataSource dataSource) : base(dataSource)
    {
    }

    public override void WriteData(string data)
    {


        base.WriteData(Compress(data));
    }

    public override string? ReadData() => Decompress(base.ReadData());

    public static string Compress(string uncompressedString)
    {
        byte[] compressedBytes;

        using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
        {
            using var compressedStream = new MemoryStream();

            using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
            {
                uncompressedStream.CopyTo(compressorStream);
            }

            compressedBytes = compressedStream.ToArray();
        }

        return Convert.ToBase64String(compressedBytes);
    }


    public static string Decompress(string? compressedString)
    {
        byte[] decompressedBytes;

        var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

        using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
        {
            using (var decompressedStream = new MemoryStream())
            {
                decompressorStream.CopyTo(decompressedStream);

                decompressedBytes = decompressedStream.ToArray();
            }
        }

        return Encoding.UTF8.GetString(decompressedBytes);
    }
}

// firlatdigim yer:https://qawithexperts.com/article/c-sharp/encrypt-password-decrypt-it-c-console-application-example/169
public class DataSourceEncryptionDecorator : DataSourceDecorator
{
    public DataSourceEncryptionDecorator(IDataSource dataSource) : base(dataSource)
    {
    }

    public override void WriteData(string data)
    {
        string encryptedData = EncryptPlainTextToCipherText(data);
        base.WriteData(encryptedData);
    }

    public override string? ReadData()
    {
        return DecryptCipherTextToPlainText(base.ReadData());
    }

    private const string SecurityKey = "ComplexKey";




    public static string EncryptPlainTextToCipherText(string PlainText)
    {
       
        byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);

        MD5CryptoServiceProvider objMD5CryptoService = new();
        byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
        objMD5CryptoService.Clear();

        var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
        objTripleDESCryptoService.Key = securityKeyArray;
        objTripleDESCryptoService.Mode = CipherMode.ECB;
        objTripleDESCryptoService.Padding = PaddingMode.PKCS7;


        var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
        byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
        objTripleDESCryptoService.Clear();
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public static string DecryptCipherTextToPlainText(string CipherText)
    {
        byte[] toEncryptArray = Convert.FromBase64String(CipherText);
        MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();

        byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
        objMD5CryptoService.Clear();

        var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
        objTripleDESCryptoService.Key = securityKeyArray;
        objTripleDESCryptoService.Mode = CipherMode.ECB;
        objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
        byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        objTripleDESCryptoService.Clear();

        return Encoding.UTF8.GetString(resultArray);
    }

}


public class FileDataSource : IDataSource
{
    private string _filePath;

    public FileDataSource(string filePath) => _filePath = filePath;
    public string? ReadData()
    {
        FileInfo file = new FileInfo(_filePath);

        if (file.Exists)
            return File.ReadAllText(file.FullName);

        return null;
    }

    public void WriteData(string data) => File.WriteAllText(_filePath, data);

    public string GetFilePath() => _filePath;
}