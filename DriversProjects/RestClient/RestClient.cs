using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestApi
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    namespace HttpUtils
    {
        public class RestClient
        {
            public string EndPoint { get; set; }
            public HttpVerb Method { get; set; }
            public string ContentType { get; set; }
            public string PostData { get; set; }

            public RestClient()
            {
                EndPoint = "";
                Method = HttpVerb.GET;
                ContentType = "text/xml";
                PostData = "";
            }
            public RestClient(string endpoint)
            {
                EndPoint = endpoint;
                Method = HttpVerb.GET;
                ContentType = "text/xml";
                PostData = "";
            }
            public void Close()
            {

            }
            public RestClient(string endpoint, HttpVerb method)
            {
                EndPoint = endpoint;
                Method = method;
                ContentType = "text/xml";
                PostData = "";
            }

            public RestClient(string endpoint, HttpVerb method, string postData)
            {
                EndPoint = endpoint;
                Method = method;
                ContentType = "text/xml";
                PostData = postData;
            }

            public string MakeRequest()
            {
                return MakeRequest("");
            }

            public string MakeRequest(string parameters)
            {
                try
                {

                    var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);
                    request.Method = Method.ToString();
                    request.ContentLength = 0;
                    request.ContentType = ContentType;
                    request.Proxy = null;
                    request.ServicePoint.ConnectionLimit = 100;
                    request.Credentials = new NetworkCredential("admin", "admin");
                    
                    request.Timeout = 4000;

                    if (!string.IsNullOrEmpty(PostData) && (Method == HttpVerb.POST) || (Method == HttpVerb.PUT))
                    {
                        var encoding = new UTF8Encoding();

                        string replacement = PostData.Replace("\r\n", string.Empty);
                        //replacement = Regex.Replace(replacement, @"\s+", string.Empty);
                        replacement = string.Join("", replacement.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                        //var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(replacement);
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(replacement);
                        
                        request.ContentLength = bytes.Length;

                        using (var writeStream = request.GetRequestStream())
                        {
                            writeStream.Write(bytes, 0, bytes.Length);
                        }
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        var responseValue = string.Empty;

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                            throw new ApplicationException(message);
                        }

                        // grab the response
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (var reader = new StreamReader(responseStream))
                                {
                                    responseValue = reader.ReadToEnd();
                                }
                        }
                        
                        return responseValue;
                    }                
                     
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
           }            
 
        } // class

    }
}
