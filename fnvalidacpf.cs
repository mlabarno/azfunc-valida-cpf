using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace azfunc_valida_cpf
{
    public static class fnValidaCPF
    {
        [Function("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, 
            ILogger log)
        {
            log.LogInformation("Iniciando a validacao do CPF.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync() ?? string.Empty;
            var data = JsonConvert.DeserializeObject<dynamic>(requestBody);

            if (data == null) {
                return new BadRequestObjectResult("Por favor informe o CPF");
            }

            string cpf = data?.cpf ?? string.Empty;

            if (ValidaCPF(cpf) == false) {
                return new BadRequestObjectResult("CPF Invalido!");
            }

            var responseMsg = "CPF Valido!";

            return new OkObjectResult(responseMsg);
        }

        public static bool ValidaCPF(string cpf) {
            if (string.IsNullOrEmpty(cpf)) {
                return false;
            }

            // Remove non-numeric characters
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Check if the CPF has 11 digits
            if (cpf.Length != 11) {
                return false;
            }

            // Check for invalid CPF numbers
            if (cpf.All(c => c == cpf[0])) {
                return false;
            }

            // Validate CPF using the check digits
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

    }
    
}
