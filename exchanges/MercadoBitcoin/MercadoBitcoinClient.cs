using RestSharp;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

public class MercadoBitcoinClient : ClientBase
{
    private readonly string apiSegredo, apiId, dominio, esqueletoMensagem, path, enderecoAPI;
    private Int64 nonce;
    private string url;
    private Method method;
    private object trava = new object();
    private RestRequest request;

    public MercadoBitcoinClient(String param = "")
    {
        apiSegredo = "";
        apiId = "";
        dominio = "https://www.mercadobitcoin.net";
        path = "/tapi/v3/";
        esqueletoMensagem = "tapi_method={0}&tapi_nonce={1}" + param;
        enderecoAPI = string.Concat(dominio, path);
        nonce = UnixTimeStampUtc();
    }

    public void DefinirMetodo(Method method, string url)
    {
        this.method = method;
        this.url = url;
    }

    public void Autenticar()
    {
        lock (trava)
        {
            string mensagemAssinadaHCMAC;

            request = new RestRequest();

            request.AddHeader("TAPI-ID", apiId);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            mensagemAssinadaHCMAC = CriarTAPIMAC(url);
            request.AddHeader("TAPI-MAC", mensagemAssinadaHCMAC);
        }
    }


    const string _REQUEST_HOST = "https://www.mercadobitcoin.net";
    const string _REQUEST_PATH = "/tapi/v3/";

    private string cripParametersSign(string pParamentersCallback)
    {
        string _sign = string.Empty;
        byte[] _signByte;

        try
        {
            HMACSHA512 _crip;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            _crip = new HMACSHA512(encoding.GetBytes(this.apiSegredo));
            _signByte = _crip.ComputeHash(encoding.GetBytes(string.Format("{0}?{1}", _REQUEST_PATH, pParamentersCallback)));

            StringBuilder _sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int _i = 0; _i < _signByte.Length; _i++)
            {
                _sBuilder.Append(_signByte[_i].ToString("x2"));
            }

            // Return the hexadecimal string.
            _sign = _sBuilder.ToString();

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }

        return _sign;
    }

    private string bindParmeters(String pMethod, string pParameters)
    {
        string _parameters = string.Empty;
        string _method = string.Empty;

        string _bodyText = string.Empty;

        try
        {

            _method = pMethod;
            //Calcula a hora que está sendo feito o post para ser validado
            //no servidor da API do Mercado Bitcoin para não ter 
            
            _bodyText = "tapi_method={0}&tapi_nonce={1}{2}";
            _bodyText = string.Format(_bodyText, _method, nonce, pParameters);
            _parameters = _bodyText;

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }

        return _parameters;
    }

    public string getRequestPrivate(String method, string pParameters)
    {
        string _sign = string.Empty;
        byte[] _body = null;
        string _return = string.Empty;
        string _MB_TAPI_ID = this.apiId;

        try
        {

            nonce = UnixTimeStampUtc();
            //Criando o bind dos parâmentros que serão passados para o API
            //e convertendo em ByteArray para populado no corpo do Request 
            //para o Server
            string _paramenterText = bindParmeters(method, pParameters);
            _body = Encoding.UTF8.GetBytes(_paramenterText);

            //Chamando função que criptografará os parâmentros a serem enviados
            _sign = cripParametersSign(_paramenterText);

            //Criando Metodo de Request para o Servidor do Mercado Bitcoin
            WebRequest request = null;
            request = WebRequest.Create(_REQUEST_HOST + _REQUEST_PATH);

            request.Method = "POST";
            request.Headers.Add("tapi-id", _MB_TAPI_ID);
            request.Headers.Add("tapi-mac", _sign);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = _body.Length;
            request.Timeout = 360000;

            //Escrevendo parâmentros no corpo do Request para serem enviados a API
            Stream _req = request.GetRequestStream();
            _req.Write(_body, 0, _body.Length);
            _req.Close();

            //Pegando retorno do servidor
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();

            //Convertendo Stream de retorno em texto para 
            //Texto de retorno será um JSON 
            using (StreamReader reader = new StreamReader(dataStream))
                _return = reader.ReadToEnd();

            //Liberando objetos para o Coletor de Lixo
            dataStream.Close();
            dataStream.Dispose();
            response.Close();
            response.Dispose();

        }
        catch (Exception ex)
        {

            _return = "";
        }

        return _return;
    }
    public string Requisitar()
    {        
        request.AddParameter("tapi_method", url);
        request.AddParameter("tapi_nonce", nonce);

        request.Method = method;

        var client = new RestClient(enderecoAPI);
        var response = client.Execute(request);

        return response.Content;
    }

    private string CriarTAPIMAC(string metodo)
    {
        string tapiMac, corpoFormatado, tapiMACAssinadaHCMAC;

        
        corpoFormatado = string.Format(esqueletoMensagem, metodo, nonce);
        tapiMac = string.Format("{0}?{1}", path, corpoFormatado);

        tapiMACAssinadaHCMAC = CriarAssinaturaHMACSHA512(tapiMac, apiSegredo);
        return tapiMACAssinadaHCMAC;
    }

    private static Int64 UnixTimeStampUtc()
    {        
        var horaAtual = DateTime.Now;
        var dt = horaAtual.ToUniversalTime();
        var unixEpoch = new DateTime(1970, 1, 1);

        return (Int64)(dt.Subtract(unixEpoch)).TotalMilliseconds;

    }
}
