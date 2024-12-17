using Payments.ClientHttp.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.ClientHttp
{
    public class ClientHttp : IClientHttp
    {
        private readonly HttpClient _httpClient;
        public ClientHttp(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
