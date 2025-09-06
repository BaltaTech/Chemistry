using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnalisisGranulometrico
{
    public class ModuloEntrada : IModuloEntrada
    {
        public  List<DatosTamiz> ObtenerDatos()
        {
            Console.WriteLine("\nSeleccione opción de entrada:");
            Console.WriteLine("1. Entrada manual");
            Console.WriteLine("2. Cargar desde archivo CSV");

            while (true)
            {
                var opcion = Console.ReadLine();

                if (opcion == "1") return EntradaManual();
                if (opcion == "2") return CargarDesdeCSV();

                Console.WriteLine("Opción no válida. Intente nuevamente.");
            }
        }

        private static List<DatosTamiz> EntradaManual()
        {

            // Variable para almacenar entrada de usuaio 

            var datos = new List<DatosTamiz>();

            Console.WriteLine("\nIntroduzca los datos (tamaño de malla en mm y masa retenida en g):");
            Console.WriteLine("Ingrese 'FIN' en el tamaño de malla cuando haya terminado");

            while (true)
            {
                Console.Write("Tamaño de malla (mm): ");
                var tamizInput = Console.ReadLine();

                if (tamizInput.ToUpper() == "FIN")
                {
                    if (datos.Count == 0)
                    {
                        Console.WriteLine("Debe ingresar al menos un tamiz.");
                        continue;
                    }
                    break;
                }

                if (!double.TryParse(tamizInput, out double tamiz) || tamiz <= 0)
                {
                    Console.WriteLine("Valor no válido. Debe ser un número positivo.");
                    continue;
                }

                Console.Write("Masa retenida (g): ");
                var masaInput = Console.ReadLine();

                if (!double.TryParse(masaInput, out double masa) || masa < 0)
                {
                    Console.WriteLine("Valor no válido. Debe ser un número no negativo.");
                    continue;
                }

                datos.Add(new DatosTamiz { TamañoMalla = tamiz, MasaRetenida = masa });
                Console.WriteLine($"Tamiz agregado: {tamiz} mm - {masa} g\n");
            }

            // Ordenar de mayor a menor tamaño de malla
            return datos.OrderByDescending(d => d.TamañoMalla).ToList();
        }

        private static List<DatosTamiz> CargarDesdeCSV()
        {
            Console.Write("Ruta del archivo CSV: ");
            var path = Console.ReadLine();

            try
            {
                var datos = File.ReadAllLines(path)
                    .Skip(1) // Saltar encabezado
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l =>
                    {
                        var partes = l.Split(',');
                        return new DatosTamiz
                        {
                            TamañoMalla = double.Parse(partes[0].Trim()),
                            MasaRetenida = double.Parse(partes[1].Trim())
                        };
                    })
                    .OrderByDescending(d => d.TamañoMalla)
                    .ToList();

                Console.WriteLine($"Se cargaron {datos.Count} tamices desde el archivo.");
                return datos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo: {ex.Message}");
                return new List<DatosTamiz>();
            }
        }
    }
}