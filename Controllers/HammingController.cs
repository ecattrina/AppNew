using AppNew.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppNew.Controllers
{
    public class HammingController : Controller
    {
        public IActionResult Index()
        {
            var model = new HammingInputModel
            {
                CodeLength = 7,
                DataBits = 4,
                CodeCombination = "0001101"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(HammingInputModel input)
        {
            // Валидация: проверяем, что n >= k + количество необходимых контрольных битов
            int requiredParityBits = CalculateRequiredParityBits(input.DataBits);
            int minCodeLength = input.DataBits + requiredParityBits;
            
            if (input.CodeLength < minCodeLength)
            {
                ModelState.AddModelError("CodeLength", 
                    $"Минимальная длина кода для {input.DataBits} информационных битов: {minCodeLength} (нужно {requiredParityBits} контрольных битов)");
            }

            // Валидация длины кодовой комбинации
            if (!string.IsNullOrEmpty(input.CodeCombination))
            {
                if (input.CodeCombination.Length != input.CodeLength)
                {
                    ModelState.AddModelError("CodeCombination", 
                        $"Длина кодовой комбинации должна быть {input.CodeLength} бит");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(input.CodeCombination, @"^[01]+$"))
                {
                    ModelState.AddModelError("CodeCombination", 
                        "Кодовая комбинация должна содержать только 0 и 1");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Index", input);
            }

            var result = CalculateHammingError(input.CodeLength, input.DataBits, input.CodeCombination);
            return View("Result", result);
        }

        private int CalculateRequiredParityBits(int dataBits)
        {
            // Для кода Хэмминга: 2^r >= k + r + 1, где r - количество контрольных битов, k - информационных
            int r = 0;
            while (Math.Pow(2, r) < dataBits + r + 1)
            {
                r++;
            }
            return r;
        }

        private HammingResultModel CalculateHammingError(int codeLength, int dataBits, string code)
        {
            var result = new HammingResultModel
            {
                CodeLength = codeLength,
                DataBits = dataBits,
                ParityBits = codeLength - dataBits,
                ReceivedCode = code
            };

            // Определяем позиции контрольных битов (степени двойки: 1, 2, 4, 8, 16, ...)
            var parityPositions = new List<int>();
            for (int i = 0; i < codeLength; i++)
            {
                int position = i + 1;
                // Проверяем, является ли позиция степенью двойки
                if ((position & (position - 1)) == 0) // Это степень двойки
                {
                    parityPositions.Add(position);
                }
            }

            // Ограничиваем количество контрольных битов до необходимого
            int requiredParityBits = CalculateRequiredParityBits(dataBits);
            if (parityPositions.Count > requiredParityBits)
            {
                parityPositions = parityPositions.Take(requiredParityBits).ToList();
            }

            result.ParityPositions = parityPositions;

            // Определяем позиции информационных битов (все остальные)
            var dataPositions = new List<int>();
            for (int i = 1; i <= codeLength; i++)
            {
                if (!parityPositions.Contains(i))
                {
                    dataPositions.Add(i);
                }
            }
            result.DataPositions = dataPositions;

            // Заполняем информацию о позициях битов
            for (int i = 0; i < code.Length; i++)
            {
                int position = i + 1;
                result.BitPositions.Add(new BitPosition
                {
                    Position = position,
                    Bit = code[i],
                    Type = parityPositions.Contains(position) ? "Parity" : "Data"
                });
            }

            // Сортируем позиции контрольных битов по убыванию для правильного формирования синдрома
            var sortedParityPositions = parityPositions.OrderByDescending(p => p).ToList();

            // Проверяем контрольные суммы для каждого бита четности
            foreach (var parityPos in sortedParityPositions)
            {
                var controlledPositions = GetControlledPositions(parityPos, codeLength);
                var check = CalculateParityCheck(code, parityPos, controlledPositions);
                result.ParityChecks.Add(check);
            }

            // Формируем синдром: результаты проверки контрольных битов в порядке убывания позиций
            var syndromeBits = new List<int>();
            foreach (var check in result.ParityChecks)
            {
                syndromeBits.Add(check.IsCorrect ? 0 : 1);
            }
            result.Syndrome = string.Join("", syndromeBits);

            // Преобразуем синдром в десятичное число для определения позиции ошибки
            int errorPosition = 0;
            if (!string.IsNullOrEmpty(result.Syndrome))
            {
                errorPosition = Convert.ToInt32(result.Syndrome, 2);
            }

            result.ErrorPosition = errorPosition;
            result.HasError = errorPosition != 0;

            // Исправляем ошибку, если она есть
            if (result.HasError && errorPosition > 0 && errorPosition <= code.Length)
            {
                var correctedCode = code.ToCharArray();
                int index = errorPosition - 1; // Индекс в массиве (0-based)
                correctedCode[index] = correctedCode[index] == '0' ? '1' : '0';
                result.CorrectedCode = new string(correctedCode);
            }
            else
            {
                result.CorrectedCode = code;
            }

            return result;
        }

        private List<int> GetControlledPositions(int parityPosition, int codeLength)
        {
            // Бит четности на позиции 2^i контролирует все позиции, у которых в двоичном представлении i-й бит равен 1
            // Например, позиция 1 (2^0) контролирует позиции с младшим битом = 1: 1, 3, 5, 7, ...
            // Позиция 2 (2^1) контролирует позиции со вторым битом = 1: 2, 3, 6, 7, 10, 11, ...
            // Позиция 4 (2^2) контролирует позиции с третьим битом = 1: 4, 5, 6, 7, 12, 13, 14, 15, ...

            var controlled = new List<int>();
            for (int pos = 1; pos <= codeLength; pos++)
            {
                // Проверяем, контролирует ли данный бит четности эту позицию
                // Позиция контролируется, если (pos & parityPosition) != 0
                if ((pos & parityPosition) != 0)
                {
                    controlled.Add(pos);
                }
            }
            return controlled;
        }

        private ParityCheck CalculateParityCheck(string code, int parityBitPosition, List<int> controlledPositions)
        {
            var check = new ParityCheck
            {
                ParityBitPosition = parityBitPosition,
                ControlledPositions = controlledPositions
            };

            int sum = 0;
            foreach (int pos in controlledPositions)
            {
                int index = pos - 1; // Позиция 1-based, индекс 0-based
                if (index >= 0 && index < code.Length)
                {
                    char bit = code[index];
                    check.ControlledBits.Add(bit);
                    sum += bit == '1' ? 1 : 0;
                }
            }

            check.Sum = sum;
            // Контрольная сумма должна быть четной (сумма по модулю 2 = 0)
            check.IsCorrect = (sum % 2) == 0;

            return check;
        }
    }
}
