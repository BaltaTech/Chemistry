using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnalisisGranulometrico
{
    public class DatosTamiz
    {
        public double TamanoMalla { get; set; }
        public double MasaRetenida { get; set; }
    }

    public class ResultadosAnalisis
    {
        public List<DatosTamiz> Datos { get; set; }
        public double MasaTotal { get; set; }
        public List<double> PorcentajeRetenido { get; set; }
        public List<double> PorcentajeAcumulado { get; set; }
        public List<double> PorcentajePasante { get; set; }
        public double D10 { get; set; }
        public double D30 { get; set; }
        public double D50 { get; set; }
        public double D60 { get; set; }
        public double D90 { get; set; }
        public double CoeficienteUniformidad { get; set; }
        public double CoeficienteCurvatura { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ANÁLISIS GRANULOMÉTRICO PARA BENEFICIO DE MINERALES ===");

            var datos = ModuloEntrada.ObtenerDatos();
            var resultados = ModuloCalculo.RealizarCalculos(datos);
            ModuloSalida.MostrarResultados(resultados);

            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }

    static class ModuloEntrada
    {
        public static List<DatosTamiz> ObtenerDatos()
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

                datos.Add(new DatosTamiz { TamanoMalla = tamiz, MasaRetenida = masa });
                Console.WriteLine($"Tamiz agregado: {tamiz} mm - {masa} g\n");
            }

            // Ordenar de mayor a menor tamaño de malla
            return datos.OrderByDescending(d => d.TamanoMalla).ToList();
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
                            TamanoMalla = double.Parse(partes[0].Trim()),
                            MasaRetenida = double.Parse(partes[1].Trim())
                        };
                    })
                    .OrderByDescending(d => d.TamanoMalla)
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

    static class ModuloCalculo
    {
        public static ResultadosAnalisis RealizarCalculos(List<DatosTamiz> datos)
        {
            var resultados = new ResultadosAnalisis { Datos = datos };

            // Calcular masa total
            resultados.MasaTotal = datos.Sum(d => d.MasaRetenida);

            // Validar masa total
            if (resultados.MasaTotal <= 0)
                throw new ArgumentException("La masa total debe ser mayor que cero.");

            // Calcular porcentajes
            resultados.PorcentajeRetenido = datos.Select(d => d.MasaRetenida / resultados.MasaTotal * 100).ToList();

            // Calcular porcentajes acumulados
            resultados.PorcentajeAcumulado = new List<double>();
            double acumulado = 0;
            foreach (var pr in resultados.PorcentajeRetenido)
            {
                acumulado += pr;
                resultados.PorcentajeAcumulado.Add(acumulado);
            }

            // Calcular porcentajes pasantes
            resultados.PorcentajePasante = resultados.PorcentajeAcumulado.Select(a => 100 - a).ToList();

            // Calcular diámetros característicos
            CalcularDiametrosClave(resultados);

            // Calcular coeficientes
            CalcularCoeficientes(resultados);

            return resultados;
        }

        private static void CalcularDiametrosClave(ResultadosAnalisis resultados)
        {
            resultados.D10 = InterpolarDiametro(resultados, 10);
            resultados.D30 = InterpolarDiametro(resultados, 30);
            resultados.D50 = InterpolarDiametro(resultados, 50);
            resultados.D60 = InterpolarDiametro(resultados, 60);
            resultados.D90 = InterpolarDiametro(resultados, 90);
        }

        private static void CalcularCoeficientes(ResultadosAnalisis resultados)
        {
            // Coeficiente de uniformidad (Cu)
            if (resultados.D10 > 0)
                resultados.CoeficienteUniformidad = resultados.D60 / resultados.D10;
            else
                resultados.CoeficienteUniformidad = double.PositiveInfinity;

            // Coeficiente de curvatura (Cc)
            if (resultados.D10 > 0 && resultados.D60 > 0)
                resultados.CoeficienteCurvatura = Math.Pow(resultados.D30, 2) / (resultados.D10 * resultados.D60);
            else
                resultados.CoeficienteCurvatura = double.NaN;
        }

        private static double InterpolarDiametro(ResultadosAnalisis resultados, double porcentaje)
        {
            for (int i = 0; i < resultados.PorcentajePasante.Count - 1; i++)
            {
                if (resultados.PorcentajePasante[i] >= porcentaje &&
                    resultados.PorcentajePasante[i + 1] <= porcentaje)
                {
                    return Interpolar(
                        resultados.Datos[i].TamanoMalla,
                        resultados.Datos[i + 1].TamanoMalla,
                        resultados.PorcentajePasante[i],
                        resultados.PorcentajePasante[i + 1],
                        porcentaje);
                }
            }
            return double.NaN;
        }

        private static double Interpolar(double x1, double x2, double y1, double y2, double y)
        {
            return x1 + ((x2 - x1) * (y - y1)) / (y2 - y1);
        }
    }

    static class ModuloSalida
    {
        public static void MostrarResultados(ResultadosAnalisis resultados)
        {
            Console.WriteLine("\n=== RESULTADOS DEL ANÁLISIS GRANULOMÉTRICO ===");
            Console.WriteLine($"Masa total de la muestra: {resultados.MasaTotal:0.00} g");

            // Tabla de resultados
            Console.WriteLine("\n-----------------------------------------------------------------------------");
            Console.WriteLine("Malla (mm)\tMasa Ret (g)\t% Retenido\t% Acumulado\t% Pasante");
            Console.WriteLine("-----------------------------------------------------------------------------");

            for (int i = 0; i < resultados.Datos.Count; i++)
            {
                Console.WriteLine($"{resultados.Datos[i].TamanoMalla,8:0.000}\t" +
                                $"{resultados.Datos[i].MasaRetenida,10:0.00}\t" +
                                $"{resultados.PorcentajeRetenido[i],10:0.00}\t" +
                                $"{resultados.PorcentajeAcumulado[i],10:0.00}\t" +
                                $"{resultados.PorcentajePasante[i],10:0.00}");
            }
            Console.WriteLine("-----------------------------------------------------------------------------");

            // Diámetros característicos
            Console.WriteLine("\nDIÁMETROS CARACTERÍSTICOS:");
            Console.WriteLine($"D10: {resultados.D10:0.000} mm");
            Console.WriteLine($"D30: {resultados.D30:0.000} mm");
            Console.WriteLine($"D50: {resultados.D50:0.000} mm");
            Console.WriteLine($"D60: {resultados.D60:0.000} mm");
            Console.WriteLine($"D90: {resultados.D90:0.000} mm");

            // Coeficientes
            Console.WriteLine("\nCOEFICIENTES DE CLASIFICACIÓN:");
            Console.WriteLine($"Coeficiente de Uniformidad (Cu): {resultados.CoeficienteUniformidad:0.00}");
            Console.WriteLine($"Coeficiente de Curvatura (Cc): {resultados.CoeficienteCurvatura:0.00}");

            // Interpretación
            MostrarInterpretacion(resultados);

            // Gráfico
            MostrarGraficoASCII(resultados);
        }

        private static void MostrarInterpretacion(ResultadosAnalisis resultados)
        {
            Console.WriteLine("\nINTERPRETACIÓN PARA BENEFICIO DE MINERALES:");

            // Interpretación del coeficiente de uniformidad
            if (resultados.CoeficienteUniformidad < 4)
            {
                Console.WriteLine("- La muestra tiene una distribución uniforme (Cu < 4), lo que sugiere que");
                Console.WriteLine("  la molienda podría generar una liberación mineral uniforme pero con riesgo");
                Console.WriteLine("  de sobre molienda en algunas partículas.");
            }
            else if (resultados.CoeficienteUniformidad >= 4 && resultados.CoeficienteUniformidad <= 6)
            {
                Console.WriteLine("- La muestra tiene una distribución bien gradada (4 ≤ Cu ≤ 6), ideal para");
                Console.WriteLine("  procesos de beneficio donde se busca un balance entre liberación mineral");
                Console.WriteLine("  y eficiencia energética en la molienda.");
            }
            else
            {
                Console.WriteLine("- La muestra tiene una distribución amplia (Cu > 6), lo que indica una variedad");
                Console.WriteLine("  de tamaños que podría requerir clasificación intermedia para optimizar");
                Console.WriteLine("  la molienda y evitar tanto la sub-molienda como la sobre-molienda.");
            }

            // Interpretación del coeficiente de curvatura
            if (resultados.CoeficienteCurvatura >= 1 && resultados.CoeficienteCurvatura <= 3)
            {
                Console.WriteLine("- La curva granulométrica es bien graduada (1 ≤ Cc ≤ 3), lo que favorece");
                Console.WriteLine("  una eficiente liberación de minerales valiosos con menor consumo energético.");
            }
            else
            {
                Console.WriteLine("- La curva granulométrica no está bien graduada (Cc fuera del rango 1-3),");
                Console.WriteLine("  lo que podría indicar la necesidad de ajustar los parámetros de molienda");
                Console.WriteLine("  o clasificación para mejorar la eficiencia del proceso.");
            }

            // Interpretación basada en D50
            Console.WriteLine($"- El D50 de {resultados.D50:0.000} mm indica el tamaño medio de las partículas.");
            if (resultados.D50 > 1.0)
            {
                Console.WriteLine("  Este tamaño sugiere que podría ser necesaria una molienda más fina para");
                Console.WriteLine("  lograr una liberación adecuada de los minerales valiosos.");
            }
            else if (resultados.D50 < 0.1)
            {
                Console.WriteLine("  Este tamaño fino sugiere que posiblemente ya hay liberación mineral pero");
                Console.WriteLine("  con riesgo de sobre molienda y alto consumo energético.");
            }
            else
            {
                Console.WriteLine("  Este tamaño es adecuado para muchos procesos de concentración de minerales,");
                Console.WriteLine("  equilibrando liberación mineral y costos de molienda.");
            }
        }

        private static void MostrarGraficoASCII(ResultadosAnalisis resultados)
        {
            Console.WriteLine("\nCURVA GRANULOMÉTRICA:");
            Console.WriteLine("(Porcentaje Pasante vs. Tamaño de Partícula - Escala logarítmica)");
            Console.WriteLine();

            int altura = 20;
            int ancho = 60;

            // Encontrar los límites para la escala logarítmica
            double minTamiz = resultados.Datos.Min(d => d.TamanoMalla);
            double maxTamiz = resultados.Datos.Max(d => d.TamanoMalla);

            // Crear matriz para el gráfico
            char[,] grafico = new char[altura + 2, ancho + 10];

            // Inicializar con espacios
            for (int i = 0; i <= altura + 1; i++)
                for (int j = 0; j <= ancho + 9; j++)
                    grafico[i, j] = ' ';

            // Eje Y (Porcentaje Pasante)
            for (int i = 0; i <= altura; i++)
            {
                int porcentaje = 100 - i * (100 / altura);
                grafico[i, 0] = '|';
                grafico[i, 1] = ' ';
                grafico[i, 2] = (porcentaje == 100 || porcentaje % 20 == 0) ? (char)('0' + porcentaje / 100) : ' ';
                grafico[i, 3] = (porcentaje == 100 || porcentaje % 20 == 0) ? (char)('0' + (porcentaje / 10) % 10) : ' ';
                grafico[i, 4] = (porcentaje == 100 || porcentaje % 20 == 0) ? (char)('0' + porcentaje % 10) : ' ';
                grafico[i, 5] = (porcentaje == 100 || porcentaje % 20 == 0) ? '%' : ' ';
            }

            // Eje X (Tamaño de partícula en escala logarítmica)
            grafico[altura + 1, 0] = '+';
            for (int j = 0; j <= ancho; j++)
            {
                grafico[altura + 1, j + 6] = '-';
            }

            // Marcar puntos clave en el eje X
            double logMin = Math.Log10(minTamiz);
            double logMax = Math.Log10(maxTamiz);
            double pasoLog = (logMax - logMin) / ancho;

            for (int j = 0; j <= ancho; j += 10)
            {
                double valorLog = logMin + j * pasoLog;
                double valor = Math.Pow(10, valorLog);
                string etiqueta = valor.ToString("0.0");

                int pos = j + 6;
                for (int k = 0; k < etiqueta.Length && pos + k < ancho + 6; k++)
                {
                    grafico[altura + 1, pos + k] = etiqueta[k];
                }
            }

            // Dibujar puntos de la curva
            foreach (var punto in resultados.Datos.Select((d, i) => new { d.TamanoMalla, Porcentaje = resultados.PorcentajePasante[i] }))
            {
                if (punto.TamanoMalla <= 0) continue;

                double xLog = (Math.Log10(punto.TamanoMalla) - logMin) / (logMax - logMin) * ancho;
                int x = (int)Math.Round(xLog) + 6;
                int y = altura - (int)Math.Round(punto.Porcentaje / 100 * altura);

                if (x >= 6 && x <= ancho + 6 && y >= 0 && y <= altura)
                {
                    grafico[y, x] = '*';

                    // Conectar con línea si hay puntos adyacentes
                    if (x > 6 && y > 0 && grafico[y, x - 1] == ' ')
                        grafico[y, x - 1] = '-';
                    if (x < ancho + 5 && y > 0 && grafico[y, x + 1] == ' ')
                        grafico[y, x + 1] = '-';
                }
            }

            // Imprimir el gráfico
            for (int i = 0; i <= altura + 1; i++)
            {
                for (int j = 0; j <= ancho + 9; j++)
                {
                    Console.Write(grafico[i, j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nLeyenda:");
            Console.WriteLine("Eje Y: Porcentaje Acumulado Pasante (%)");
            Console.WriteLine("Eje X: Tamaño de Partícula (mm) - Escala Logarítmica");
        }
    }
}