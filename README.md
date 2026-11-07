# Masareef API

نظام إدارة مصاريف شخصية ومحلات تجارية — ASP.NET Core 8 بمعمارية طبقات
(`Api` → `BLL` → `DAL` → `Entities`) مع SQL Server و React frontend.

---

## التشغيل خطوة بخطوة

### 1. الداتابيز

شغّل سكريبت الـ schema الأساسي بتاع المشروع، وبعده **لازم**:

```bash
sqlcmd -S <server> -d MasareefDB -E -i Database/01_security_schema.sql
```

ده بيضيف اللي نظام الأمان محتاجه: عمود `Users.Role` وجدولي `RefreshTokens`
و `AuditLogs`. السكريبت آمن لو اتشغّل أكتر من مرة، وفي آخره تحقق بيطبع
`OK` لكل عنصر.

> **لو مشغّلتهوش:** التطبيق هيقلع عادي، لكن **كل** تسجيل دخول هيفشل بـ 500
> (`Invalid column name 'Role'`) وبالتالي التطبيق كله مش هينفع يستخدم.

### 2. المفتاح السري (Jwt:Key)

المفتاح **مش موجود في المشروع** ومش بيتنقل معاه. كل بيئة بتولّد مفتاحها الخاص.

ولّد مفتاح عشوائي:

```bash
openssl rand -base64 48
```

**للتطوير** (المفتاح بيتخزّن بره مجلد المشروع):

```bash
dotnet user-secrets set "Jwt:Key" "<المفتاح>" --project Masareef.Api
```

**للإنتاج** — environment variable:

| الاستضافة | الطريقة |
|---|---|
| Azure App Service | Configuration → Application settings → `Jwt__Key` |
| IIS / Windows Server | System Environment Variables، أو `web.config` → `environmentVariables` |
| Linux + systemd | `Environment="Jwt__Key=..."` في ملف الـ service |
| Docker | `docker run -e Jwt__Key="..."` |

> `__` (شرطتين سفليتين) = `:` في مسار الإعدادات، لأن الـ environment variables
> مابتقبلش `:` على كل أنظمة التشغيل.

⚠️ المفتاح لازم يفضل **ثابت**. تغييره يُبطل كل التوكنات الموجودة ويسجّل خروج
كل المستخدمين فورًا.

⚠️ لو المفتاح ناقص أو أقل من 32 حرف، التطبيق **يرفض يقلع** برسالة واضحة —
ده مقصود، أحسن من إنه يشتغل بأمان وهمي.

### 3. باقي الإعدادات

| الإعداد | التطوير | الإنتاج (environment variable) |
|---|---|---|
| الداتابيز | `appsettings.Development.json` | `ConnectionStrings__DefaultConnection` |
| دومين الفرونت إند | `appsettings.Development.json` | `Cors__Origins` |
| المفتاح السري | User Secrets | `Jwt__Key` |

الإعدادات الناقصة بتوقف التشغيل برسالة بتقول الناقص ايه بالظبط.

### 4. التشغيل

```bash
dotnet run --project Masareef.Api --launch-profile https
```

Swagger متاح في بيئة التطوير بس: `https://localhost:7146/swagger`

---

## 🔴 HTTPS إلزامي في الإنتاج

الـ refresh token بيتخزّن في كوكي عليها `Secure=true`، يعني **المتصفح مش
هيبعتها أبدًا على HTTP عادي**. لو نشرت على HTTP:

- المتصفح يرفض الكوكي بصمت، من غير أي رسالة خطأ
- `POST /api/auth/refresh` يرجّع 401 دايمًا
- المستخدمين يتسجّل خروجهم **كل 15 دقيقة** (عند انتهاء الـ access token)

وده هيبان كـ "بَج غامض" مش كخطأ إعداد. `localhost` استثناء (المتصفحات
بتعتبره سياق آمن) وعشان كده التطوير المحلي بيشتغل على HTTP.

**الخلاصة:** سيرفر الإنتاج لازم شهادة TLS حقيقية، والـ `Cors__Origins`
لازم تبدأ بـ `https://`.

---

## نظرة على نظام الأمان

| الطبقة | التنفيذ |
|---|---|
| **Authentication** | JWT موقّع بـ HMAC-SHA256، عمره 15 دقيقة. التحقق من Issuer/Audience/Lifetime/Signature |
| **كلمات السر** | BCrypt (hash + salt تلقائي) |
| **Authorization** | Policy-based: `AdminOnly` عبر `AdminRequirement` + Handler يقرا الـ Role claim |
| **Ownership** | فلترة بالـ `userId` في كل استعلام، بالإضافة لـ `ResourceOwner` policy كبوابة صريحة على العمليات اللي لا ترجع (شطب الدين) |
| **Refresh Token** | 7 أيام، HttpOnly + Secure cookie على `Path=/api/auth`. مخزّن كـ SHA-256 hash مش خام. **Rotation** في كل استخدام + كشف إعادة استخدام التوكنات الملغية |
| **Logout** | إبطال فوري للـ refresh token في الداتابيز + مسح الكوكي |
| **Rate limiting** | 5 طلبات/دقيقة لكل IP على كل `/api/auth/*` (حماية من brute force) |
| **Logging** | Serilog → console + ملفات يومية في `Logs/` |
| **Auditing** | جدول `AuditLogs` دائم: محاولات الدخول، الشطب، الحذف، محاولات انتهاك الملكية. فشل التسجيل مابيوقّفش العملية (بيتسجّل كـ error في الـ log) |

### الأدوار

- التسجيل من الـ API بينتج `Role = 'User'` **دايمًا** — مفيش طريقة لإنشاء Admin من بره.
- ترقية مستخدم لـ Admin بتتم بـ SQL مباشر (شوف آخر السكريبت).
- الـ Role جوه التوكن، فالمستخدم لازم يعمل login من جديد بعد ترقيته.

### `/api/admin/audit`

محمي بـ `AdminOnly`. بيرجّع آخر 200 حدث، بفلترة اختيارية:
`?userId=12` أو `?action=Debt.WriteOffDenied`

---

## ملفات مش بتترفع على git

`.gitignore` بيستثني: `bin/`, `obj/`, `.vs/`, `node_modules/`, و **`Logs/`**
(ملفات الـ log فيها إيميلات مستخدمين وبيانات طلبات). الأسرار مابتترفعش لأنها
في User Secrets / environment variables من الأساس.
