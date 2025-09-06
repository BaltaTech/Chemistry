using System.Collections.Generic;

namespace AnalisisGranulometrico
{
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
}