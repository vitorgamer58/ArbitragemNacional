using System;
using System.Security.Cryptography;
using System.Text;


public class ClientBase
{
    public ClientBase()
    {
    }

    public string CriarAssinaturaHMACSHA512(string mensagem, string segredo)
    {
        return ConverterByteArrayParaString(AssinarMensagemComHMACSHA512(segredo, StringToByteArray(mensagem)));
    }

    public static byte[] AssinarMensagemComHMACSHA512(string key, byte[] data)
    {
        var hashMaker = new HMACSHA512(Encoding.ASCII.GetBytes(key));
        return hashMaker.ComputeHash(data);
    }

    public static byte[] StringToByteArray(string str)
    {
        return Encoding.ASCII.GetBytes(str);
    }

    public static string ConverterByteArrayParaString(byte[] hash)
    {
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
