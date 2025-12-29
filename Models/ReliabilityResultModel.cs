namespace AppNew.Models
{
    public class ReliabilityResultModel
    {
        public int TotalObjects { get; set; }
        public List<ReliabilityCalculation> Calculations { get; set; } = new List<ReliabilityCalculation>();
    }

    public class ReliabilityCalculation
    {
        public double Time { get; set; }
        public int Failures { get; set; }
        public double ReliabilityProbability { get; set; } // P̂(t_i) - Вероятность безотказной работы
        public double FailureProbability { get; set; } // F̂(t_i) - Вероятность отказов
        public double FailureDensity { get; set; } // f̂(t_i) - Плотность распределения отказов
        public double FailureRate { get; set; } // λ̂(t_i) - Интенсивность отказов
        public double DeltaN { get; set; } // Δn_i
        public double DeltaT { get; set; } // Δt_i
        public int WorkingObjectsAtStart { get; set; } // Количество работающих объектов на начало интервала
    }
}


