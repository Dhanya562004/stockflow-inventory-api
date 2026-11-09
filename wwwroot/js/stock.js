/**
 * Stock Management Frontend Script
 * Handles stock adjustments, real-time DOM updates, and product catalog display.
 * Strictly uses textContent to prevent XSS vulnerabilities.
 */

async function handleStockAdjustment(event) {
    if (event) {
        event.preventDefault();
    }

    const productIdInput = document.getElementById('productId');
    const deltaInput = document.getElementById('delta');
    const statusMessage = document.getElementById('statusMessage');
    const quantityDisplay = document.getElementById('quantityDisplay');
    const submitBtn = document.getElementById('submitBtn');

    if (!productIdInput || !deltaInput || !statusMessage || !quantityDisplay || !submitBtn) {
        return;
    }

    // Reset previous feedback
    statusMessage.className = 'status-box';
    statusMessage.textContent = '';
    quantityDisplay.textContent = '';

    const productId = productIdInput.value.trim();
    const deltaValue = parseInt(deltaInput.value, 10);

    if (!productId) {
        statusMessage.className = 'status-box error';
        statusMessage.textContent = 'Please enter a valid Product ID.';
        return;
    }

    if (isNaN(deltaValue) || deltaValue === 0) {
        statusMessage.className = 'status-box error';
        statusMessage.textContent = 'Please enter a non-zero adjustment amount (delta).';
        return;
    }

    // Enter loading state
    submitBtn.disabled = true;

    try {
        const response = await fetch('/api/stock/adjust', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                productId: productId,
                delta: deltaValue
            })
        });

        let data = null;
        try {
            data = await response.json();
        } catch (e) {
            data = null;
        }

        if (response.status === 200) {
            statusMessage.className = 'status-box success';
            statusMessage.textContent = data && data.message ? data.message : 'Stock adjusted successfully.';
            const newQuantity = (data && data.quantity !== undefined && data.quantity !== null) ? data.quantity : '';
            quantityDisplay.textContent = 'Updated Quantity: ' + newQuantity;
            
            // Refresh product catalog to show updated stock
            await loadProducts();
        } else if (response.status === 400) {
            statusMessage.className = 'status-box error';
            let validationText = 'Validation failed. Please verify your inputs.';
            if (data && data.errors && typeof data.errors === 'object') {
                const errorList = [];
                for (const key of Object.keys(data.errors)) {
                    const fieldErrors = data.errors[key];
                    if (Array.isArray(fieldErrors)) {
                        errorList.push(...fieldErrors);
                    } else if (typeof fieldErrors === 'string') {
                        errorList.push(fieldErrors);
                    }
                }
                if (errorList.length > 0) {
                    validationText = errorList.join(' ');
                }
            } else if (data && data.message) {
                validationText = data.message;
            }
            statusMessage.textContent = validationText;
        } else if (response.status === 404) {
            statusMessage.className = 'status-box error';
            statusMessage.textContent = 'product not found';
        } else if (response.status === 409) {
            statusMessage.className = 'status-box error';
            statusMessage.textContent = 'stock would go negative';
        } else {
            // 5xx or unhandled status codes
            statusMessage.className = 'status-box error';
            statusMessage.textContent = 'Server error encountered. Please retry.';
        }
    } catch (networkError) {
        statusMessage.className = 'status-box error';
        statusMessage.textContent = 'Network connection failure. Please retry.';
    } finally {
        // Exit loading state
        submitBtn.disabled = false;
    }
}

function renderProductList(products) {
    const table = document.getElementById('productTable');
    const tbody = document.getElementById('productTableBody');
    const emptyState = document.getElementById('emptyState');
    const productIdInput = document.getElementById('productId');

    if (!table || !tbody || !emptyState) {
        return;
    }

    // Clear existing table rows safely
    while (tbody.firstChild) {
        tbody.removeChild(tbody.firstChild);
    }

    if (!products || !Array.isArray(products) || products.length === 0) {
        table.style.display = 'none';
        emptyState.style.display = 'block';
        emptyState.textContent = 'No products available.';
        return;
    }

    emptyState.style.display = 'none';
    table.style.display = 'table';

    for (let i = 0; i < products.length; i++) {
        const prod = products[i];
        const row = document.createElement('tr');

        const tdName = document.createElement('td');
        tdName.textContent = prod.name || 'Unnamed';

        const tdSku = document.createElement('td');
        tdSku.textContent = prod.sku || '-';

        const tdQuantity = document.createElement('td');
        tdQuantity.textContent = prod.quantityInStock !== undefined ? String(prod.quantityInStock) : '0';

        const tdId = document.createElement('td');
        tdId.textContent = prod.id || '';

        const tdAction = document.createElement('td');
        const selectBtn = document.createElement('button');
        selectBtn.type = 'button';
        selectBtn.className = 'select-btn';
        selectBtn.textContent = 'Select';
        selectBtn.setAttribute('aria-label', 'Select product ' + (prod.name || prod.sku));
        selectBtn.addEventListener('click', function () {
            if (productIdInput) {
                productIdInput.value = prod.id;
                productIdInput.focus();
            }
        });
        tdAction.appendChild(selectBtn);

        row.appendChild(tdName);
        row.appendChild(tdSku);
        row.appendChild(tdQuantity);
        row.appendChild(tdId);
        row.appendChild(tdAction);

        tbody.appendChild(row);
    }
}

async function loadProducts() {
    const table = document.getElementById('productTable');
    const emptyState = document.getElementById('emptyState');

    try {
        const response = await fetch('/api/products', {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            if (emptyState) {
                emptyState.style.display = 'block';
                emptyState.textContent = 'Unable to load product list.';
            }
            if (table) {
                table.style.display = 'none';
            }
            return;
        }

        const data = await response.json();
        renderProductList(data);
    } catch (err) {
        if (emptyState) {
            emptyState.style.display = 'block';
            emptyState.textContent = 'Unable to load product list.';
        }
        if (table) {
            table.style.display = 'none';
        }
    }
}

async function handleAiStockAnalysis() {
    const thresholdInput = document.getElementById('aiThreshold');
    const analyzeBtn = document.getElementById('aiAnalyzeBtn');
    const loadingIndicator = document.getElementById('aiLoadingIndicator');
    const statusMessage = document.getElementById('aiStatusMessage');
    const resultCard = document.getElementById('aiResultCard');

    if (!analyzeBtn) {
        return;
    }

    let threshold = 20;
    if (thresholdInput) {
        const parsed = parseInt(thresholdInput.value, 10);
        if (!isNaN(parsed) && parsed >= 0) {
            threshold = parsed;
        } else {
            if (statusMessage) {
                statusMessage.className = 'status-box error';
                statusMessage.style.display = 'block';
                statusMessage.textContent = 'Please enter a valid threshold (0 or greater).';
            }
            return;
        }
    }

    // Reset status and result card
    if (statusMessage) {
        statusMessage.className = 'status-box';
        statusMessage.style.display = 'none';
        statusMessage.textContent = '';
    }
    if (resultCard) {
        resultCard.style.display = 'none';
    }

    // Enter loading state
    analyzeBtn.disabled = true;
    if (loadingIndicator) {
        loadingIndicator.style.display = 'flex';
    }

    try {
        const response = await fetch('/api/reports/low-stock/summary?threshold=' + encodeURIComponent(threshold), {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });

        let data = null;
        try {
            data = await response.json();
        } catch (e) {
            data = null;
        }

        if (response.status === 200 && data) {
            renderAiSummary(data);
        } else if (response.status === 400) {
            if (statusMessage) {
                statusMessage.className = 'status-box error';
                statusMessage.style.display = 'block';
                statusMessage.textContent = (data && data.message) ? data.message : 'Invalid threshold parameter.';
            }
        } else if (response.status === 503) {
            if (statusMessage) {
                statusMessage.className = 'status-box error';
                statusMessage.style.display = 'block';
                statusMessage.textContent = (data && data.message) ? data.message : 'AI replenishment advisor is temporarily unavailable. Verify OpenAI API key configuration.';
            }
        } else {
            if (statusMessage) {
                statusMessage.className = 'status-box error';
                statusMessage.style.display = 'block';
                statusMessage.textContent = 'Failed to generate AI analysis. Please retry.';
            }
        }
    } catch (networkError) {
        if (statusMessage) {
            statusMessage.className = 'status-box error';
            statusMessage.style.display = 'block';
            statusMessage.textContent = 'Network connection failure while requesting AI summary. Please retry.';
        }
    } finally {
        analyzeBtn.disabled = false;
        if (loadingIndicator) {
            loadingIndicator.style.display = 'none';
        }
    }
}

function renderAiSummary(data) {
    const resultCard = document.getElementById('aiResultCard');
    const executiveSummary = document.getElementById('aiExecutiveSummary');
    const recommendedAction = document.getElementById('aiRecommendedAction');
    const itemsList = document.getElementById('aiItemsList');
    const itemsCount = document.getElementById('aiItemsCount');

    if (!resultCard) {
        return;
    }

    if (executiveSummary) {
        executiveSummary.textContent = data.summary || 'All inventory levels currently meet the target threshold.';
    }

    if (recommendedAction) {
        const actionText = (data.analysis && data.analysis.recommendedAction)
            ? data.analysis.recommendedAction
            : 'No immediate reorders required.';
        recommendedAction.textContent = actionText;
    }

    if (itemsCount) {
        const count = (data.itemCount !== undefined)
            ? data.itemCount
            : (data.analysis && data.analysis.items ? data.analysis.items.length : 0);
        itemsCount.textContent = count + ' At-Risk Items';
    }

    if (itemsList) {
        while (itemsList.firstChild) {
            itemsList.removeChild(itemsList.firstChild);
        }

        const items = (data.analysis && Array.isArray(data.analysis.items))
            ? data.analysis.items
            : [];

        if (items.length === 0) {
            const li = document.createElement('li');
            li.textContent = 'No critical SKUs currently below the threshold.';
            itemsList.appendChild(li);
        } else {
            for (let i = 0; i < items.length; i++) {
                const li = document.createElement('li');
                li.textContent = items[i];
                itemsList.appendChild(li);
            }
        }
    }

    resultCard.style.display = 'block';
}

function initStockPage() {
    const form = document.getElementById('stockAdjustmentForm');
    if (form) {
        form.addEventListener('submit', handleStockAdjustment);
    }
    const aiBtn = document.getElementById('aiAnalyzeBtn');
    if (aiBtn) {
        aiBtn.addEventListener('click', handleAiStockAnalysis);
    }
    loadProducts();
}

if (typeof document !== 'undefined') {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initStockPage);
    } else {
        initStockPage();
    }
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        handleStockAdjustment,
        renderProductList,
        loadProducts,
        handleAiStockAnalysis,
        renderAiSummary,
        initStockPage
    };
}

