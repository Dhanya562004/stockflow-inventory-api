/**
 * @jest-environment jsdom
 */

const {
    handleStockAdjustment,
    renderProductList,
    loadProducts,
    handleAiStockAnalysis,
    renderAiSummary,
    initStockPage
} = require('../../wwwroot/js/stock.js');

describe('Stock Management Frontend Unit Tests', () => {
    beforeEach(() => {
        document.body.innerHTML = `
            <form id="stockAdjustmentForm">
                <input type="text" id="productId" value="3fa85f64-5717-4562-b3fc-2c963f66afa6" />
                <input type="number" id="delta" value="5" />
                <button type="submit" id="submitBtn">Adjust Stock</button>
            </form>
            <div id="statusMessage" class="status-box" role="status" aria-live="polite"></div>
            <div id="quantityDisplay" class="quantity-display" role="status" aria-live="polite"></div>
            <div id="emptyState" class="empty-state" role="status" aria-live="polite" style="display: none;"></div>
            <table id="productTable" class="product-table" style="display: none;">
                <thead>
                    <tr>
                        <th>Product Name</th>
                        <th>SKU</th>
                        <th>Current Stock</th>
                        <th>Product ID</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody id="productTableBody"></tbody>
            </table>

            <input type="number" id="aiThreshold" value="20" />
            <button type="button" id="aiAnalyzeBtn">Analyze Stock with AI</button>
            <div id="aiLoadingIndicator" style="display: none;"></div>
            <div id="aiStatusMessage" class="status-box" role="status" style="display: none;"></div>
            <div id="aiResultCard" style="display: none;">
                <span id="aiItemsCount"></span>
                <p id="aiExecutiveSummary"></p>
                <div id="aiRecommendedAction"></div>
                <ul id="aiItemsList"></ul>
            </div>
        `;
        global.fetch = jest.fn();
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    test('200 path updates the DOM with success message and new quantity', async () => {
        // Arrange
        global.fetch.mockResolvedValueOnce({
            status: 200,
            ok: true,
            json: async () => ({
                productId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
                quantity: 42,
                message: 'Stock adjusted successfully.'
            })
        });

        // Mock subsequent loadProducts call
        global.fetch.mockResolvedValueOnce({
            status: 200,
            ok: true,
            json: async () => []
        });

        const statusMessage = document.getElementById('statusMessage');
        const quantityDisplay = document.getElementById('quantityDisplay');

        // Act
        await handleStockAdjustment();

        // Assert
        expect(statusMessage.className).toBe('status-box success');
        expect(statusMessage.textContent).toBe('Stock adjusted successfully.');
        expect(quantityDisplay.textContent).toBe('Updated Quantity: 42');
    });

    test('400 response shows validation messages from the response body', async () => {
        global.fetch.mockResolvedValueOnce({
            status: 400,
            ok: false,
            json: async () => ({
                statusCode: 400,
                message: 'Validation failed.',
                errors: {
                    delta: ['Adjustment amount delta cannot be zero.'],
                    productId: ['Invalid product GUID format.']
                }
            })
        });

        const statusMessage = document.getElementById('statusMessage');

        await handleStockAdjustment();

        expect(statusMessage.className).toBe('status-box error');
        expect(statusMessage.textContent).toContain('Adjustment amount delta cannot be zero.');
        expect(statusMessage.textContent).toContain('Invalid product GUID format.');
    });

    test('404 response shows "product not found"', async () => {
        global.fetch.mockResolvedValueOnce({
            status: 404,
            ok: false,
            json: async () => ({
                message: 'Product with specified ID was not found.'
            })
        });

        const statusMessage = document.getElementById('statusMessage');

        await handleStockAdjustment();

        expect(statusMessage.className).toBe('status-box error');
        expect(statusMessage.textContent).toBe('product not found');
    });

    test('409 response shows "stock would go negative"', async () => {
        global.fetch.mockResolvedValueOnce({
            status: 409,
            ok: false,
            json: async () => ({
                message: 'Adjustment rejected: resulting stock cannot be negative.'
            })
        });

        const statusMessage = document.getElementById('statusMessage');

        await handleStockAdjustment();

        expect(statusMessage.className).toBe('status-box error');
        expect(statusMessage.textContent).toBe('stock would go negative');
    });

    test('500 server error shows a retry message', async () => {
        global.fetch.mockResolvedValueOnce({
            status: 500,
            ok: false,
            json: async () => ({ message: 'Internal Server Error' })
        });

        const statusMessage = document.getElementById('statusMessage');

        await handleStockAdjustment();

        expect(statusMessage.className).toBe('status-box error');
        expect(statusMessage.textContent).toContain('retry');
    });

    test('network exception shows a retry message', async () => {
        global.fetch.mockRejectedValueOnce(new Error('Failed to fetch'));

        const statusMessage = document.getElementById('statusMessage');

        await handleStockAdjustment();

        expect(statusMessage.className).toBe('status-box error');
        expect(statusMessage.textContent).toContain('retry');
    });

    test('submit button is disabled while request is in flight and re-enabled afterwards', async () => {
        let buttonStateDuringFetch = null;
        const submitBtn = document.getElementById('submitBtn');

        global.fetch.mockImplementationOnce(() => {
            buttonStateDuringFetch = submitBtn.disabled;
            return Promise.resolve({
                status: 200,
                ok: true,
                json: async () => ({ quantity: 15, message: 'Done' })
            });
        });

        global.fetch.mockResolvedValueOnce({
            status: 200,
            ok: true,
            json: async () => []
        });

        expect(submitBtn.disabled).toBe(false);

        const adjustPromise = handleStockAdjustment();

        expect(buttonStateDuringFetch).toBe(true);

        await adjustPromise;

        expect(submitBtn.disabled).toBe(false);
    });

    test('product list renders an empty state when there are no products', () => {
        const emptyState = document.getElementById('emptyState');
        const productTable = document.getElementById('productTable');

        renderProductList([]);

        expect(emptyState.style.display).toBe('block');
        expect(emptyState.textContent).toBe('No products available.');
        expect(productTable.style.display).toBe('none');
    });

    test('product list renders rows using createElement and textContent, and allows row selection', () => {
        const emptyState = document.getElementById('emptyState');
        const productTable = document.getElementById('productTable');
        const tbody = document.getElementById('productTableBody');
        const productIdInput = document.getElementById('productId');

        const products = [
            {
                id: '11111111-1111-1111-1111-111111111111',
                name: 'Mechanical Keyboard',
                sku: 'MK-100',
                quantityInStock: 12
            }
        ];

        renderProductList(products);

        expect(emptyState.style.display).toBe('none');
        expect(productTable.style.display).toBe('table');
        expect(tbody.children.length).toBe(1);

        const row = tbody.children[0];
        expect(row.children[0].textContent).toBe('Mechanical Keyboard');
        expect(row.children[1].textContent).toBe('MK-100');
        expect(row.children[2].textContent).toBe('12');
        expect(row.children[3].textContent).toBe('11111111-1111-1111-1111-111111111111');

        // Test clicking Select button populates input
        const selectBtn = row.querySelector('.select-btn');
        selectBtn.click();
        expect(productIdInput.value).toBe('11111111-1111-1111-1111-111111111111');
    });

    test('handleAiStockAnalysis 200 path renders AI summary, recommended action, and items', async () => {
        const analyzeBtn = document.getElementById('aiAnalyzeBtn');
        const loadingIndicator = document.getElementById('aiLoadingIndicator');
        const resultCard = document.getElementById('aiResultCard');
        const executiveSummary = document.getElementById('aiExecutiveSummary');
        const recommendedAction = document.getElementById('aiRecommendedAction');
        const itemsList = document.getElementById('aiItemsList');
        const itemsCount = document.getElementById('aiItemsCount');

        global.fetch.mockResolvedValueOnce({
            status: 200,
            ok: true,
            json: async () => ({
                threshold: 20,
                itemCount: 2,
                summary: 'Immediate purchase order needed for low stock peripherals.',
                analysis: {
                    summary: 'Immediate purchase order needed for low stock peripherals.',
                    items: ['Logitech MX Master 3S (Stock: 12)', 'Dell Laptop (Stock: 5)'],
                    recommendedAction: 'Order 30 units of Logitech mouse from Supplier A.'
                }
            })
        });

        await handleAiStockAnalysis();

        expect(analyzeBtn.disabled).toBe(false);
        expect(loadingIndicator.style.display).toBe('none');
        expect(resultCard.style.display).toBe('block');
        expect(executiveSummary.textContent).toBe('Immediate purchase order needed for low stock peripherals.');
        expect(recommendedAction.textContent).toBe('Order 30 units of Logitech mouse from Supplier A.');
        expect(itemsCount.textContent).toBe('2 At-Risk Items');
        expect(itemsList.children.length).toBe(2);
        expect(itemsList.children[0].textContent).toBe('Logitech MX Master 3S (Stock: 12)');
    });

    test('handleAiStockAnalysis handles 503 service unavailable cleanly', async () => {
        const statusMessage = document.getElementById('aiStatusMessage');
        const resultCard = document.getElementById('aiResultCard');

        global.fetch.mockResolvedValueOnce({
            status: 503,
            ok: false,
            json: async () => ({ message: 'LLM summary service is currently unavailable.' })
        });

        await handleAiStockAnalysis();

        expect(resultCard.style.display).toBe('none');
        expect(statusMessage.style.display).toBe('block');
        expect(statusMessage.textContent).toBe('LLM summary service is currently unavailable.');
    });

    test('handleAiStockAnalysis handles network exceptions gracefully', async () => {
        const statusMessage = document.getElementById('aiStatusMessage');
        const resultCard = document.getElementById('aiResultCard');

        global.fetch.mockRejectedValueOnce(new Error('Network error'));

        await handleAiStockAnalysis();

        expect(resultCard.style.display).toBe('none');
        expect(statusMessage.style.display).toBe('block');
        expect(statusMessage.textContent).toContain('Network connection failure');
    });
});

