import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { Field } from '../components/UI';
import { Logo, IconSun, IconMoon, IconAlert } from '../components/Icons';

export default function Login() {
  const { login, register } = useAuth();
  const { isDark, toggle } = useTheme();
  const navigate = useNavigate();

  const [mode, setMode] = useState('login');
  const [form, setForm] = useState({ name: '', email: '', password: '', phoneNumber: '' });
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setBusy(true);
    try {
      if (mode === 'login') {
        await login(form.email, form.password);
      } else {
        await register({
          name: form.name,
          email: form.email,
          password: form.password,
          phoneNumber: form.phoneNumber || null,
        });
      }
      navigate('/');
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="auth">
      {/* لوحة الهوية */}
      <aside className="auth-brand">
        <div className="auth-top">
          <Logo size={40} />
          <span className="auth-wordmark">مصاريف</span>
        </div>

        <div className="auth-mid">
          <h2 className="auth-thesis">
            كل قرش<br />
            <span className="thesis-accent">في مكانه</span>
          </h2>
          <p className="auth-line">
            دفتر حساباتك — البيت، المحل، والديون. في صفحة واحدة.
          </p>

          {/* زخرفة الميزان */}
          <svg className="auth-beam" viewBox="0 0 260 110" fill="none" aria-hidden="true">
            <path d="M130 22 L130 88" className="ab-post" />
            <path d="M110 90 L150 90" className="ab-post" />
            <g className="ab-arm">
              <path d="M38 22 L222 22" className="ab-post" />
              <circle cx="130" cy="22" r="3.6" className="ab-pivot" />
              <path d="M54 22 L54 38M206 22 L206 38" className="ab-str" />
            </g>
            <path d="M34 38 L74 38 L67 54 L41 54 Z" className="ab-pan ab-out" />
            <path d="M186 38 L226 38 L219 54 L193 54 Z" className="ab-pan ab-in" />
          </svg>
        </div>

        <div className="auth-foot">
          <span className="foot-dot" /> إدارة مالية شخصية وتجارية
        </div>
      </aside>

      {/* لوحة النموذج */}
      <main className="auth-side">
        <button className="auth-theme" onClick={toggle}
          aria-label={isDark ? 'الوضع النهاري' : 'الوضع الليلي'}>
          {isDark ? <IconSun size={18} /> : <IconMoon size={18} />}
        </button>

        <form className="auth-form" onSubmit={submit}>
          <h1 className="auth-title">
            {mode === 'login' ? 'افتح دفترك' : 'ابدأ دفترك'}
          </h1>
          <p className="auth-sub">
            {mode === 'login'
              ? 'ادخل بياناتك للمتابعة'
              : 'حساب جديد في أقل من دقيقة'}
          </p>

          {mode === 'register' && (
            <Field label="الاسم">
              <input value={form.name} onChange={set('name')} placeholder="اسمك الكامل" required />
            </Field>
          )}

          <Field label="البريد الإلكتروني">
            <input type="email" value={form.email} onChange={set('email')}
              placeholder="you@example.com" required autoComplete="email" />
          </Field>

          <Field label="كلمة المرور">
            <input type="password" value={form.password} onChange={set('password')}
              placeholder="٦ حروف على الأقل" required
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'} />
          </Field>

          {mode === 'register' && (
            <Field label="رقم الهاتف (اختياري)">
              <input value={form.phoneNumber} onChange={set('phoneNumber')} placeholder="01xxxxxxxxx" />
            </Field>
          )}

          {error && (
            <div className="auth-error">
              <IconAlert size={16} />
              <span>{error}</span>
            </div>
          )}

          <button className="btn btn-primary auth-submit" disabled={busy}>
            {busy ? 'جارٍ...' : mode === 'login' ? 'دخول' : 'إنشاء الحساب'}
          </button>

          <div className="auth-switch">
            {mode === 'login' ? (
              <>ماعندكش حساب؟{' '}
                <button type="button" onClick={() => { setMode('register'); setError(''); }}>
                  أنشئ حساب
                </button></>
            ) : (
              <>عندك حساب؟{' '}
                <button type="button" onClick={() => { setMode('login'); setError(''); }}>
                  سجّل الدخول
                </button></>
            )}
          </div>
        </form>
      </main>

      <style>{`
        .auth { min-height: 100vh; display: grid; grid-template-columns: 1.05fr 1fr; }

        /* ── لوحة الهوية ── */
        .auth-brand {
          border-radius: 0 0 0 2rem;
          background: linear-gradient(160deg, var(--brand-deep) 0%, var(--brand) 65%, #17435f 100%);
          color: #fff; padding: 42px 46px;
          display: flex; flex-direction: column; justify-content: space-between;
          position: relative; overflow: hidden;
        }
        /* شبكة دفتر خفيفة */
        .auth-brand::before {
          content: ''; position: absolute; inset: 0;
          background-image:
            linear-gradient(rgba(255,255,255,.045) 1px, transparent 1px),
            linear-gradient(90deg, rgba(255,255,255,.045) 1px, transparent 1px);
          background-size: 34px 34px;
          mask-image: radial-gradient(ellipse at 60% 40%, #000 30%, transparent 78%);
        }
        .auth-brand > * { position: relative; z-index: 1; }

        .auth-top { display: flex; align-items: center; gap: 12px; }
        .auth-wordmark {
          font-family: var(--font-display); font-size: 1.5rem; font-weight: 700;
          letter-spacing: -.02em;
        }

        .auth-mid { margin: auto 0; }
        .auth-thesis {
          font-family: var(--font-display); font-size: 3.1rem; font-weight: 700;
          line-height: 1.15; letter-spacing: -.035em;
          animation: riseIn .7s var(--ease) both;
        }
        .thesis-accent { color: var(--copper); }
        [data-theme='dark'] .thesis-accent { color: #e0a878; }
        .auth-line {
          font-size: 1.02rem; opacity: .78; margin-top: 14px; max-width: 300px; line-height: 1.75;
          animation: riseIn .7s var(--ease) .12s both;
        }

        .auth-beam { width: 260px; margin-top: 34px; opacity: .9; }
        .ab-post, .ab-str {
          stroke: rgba(255,255,255,.5); fill: none;
          stroke-width: 1.8; stroke-linecap: round;
        }
        .ab-str { stroke-width: 1.1; }
        .ab-pivot { fill: var(--copper); }
        .ab-pan { stroke-width: 1.5; stroke-linejoin: round; }
        .ab-in  { fill: rgba(45,212,191,.22); stroke: rgba(45,212,191,.75); }
        .ab-out { fill: rgba(248,113,113,.18); stroke: rgba(248,113,113,.7); }
        .ab-arm {
          transform-origin: 130px 22px;
          animation: sway 7s ease-in-out infinite;
        }
        @keyframes sway {
          0%, 100% { transform: rotate(-4deg); }
          50%      { transform: rotate(4deg); }
        }

        .auth-foot {
          display: flex; align-items: center; gap: 8px;
          font-size: .84rem; opacity: .62;
        }
        .foot-dot { width: 5px; height: 5px; border-radius: 50%; background: var(--copper); }

        /* ── لوحة النموذج ── */
        .auth-side {
          display: flex; align-items: center; justify-content: center;
          padding: 40px; background: var(--paper); position: relative;
        }
        .auth-theme {
          position: absolute; top: 22px; left: 24px;
          width: 38px; height: 38px; border-radius: 11px;
          display: grid; place-items: center; color: var(--ink-soft);
          background: var(--surface); border: 1px solid var(--line);
          transition: all .2s var(--ease);
        }
        .auth-theme:hover { color: var(--brand); border-color: var(--brand); transform: rotate(-18deg); }

        .auth-form { width: 100%; max-width: 372px; animation: riseIn .55s var(--ease) both; }
        .auth-title { font-size: 1.95rem; letter-spacing: -.025em; }
        .auth-sub { color: var(--ink-soft); margin: 7px 0 26px; font-size: .95rem; }

        .auth-error {
          display: flex; align-items: center; gap: 8px;
          background: var(--down-soft); color: var(--down);
          padding: 11px 14px; border-radius: var(--r-sm);
          font-size: .88rem; font-weight: 500; margin-bottom: 15px;
          animation: shake .4s var(--ease);
        }
        @keyframes shake {
          0%,100% { transform: translateX(0); }
          25% { transform: translateX(-5px); }
          75% { transform: translateX(5px); }
        }

        .auth-submit { width: 100%; margin-top: 4px; padding: 13px; }

        .auth-switch { text-align: center; margin-top: 20px; font-size: .91rem; color: var(--ink-soft); }
        .auth-switch button { color: var(--brand); font-weight: 700; }
        .auth-switch button:hover { text-decoration: underline; }
        [data-theme='dark'] .auth-switch button { color: var(--brand-lift); }

        @media (max-width: 860px) {
          .auth { grid-template-columns: 1fr; }
          .auth-brand { padding: 30px 28px; min-height: 220px; }
          .auth-thesis { font-size: 2.1rem; }
          .auth-beam, .auth-foot { display: none; }
          .auth-mid { margin: 20px 0 0; }
        }
      `}</style>
    </div>
  );
}
