using AppNew.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;

namespace AppNew.Controllers
{
    public class CyclicCodeErrorController : Controller
    {
        public IActionResult Index()
        {
            var model = new CyclicCodeErrorInputModel
            {
                CodeLength = 7,
                DataBits = 4,
                GeneratorPolynomial = "x^3+x+1",
                ReceivedCode = "0101001"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DetectError(CyclicCodeErrorInputModel input)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(input.ReceivedCode))
            {
                ModelState.AddModelError("ReceivedCode", "Принятая кодовая комбинация не может быть пустой");
            }
            else if (input.ReceivedCode.Length != input.CodeLength)
            {
                ModelState.AddModelError("ReceivedCode", $"Длина кодовой комбинации должна быть {input.CodeLength} бит");
            }
            else if (!Regex.IsMatch(input.ReceivedCode, @"^[01]+$"))
            {
                ModelState.AddModelError("ReceivedCode", "Кодовая комбинация должна содержать только 0 и 1");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", input);
            }

            var result = DetectErrorInCode(input);
            return View("Result", result);
        }

        private CyclicCodeErrorResultModel DetectErrorInCode(CyclicCodeErrorInputModel input)
        {
            var result = new CyclicCodeErrorResultModel
            {
                CodeLength = input.CodeLength,
                DataBits = input.DataBits,
                ParityBits = input.CodeLength - input.DataBits,
                GeneratorPolynomial = input.GeneratorPolynomial,
                ReceivedCode = input.ReceivedCode
            };

            // Парсим порождающий многочлен
            int parityBits = input.CodeLength - input.DataBits;
            var generatorCoeffs = ParsePolynomial(input.GeneratorPolynomial, parityBits);

            // Шаг 1: Преобразуем принятую комбинацию в полином
            var receivedPolynomial = BinaryToPolynomial(input.ReceivedCode);
            result.ReceivedPolynomial = receivedPolynomial;
            result.Steps.Add(new ErrorDetectionStep
            {
                Description = "Шаг 1: Преобразовать принятую комбинацию в полином r(x)",
                Calculation = $"r(x) = {receivedPolynomial}",
                Result = $"Для комбинации {input.ReceivedCode}: r(x) = {receivedPolynomial}"
            });

            // Шаг 2: Вычисляем синдром s(x) = r(x) mod g(x)
            var receivedCoeffs = BinaryToCoefficients(input.ReceivedCode);
            var syndromeCoeffs = PolynomialDivision(receivedCoeffs, generatorCoeffs);
            var syndromePolynomial = CoefficientsToPolynomial(syndromeCoeffs);
            var syndromeBinary = CoefficientsToBinary(syndromeCoeffs, parityBits);

            result.Syndrome = syndromeBinary;
            result.SyndromePolynomial = syndromePolynomial;

            // Проверяем, есть ли ошибка
            bool hasError = !IsZero(syndromeCoeffs);
            result.HasError = hasError;

            result.Steps.Add(new ErrorDetectionStep
            {
                Description = "Шаг 2: Вычислить синдром s(x) = r(x) mod g(x)",
                Calculation = $"{receivedPolynomial} mod ({input.GeneratorPolynomial})",
                Result = $"s(x) = {syndromePolynomial} = {syndromeBinary}"
            });

            if (hasError)
            {
                result.Steps.Add(new ErrorDetectionStep
                {
                    Description = "Обнаружена ошибка",
                    Calculation = "s(x) ≠ 0",
                    Result = "Синдром не равен нулю, значит в коде есть ошибка"
                });

                // Шаг 3: Строим таблицу синдромов для ошибок в каждой позиции
                result.Steps.Add(new ErrorDetectionStep
                {
                    Description = "Шаг 3: Построить таблицу синдромов для ошибок в каждой позиции",
                    Calculation = "Для каждой позиции i вычисляем синдром ошибки e(x) = x^i",
                    Result = "См. таблицу ниже"
                });

                // Строим таблицу синдромов
                for (int i = 0; i < input.CodeLength; i++)
                {
                    var errorCoeffs = new int[input.CodeLength];
                    errorCoeffs[i] = 1; // Ошибка в позиции i
                    var errorSyndromeCoeffs = PolynomialDivision(errorCoeffs, generatorCoeffs);
                    var errorSyndromePolynomial = CoefficientsToPolynomial(errorSyndromeCoeffs);

                    bool isMatch = ComparePolynomials(syndromeCoeffs, errorSyndromeCoeffs);

                    result.SyndromeTable.Add(new SyndromeTableEntry
                    {
                        Position = i + 1, // Позиция от 1 до n
                        ErrorPolynomial = $"x^{i}",
                        SyndromePolynomial = errorSyndromePolynomial,
                        IsMatch = isMatch
                    });

                    if (isMatch)
                    {
                        result.ErrorPosition = i + 1;
                    }
                }

                // Шаг 4: Исправляем ошибку
                if (result.ErrorPosition > 0)
                {
                    var correctedCode = input.ReceivedCode.ToCharArray();
                    int index = result.ErrorPosition - 1; // Индекс в массиве (0-based)
                    correctedCode[index] = correctedCode[index] == '0' ? '1' : '0';
                    result.CorrectedCode = new string(correctedCode);
                    result.CorrectedPolynomial = BinaryToPolynomial(result.CorrectedCode);

                    result.Steps.Add(new ErrorDetectionStep
                    {
                        Description = "Шаг 4: Исправить ошибку",
                        Calculation = $"Инвертировать бит в позиции {result.ErrorPosition}",
                        Result = $"Исправленный код: {result.CorrectedCode} = {result.CorrectedPolynomial}"
                    });
                }
            }
            else
            {
                result.Steps.Add(new ErrorDetectionStep
                {
                    Description = "Ошибок не обнаружено",
                    Calculation = "s(x) = 0",
                    Result = "Синдром равен нулю, ошибок нет"
                });
                result.CorrectedCode = input.ReceivedCode;
                result.CorrectedPolynomial = receivedPolynomial;
            }

            return result;
        }

        private int[] ParsePolynomial(string polynomial, int expectedDegree)
        {
            var coeffs = new int[expectedDegree + 1];
            polynomial = polynomial.Replace(" ", "").ToLower();

            var terms = polynomial.Split('+');
            foreach (var term in terms)
            {
                if (term.Contains("x^"))
                {
                    var parts = term.Split('^');
                    int degree = int.Parse(parts[1]);
                    if (degree <= expectedDegree)
                    {
                        coeffs[degree] = 1;
                    }
                }
                else if (term == "x")
                {
                    coeffs[1] = 1;
                }
                else if (term == "1")
                {
                    coeffs[0] = 1;
                }
            }

            return coeffs;
        }

        private string BinaryToPolynomial(string binary)
        {
            var terms = new List<string>();
            for (int i = 0; i < binary.Length; i++)
            {
                if (binary[i] == '1')
                {
                    int degree = binary.Length - 1 - i;
                    if (degree == 0)
                    {
                        terms.Add("1");
                    }
                    else if (degree == 1)
                    {
                        terms.Add("x");
                    }
                    else
                    {
                        terms.Add($"x^{degree}");
                    }
                }
            }
            return terms.Count > 0 ? string.Join("+", terms) : "0";
        }

        private int[] BinaryToCoefficients(string binary)
        {
            var coeffs = new int[binary.Length];
            for (int i = 0; i < binary.Length; i++)
            {
                coeffs[binary.Length - 1 - i] = binary[i] == '1' ? 1 : 0;
            }
            return coeffs;
        }

        private int[] PolynomialDivision(int[] dividend, int[] divisor)
        {
            int dividendDegree = GetDegree(dividend);
            int divisorDegree = GetDegree(divisor);

            var remainder = (int[])dividend.Clone();

            while (dividendDegree >= divisorDegree)
            {
                int shift = dividendDegree - divisorDegree;

                for (int i = 0; i <= divisorDegree; i++)
                {
                    remainder[i + shift] = (remainder[i + shift] + divisor[i]) % 2;
                }

                dividendDegree = GetDegree(remainder);
            }

            // Возвращаем остаток нужной длины
            int remainderDegree = GetDegree(remainder);
            var result = new int[divisorDegree];
            for (int i = 0; i <= remainderDegree && i < divisorDegree; i++)
            {
                result[i] = remainder[i];
            }
            return result;
        }

        private int GetDegree(int[] coeffs)
        {
            for (int i = coeffs.Length - 1; i >= 0; i--)
            {
                if (coeffs[i] != 0)
                    return i;
            }
            return -1;
        }

        private string CoefficientsToPolynomial(int[] coeffs)
        {
            var terms = new List<string>();
            for (int i = coeffs.Length - 1; i >= 0; i--)
            {
                if (coeffs[i] == 1)
                {
                    if (i == 0)
                    {
                        terms.Add("1");
                    }
                    else if (i == 1)
                    {
                        terms.Add("x");
                    }
                    else
                    {
                        terms.Add($"x^{i}");
                    }
                }
            }
            return terms.Count > 0 ? string.Join("+", terms) : "0";
        }

        private string CoefficientsToBinary(int[] coeffs, int length)
        {
            var binary = new StringBuilder();
            for (int i = length - 1; i >= 0; i--)
            {
                binary.Append(i < coeffs.Length ? coeffs[i].ToString() : "0");
            }
            return binary.ToString();
        }

        private bool IsZero(int[] coeffs)
        {
            return coeffs.All(c => c == 0);
        }

        private bool ComparePolynomials(int[] poly1, int[] poly2)
        {
            int maxLen = Math.Max(poly1.Length, poly2.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int val1 = i < poly1.Length ? poly1[i] : 0;
                int val2 = i < poly2.Length ? poly2[i] : 0;
                if (val1 != val2)
                    return false;
            }
            return true;
        }
    }
}

