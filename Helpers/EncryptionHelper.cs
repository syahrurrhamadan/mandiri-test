namespace WebApi.Helpers;

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WebApi.Dto;

// custom exception class for throwing application specific exceptions 
// that can be caught and handled within the application
public class EncryptionHelper
{
    private readonly GeneralSettings _generalSettings;

    public EncryptionHelper(IOptions<GeneralSettings> generalSettings)
    {
        _generalSettings = generalSettings.Value;
    }

    public string DecryptData(string? encryptedData)
    {
        if (encryptedData == null)
            throw new ArgumentNullException(nameof(encryptedData));

        byte[] dataToDecrypt = Convert.FromBase64String(encryptedData);
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportFromPem(_generalSettings.PrivateKey.ToCharArray());
            byte[] decryptedBytes = rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}