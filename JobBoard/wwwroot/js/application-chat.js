(() => {
    const box = document.querySelector('#chatBox');
    const form = document.querySelector('#chatForm');
    if (!box || !form) return;
    const input = form.querySelector('textarea');
    async function load(scroll = false) {
        const response = await fetch(box.dataset.messagesUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (response.ok) box.innerHTML = await response.text();
        if (scroll) box.scrollTop = box.scrollHeight;
    }
    form.addEventListener('submit', async event => {
        event.preventDefault();
        const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (response.ok) { input.value = ''; await load(true); }
    });
    box.scrollTop = box.scrollHeight;
    setInterval(() => load(false), 5000);
})();
