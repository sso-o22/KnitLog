window.patternViewer = (() => {
    let pdfDoc = null, pdfCanvas = null, annoCanvas = null;
    let pdfCtx = null, annoCtx = null, dotNetRef = null;
    let isDrawing = false;

    // --- Undo를 위한 추가 변수 ---
    let paths = [];
    let currentPath = null;
    let currentZoom = 1.0;
    let currentPageNum = 1;
    // --------------------------

    async function ensurePdfJs() {
        if (window.pdfjsLib) return;
        const s = document.createElement('script');
        s.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
        document.head.appendChild(s);
        await new Promise(r => s.onload = r);
        window.pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
    }

    function getCanvases() {
        pdfCanvas = document.getElementById('pdf-canvas');
        annoCanvas = document.getElementById('anno-canvas');
        if (pdfCanvas) pdfCtx = pdfCanvas.getContext('2d');
        if (annoCanvas) annoCtx = annoCanvas.getContext('2d');
    }

    // --- Undo를 위한 다시 그리기 함수 ---
    function redrawPaths() {
        if (!annoCtx) return;
        annoCtx.clearRect(0, 0, annoCanvas.width, annoCanvas.height);

        const pagePaths = paths.filter(p => p.page === currentPageNum);
        pagePaths.forEach(p => {
            annoCtx.beginPath();
            annoCtx.lineWidth = p.size * (currentZoom / p.originZoom); // 배율 대응
            annoCtx.lineCap = 'round';
            annoCtx.lineJoin = 'round';
            annoCtx.strokeStyle = p.color;
            annoCtx.globalCompositeOperation = p.isEraser ? 'destination-out' : 'source-over';

            if (p.points.length > 0) {
                const first = p.points[0];
                annoCtx.moveTo(first.x * currentZoom, first.y * currentZoom);
                for (let i = 1; i < p.points.length; i++) {
                    annoCtx.lineTo(p.points[i].x * currentZoom, p.points[i].y * currentZoom);
                }
                annoCtx.stroke();
            }
        });
        annoCtx.globalCompositeOperation = 'source-over';
    }
    // --------------------------

    return {
        init(ref) { dotNetRef = ref; },
        async loadPdfFromStream(streamRef, zoom) {
            await ensurePdfJs();
            const bytes = new Uint8Array(await streamRef.arrayBuffer());
            pdfDoc = await window.pdfjsLib.getDocument({ data: bytes }).promise;
            await this.renderPage(1, zoom);
            return pdfDoc.numPages;
        },
        async renderPage(pageNum, zoom) {
            if (!pdfDoc) return;
            currentPageNum = pageNum;
            currentZoom = zoom;
            getCanvases();
            const page = await pdfDoc.getPage(pageNum);
            const viewport = page.getViewport({ scale: zoom });
            pdfCanvas.width = annoCanvas.width = viewport.width;
            pdfCanvas.height = annoCanvas.height = viewport.height;
            await page.render({ canvasContext: pdfCtx, viewport }).promise;

            redrawPaths(); // 페이지 전환이나 확대 시 그림 복구
        },
        startDraw(x, y, color, size, isEraser) {
            getCanvases();
            isDrawing = true;

            // --- 경로 저장 시작 ---
            currentPath = {
                page: currentPageNum,
                color: color,
                size: size,
                isEraser: isEraser,
                originZoom: currentZoom,
                points: [{ x: x / currentZoom, y: y / currentZoom }]
            };
            // --------------------

            annoCtx.beginPath();
            annoCtx.moveTo(x, y);
            annoCtx.lineWidth = size;
            annoCtx.lineCap = 'round';
            annoCtx.strokeStyle = isEraser ? '#ffffff' : color;
            annoCtx.globalCompositeOperation = isEraser ? 'destination-out' : 'source-over';
        },
        draw(x, y) {
            if (!isDrawing) return;

            // --- 포인트 저장 ---
            if (currentPath) currentPath.points.push({ x: x / currentZoom, y: y / currentZoom });
            // ------------------

            annoCtx.lineTo(x, y);
            annoCtx.stroke();
        },
        endDraw() {
            isDrawing = false;
            // --- 경로 저장 완료 ---
            if (currentPath) {
                paths.push(currentPath);
                currentPath = null;
            }
        },
        // --- 신규 추가: Undo 함수 ---
        undo() {
            paths.pop();
            redrawPaths();
        },
        getRect() {
            const el = document.getElementById('canvas-container');
            if (!el) return [0, 0, 0, 0];
            const r = el.getBoundingClientRect();
            return [r.left, r.top, r.width, r.height];
        },
        clearAnnotations() {
            getCanvases();
            paths = []; // 데이터 삭제
            if (annoCtx) annoCtx.clearRect(0, 0, annoCanvas.width, annoCanvas.height);
        },
        dispose() { pdfDoc = null; }
    };
})();