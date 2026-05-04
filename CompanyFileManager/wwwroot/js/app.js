// Theme helpers
window.getLocalStorage = (key) => localStorage.getItem(key);
window.setLocalStorage = (key, value) => localStorage.setItem(key, value);

// Schedule navigation after delay (for restart redirect)
window.scheduleRedirect = (url, delayMs) => {
    setTimeout(() => { window.location.href = url; }, delayMs);
};

window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
    } catch {
        const el = document.createElement('textarea');
        el.value = text; document.body.appendChild(el);
        el.select(); document.execCommand('copy');
        document.body.removeChild(el);
    }
};

// Drag & Drop file upload
const _dndCleanup = {};

window.initDragDrop = (elementId, dotnetRef) => {
    const el = document.getElementById(elementId);
    if (!el) return;

    const onDragOver = (e) => { e.preventDefault(); el.classList.add('drag-active'); };
    const onDragLeave = (e) => { if (!el.contains(e.relatedTarget)) el.classList.remove('drag-active'); };
    const onDrop = async (e) => {
        e.preventDefault();
        el.classList.remove('drag-active');
        const files = Array.from(e.dataTransfer?.files ?? []);
        if (files.length === 0) return;
        for (const file of files) {
            await dotnetRef.invokeMethodAsync('OnDropFile', file.name, file.size);
        }
        // Store files for upload
        window._pendingDropFiles = files;
        await dotnetRef.invokeMethodAsync('OnDropComplete', files.length);
    };

    el.addEventListener('dragover', onDragOver);
    el.addEventListener('dragleave', onDragLeave);
    el.addEventListener('drop', onDrop);

    _dndCleanup[elementId] = () => {
        el.removeEventListener('dragover', onDragOver);
        el.removeEventListener('dragleave', onDragLeave);
        el.removeEventListener('drop', onDrop);
    };
};

window.destroyDragDrop = (elementId) => {
    if (_dndCleanup[elementId]) { _dndCleanup[elementId](); delete _dndCleanup[elementId]; }
};

// Upload pending dropped files to server
window.uploadDroppedFiles = async (uploadUrl) => {
    const files = window._pendingDropFiles ?? [];
    const results = [];
    for (const file of files) {
        try {
            const fd = new FormData();
            fd.append('file', file);
            const r = await fetch(uploadUrl, { method: 'POST', body: fd });
            results.push({ name: file.name, ok: r.ok, status: r.status });
        } catch (e) {
            results.push({ name: file.name, ok: false, status: 0, error: e.message });
        }
    }
    window._pendingDropFiles = [];
    return JSON.stringify(results);
};
