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

function initStockPage() {
    const form = document.getElementById('stockAdjustmentForm');
    if (form) {
        form.addEventListener('submit', handleStockAdjustment);
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
        initStockPage
    };
}

