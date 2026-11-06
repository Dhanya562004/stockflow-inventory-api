const { test, expect } = require('@playwright/test');

test.describe('Stock Page E2E', () => {
    test('opens /stock, submits an adjustment, and checks displayed quantity', async ({ page }) => {
        // Intercept API routes to make E2E test deterministic across environments
        await page.route('**/api/products', async (route) => {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify([
                    {
                        id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
                        name: 'Wireless Mouse',
                        sku: 'WM-01',
                        quantityInStock: 20
                    }
                ])
            });
        });

        await page.route('**/api/stock/adjust', async (route) => {
            const requestBody = JSON.parse(route.request().postData() || '{}');
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify({
                    productId: requestBody.productId,
                    quantity: 25,
                    previousQuantity: 20,
                    delta: requestBody.delta,
                    message: 'Stock adjusted successfully.'
                })
            });
        });

        // Navigate to the rendered Razor view served by ASP.NET Core
        await page.goto('/stock');

        // Verify page layout and accessibility elements
        await expect(page.locator('h1')).toHaveText('Stock Adjustment');
        await expect(page.locator('#stockAdjustmentForm')).toBeVisible();

        // Fill form fields
        await page.fill('#productId', '3fa85f64-5717-4562-b3fc-2c963f66afa6');
        await page.fill('#delta', '5');

        // Submit form
        await page.click('#submitBtn');

        // Verify that DOM was updated via textContent
        await expect(page.locator('#statusMessage')).toHaveText('Stock adjusted successfully.');
        await expect(page.locator('#quantityDisplay')).toHaveText('Updated Quantity: 25');
    });
});
