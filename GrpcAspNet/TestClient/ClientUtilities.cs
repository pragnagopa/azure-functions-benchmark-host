using Google.Protobuf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using TestGrpc.Messages;
using System.Net.Http;
using Newtonsoft.Json;

namespace TestClient
{
    public static class ClientUtilities
    {
        public static TypedData ToRpc(this object value)
        {
            TypedData typedData = new TypedData();

            if (value == null)
            {
                return typedData;
            }

            if (value is byte[] arr)
            {
                typedData.Bytes = ByteString.CopyFrom(arr);
            }
            else if (value is JObject jobj)
            {
                typedData.Json = jobj.ToString();
            }
            else if (value is string str)
            {
                typedData.String = str;
            }
            else if (value is HttpResponseMessage response)
            {
                TypedData responseBody = new TypedData();
                responseBody.String = "Hello World";
                var http = new RpcHttp()
                {
                    StatusCode = response.StatusCode.ToString(),
                    Body = responseBody
                };
                typedData.Http = http;

                foreach (var pair in response.Headers)
                {
                    http.Headers.Add(pair.Key.ToLowerInvariant(), pair.Value.ToString());
                }
            }
            else
            {
                // attempt POCO / array of pocos
                try
                {
                    typedData.Json = JsonConvert.SerializeObject(value);
                }
                catch
                {
                    typedData.String = value.ToString();
                }
            }
            return typedData;
        }
    }
}
