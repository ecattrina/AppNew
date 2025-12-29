using System.ComponentModel.DataAnnotations;

namespace AppNew.Models
{
    public class ReliabilityInputModel
    {
        [Required(ErrorMessage = "Общее число объектов обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Число объектов должно быть больше 0")]
        public int TotalObjects { get; set; } = 100;

        [Required(ErrorMessage = "Необходимо указать хотя бы одну временную точку")]
        public List<TimePoint> TimePoints { get; set; } = new List<TimePoint>();
    }

    public class TimePoint
    {
        [Required(ErrorMessage = "Время обязательно")]
        [Range(0, double.MaxValue, ErrorMessage = "Время должно быть неотрицательным")]
        public double Time { get; set; }

        [Required(ErrorMessage = "Количество отказов обязательно")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество отказов должно быть неотрицательным")]
        public int Failures { get; set; }
    }
}


