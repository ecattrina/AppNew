// Словарь терминов с определениями
const termDefinitions = {
    'бит': 'Бит (binary digit) - минимальная единица информации в двоичной системе счисления, может принимать значение 0 или 1.',
    'синдром': 'Синдром - остаток от деления принятого кодового слова на порождающий многочлен. Используется для обнаружения и исправления ошибок в коде. Если синдром равен нулю, ошибок нет; если не равен нулю, указывает на позицию ошибки.',
    'полином': 'Полином (многочлен) - математическое выражение вида a₀ + a₁x + a₂x² + ... + aₙxⁿ, где коэффициенты aᵢ принадлежат полю GF(2) (0 или 1). В теории кодирования используется для представления кодовых комбинаций.',
    'многочлен': 'Полином (многочлен) - математическое выражение вида a₀ + a₁x + a₂x² + ... + aₙxⁿ, где коэффициенты aᵢ принадлежат полю GF(2) (0 или 1). В теории кодирования используется для представления кодовых комбинаций.',
    'позиция': 'Позиция - номер разряда (бита) в кодовой комбинации. Обычно нумерация начинается с 1 (слева направо) или с 0 (справа налево).',
    'вероятность безотказной работы': 'Вероятность безотказной работы P(t) - вероятность того, что объект будет работать безотказно в течение времени t. Вычисляется как отношение числа работоспособных объектов к общему числу объектов: P(t) = (N - n(t)) / N.',
    'вероятность отказов': 'Вероятность отказов F(t) - вероятность того, что объект откажет в течение времени t. Вычисляется как отношение числа отказавших объектов к общему числу объектов: F(t) = n(t) / N. Связана с вероятностью безотказной работы: F(t) = 1 - P(t).',
    'плотность распределения отказов': 'Плотность распределения отказов f(t) - статистическая оценка интенсивности отказов в единицу времени. Вычисляется как отношение приращения числа отказов к произведению общего числа объектов на интервал времени: f(t) = Δn / (N · Δt).',
    'интенсивность отказов': 'Интенсивность отказов λ(t) - условная вероятность отказа объекта в единицу времени при условии, что он работал до этого момента. Вычисляется как отношение приращения числа отказов к произведению числа работоспособных объектов на начало интервала на длину интервала: λ(t) = Δn / ((N - n(t-1)) · Δt).',
    'код хэмминга': 'Код Хэмминга - линейный блочный код, способный обнаруживать и исправлять одиночные ошибки. Контрольные биты располагаются на позициях, являющихся степенями двойки (1, 2, 4, 8, ...).',
    'циклический код': 'Циклический код - линейный блочный код, обладающий свойством цикличности: любая циклическая перестановка кодового слова также является кодовым словом. Кодирование и декодирование выполняются с помощью операций над полиномами в поле GF(2).',
    'порождающий многочлен': 'Порождающий многочлен g(x) - многочлен степени r = n - k, используемый для кодирования и декодирования в циклическом коде. Все кодовые слова делятся на g(x) без остатка. Остаток от деления используется для обнаружения ошибок.',
    'контрольный бит': 'Контрольный бит (бит четности) - дополнительный бит, добавляемый к информационным битам для обнаружения и исправления ошибок. Значение контрольного бита выбирается так, чтобы сумма контролируемых битов (по модулю 2) была четной.',
    'бит четности': 'Контрольный бит (бит четности) - дополнительный бит, добавляемый к информационным битам для обнаружения и исправления ошибок. Значение контрольного бита выбирается так, чтобы сумма контролируемых битов (по модулю 2) была четной.',
    'информационный бит': 'Информационный бит - бит, несущий полезную информацию. В коде (n, k) количество информационных битов равно k, остальные (n - k) битов являются контрольными.',
    'кодовая комбинация': 'Кодовая комбинация (кодовое слово) - последовательность битов, полученная в результате кодирования информационной комбинации. Содержит как информационные, так и контрольные биты.',
    'кодовое слово': 'Кодовая комбинация (кодовое слово) - последовательность битов, полученная в результате кодирования информационной комбинации. Содержит как информационные, так и контрольные биты.',
    'информационная комбинация': 'Информационная комбинация - исходная последовательность битов, подлежащая кодированию. После кодирования преобразуется в кодовую комбинацию.',
    'остаток': 'Остаток r(x) - результат деления полинома на порождающий многочлен g(x) по модулю 2. В циклических кодах остаток используется для формирования контрольных битов при кодировании и для вычисления синдрома при декодировании.',
    'деление по модулю 2': 'Деление по модулю 2 - операция деления полиномов в поле GF(2), где все коэффициенты равны 0 или 1, а сложение и вычитание эквивалентны операции XOR (исключающее ИЛИ).',
    'gf(2)': 'GF(2) - поле Галуа из двух элементов (0 и 1). В этом поле сложение и вычитание эквивалентны операции XOR, умножение - операции AND.',
    'поле gf(2)': 'GF(2) - поле Галуа из двух элементов (0 и 1). В этом поле сложение и вычитание эквивалентны операции XOR, умножение - операции AND.'
};

// Функция для экранирования HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Функция для обработки текстового узла
function processTextNode(textNode) {
    const parent = textNode.parentNode;
    
    // Пропускаем элементы, которые не должны обрабатываться
    if (parent.tagName === 'SCRIPT' || parent.tagName === 'STYLE' || 
        parent.tagName === 'CODE' || parent.tagName === 'PRE' ||
        parent.classList.contains('term-tooltip') || 
        parent.classList.contains('no-term-tooltip')) {
        return;
    }
    
    const text = textNode.textContent;
    if (!text || text.trim().length === 0) {
        return;
    }
    
    // Сортируем термины по длине (от длинных к коротким)
    const sortedTerms = Object.keys(termDefinitions).sort((a, b) => b.length - a.length);
    
    let newText = text;
    let hasMatches = false;
    const replacements = [];
    
    sortedTerms.forEach(term => {
        // Создаем регулярное выражение для поиска слова целиком
        const regex = new RegExp(`\\b(${term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})\\b`, 'gi');
        let match;
        
        while ((match = regex.exec(newText)) !== null) {
            // Проверяем, что это не часть уже обработанного термина
            const start = match.index;
            const end = start + match[0].length;
            let isOverlap = false;
            
            for (const rep of replacements) {
                if ((start >= rep.start && start < rep.end) || 
                    (end > rep.start && end <= rep.end) ||
                    (start <= rep.start && end >= rep.end)) {
                    isOverlap = true;
                    break;
                }
            }
            
            if (!isOverlap) {
                replacements.push({
                    start: start,
                    end: end,
                    term: match[0],
                    definition: termDefinitions[term.toLowerCase()]
                });
                hasMatches = true;
            }
        }
    });
    
    if (hasMatches && replacements.length > 0) {
        // Сортируем замены по позиции (с конца к началу, чтобы не сбивать индексы)
        replacements.sort((a, b) => b.start - a.start);
        
        // Создаем фрагмент документа с замененными терминами
        const fragment = document.createDocumentFragment();
        let lastIndex = text.length;
        
        replacements.forEach(rep => {
            // Добавляем текст после замены
            if (rep.end < lastIndex) {
                const afterText = document.createTextNode(text.substring(rep.end, lastIndex));
                fragment.insertBefore(afterText, fragment.firstChild);
            }
            
            // Добавляем элемент с tooltip
            const span = document.createElement('span');
            span.className = 'term-tooltip';
            span.setAttribute('data-bs-toggle', 'tooltip');
            span.setAttribute('data-bs-placement', 'top');
            span.setAttribute('data-bs-html', 'true');
            span.title = escapeHtml(rep.definition);
            span.textContent = rep.term;
            fragment.insertBefore(span, fragment.firstChild);
            
            // Добавляем текст перед заменой
            if (rep.start > 0) {
                const beforeText = document.createTextNode(text.substring(0, rep.start));
                fragment.insertBefore(beforeText, fragment.firstChild);
            }
            
            lastIndex = rep.start;
        });
        
        // Заменяем текстовый узел на фрагмент
        parent.replaceChild(fragment, textNode);
        
        // Инициализируем tooltips для новых элементов
        const tooltipElements = fragment.querySelectorAll('.term-tooltip');
        tooltipElements.forEach(el => {
            new bootstrap.Tooltip(el, {
                html: true,
                container: 'body'
            });
        });
    }
}

// Основная функция для обработки терминов
function processTerms() {
    const walker = document.createTreeWalker(
        document.querySelector('main, .container') || document.body,
        NodeFilter.SHOW_TEXT,
        {
            acceptNode: function(node) {
                const parent = node.parentElement;
                if (!parent) return NodeFilter.FILTER_REJECT;
                
                // Пропускаем элементы, которые не должны обрабатываться
                if (parent.tagName === 'SCRIPT' || parent.tagName === 'STYLE' || 
                    parent.tagName === 'CODE' || parent.tagName === 'PRE' ||
                    parent.classList.contains('term-tooltip') || 
                    parent.classList.contains('no-term-tooltip')) {
                    return NodeFilter.FILTER_REJECT;
                }
                
                return NodeFilter.FILTER_ACCEPT;
            }
        }
    );
    
    const textNodes = [];
    let node;
    while (node = walker.nextNode()) {
        textNodes.push(node);
    }
    
    // Обрабатываем узлы в обратном порядке
    for (let i = textNodes.length - 1; i >= 0; i--) {
        processTextNode(textNodes[i]);
    }
}

// Запускаем обработку после загрузки DOM
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        setTimeout(processTerms, 100);
    });
} else {
    setTimeout(processTerms, 100);
}

// Обрабатываем динамически добавляемый контент
const observer = new MutationObserver(function(mutations) {
    let shouldProcess = false;
    mutations.forEach(function(mutation) {
        if (mutation.addedNodes.length > 0) {
            for (let node of mutation.addedNodes) {
                if (node.nodeType === 1 && !node.classList.contains('term-tooltip')) {
                    shouldProcess = true;
                    break;
                }
            }
        }
    });
    if (shouldProcess) {
        setTimeout(processTerms, 200);
    }
});

// Начинаем наблюдение за изменениями
const mainContent = document.querySelector('main, .container');
if (mainContent) {
    observer.observe(mainContent, {
        childList: true,
        subtree: true
    });
}
