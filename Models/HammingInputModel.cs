using System.ComponentModel.DataAnnotations;

namespace AppNew.Models
{
    public class HammingInputModel
    {
        [Required(ErrorMessage = "Введите общую длину кода (n)")]
        [Range(3, 100, ErrorMessage = "Длина кода должна быть от 3 до 100")]
        public int CodeLength { get; set; } = 7;

        [Required(ErrorMessage = "Введите количество информационных битов (k)")]
        [Range(1, 50, ErrorMessage = "Количество информационных битов должно быть от 1 до 50")]
        public int DataBits { get; set; } = 4;

        [Required(ErrorMessage = "Введите код комбинацию")]
        public string CodeCombination { get; set; } = "0001101";
    }

    public class HammingResultModel
    {
        public int CodeLength { get; set; }
        public int DataBits { get; set; }
        public int ParityBits { get; set; }
        public string ReceivedCode { get; set; } = string.Empty;
        public List<int> ParityPositions { get; set; } = new List<int>();
        public List<int> DataPositions { get; set; } = new List<int>();
        public List<BitPosition> BitPositions { get; set; } = new List<BitPosition>();
        public List<ParityCheck> ParityChecks { get; set; } = new List<ParityCheck>();
        public string Syndrome { get; set; } = string.Empty;
        public int ErrorPosition { get; set; }
        public bool HasError { get; set; }
        public string CorrectedCode { get; set; } = string.Empty;
    }

    public class BitPosition
    {
        public int Position { get; set; }
        public char Bit { get; set; }
        public string Type { get; set; } = string.Empty; // "Parity" or "Data"
    }

    public class ParityCheck
    {
        public int ParityBitPosition { get; set; }
        public List<int> ControlledPositions { get; set; } = new List<int>();
        public List<char> ControlledBits { get; set; } = new List<char>();
        public int Sum { get; set; }
        public bool IsCorrect { get; set; }
    }
}

