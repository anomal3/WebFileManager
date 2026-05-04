// Remote Desktop client — RAF rendering, fullscreen, document-level keyboard
window.RemoteDesktop = (() => {
    let _connection = null;
    let _canvas = null;
    let _ctx = null;
    let _container = null;
    let _sw = 1920, _sh = 1080;
    let _active = false;
    let _pendingImage = null;       // next frame to display
    let _displayedImage = null;     // current
    let _onKeyDown = null, _onKeyUp = null;
    let _rafId = 0;

    function _renderLoop() {
        if (!_active || !_canvas || !_ctx) { _rafId = 0; return; }

        // Resize canvas to its CSS size (HiDPI-aware)
        const dpr = window.devicePixelRatio || 1;
        const rect = _canvas.getBoundingClientRect();
        const w = Math.floor(rect.width * dpr);
        const h = Math.floor(rect.height * dpr);
        if (_canvas.width !== w) _canvas.width = w;
        if (_canvas.height !== h) _canvas.height = h;

        // Promote pending → displayed
        if (_pendingImage && _pendingImage.complete) {
            _displayedImage = _pendingImage;
            _pendingImage = null;
        }

        if (_displayedImage) {
            _ctx.fillStyle = '#000';
            _ctx.fillRect(0, 0, w, h);

            // Aspect-fit (letterbox)
            const ar = _sw / _sh;
            let dw = w, dh = w / ar;
            if (dh > h) { dh = h; dw = h * ar; }
            const dx = (w - dw) / 2;
            const dy = (h - dh) / 2;
            _ctx.imageSmoothingEnabled = true;
            _ctx.imageSmoothingQuality = 'high';
            _ctx.drawImage(_displayedImage, dx, dy, dw, dh);
        }

        _rafId = requestAnimationFrame(_renderLoop);
    }

    function _toScreen(e) {
        const r = _canvas.getBoundingClientRect();
        const ar = _sw / _sh;
        let dw = r.width, dh = r.width / ar;
        if (dh > r.height) { dh = r.height; dw = r.height * ar; }
        const dx = (r.width - dw) / 2;
        const dy = (r.height - dh) / 2;
        const px = e.clientX - r.left - dx;
        const py = e.clientY - r.top - dy;
        return {
            x: Math.max(0, Math.min(_sw, Math.round(px * _sw / dw))),
            y: Math.max(0, Math.min(_sh, Math.round(py * _sh / dh)))
        };
    }

    function _setupInput() {
        if (!_canvas) return;
        _canvas.setAttribute('tabindex', '0');
        _canvas.style.outline = 'none';

        _canvas.addEventListener('mousemove', (e) => {
            if (!_active) return;
            const { x, y } = _toScreen(e);
            _connection.invoke('MoveMouse', x, y).catch(() => {});
        }, { passive: true });

        _canvas.addEventListener('mousedown', (e) => {
            if (!_active) return;
            e.preventDefault();
            _canvas.focus();
            const { x, y } = _toScreen(e);
            _connection.invoke('MouseDown', x, y, e.button).catch(() => {});
        });

        _canvas.addEventListener('mouseup', (e) => {
            if (!_active) return;
            const { x, y } = _toScreen(e);
            _connection.invoke('MouseUp', x, y, e.button).catch(() => {});
        });

        _canvas.addEventListener('contextmenu', (e) => e.preventDefault());

        _canvas.addEventListener('wheel', (e) => {
            if (!_active) return;
            e.preventDefault();
            _connection.invoke('Scroll', Math.round(e.deltaY)).catch(() => {});
        }, { passive: false });

        // Document-level keyboard so user doesn't need to focus canvas
        _onKeyDown = (e) => {
            if (!_active || !_connection) return;
            const ae = document.activeElement;
            if (ae && (ae.tagName === 'INPUT' || ae.tagName === 'TEXTAREA' || ae.isContentEditable)) return;
            // Allow F11 (fullscreen toggle by browser) and Esc to escape
            if (e.key === 'F11') return;
            e.preventDefault();
            _connection.invoke('KeyEvent', e.key, e.code, e.ctrlKey, e.altKey, e.shiftKey, false).catch(() => {});
        };
        _onKeyUp = (e) => {
            if (!_active || !_connection) return;
            const ae = document.activeElement;
            if (ae && (ae.tagName === 'INPUT' || ae.tagName === 'TEXTAREA' || ae.isContentEditable)) return;
            if (e.key === 'F11') return;
            _connection.invoke('KeyEvent', e.key, e.code, e.ctrlKey, e.altKey, e.shiftKey, true).catch(() => {});
        };
        document.addEventListener('keydown', _onKeyDown);
        document.addEventListener('keyup', _onKeyUp);
    }

    function _teardownInput() {
        if (_onKeyDown) document.removeEventListener('keydown', _onKeyDown);
        if (_onKeyUp)   document.removeEventListener('keyup', _onKeyUp);
        _onKeyDown = _onKeyUp = null;
    }

    return {
        async connect(hubUrl, canvasId, containerId) {
            _canvas = document.getElementById(canvasId);
            _container = document.getElementById(containerId);
            if (!_canvas) { console.error('Canvas not found'); return; }
            _ctx = _canvas.getContext('2d', { alpha: false });

            _connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl)
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Warning)
                .build();

            _connection.on('ReceiveFrame', (b64, w, h) => {
                _sw = w; _sh = h;
                const img = new Image();
                img.onload = () => { _pendingImage = img; };
                img.src = 'data:image/jpeg;base64,' + b64;
            });

            _connection.on('Error', (msg) => console.warn('Remote:', msg));

            await _connection.start();
            _active = true;
            _setupInput();
            _renderLoop();
            _canvas.focus();
            await _connection.invoke('StartSession');
        },

        async disconnect() {
            _active = false;
            cancelAnimationFrame(_rafId);
            _rafId = 0;
            _teardownInput();
            if (_connection) {
                try { await _connection.invoke('StopSession'); } catch {}
                try { await _connection.stop(); } catch {}
                _connection = null;
            }
            _displayedImage = null;
            _pendingImage = null;
            if (_ctx && _canvas) {
                _ctx.fillStyle = '#000';
                _ctx.fillRect(0, 0, _canvas.width, _canvas.height);
            }
        },

        async toggleFullscreen() {
            if (!_container) return;
            if (!document.fullscreenElement) {
                try { await _container.requestFullscreen(); } catch {}
            } else {
                try { await document.exitFullscreen(); } catch {}
            }
            // Refocus canvas to keep keyboard input alive
            setTimeout(() => _canvas?.focus(), 100);
        }
    };
})();
