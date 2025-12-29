using System.ComponentModel.DataAnnotations;

namespace AppNew.Models
{
    public class CyclicCodeErrorInputModel
    {
        [Required(ErrorMessage = "Введите общую длину кода (n)")]
        [Range(3, 100, ErrorMessage = "Длина кода должна быть от 3 до 100")]
        public int CodeLength { get; set; } = 7;

        [Required(ErrorMessage = "Введите количество информационных битов (k)")]
        [Range(1, 50, ErrorMessage = "Количество информационных битов должно быть от 1 до 50")]
        public int DataBits { get; set; } = 4;

        [Required(ErrorMessage = "Введите порождающий многочлен")]
        public string GeneratorPolynomial { get; set; } = "x^3+x+1";

        [Required(ErrorMessage = "Введите принятую кодовую комбинацию")]
        public string ReceivedCode { get; set; } = "0101001";
    }

    public class CyclicCodeErrorResultModel
    {
        public int CodeLength { get; set; }
        public int DataBits { get; set; }
        public int ParityBits { get; set; }
        public string GeneratorPolynomial { get; set; } = string.Empty;
        public string ReceivedCode { get; set; } = string.Empty;
        public string ReceivedPolynomial { get; set; } = string.Empty;
        public string Syndrome { get; set; } = string.Empty;
        public string SyndromePolynomial { get; set; } = string.Empty;
        public bool HasError { get; set; }
        public int ErrorPosition { get; set; }
        public string CorrectedCode { get; set; } = string.Empty;
        public string CorrectedPolynomial { get; set; } = string.Empty;
        public List<SyndromeTableEntry> SyndromeTable { get; set; } = new List<SyndromeTableEntry>();
        public List<ErrorDetectionStep> Steps { get; set; } = new List<ErrorDetectionStep>();
    }

    public class SyndromeTableEntry
    {
        public int Position { get; set; }
        public string ErrorPolynomial { get; set; } = string.Empty;
        public string SyndromePolynomial { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
    }

    public class ErrorDetectionStep
    {
        public string Description { get; set; } = string.Empty;
        public string Calculation { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
    }
}

