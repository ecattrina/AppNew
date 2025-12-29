using System.ComponentModel.DataAnnotations;

namespace AppNew.Models
{
    public class CyclicCodeInputModel
    {
        [Required(ErrorMessage = "Введите общую длину кода (n)")]
        [Range(3, 100, ErrorMessage = "Длина кода должна быть от 3 до 100")]
        public int CodeLength { get; set; } = 7;

        [Required(ErrorMessage = "Введите количество информационных битов (k)")]
        [Range(1, 50, ErrorMessage = "Количество информационных битов должно быть от 1 до 50")]
        public int DataBits { get; set; } = 4;

        [Required(ErrorMessage = "Введите порождающий многочлен")]
        public string GeneratorPolynomial { get; set; } = "x^3+x+1";

        [Required(ErrorMessage = "Введите информационные комбинации")]
        public List<string> InformationCombinations { get; set; } = new List<string> { "1111", "0110" };
    }

    public class CyclicCodeResultModel
    {
        public int CodeLength { get; set; }
        public int DataBits { get; set; }
        public int ParityBits { get; set; }
        public string GeneratorPolynomial { get; set; } = string.Empty;
        public List<EncodingResult> EncodingResults { get; set; } = new List<EncodingResult>();
    }

    public class EncodingResult
    {
        public string InformationCombination { get; set; } = string.Empty;
        public string InformationPolynomial { get; set; } = string.Empty;
        public string ShiftedPolynomial { get; set; } = string.Empty;
        public string Remainder { get; set; } = string.Empty;
        public string RemainderPolynomial { get; set; } = string.Empty;
        public string Codeword { get; set; } = string.Empty;
        public string CodewordPolynomial { get; set; } = string.Empty;
        public List<EncodingStep> Steps { get; set; } = new List<EncodingStep>();
    }

    public class EncodingStep
    {
        public string Description { get; set; } = string.Empty;
        public string Calculation { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
    }
}
