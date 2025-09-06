using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalisisGranulometrico
{
    public  class AnalisisGranulometricos
    {
        public  ResultadosAnalisis RealizarCalculos(List<DatosTamiz> datos)
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

        public void CalcularDiametrosClave(ResultadosAnalisis resultados)
        {
            resultados.D10 = InterpolarDiametro(resultados, 10);
            resultados.D30 = InterpolarDiametro(resultados, 30);
            resultados.D50 = InterpolarDiametro(resultados, 50);
            resultados.D60 = InterpolarDiametro(resultados, 60);
            resultados.D90 = InterpolarDiametro(resultados, 90);
        }

        public void CalcularCoeficientes(ResultadosAnalisis resultados)
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

        private double InterpolarDiametro(ResultadosAnalisis resultados, double porcentaje)
        {
            for (int i = 0; i < resultados.PorcentajePasante.Count - 1; i++)
            {
                if (resultados.PorcentajePasante[i] >= porcentaje &&
                    resultados.PorcentajePasante[i + 1] <= porcentaje)
                {
                    return Interpolar(
                        resultados.Datos[i].TamañoMalla,
                        resultados.Datos[i + 1].TamañoMalla,
                        resultados.PorcentajePasante[i],
                        resultados.PorcentajePasante[i + 1],
                        porcentaje);
                }
            }
            return double.NaN;
        }

        private double Interpolar(double x1, double x2, double y1, double y2, double y)
        {
            return x1 + ((x2 - x1) * (y - y1)) / (y2 - y1);
        }
    }
}