import { useEffect } from 'react';
import { IconClose, IconCheck, IconAlert } from './Icons';

// ─────────── الأرقام ───────────
export const money = (n) =>
  new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    .format(Number(n) || 0);

export const Money = ({ value, className = '' }) => (
  <span className={`num ${className}`}>{money(value)}</span>
);

// ─────────── سبينر ───────────
export const Spinner = () => <div className="spinner" />;

// ─────────── رسوم الحالات الفارغة ───────────
const art = {
  /** دفتر مفتوح بصفحات فاضية */
  ledger: (
    <svg width="96" height="76" viewBox="0 0 96 76" fill="none">
      <path d="M48 18C40 12 26 10 12 12v48c14-2 28 0 36 6 8-6 22-8 36-6V12c-14-2-28 0-36 6z"
        fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M48 18v48" stroke="var(--ink-faint)" strokeWidth="1.6" />
      <path d="M20 26h20M20 34h20M20 42h14" stroke="var(--line)" strokeWidth="1.6" strokeLinecap="round" />
      <path d="M56 26h20M56 34h20M56 42h14" stroke="var(--line)" strokeWidth="1.6" strokeLinecap="round" />
    </svg>
  ),

  /** كيس نقود فاضي */
  money: (
    <svg width="90" height="80" viewBox="0 0 90 80" fill="none">
      <path d="M38 16h14l-3-8H41z" fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M45 16c-14 0-24 12-24 26s10 22 24 22 24-8 24-22-10-26-24-26z"
        fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.6" />
      <path d="M45 32v18M40 37h10M40 45h10" stroke="var(--ink-faint)" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  ),

  /** يدان متبادلتان — للديون */
  exchange: (
    <svg width="98" height="72" viewBox="0 0 98 72" fill="none">
      <rect x="8" y="20" width="34" height="32" rx="4"
        fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.6" />
      <rect x="56" y="20" width="34" height="32" rx="4"
        fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.6" />
      <path d="M44 30h10" stroke="var(--up)" strokeWidth="1.8" strokeLinecap="round" />
      <path d="M51 27l3 3-3 3" stroke="var(--up)" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M54 42H44" stroke="var(--down)" strokeWidth="1.8" strokeLinecap="round" />
      <path d="M47 39l-3 3 3 3" stroke="var(--down)" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M16 30h14M16 38h10" stroke="var(--line)" strokeWidth="1.6" strokeLinecap="round" />
      <path d="M64 30h14M64 38h10" stroke="var(--line)" strokeWidth="1.6" strokeLinecap="round" />
    </svg>
  ),

  /** صندوق مفتوح فاضي */
  box: (
    <svg width="92" height="78" viewBox="0 0 92 78" fill="none">
      <path d="M46 14 18 26v26l28 12 28-12V26z"
        fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M18 26l28 12 28-12" stroke="var(--line)" strokeWidth="1.6" strokeLinejoin="round" />
      <path d="M46 38v26" stroke="var(--line)" strokeWidth="1.6" />
      <path d="M36 20l20 8" stroke="var(--ink-faint)" strokeWidth="1.4" strokeDasharray="3 3" />
    </svg>
  ),

  /** رسم بياني فاضي */
  chart: (
    <svg width="94" height="72" viewBox="0 0 94 72" fill="none">
      <path d="M14 12v44h66" stroke="var(--line)" strokeWidth="1.8" strokeLinecap="round" />
      <rect x="24" y="40" width="11" height="16" rx="2" fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.4" />
      <rect x="42" y="32" width="11" height="24" rx="2" fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.4" />
      <rect x="60" y="44" width="11" height="12" rx="2" fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.4" />
      <path d="M26 26c8-6 16 2 24-6" stroke="var(--ink-faint)" strokeWidth="1.6"
        strokeLinecap="round" strokeDasharray="3 4" />
    </svg>
  ),

  /** لا نتائج بحث */
  search: (
    <svg width="88" height="76" viewBox="0 0 88 76" fill="none">
      <circle cx="38" cy="32" r="18" fill="var(--surface-2)" stroke="var(--line)" strokeWidth="1.8" />
      <path d="M51 45 66 60" stroke="var(--line)" strokeWidth="2.4" strokeLinecap="round" />
      <path d="M32 32h12" stroke="var(--ink-faint)" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  ),
};

/**
 * حالة فارغة برسمة
 * kind: ledger | money | exchange | box | chart | search
 */
export const Empty = ({ kind = 'ledger', title, hint }) => (
  <div className="empty">
    <div className="empty-art">{art[kind] || art.ledger}</div>
    <div className="empty-title">{title}</div>
    {hint && <div className="empty-hint">{hint}</div>}
  </div>
);

// ─────────── مودال ───────────
export function Modal({ title, onClose, children }) {
  useEffect(() => {
    const onEsc = (e) => e.key === 'Escape' && onClose();
    window.addEventListener('keydown', onEsc);
    document.body.style.overflow = 'hidden';
    return () => {
      window.removeEventListener('keydown', onEsc);
      document.body.style.overflow = '';
    };
  }, [onClose]);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
        <div className="modal-head">
          <h3>{title}</h3>
          <button className="modal-close" onClick={onClose} aria-label="إغلاق">
            <IconClose size={18} />
          </button>
        </div>
        <div className="modal-body">{children}</div>
      </div>
    </div>
  );
}

// ─────────── توست ───────────
export function Toast({ toast, onDone }) {
  useEffect(() => {
    if (!toast) return;
    const t = setTimeout(onDone, 2800);
    return () => clearTimeout(t);
  }, [toast, onDone]);

  if (!toast) return null;
  const err = toast.type === 'err';

  return (
    <div className={`toast toast-${err ? 'err' : 'ok'}`} role="status">
      {err ? <IconAlert size={17} /> : <IconCheck size={17} />}
      <span>{toast.msg}</span>
    </div>
  );
}

// ─────────── حقل ───────────
export function Field({ label, children }) {
  return (
    <div className="field">
      <label>{label}</label>
      {children}
    </div>
  );
}

// ─────────── شارة حالة الدين ───────────
const STATUS = {
  1: { cls: 'badge-open', txt: 'مفتوح' },
  2: { cls: 'badge-partial', txt: 'جزئي' },
  3: { cls: 'badge-paid', txt: 'مدفوع' },
  4: { cls: 'badge-off', txt: 'مشطوب' },
};

export const StatusBadge = ({ status, overdue }) => {
  const s = STATUS[status] || STATUS[1];
  return (
    <span style={{ display: 'inline-flex', gap: 6, flexWrap: 'wrap' }}>
      <span className={`badge ${s.cls}`}>{s.txt}</span>
      {overdue && <span className="badge badge-overdue">متأخر</span>}
    </span>
  );
};

export const methodName = (m) => ({ 1: 'كاش', 2: 'كارت', 3: 'تحويل', 4: 'شيك' }[m] || 'أخرى');
