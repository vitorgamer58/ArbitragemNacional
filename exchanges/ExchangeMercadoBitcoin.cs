using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

    public class ExchangeMercadoBitcoin : ExchangeBase, IExchange
    {
        public static decimal balance_usdt = 0;

        public static decimal balance_btc = 0;
        public ExchangeMercadoBitcoin()
        {
            this.urlTicker = "https://api.binance.com/api/v1/ticker/24hr";
            this.key = "aa86c42e38411d231fe26550c7dc326e";
            this.secret = "e2e8f0928983b706558f37d6a9b9c0650bf6c309eb29362131305b11081519b8";
            this.lockQuantity = true;
            this.fee = 0.70m;
        }

        public decimal getFee()
        {
            return this.fee;
        }

        public bool isLockQuantity()
        {
            return this.lockQuantity;
        }

        public decimal getBalance(string pair)
        {
            pair = pair.Replace("BCH", "BCC");
            if (pair.ToUpper().Trim().IndexOf("BTC") >= 0)
                return balance_btc;
            if (pair.ToUpper().Trim().IndexOf("USDT") >= 0)
                return balance_usdt;

            return this.getBalance(this.getName(), pair);
        }

        public decimal[] getHighestBid(string pair, decimal amount)
        {
            try
            {

                String json = Http.get("https://www.mercadobitcoin.net/api/BTC/orderbook/");
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));


                decimal[] arrayValue = new decimal[2];
                arrayValue[0] = arrayValue[1] = 0;
                decimal amountBook = 0;
                decimal amountAux = 0;
                decimal total = 0;
                int lines = 0;

                foreach (var item in jCointaner["bids"])
                {
                    lines++;
                    amountBook += decimal.Parse(item[1].ToString().Replace(".", ","));

                    if (amount > amountBook)
                    {
                        total += decimal.Parse(item[1].ToString().Replace(".", ",")) * decimal.Parse(item[0].ToString().Replace(".", ","));
                        amountAux += decimal.Parse(item[1].ToString().Replace(".", ","));
                    }
                    else if (lines == 1)
                    {
                        arrayValue[0] = decimal.Parse(item[0].ToString().Replace(".", ","));
                        arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                        return arrayValue;
                    }
                    else
                        total += (amount - amountAux) * decimal.Parse(item[0].ToString().Replace(".", ","));

                    if (amountBook >= amount)
                    {
                        arrayValue[0] = (total / amount);
                        arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                        return arrayValue;
                    }
                }
            }
            catch
            {
            }
            return new decimal[2];
        }

        public string getKey()
        {
            return this.key;
        }

        public decimal getLastValue(string pair)
        {
            try
            {
                String json = Http.get("https://www.mercadobitcoin.net/api/BTC/ticker/");

                JContainer j = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

                return decimal.Parse(j["ticker"]["last"].ToString().Replace(".", ","), System.Globalization.NumberStyles.Float);
            }
            catch
            {
            }
            return 0;
        }

        public decimal[] getLowestAsk(string pair, decimal amount)
        {
            try
            {

                String json = Http.get("https://www.mercadobitcoin.net/api/BTC/orderbook/");
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));


                decimal[] arrayValue = new decimal[2];
                arrayValue[0] = arrayValue[1] = 0;
                decimal amountBook = 0;
                decimal amountAux = 0;
                decimal total = 0;
                int lines = 0;

                foreach (var item in jCointaner["asks"])
                {
                    lines++;
                    amountBook += decimal.Parse(item[1].ToString().Replace(".", ","));

                    if (amount > amountBook)
                    {
                        total += decimal.Parse(item[1].ToString().Replace(".", ",")) * decimal.Parse(item[0].ToString().Replace(".", ","));
                        amountAux += decimal.Parse(item[1].ToString().Replace(".", ","));
                    }
                    else if (lines == 1)
                    {
                        arrayValue[0] = decimal.Parse(item[0].ToString().Replace(".", ","));
                        arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                        return arrayValue;
                    }
                    else
                        total += (amount - amountAux) * decimal.Parse(item[0].ToString().Replace(".", ","));

                    if (amountBook >= amount)
                    {
                        arrayValue[0] = (total / amount);
                        arrayValue[1] = decimal.Parse(item[0].ToString().Replace(".", ","));
                        return arrayValue;
                    }
                }
            }
            catch
            {
            }
            return new decimal[2];
        }

        public void getMarket()
        {
            String json = Http.get(this.urlTicker).Replace("\"success\":true,\"message\":\"\",\"result\":", "").Replace("]}", "").Replace("{[", "");

            this.dataSource = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

        }

        public string getName()
        {
            return "MERCADOBITCOIN";
        }

        public OrderStatus getOrder(string idOrder)
        {
            throw new NotImplementedException();
        }

        public string getSecret()
        {
            return this.secret;
        }

        public void loadBalances()
        {
            throw new NotImplementedException();
        }


        public string post(String url, String parameters, String key, String secret)
        {
            try
            {
                // lock (objLock)
                {
                    Logger.log(url + parameters);

                    //System.Threading.Thread.Sleep(1000);                    
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var client = new RestClient("https://api.binance.com");

                    HMACSHA256 encryptor = new HMACSHA256();
                    encryptor.Key = Encoding.ASCII.GetBytes(this.getSecret());
                    String sign = BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(parameters))).Replace("-", "");
                    parameters += "&signature=" + sign;

                    var request = new RestRequest("/api/v3/order?" + parameters, Method.POST);
                    request.AddHeader("X-MBX-APIKEY", this.getKey());
                    var response = client.Execute(request);
                    Console.WriteLine(response.Content);
                    return response.Content.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.log("ERROR POST " + ex.Message + ex.StackTrace);
                return null;
            }
            finally
            {
            }
        }


        public Operation order(string type, string pair, decimal amount, decimal price, bool lockQuantity)
        {
            amount = this.fixAmount(amount);
            Operation operation = new Operation();
            operation.success = false;


            try
            {
                string response, tapiMetodo = "";
                if (type == "sell")
                    tapiMetodo = "place_sell_order";
                if (type == "buy")
                    tapiMetodo = "place_buy_order";
                MercadoBitcoinClient mercadoBitCoinClient = new MercadoBitcoinClient();

                String json = mercadoBitCoinClient.getRequestPrivate(tapiMetodo, "&coin_pair=BRLBTC&quantity=" + amount.ToString().Replace(",", ".") + "&limit_price=" + (Math.Round(price , 2)).ToString().Replace(",", "."));

                JContainer dt = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));


                if (json.IndexOf("true") >= 0)
                    operation.success = true;
                operation.json = json;
                return operation;
            }
            catch
            {
                return null;
            }
        }

        public string getBalances()
        {
            try
            {
                string response, tapiMetodo;
                tapiMetodo = "get_account_info";
                MercadoBitcoinClient mercadoBitCoinClient = new MercadoBitcoinClient();
                mercadoBitCoinClient.DefinirMetodo(Method.POST, tapiMetodo);
                mercadoBitCoinClient.Autenticar();
                response = mercadoBitCoinClient.Requisitar();
                String json = response;
                JContainer dt = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

                foreach (var item in dt["response_data"]["balance"])
                {
                    JToken j = (JToken)item;
                    String[] array = j.ToString().Split('\"');
                    if (array[1] == "brl")
                    {
                        decimal value = decimal.Parse(array[5].ToString().ToString().Replace(".", ","));

                        balance_usdt = value;
                    }
                    if (array[1] == "btc")
                    {
                        decimal value = decimal.Parse(array[5].ToString().ToString().Replace(".", ","));

                        balance_btc = value;

                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                Logger.log("ERROR GET " + ex.Message + ex.StackTrace);
                return null;
            }
            finally
            {
            }


        }


        public decimal getStepSize(String pair)
        {
            if (pair.IndexOf("BCH") >= 0 || pair.IndexOf("BCC") >= 0)
                return 0.00001000m;
            if (pair.IndexOf("LTC") >= 0)
                return 0.00001000m;
            if (pair.IndexOf("ETH") >= 0)
                return 0.00001000m;
            if (pair.IndexOf("BTC") >= 0)
                return 0.00000100m;
            else
                return 0;
        }

        public decimal calculateAmount(decimal amount, string pair)
        {
            pair = pair.Replace("BCH", "BCC");
            decimal stepSize = getStepSize(pair);
            decimal rest = amount % stepSize;
            if (rest == 0)
                return amount;
            else
            {
                amount = Convert.ToInt32(amount / stepSize) * stepSize;
                return amount;
            }
        }



    }
