using Microsoft.AspNetCore.Http;
using Registry.ClientHttp.Abstraction;
using Registry.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Registry.ClientHttp
{
    public class ClientHttp : IClientHttp
    {
        private readonly HttpClient _httpClient;

        public ClientHttp(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CustomerReadDto?> ReadCustomer(int id, CancellationToken cancellationToken = default)
        {
            var queryString = QueryString.Create(new Dictionary<string, string?>() {
                { "id", id.ToString(CultureInfo.InvariantCulture) }
            });

            var response = await _httpClient.GetAsync($"/Registry/ReadCustomer{queryString}", cancellationToken);
            return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<CustomerReadDto?>(cancellationToken: cancellationToken);
        }

        public async Task<SupplierReadDto?> ReadSupplier(int id, CancellationToken cancellationToken = default)
        {
            var queryString = QueryString.Create(new Dictionary<string, string?>() {
                { "id", id.ToString(CultureInfo.InvariantCulture) }
            });

            var response = await _httpClient.GetAsync($"/Registry/ReadSupplier{queryString}", cancellationToken);
            return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierReadDto?>(cancellationToken: cancellationToken);
        }
    }
}
