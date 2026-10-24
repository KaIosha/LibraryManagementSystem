# Stripe Checkout Template (reuse per project)

Pattern used here: create a local `Pending` payment row, redirect to Stripe, verify on return, flip to `Paid`. Cash path stays separate and immediate.

## 1. Packages and config

```powershell
dotnet add package Stripe.net
```

Secrets (never commit keys):

```powershell
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Stripe:SuccessUrl" "https://localhost:<PORT>/api/payment/success?session_id={CHECKOUT_SESSION_ID}"
dotnet user-secrets set "Stripe:CancelUrl" "https://localhost:<PORT>/api/payment/cancel"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_... (later, optional for local)"
```

Options class: `SecretKey, PublishableKey, SuccessUrl, CancelUrl, WebhookSecret`.

Startup:

```csharp
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
```

## 2. Data shape

`Payment`: `PaymentId, OrderId/BorrowId, Amount, Method (Cash/Card), Status (Pending/Paid/Failed), StripeSessionId, StripePaymentIntentId?, Notes, PaymentDate`.

Repo needs two lookups: `GetByOrderIdAsync`, `GetByStripeSessionIdAsync`.

## 3. Service methods (copy this contract)

- `CreateCheckoutSessionAsync(dto)` — validate order, block if already `Paid`, build Stripe session, save `Pending` row with `session.Id`, return `session.Url`.
- `VerifyCheckoutSessionAsync(sessionId)` — find row by session id, `GetAsync(sessionId)` from Stripe, if `PaymentStatus == "paid"` mark `Paid` + store `PaymentIntentId`.
- `RecordCashPaymentAsync(dto)` — validate, save `Paid/Cash` directly.
- `GetPaymentsByOrderIdAsync(orderId)` — history list, newest first.

## 4. Stripe session minimum

```csharp
var options = new SessionCreateOptions
{
    Mode = "payment",
    SuccessUrl = _stripe.SuccessUrl,
    CancelUrl = _stripe.CancelUrl,
    ClientReferenceId = orderId.ToString(),
    Metadata = new Dictionary<string, string> { ["orderId"] = orderId.ToString() },
    LineItems = new()
    {
        new()
        {
            Quantity = 1,
            PriceData = new()
            {
                Currency = "usd",
                UnitAmountDecimal = amount * 100, // cents
                ProductData = new() { Name = $"Payment for order {orderId}" }
            }
        }
    }
};
var session = await _sessionService.CreateAsync(options);
```

## 5. Controller minimum

- `POST /checkout` — returns `SessionUrl`
- `GET /success?session_id=...` — calls verify, returns final status
- `GET /cancel` — informational, row stays `Pending`
- `POST /cash` — staff path
- `GET /borrow/{id}` (rename to `/order/{id}`) — history

## 6. Rules that saved us

- Amount truth lives server-side (from order/fees), never trust `dto.Amount` for charging.
- Stripe call goes outside any DB transaction.
- Never swallow `not found / already paid` into a generic error.
- Test cards: `4242 4242 4242 4242`, any future date, any CVC.
- Production follow-up: real webhook with signature check instead of redirect-only verify.
