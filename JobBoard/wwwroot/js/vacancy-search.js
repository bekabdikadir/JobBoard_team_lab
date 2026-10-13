(() => {
    const form = document.querySelector('#vacancyFilters');
    const results = document.querySelector('#vacancyResults');
    if (!form || !results) return;
    let timer;
    async function search(page = 1) {
        const params = new URLSearchParams(new FormData(form));
        params.set('page', page);
        results.setAttribute('aria-busy', 'true');
        const response = await fetch(`${form.dataset.searchUrl}?${params}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (response.ok) {
            results.innerHTML = await response.text();
            history.replaceState(null, '', `${form.action}?${params}`);
        }
        results.removeAttribute('aria-busy');
    }
    form.addEventListener('submit', event => { event.preventDefault(); search(); });
    form.querySelector('[name="q"]').addEventListener('input', () => { clearTimeout(timer); timer = setTimeout(() => search(), 300); });
    form.querySelectorAll('select').forEach(select => select.addEventListener('change', () => search()));
    results.addEventListener('click', event => { const button = event.target.closest('[data-page]'); if (button) search(button.dataset.page); });
})();
