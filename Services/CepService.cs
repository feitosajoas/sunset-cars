namespace SunsetCars.Services
{
    public interface ICepService
    {
        Task<CepResponse?> GetAddressByCepAsync(string cep);
    }

    public class CepResponse
    {
        public string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Ibge { get; set; } = string.Empty;
        public string Gia { get; set; } = string.Empty;
        public string Ddd { get; set; } = string.Empty;
        public string Siafi { get; set; } = string.Empty;
        public bool Erro { get; set; }
    }

    public class CepService : ICepService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CepService> _logger;

        public CepService(HttpClient httpClient, ILogger<CepService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CepResponse?> GetAddressByCepAsync(string cep)
        {
            try
            {
                // Remove formatação do CEP
                cep = cep.Replace("-", "").Replace(".", "");

                if (cep.Length != 8 || !cep.All(char.IsDigit))
                {
                    return null;
                }

                var url = $"https://viacep.com.br/ws/{cep}/json/";
                var response = await _httpClient.GetFromJsonAsync<CepResponse>(url);

                if (response != null && response.Erro)
                {
                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar CEP: {Cep}", cep);
                return null;
            }
        }
    }
}