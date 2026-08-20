document.addEventListener('DOMContentLoaded', function () {
    if (typeof katex === 'undefined') return;
    document.querySelectorAll('.math').forEach(function (el) {
        var tex = el.textContent.trim()
            .replace(/^\\[\(\[]/, '')
            .replace(/\\[\)\]]$/, '');
        katex.render(tex, el, {
            throwOnError: false,
            displayMode: el.tagName === 'DIV'
        });
    });
});
