import { useState } from 'react';

/**
 * شريط بحث موحّد
 * fields: [{ key, label, type }]
 *   type: 'text' | 'range' | 'dateRange' | 'select'
 *   للـ select: options: [{ value, label }]
 * onSearch(filterObject) — بيترجّع الفلتر جاهز للـ API
 */
export default function SearchBar({ fields, onSearch, onReset }) {
  const [fieldKey, setFieldKey] = useState(fields[0].key);
  const [text, setText] = useState('');
  const [min, setMin] = useState('');
  const [max, setMax] = useState('');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [selectVal, setSelectVal] = useState('');

  const field = fields.find((f) => f.key === fieldKey);

  const clearInputs = () => {
    setText(''); setMin(''); setMax(''); setFrom(''); setTo(''); setSelectVal('');
  };

  const changeField = (key) => {
    setFieldKey(key);
    clearInputs();
  };

  const submit = (e) => {
    e.preventDefault();

    const filter = {};

    if (field.type === 'text' && text.trim()) {
      filter[field.param] = text.trim();
    } else if (field.type === 'range') {
      if (min) filter[field.minParam] = Number(min);
      if (max) filter[field.maxParam] = Number(max);
    } else if (field.type === 'dateRange') {
      if (from) filter[field.fromParam] = from;
      if (to) filter[field.toParam] = to;
    } else if (field.type === 'select' && selectVal !== '') {
      filter[field.param] = isNaN(selectVal) ? selectVal : Number(selectVal);
    }

    onSearch(filter);
  };

  const reset = () => {
    clearInputs();
    onReset();
  };

  return (
    <form className="search-bar" onSubmit={submit}>
      {/* اختيار الحقل */}
      <select className="search-field" value={fieldKey} onChange={(e) => changeField(e.target.value)}>
        {fields.map((f) => (
          <option key={f.key} value={f.key}>{f.label}</option>
        ))}
      </select>

      {/* المدخلات — بتتغيّر حسب نوع الحقل */}
      <div className="search-inputs">
        {field.type === 'text' && (
          <input
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder={field.placeholder || 'اكتب للبحث...'}
          />
        )}

        {field.type === 'range' && (
          <>
            <input type="number" step="0.01" value={min}
              onChange={(e) => setMin(e.target.value)} placeholder="من" />
            <span className="search-sep">—</span>
            <input type="number" step="0.01" value={max}
              onChange={(e) => setMax(e.target.value)} placeholder="إلى" />
          </>
        )}

        {field.type === 'dateRange' && (
          <>
            <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
            <span className="search-sep">—</span>
            <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
          </>
        )}

        {field.type === 'select' && (
          <select value={selectVal} onChange={(e) => setSelectVal(e.target.value)}>
            <option value="">اختر...</option>
            {field.options.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        )}
      </div>

      <button type="submit" className="btn btn-primary btn-sm">بحث</button>
      <button type="button" className="btn btn-ghost btn-sm" onClick={reset}>مسح</button>

      <style>{`
        .search-bar {
          display: flex; align-items: center; gap: 8px; flex-wrap: wrap;
          background: var(--surface); border: 1px solid var(--line);
          border-radius: var(--r-md); padding: 12px 14px; margin-bottom: 16px;
        }
        .search-field {
          padding: 9px 12px; border: 1px solid var(--line); border-radius: var(--r-sm);
          background: var(--surface-2); font-weight: 600; color: var(--ink);
          min-width: 130px; cursor: pointer;
        }
        .search-field:focus { outline: none; border-color: var(--brand); }
        .search-inputs { display: flex; align-items: center; gap: 8px; flex: 1; min-width: 200px; }
        .search-inputs input, .search-inputs select {
          padding: 9px 12px; border: 1px solid var(--line); border-radius: var(--r-sm);
          background: var(--surface); color: var(--ink); flex: 1; min-width: 100px;
        }
        .search-inputs input:focus, .search-inputs select:focus {
          outline: none; border-color: var(--brand); box-shadow: 0 0 0 3px var(--brand-soft);
        }
        .search-sep { color: var(--ink-faint); font-weight: 600; }
        @media (max-width: 640px) {
          .search-bar { flex-direction: column; align-items: stretch; }
          .search-field, .search-inputs { width: 100%; }
        }
      `}</style>
    </form>
  );
}
