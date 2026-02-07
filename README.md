# Coupons Management API

## Implemented Cases
- Cart-wise: discount % applied on total cart when `MinCartTotal` is met, with optional `MaxDiscount` cap.
- Product-wise: discount % applied to a single product, with optional `MinCartTotal` and `MaxDiscount`.
- BxGy: buy array + get array, repetition limit applied by minimum matching buy counts.

## Unimplemented / Considered Cases
- Coupon stacking or prioritization between multiple coupons
- Per-user redemption limits and coupon usage tracking
- Coupon eligibility by user segment or location
- SKU/category-level coupon filters
- Overlap between buy and get items in BxGy
- Partial fulfillment rules (e.g., insufficient get-items in cart)
- Expired or scheduled coupons (currently checked only at apply time)
- Concurrency handling for MaxRedemptions
- Taxes and rounding policy across line items

## Assumptions
- One coupon applied per request.
- Prices are pre-tax.
- Product IDs in cart are valid.
- DiscountPercent is between 0–100.
- BxGy free items are added to quantity (and discounted), not removed from price.
- Cart item prices are trusted and not re-fetched.

## Limitations
- Details stored as JSON (`DetailsJson`), so database cannot easily query coupon specifics.
- No inventory validation for free items.
- No authentication or user context.
- No caching/performance tuning for large coupon sets.

## API Endpoints
- POST `/coupons`
- GET `/coupons`
- GET `/coupons/{id}`
- PUT `/coupons/{id}`
- DELETE `/coupons/{id}`
- POST `/applicable-coupons`
- POST `/apply-coupon/{id}`
