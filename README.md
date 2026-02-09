# Coupons Management API

## Implemented Cases
- Cart-wise: discount % applied on total cart when `MinCartTotal` is met, with optional `MaxDiscount` cap.
- Product-wise: discount % applied to a single product, with optional `MinCartTotal` and `MaxDiscount`.
- BxGy: buy array + get array, repetition limit applied by minimum matching buy counts.
- Coupon lifecycle: `IsActive`, `StartDate`, `EndDate` validated during apply/applicable checks.
- Redemption limits: `MaxRedemptions` enforced; `CurrentRedemptions` increments on apply and deactivates the coupon when limit is reached.
- DetailsJson validation: create/update validates `DetailsJson` matches coupon type and required fields.

## Unimplemented / Considered Cases
- Coupon stacking or prioritization between multiple coupons
- Per-user redemption limits and coupon usage tracking
- Coupon eligibility by user segment or location
- SKU/category-level coupon filters
- Overlap between buy and get items in BxGy
- Partial fulfillment rules (e.g., insufficient get-items in cart)
- Expired or scheduled coupons (currently checked only at apply/applicable time)
- Concurrency handling for apply + redemption update in a single transaction
- Taxes and rounding policy across line items
- Logging for invalid coupon configurations

## Assumptions
- One coupon applied per request.
- Prices are pre-tax.
- Product IDs in cart are valid.
- DiscountPercent is between 0–100.
- BxGy free items are added to quantity (and discounted), not removed from price.
- Cart item prices are trusted and not re-fetched.
- `DetailsJson` is validated on create/update; invalid configuration returns HTTP 400.

## Limitations
- Details stored as JSON (`DetailsJson`), so database cannot easily query coupon specifics.
- No inventory validation for free items.
- No authentication or user context.
- No caching/performance tuning for large coupon sets.
- Apply + redemption update are not wrapped in a DB transaction.

## API Endpoints
- POST `/coupons`
- GET `/coupons`
- GET `/coupons/{id}`
- PUT `/coupons/{id}`
- DELETE `/coupons/{id}`
- POST `/applicable-coupons`
- POST `/apply-coupon/{id}`
