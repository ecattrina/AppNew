// Функция для преобразования степеней в надстрочные символы
function formatPolynomials() {
    // Находим все элементы, которые могут содержать полиномы
    // Исключаем input поля, script, style
    const selector = 'p, div, td, li, span, code, .card-body, .alert, h5, h6';
    const allElements = document.querySelectorAll(selector);
    
    allElements.forEach(element => {
        // Пропускаем input поля и элементы, которые уже обработаны
        if (element.tagName === 'INPUT' || 
            element.tagName === 'SCRIPT' || 
            element.tagName === 'STYLE' ||
            element.classList.contains('polynomial-formatted') ||
            element.closest('script') ||
            element.closest('style')) {
            return;
        }
        
        // Проверяем, есть ли в элементе текст со степенями
        const text = element.textContent || element.innerText;
        if (!text || !text.match(/x\^/)) {
            return;
        }
        
        // Если элемент содержит только текст (нет дочерних элементов с HTML)
        if (element.children.length === 0 && element.textContent === text) {
            let html = element.innerHTML;
            // Заменяем x^число на x<sup>число</sup>
            html = html.replace(/x\^(\d+)/g, 'x<sup>$1</sup>');
            // Заменяем x^r на x<sup>r</sup>
            html = html.replace(/x\^([a-z])/g, 'x<sup>$1</sup>');
            // Заменяем x^i на x<sup>i</sup>
            html = html.replace(/x\^(i)/g, 'x<sup>i</sup>');
            
            if (html !== element.innerHTML) {
                element.innerHTML = html;
                element.classList.add('polynomial-formatted');
            }
        } else {
            // Обрабатываем текстовые узлы внутри элемента
            const walker = document.createTreeWalker(
                element,
                NodeFilter.SHOW_TEXT,
                {
                    acceptNode: function(node) {
                        const parent = node.parentElement;
                        if (parent && (parent.tagName === 'SCRIPT' || parent.tagName === 'STYLE' || parent.tagName === 'INPUT')) {
                            return NodeFilter.FILTER_REJECT;
                        }
                        if (node.textContent && node.textContent.match(/x\^/)) {
                            return NodeFilter.FILTER_ACCEPT;
                        }
                        return NodeFilter.FILTER_REJECT;
                    }
                }
            );
            
            const textNodes = [];
            let node;
            while (node = walker.nextNode()) {
                textNodes.push(node);
            }
            
            // Обрабатываем в обратном порядке, чтобы не сбить индексы
            for (let i = textNodes.length - 1; i >= 0; i--) {
                const textNode = textNodes[i];
                let text = textNode.textContent;
                let newText = text;
                
                // Заменяем степени
                newText = newText.replace(/x\^(\d+)/g, 'x<sup>$1</sup>');
                newText = newText.replace(/x\^([a-z])/g, 'x<sup>$1</sup>');
                
                if (newText !== text) {
                    const tempDiv = document.createElement('span');
                    tempDiv.innerHTML = newText;
                    tempDiv.classList.add('polynomial-formatted');
                    
                    const parent = textNode.parentNode;
                    const fragment = document.createDocumentFragment();
                    while (tempDiv.firstChild) {
                        fragment.appendChild(tempDiv.firstChild);
                    }
                    parent.replaceChild(fragment, textNode);
                }
            }
        }
    });
}

// Запускаем форматирование после загрузки DOM
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        setTimeout(formatPolynomials, 200);
    });
} else {
    setTimeout(formatPolynomials, 200);
}

// Обрабатываем динамически добавляемый контент
const observer = new MutationObserver(function(mutations) {
    let shouldFormat = false;
    mutations.forEach(function(mutation) {
        if (mutation.addedNodes.length > 0) {
            for (let node of mutation.addedNodes) {
                if (node.nodeType === 1 && !node.classList.contains('polynomial-formatted')) {
                    shouldFormat = true;
                    break;
                }
            }
        }
    });
    if (shouldFormat) {
        setTimeout(formatPolynomials, 300);
    }
});

const mainContent = document.querySelector('main, .container');
if (mainContent) {
    observer.observe(mainContent, {
        childList: true,
        subtree: true
    });
}
