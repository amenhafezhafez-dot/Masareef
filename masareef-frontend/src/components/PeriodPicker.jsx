import { useState } from 'react';

const MONTHS = [
  'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
  'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر',
];

/**
 * منتقي الفترة — شهر معيّن / سنة كاملة / كل الفترات
 * onChange({ mode, year, month })
 *   mode: 'month' | 'year' | 'all'
 *   month بيكون null في وضع السنة والكل
 */
export default function PeriodPicker({ value, onChange }) {
  const now = new Date();
  const [mode, setMode] = useState(value?.mode || 'month');
  const [year, setYear] = useState(value?.year || now.getFullYear());
  const [month, setMonth] = useState(value?.month || now.getMonth() + 1);

  // سنوات من 2020 للسنة الجاية
  const years = [];
  for (let y = now.getFullYear() + 1; y >= 2020; y--) years.push(y);

  const apply = (newMode, newYear, newMonth) => {
    setMode(newMode);
    setYear(newYear);
    setMonth(newMonth);

    onChange({
      mode: newMode,
      year: newMode === 'all' ? null : newYear,
      month: newMode === 'month' ? newMonth : null,
    });
  };

  const prev = () => {
    if (mode === 'month') {
      const m = month === 1 ? 12 : month - 1;
      const y = month === 1 ? year - 1 : year;
      apply('month', y, m);
    } else if (mode === 'year') {
      apply('year', year - 1, null);
    }
  };

  const next = () => {
    if (mode === 'month') {
      const m = month === 12 ? 1 : month + 1;
      const y = month === 12 ? year + 1 : year;
      apply('month', y, m);
    } else if (mode === 'year') {
      apply('year', year + 1, null);
    }
  };

  const goToday = () => apply('month', now.getFullYear(), now.getMonth() + 1);

  const label =
    mode === 'all' ? 'كل الفترات'
      : mode === 'year' ? `سنة ${year}`
        : `${MONTHS[month - 1]} ${year}`;

  return (
    <div className="period">
      {/* أنماط العرض */}
      <div className="period-modes">
        <button className={`pm ${mode === 'month' ? 'on' : ''}`}
          onClick={() => apply('month', year, month)}>شهري</button>
        <button className={`pm ${mode === 'year' ? 'on' : ''}`}
          onClick={() => apply('year', year, null)}>سنوي</button>
        <button className={`pm ${mode === 'all' ? 'on' : ''}`}
          onClick={() => apply('all', null, null)}>الكل</button>
      </div>

      {mode !== 'all' && (
        <div className="period-nav">
          <button className="pn-arrow" onClick={next} title="التالي">‹</button>

          <div className="period-selects">
            {mode === 'month' && (
              <select value={month} onChange={(e) => apply('month', year, Number(e.target.value))}>
                {MONTHS.map((m, i) => (
                  <option key={i} value={i + 1}>{m}</option>
                ))}
              </select>
            )}
            <select value={year}
              onChange={(e) => apply(mode, Number(e.target.value), month)}>
              {years.map((y) => <option key={y} value={y}>{y}</option>)}
            </select>
          </div>

          <button className="pn-arrow" onClick={prev} title="السابق">›</button>
        </div>
      )}

      <div className="period-label">{label}</div>

      {mode === 'month' && (
        <button className="period-today" onClick={goToday}>الشهر الحالي</button>
      )}

      <style>{`
        .period {
          display: flex; align-items: center; gap: 12px; flex-wrap: wrap;
          background: var(--surface); border: 1px solid var(--line);
          border-radius: var(--r-md); padding: 10px 14px; margin-bottom: 16px;
        }
        .period-modes { display: flex; gap: 3px; background: var(--surface-2); padding: 3px; border-radius: var(--r-sm); }
        .pm { padding: 6px 16px; border-radius: 7px; font-weight: 600; font-size: .87rem; color: var(--ink-soft); transition: all .15s ease; }
        .pm.on { background: var(--surface); color: var(--brand-deep); box-shadow: var(--shadow-sm); }

        .period-nav { display: flex; align-items: center; gap: 6px; }
        .pn-arrow {
          width: 30px; height: 30px; border-radius: 7px; font-size: 1.3rem;
          color: var(--ink-soft); background: var(--surface-2); line-height: 1;
        }
        .pn-arrow:hover { background: var(--brand-soft); color: var(--brand-deep); }
        .period-selects { display: flex; gap: 6px; }
        .period-selects select {
          padding: 7px 10px; border: 1px solid var(--line); border-radius: var(--r-sm);
          background: var(--surface); color: var(--ink); font-weight: 600; font-size: .88rem;
          cursor: pointer;
        }
        .period-selects select:focus { outline: none; border-color: var(--brand); }

        .period-label {
          font-family: var(--font-display); font-weight: 700; color: var(--brand-deep);
          margin-right: auto; font-size: 1rem;
        }
        .period-today {
          font-size: .82rem; color: var(--brand); font-weight: 600;
          padding: 5px 12px; border-radius: var(--r-sm); background: var(--brand-soft);
        }
        .period-today:hover { background: var(--brand); color: #fff; }

        @media (max-width: 640px) {
          .period { flex-direction: column; align-items: stretch; }
          .period-label { margin-right: 0; text-align: center; }
          .period-nav { justify-content: center; }
        }
      `}</style>
    </div>
  );
}
