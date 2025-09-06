using System;
using System.Collections.Generic;

namespace AnalisisGranulometrico
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== ANÁLISIS GRANULOMÉTRICO PARA BENEFICIO DE MINERALES ===");

            IModuloEntrada moduloEntrada = new ModuloEntrada();
            var datos = moduloEntrada.ObtenerDatos();
            
            //2. Instancia Análisis Granulométrico

            var analisisGranulometricos = new AnalisisGranulometricos();
            var resultados = analisisGranulometricos.RealizarCalculos(datos);

            
            //3. Modúlo Salida
            
            IModuloSalida moduloSalida = new ModuloSalida();
            moduloSalida.MostrarResultados(resultados);
            Console.WriteLine("Presione cualquier tecla para salir");
            Console.ReadKey();
        }
    }
    public class DatosTamiz
    {
        public double TamañoMalla { get; set; }
        public double MasaRetenida { get; set; }
    }
    
    public interface IModuloEntrada
    {
        List<DatosTamiz> ObtenerDatos();
    }

    public  class ModuloCalculo
    {
    }

    public interface IModuloSalida
    {
        void MostrarResultados(ResultadosAnalisis resultados);
    }
}