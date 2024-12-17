using Microsoft.AspNetCore.Http;
using Orders.ClientHttp.Abstraction;
using Orders.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Orders.ClientHttp
{
    public class ClientHttp : IClientHttp
    {
        private readonly HttpClient _httpClient;

        public ClientHttp(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OrderReadDto?> ReadOrder(int id, CancellationToken cancellationToken = default)
        {
            var queryString = QueryString.Create(new Dictionary<string, string?>() {
                { "id", id.ToString(CultureInfo.InvariantCulture) }
            });

            var response = await _httpClient.GetAsync($"/Orders/ReadOrder{queryString}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<OrderReadDto?>(cancellationToken: cancellationToken);
        }
    }
}
