using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace httpValidaCpf
{
    public static class validacpf
    {
        [FunctionName("validacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do cpf");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data == null || data.cpf == null)
            {
                return new BadRequestObjectResult("Por favor, informe o cpf");
            }

            string cpf = data.cpf;

            if (ValidaCpf(cpf) == false)
            {
                return new BadRequestObjectResult("Cpf inválido");
            }

            return new OkObjectResult("Cpf válido");
        }

        public static bool ValidaCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            cpf = cpf.Trim().Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais
            if (new string(cpf[0], cpf.Length) == cpf)
                return false;

            // Calcula os dígitos verificadores
            int[] pesos1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesos2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string cpfBase = cpf.Substring(0, 9);
            string digitosVerificadores = cpf.Substring(9, 2);

            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(cpfBase[i].ToString()) * pesos1[i];
            }

            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            soma = 0;
            cpfBase += primeiroDigito;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(cpfBase[i].ToString()) * pesos2[i];
            }

            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            return true;
        }
    }
}
