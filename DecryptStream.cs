public static void DecryptStream(Stream inputStream, Stream outputStream, Stream privateKeyStream, string password)
{
    if (inputStream == null || outputStream == null || privateKeyStream == null)
    {
        throw new ArgumentNullException("Streams must not be null.");
    }

    try
    {
        Stream decoderStream = PgpUtilities.GetDecoderStream(inputStream);
        PgpObjectFactory pgpObjectFactory = new PgpObjectFactory(decoderStream);
        
        if (!(pgpObjectFactory.NextPgpObject() is PgpEncryptedDataList encryptedDataList))
        {
            encryptedDataList = (PgpEncryptedDataList)pgpObjectFactory.NextPgpObject();
        }

        PgpPrivateKey privKey = null;
        PgpPublicKeyEncryptedData keyEncryptedData = null;
        PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

        foreach (PgpPublicKeyEncryptedData encryptedDataObject in encryptedDataList.GetEncryptedDataObjects())
        {
            privKey = PgpHelper.FindSecretKey(pgpSec, encryptedDataObject.KeyId, password);
            if (privKey != null)
            {
                keyEncryptedData = encryptedDataObject;
                break;
            }
        }

        if (privKey == null)
        {
            throw new ArgumentException("Secret key for message not found.");
        }

        PgpObject pgpObject = new PgpObjectFactory(keyEncryptedData.GetDataStream(privKey)).NextPgpObject();

        if (pgpObject is PgpCompressedData pgpCompressedData)
        {
            pgpObject = new PgpObjectFactory(pgpCompressedData.GetDataStream()).NextPgpObject();
        }

        if (pgpObject is PgpLiteralData pgpLiteralData)
        {
            Streams.PipeAll(pgpLiteralData.GetInputStream(), outputStream);
            outputStream.Position = 0L;

            if (keyEncryptedData.IsIntegrityProtected() && !keyEncryptedData.Verify())
            {
                throw new Exception("Failed to verify data integrity.");
            }
        }
        else if (pgpObject is PgpOnePassSignatureList)
        {
            throw new PgpException("Encrypted message contains a signed message - not literal data.");
        }
        else
        {
            throw new PgpException("Message is not a simple encrypted file - type unknown.");
        }
    }
    catch (PgpException ex)
    {
        throw new Exception("Failed to decrypt data: " + ex.Message, ex);
    }
}