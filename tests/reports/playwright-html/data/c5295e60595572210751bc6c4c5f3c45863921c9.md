# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: navigation-and-filters.spec.js >> CRM navigation and filter UX >> list pages use popup filter controls @crud @lookup @sequence
- Location: ..\tests\e2e\navigation-and-filters.spec.js:33:3

# Error details

```
Error: locator.click: Target page, context or browser has been closed
Call log:
  - waiting for getByTestId('grid-filter-button').first()
    - locator resolved to <button tabindex="0" type="button" role="button" aria-expanded="false" data-testid="grid-filter-button" data-tabster="{"restorer":{"type":1}}" class="fui-Button r1f29ykk ___1jmuyv0 fhovq9v f1p3nwhy f11589ue f1q5o8ev f1pdflbu fkfq4zb f1t94bn6 f1s2uweq fr80ssc f1ukrpxl fecsdlb fnwyq0v ft1hn21 fuxngvv fy5bs14 f1q1yqic fhvnf4x fb6swo4 f1klyf7k f232fm2 fwga7ee f1nhwcv0 f1gm6xmp f1xxsver f1v3eptx fivsta0 fd4bjan f3m6zum fh7ncta f1brlhvm f1sl3k7w fneth5b ft85np5 fy9rknc figsok6 fwrc4pm fazmxh">…</button>
  - attempting click action
    - scrolling into view if needed
    - done scrolling
    - forcing action
    - performing click action

```

```
Error: apiRequestContext._wrapApiCall: Target page, context or browser has been closed
```