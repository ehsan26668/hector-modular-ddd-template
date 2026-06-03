# ADR-0003: Adopt TDD for Building Blocks

**Status:** Accepted
**Date:** 2026-06-03

## Context

به منظور تضمین پایداری و صحت کدهای زیرساختی (Framework)، نیاز به یک مکانیزم تایید خودکار داریم.

## Decision

تمامی اجزای `Hector.BuildingBlocks` باید با رویکرد Test-Driven Development توسعه یابند. 

- پوشش تست (Code Coverage) برای BuildingBlocks باید بالای ۹۵٪ باشد.
- تست‌ها باید به عنوان مستندات زنده (Living Documentation) عمل کنند.

## Consequences

- **Pros:** کاهش باگ‌های رگرسیون، طراحی بهتر APIها، اطمینان بالا در هنگام ریفکتور.
- **Cons:** افزایش زمان اولیه توسعه.
