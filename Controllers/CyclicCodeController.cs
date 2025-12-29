using AppNew.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;

namespace AppNew.Controllers
{
    public class CyclicCodeController : Controller
    {
        public IActionResult Index()
        {
            var model = new CyclicCodeInputModel
            {
                CodeLength = 7,
                DataBits = 4,
                GeneratorPolynomial = "x^3+x+1",
                InformationCombinations = new List<string> { "1111", "0110" }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Encode(CyclicCodeInputModel input)
        {
            // Валидация
            if (input.InformationCombinations == null || input.InformationCombinations.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо указать хотя бы одну информационную комбинацию");
            }

            // Проверка длины комбинаций
            foreach (var combo in input.InformationCombinations ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(combo))
                {
                    ModelState.AddModelError("", "Информационная комбинация не может быть пустой");
                }
                else if (combo.Length != input.DataBits)
                {
                    ModelState.AddModelError("", $"Комбинация '{combo}' должна содержать {input.DataBits} бит");
                }
                else if (!Regex.IsMatch(combo, @"^[01]+$"))
                {
                    ModelState.AddModelError("", $"Комбинация '{combo}' должна содержать только 0 и 1");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Index", input);
            }

            var result = EncodeCombinations(input);
            return View("Result", result);
        }

        private CyclicCodeResultModel EncodeCombinations(CyclicCodeInputModel input)
        {
            var result = new CyclicCodeResultModel
            {
                CodeLength = input.CodeLength,
                DataBits = input.DataBits,
                ParityBits = input.CodeLength - input.DataBits,
                GeneratorPolynomial = input.GeneratorPolynomial
            };

            // Парсим порождающий многочлен
            int parityBits = input.CodeLength - input.DataBits;
            var generatorCoeffs = ParsePolynomial(input.GeneratorPolynomial, parityBits);

            foreach (var infoCombo in input.InformationCombinations)
            {
                var encodingResult = EncodeSingleCombination(infoCombo, generatorCoeffs, input.DataBits, parityBits);
                result.EncodingResults.Add(encodingResult);
            }

            return result;
        }

        private EncodingResult EncodeSingleCombination(string informationCombination, int[] generatorCoeffs, int dataBits, int parityBits)
        {
            var result = new EncodingResult
            {
                InformationCombination = informationCombination
            };

            // Шаг 1: Записать информационный полином m(x)
            var infoPolynomial = BinaryToPolynomial(informationCombination);
            result.InformationPolynomial = infoPolynomial;
            result.Steps.Add(new EncodingStep
            {
                Description = "Шаг 1: Записать информационный полином m(x)",
                Calculation = $"m(x) = {infoPolynomial}",
                Result = $"Для комбинации {informationCombination}: m(x) = {infoPolynomial}"
            });

            // Шаг 2: Умножить m(x) на x^r
            var shiftedPolynomial = MultiplyByXPower(infoPolynomial, parityBits);
            result.ShiftedPolynomial = shiftedPolynomial;
            result.Steps.Add(new EncodingStep
            {
                Description = $"Шаг 2: Умножить m(x) на x^{parityBits}",
                Calculation = $"x^{parityBits} * m(x) = x^{parityBits} * ({infoPolynomial})",
                Result = $"x^{parityBits} * m(x) = {shiftedPolynomial}"
            });

            // Шаг 3: Разделить на g(x) и найти остаток r(x)
            var shiftedCoeffs = PolynomialToCoefficients(shiftedPolynomial, dataBits + parityBits);
            var remainderCoeffs = PolynomialDivision(shiftedCoeffs, generatorCoeffs);
            var remainderPolynomial = CoefficientsToPolynomial(remainderCoeffs);
            var remainderBinary = CoefficientsToBinary(remainderCoeffs, parityBits);

            result.Remainder = remainderBinary;
            result.RemainderPolynomial = remainderPolynomial;
            result.Steps.Add(new EncodingStep
            {
                Description = "Шаг 3: Разделить полученный полином на g(x) и найти остаток r(x)",
                Calculation = $"{shiftedPolynomial} / g(x) = ... (остаток)",
                Result = $"r(x) = {remainderPolynomial} = {remainderBinary}"
            });

            // Шаг 4: Кодовая комбинация c(x) = x^r * m(x) + r(x)
            // Массив коэффициентов: индекс = степень x
            // codewordCoeffs[0] = коэффициент при x⁰, codewordCoeffs[n-1] = коэффициент при x^(n-1)
            var codewordCoeffs = new int[dataBits + parityBits];
            
            // Записываем информационные биты в позиции от x^r до x^(r+k-1)
            // informationCombination читается слева направо как старший бит к младшему
            for (int i = 0; i < dataBits; i++)
            {
                int bitValue = informationCombination[i] == '1' ? 1 : 0;
                int position = parityBits + (dataBits - 1 - i);
                codewordCoeffs[position] = bitValue;
            }
            
            // Добавляем остаток в младшие позиции (от x⁰ до x^(r-1))
            for (int i = 0; i < remainderCoeffs.Length && i < parityBits; i++)
            {
                codewordCoeffs[i] = (codewordCoeffs[i] + remainderCoeffs[i]) % 2;
            }

            // Преобразуем в строку: от старшего бита (x^(n-1)) к младшему (x⁰)
            var codewordArray = new List<int>();
            for (int i = codewordCoeffs.Length - 1; i >= 0; i--)
            {
                codewordArray.Add(codewordCoeffs[i]);
            }
            var codeword = string.Join("", codewordArray.Select(c => c.ToString()));
            var codewordPolynomial = CoefficientsToPolynomial(codewordCoeffs);

            result.Codeword = codeword;
            result.CodewordPolynomial = codewordPolynomial;
            result.Steps.Add(new EncodingStep
            {
                Description = "Шаг 4: Кодовая комбинация c(x) = x^r * m(x) + r(x)",
                Calculation = $"c(x) = {shiftedPolynomial} + {remainderPolynomial}",
                Result = $"c(x) = {codewordPolynomial} = {codeword}"
            });

            return result;
        }

        private int[] ParsePolynomial(string polynomial, int expectedDegree)
        {
            // Парсим многочлен вида "x^3+x+1" или "x^3 + x + 1"
            var coeffs = new int[expectedDegree + 1];
            polynomial = polynomial.Replace(" ", "").ToLower();

            // Обрабатываем каждый член
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

        private string MultiplyByXPower(string polynomial, int power)
        {
            if (polynomial == "0")
                return "0";

            var terms = polynomial.Split('+');
            var newTerms = new List<string>();
            foreach (var term in terms)
            {
                if (term == "1")
                {
                    newTerms.Add($"x^{power}");
                }
                else if (term == "x")
                {
                    newTerms.Add($"x^{power + 1}");
                }
                else if (term.StartsWith("x^"))
                {
                    int degree = int.Parse(term.Substring(2));
                    newTerms.Add($"x^{degree + power}");
                }
            }
            return string.Join("+", newTerms);
        }

        private int[] PolynomialToCoefficients(string polynomial, int maxLength)
        {
            var coeffs = new int[maxLength];
            if (polynomial == "0")
                return coeffs;

            var terms = polynomial.Split('+');
            foreach (var term in terms)
            {
                if (term == "1")
                {
                    coeffs[0] = 1;
                }
                else if (term == "x")
                {
                    coeffs[1] = 1;
                }
                else if (term.StartsWith("x^"))
                {
                    int degree = int.Parse(term.Substring(2));
                    if (degree < maxLength)
                    {
                        coeffs[degree] = 1;
                    }
                }
            }
            return coeffs;
        }

        private int[] PolynomialDivision(int[] dividend, int[] divisor)
        {
            // Деление полиномов по модулю 2
            int dividendDegree = GetDegree(dividend);
            int divisorDegree = GetDegree(divisor);

            var remainder = (int[])dividend.Clone();
            var quotient = new int[dividend.Length];

            while (dividendDegree >= divisorDegree)
            {
                int shift = dividendDegree - divisorDegree;
                quotient[shift] = 1;

                // Вычитаем (XOR) делитель, сдвинутый на shift позиций
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
    }
}
