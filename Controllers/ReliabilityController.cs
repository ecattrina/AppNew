using AppNew.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppNew.Controllers
{
    public class ReliabilityController : Controller
    {
        public IActionResult Index()
        {
            var model = new ReliabilityInputModel
            {
                TotalObjects = 100,
                TimePoints = new List<TimePoint>
                {
                    new TimePoint { Time = 200, Failures = 13 },
                    new TimePoint { Time = 250, Failures = 27 },
                    new TimePoint { Time = 300, Failures = 32 }
                }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(ReliabilityInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", input);
            }

            // Проверка, что временные точки отсортированы по времени
            var sortedTimePoints = input.TimePoints.OrderBy(tp => tp.Time).ToList();
            
            // Проверка, что количество отказов не убывает
            for (int i = 1; i < sortedTimePoints.Count; i++)
            {
                if (sortedTimePoints[i].Failures < sortedTimePoints[i - 1].Failures)
                {
                    ModelState.AddModelError("", $"Количество отказов в момент времени {sortedTimePoints[i].Time} не может быть меньше, чем в предыдущий момент времени.");
                    return View("Index", input);
                }
            }

            // Проверка, что количество отказов не превышает общее число объектов
            foreach (var tp in sortedTimePoints)
            {
                if (tp.Failures > input.TotalObjects)
                {
                    ModelState.AddModelError("", $"Количество отказов ({tp.Failures}) не может превышать общее число объектов ({input.TotalObjects}).");
                    return View("Index", input);
                }
            }

            var result = CalculateReliability(input.TotalObjects, sortedTimePoints);
            return View("Result", result);
        }

        private ReliabilityResultModel CalculateReliability(int totalObjects, List<TimePoint> timePoints)
        {
            var result = new ReliabilityResultModel
            {
                TotalObjects = totalObjects,
                Calculations = new List<ReliabilityCalculation>()
            };

            for (int i = 0; i < timePoints.Count; i++)
            {
                var currentPoint = timePoints[i];
                var calculation = new ReliabilityCalculation
                {
                    Time = currentPoint.Time,
                    Failures = currentPoint.Failures
                };

                // 1. Вероятность безотказной работы P̂(t_i) = (N - n(t_i)) / N
                calculation.ReliabilityProbability = (double)(totalObjects - currentPoint.Failures) / totalObjects;

                // 2. Вероятность отказов F̂(t_i) = n(t_i) / N
                calculation.FailureProbability = (double)currentPoint.Failures / totalObjects;

                if (i == 0)
                {
                    // Для первого интервала: t = 0 до t = t_0, n(0) = 0
                    calculation.DeltaN = currentPoint.Failures - 0;
                    calculation.DeltaT = currentPoint.Time - 0;
                    calculation.WorkingObjectsAtStart = totalObjects - 0;
                }
                else
                {
                    var previousPoint = timePoints[i - 1];
                    calculation.DeltaN = currentPoint.Failures - previousPoint.Failures;
                    calculation.DeltaT = currentPoint.Time - previousPoint.Time;
                    calculation.WorkingObjectsAtStart = totalObjects - previousPoint.Failures;
                }

                // 3. Плотность распределения отказов f̂(t_i) = Δn_i / (N * Δt_i)
                if (calculation.DeltaT > 0)
                {
                    calculation.FailureDensity = calculation.DeltaN / (totalObjects * calculation.DeltaT);
                }

                // 4. Интенсивность отказов λ̂(t_i) = Δn_i / ((N - n(t_i-1)) * Δt_i)
                if (calculation.DeltaT > 0 && calculation.WorkingObjectsAtStart > 0)
                {
                    calculation.FailureRate = calculation.DeltaN / (calculation.WorkingObjectsAtStart * calculation.DeltaT);
                }

                result.Calculations.Add(calculation);
            }

            return result;
        }
    }
}


