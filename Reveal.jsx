import { useInView, useScrollCount } from '../lib/motion';
import { IconArrowLeft } from './Icons';

// ═══════════════════════════════════════════════════════
//  عناصر الكشف والحركة
// ═══════════════════════════════════════════════════════

/**
 * كشف سطر بسطر — كل سطر بيطلع من تحت قصّة
 * lines: مصفوفة نصوص
 */
export function LineReveal({ lines = [], stagger = 120, className = '', as: Tag = 'h2' }) {
  const [ref, seen] = useInView();

  return (
    <Tag ref={ref} className={className}>
      {lines.map((line, i) => (
        <span key={i} className={`line-clip ${seen ? 'seen' : ''}`}>
          <span style={{ transitionDelay: `${i * stagger}ms` }}>{line}</span>
        </span>
      ))}
    </Tag>
  );
}

/**
 * كشف كلمة كلمة
 */
export function WordReveal({ text = '', stagger = 35, className = '', as: Tag = 'p' }) {
  const [ref, seen] = useInView();
  const words = text.split(' ');

  return (
    <Tag ref={ref} className={`word-reveal ${seen ? 'seen' : ''} ${className}`}>
      {words.map((w, i) => (
        <span key={i} style={{ transitionDelay: `${i * stagger}ms` }}>
          {w}{i < words.length - 1 ? '\u00A0' : ''}
        </span>
      ))}
    </Tag>
  );
}

/**
 * رقم بيعدّ حسب التمرير
 */
export function CountUp({ value = 0, decimals = 0, suffix = '', className = '' }) {
  const [ref, n] = useScrollCount(Math.round(Number(value) || 0));

  const shown = decimals > 0
    ? (n).toFixed(decimals)
    : new Intl.NumberFormat('en-US').format(n);

  return (
    <span ref={ref} className={`num ${className}`}>
      {shown}{suffix}
    </span>
  );
}

/**
 * مبلغ بيعدّ — بيحافظ على المنازل العشرية
 */
export function CountMoney({ value = 0, className = '' }) {
  const whole = Math.round(Math.abs(Number(value) || 0));
  const [ref, n] = useScrollCount(whole);
  const sign = Number(value) < 0 ? '-' : '';

  return (
    <span ref={ref} className={`num ${className}`}>
      {sign}{new Intl.NumberFormat('en-US').format(n)}
    </span>
  );
}

/**
 * زرّ الحبّة بشارة السهم
 */
export function Pill({
  children, variant = 'dark', arrow = true, onClick, type = 'button', disabled,
}) {
  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`pill pill-${variant} ${arrow ? '' : 'pill-plain'}`}
    >
      <span>{children}</span>
      {arrow && (
        <span className="pill-badge">
          <IconArrowLeft size={16} />
        </span>
      )}
    </button>
  );
}

/**
 * عنوان صغير بنقطة
 */
export function Eyebrow({ children, ring = false }) {
  return <span className={`eyebrow ${ring ? 'eyebrow-ring' : ''}`}>{children}</span>;
}
