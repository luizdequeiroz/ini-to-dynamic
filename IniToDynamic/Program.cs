using System;

namespace IniToDynamic
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = @"C:\file.ini";
            var ini = IniReader.GetIniAsDynamic(fileName);

            Console.WriteLine("PESSOA");
            Console.WriteLine($"Nome: {ini.Pessoa.Nome} {ini.Pessoa.Sobrenome}");
            Console.WriteLine($"Data de Nascimento: {ini.Pessoa.DataNascimento}");
            Console.WriteLine($"Telefone: {ini.Pessoa.Telefone}");
            Console.WriteLine();
            Console.WriteLine("ANIMAL");
            Console.WriteLine($"Nome: {ini.Animal.Nome}");
            Console.WriteLine($"Tipo: {ini.Animal.Tipo}");
            Console.WriteLine($"Raca: {ini.Animal.Raca}");
            Console.WriteLine($"Porte: {ini.Animal.Porte}");

            Console.ReadKey();
        }        
    }
}