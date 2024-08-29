using Atividade_Cliente.DTOS;
using Atividade_Cliente.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;


namespace Atividade_Cliente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
        public class ClienteController : ControllerBase
    {
        private const string ARQUIVOTEXTO= "clientes.txt";

        public ClienteController()
        {
            
            if (!System.IO.File.Exists(ARQUIVOTEXTO))
            {
                System.IO.File.Create(ARQUIVOTEXTO).Dispose();
            }
        }

      

        private List<Cliente> LerClientesDoArquivo()
        {
            var clientes = new List<Cliente>();

            if (!System.IO.File.Exists(ARQUIVOTEXTO))
            {
                return clientes;
            }

            var linhas = System.IO.File.ReadAllLines(ARQUIVOTEXTO);
            foreach (var linha in linhas)
            {
                var dados = linha.Split('|');
                if (dados.Length == 10)
                {
                    clientes.Add(new Cliente
                    {
                        Nome = dados[0],
                        DataNascimento = DateTime.Parse(dados[1]),
                        Sexo = dados[2],
                        RG = dados[3],
                        CPF = dados[4],
                        Endereco = dados[5],
                        Cidade = dados[6],
                        Estado = dados[7],
                        Telefone = dados[8],
                        Email = dados[9]
                    });
                }
            }

            return clientes;
        }
        private bool ValidarCPF(string cpf)
        {
            cpf = Regex.Replace(cpf, @"\D", "");
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            {
                return false;
            }

            var digito1 = 0;
            var digito2 = 0;
            var peso = 10;

            for (var i = 0; i < 9; i++)
            {
                digito1 += int.Parse(cpf[i].ToString()) * peso--;
            }

            digito1 = (digito1 % 11) < 2 ? 0 : 11 - (digito1 % 11);

            peso = 11;
            for (var i = 0; i < 10; i++)
            {
                digito2 += int.Parse(cpf[i].ToString()) * peso--;
            }

            digito2 = (digito2 % 11) < 2 ? 0 : 11 - (digito2 % 11);

            return cpf[9] == digito1.ToString()[0] && cpf[10] == digito2.ToString()[0];
        }

        private void GravarClientesNoArquivo(List<Cliente> clientes)
        {
            var linhas = clientes.Select(c => $"{c.Nome}|{c.DataNascimento:dd-MM-yyyy}|{c.Sexo}|{c.RG}|{c.CPF}|{c.Endereco}|{c.Cidade}|{c.Estado}|{c.Telefone}|{c.Email}");
            System.IO.File.WriteAllLines(ARQUIVOTEXTO, linhas);
        }

       

        [HttpGet("{cpf}")]
        public IActionResult GetByCPF(string cpf)
        {
            if (!ValidarCPF(cpf))
            {
                return BadRequest("CPF inválido.");
            }

            var clientes = LerClientesDoArquivo();
            var cliente = clientes.FirstOrDefault(c => c.CPF == cpf);

            if (cliente == null)
            {
                return NotFound();
            }

            return Ok(cliente);
        }
        [HttpGet]
        public IActionResult Get()
        {
            var clientes = LerClientesDoArquivo();
            return Ok(clientes);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ClienteDOTS dto)
        {
            if (dto == null || !ValidarCPF(dto.CPF))
            {
                return BadRequest("Dados inválidos");
            }

            var clientes = LerClientesDoArquivo();
            if (clientes.Any(c => c.CPF == dto.CPF))
            {
                return Conflict("Cliente já esta cadastrado.");
            }

            var cliente = new Cliente
            {
                Nome = dto.Nome,
                DataNascimento = dto.DataNascimento,
                Sexo = dto.Sexo,
                RG = dto.RG,
                CPF = dto.CPF,
                Endereco = dto.Endereco,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Telefone = dto.Telefone,
                Email = dto.Email
            };

            clientes.Add(cliente);
            GravarClientesNoArquivo(clientes);

            return CreatedAtAction(nameof(GetByCPF), new { cpf = cliente.CPF }, cliente);
        }

        [HttpPut("{cpf}")]
        public IActionResult Put(string cpf, [FromBody] ClienteDOTS dto)
        {
            if (!ValidarCPF(cpf))
            {
                return BadRequest("CPF inválido.");
            }

            var clientes = LerClientesDoArquivo();
            var cliente = clientes.FirstOrDefault(c => c.CPF == cpf);

            if (cliente == null)
            {
                return NotFound();
            }

            cliente.Nome = dto.Nome ?? cliente.Nome;
            cliente.DataNascimento = dto.DataNascimento != default ? dto.DataNascimento : cliente.DataNascimento;
            cliente.Sexo = dto.Sexo ?? cliente.Sexo;
            cliente.RG = dto.RG ?? cliente.RG;
            cliente.Endereco = dto.Endereco ?? cliente.Endereco;
            cliente.Cidade = dto.Cidade ?? cliente.Cidade;
            cliente.Estado = dto.Estado ?? cliente.Estado;
            cliente.Telefone = dto.Telefone ?? cliente.Telefone;
            cliente.Email = dto.Email ?? cliente.Email;

            GravarClientesNoArquivo(clientes);

            return Ok(cliente);
        }

        [HttpDelete("{cpf}")]
        public IActionResult Delete(string cpf)
        {
            if (!ValidarCPF(cpf))
            {
                return BadRequest("CPF inválido.");
            }

            var clientes = LerClientesDoArquivo();
            var cliente = clientes.FirstOrDefault(c => c.CPF == cpf);

            if (cliente == null)
            {
                return NotFound();
            }

            clientes.Remove(cliente);
            GravarClientesNoArquivo(clientes);

            return Ok(cliente);
        }
    }
}