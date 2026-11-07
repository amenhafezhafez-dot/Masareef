import { useEffect, useState, useCallback } from 'react';
import { categoriesApi } from '../api/client';
import { Spinner, Empty, Modal, Field, Toast } from '../components/UI';

const SCOPES = [
  { value: 1, label: 'البيزنس' },
  { value: 2, label: 'البيت' },
  { value: 3, label: 'الاتنين' },
];

const KINDS = [
  { value: 1, label: 'مصروف' },
  { value: 2, label: 'دخل' },
];

export default function Categories() {
  const [scope, setScope] = useState(2);
  const [kind, setKind] = useState(1);
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [toast, setToast] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await categoriesApi.get(scope, kind);
      setRows(data || []);
    } catch (e) { setToast({ type: 'err', msg: e.message }); setRows([]); }
    finally { setLoading(false); }
  }, [scope, kind]);

  useEffect(() => { load(); }, [load]);

  const del = async (c) => {
    if (!confirm(`حذف تصنيف "${c.categoryName}"؟`)) return;
    try {
      await categoriesApi.delete(c.categoryId);
      setToast({ type: 'ok', msg: 'تم الحذف' });
      load();
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
  };

  return (
    <div>
      <div className="page-head-row">
        <div>
          <h1>التصنيفات</h1>
          <p className="page-sub">نظّم مصاريفك ودخلك</p>
        </div>
        <button className="btn btn-primary pill-lite" onClick={() => { setEditing(null); setShowForm(true); }}>
          + تصنيف جديد
        </button>
      </div>

      <div className="cat-filters">
        <div className="filter-group">
          <label>النطاق</label>
          <div className="tabs">
            {SCOPES.map((s) => (
              <button key={s.value} className={`tab ${scope === s.value ? 'active' : ''}`}
                onClick={() => setScope(s.value)}>{s.label}</button>
            ))}
          </div>
        </div>

        <div className="filter-group">
          <label>النوع</label>
          <div className="tabs">
            {KINDS.map((k) => (
              <button key={k.value} className={`tab ${kind === k.value ? 'active' : ''}`}
                onClick={() => setKind(k.value)}>{k.label}</button>
            ))}
          </div>
        </div>
      </div>

      <div className="card">
        {loading ? <Spinner /> : rows.length === 0 ? (
          <Empty kind="chart" title="لا يوجد تصنيفات" hint="أضف أول تصنيف" />
        ) : (
          <table className="table table-fill">
            <thead>
              <tr>
                <th>الاسم</th>
                <th>النوع</th>
                <th>النطاق</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {rows.map((c) => (
                <tr key={c.categoryId}>
                  <td style={{ fontWeight: 600 }}>{c.categoryName}</td>
                  <td>{KINDS.find((k) => k.value === c.categoryKind)?.label}</td>
                  <td>{SCOPES.find((s) => s.value === c.categoryScope)?.label}</td>
                  <td>
                    <div className="row-actions">
                      <button className="btn btn-ghost btn-sm"
                        onClick={() => { setEditing(c); setShowForm(true); }}>تعديل</button>
                      <button className="btn btn-danger btn-sm" onClick={() => del(c)}>حذف</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="cat-note">
        التصنيفات الأساسية للنظام غير قابلة للتعديل أو الحذف، والتصنيف المستخدم في عمليات لا يمكن حذفه.
      </div>

      {showForm && (
        <CategoryForm editing={editing} defaultScope={scope} defaultKind={kind}
          onClose={() => { setShowForm(false); setEditing(null); }}
          onDone={(msg) => { setShowForm(false); setEditing(null); setToast({ type: 'ok', msg }); load(); }} />
      )}

      <Toast toast={toast} onDone={() => setToast(null)} />

      <style>{`
        .page-head-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
        .page-head-row h1 { font-size: 1.9rem; }
        .page-sub { color: var(--ink-soft); margin-top: 4px; }
        .cat-filters { display: flex; gap: 24px; margin-bottom: 18px; flex-wrap: wrap; }
        .filter-group label { display: block; font-size: .82rem; font-weight: 600; color: var(--ink-soft); margin-bottom: 6px; }
        .tabs { display: flex; gap: 4px; background: var(--surface-2); padding: 4px; border-radius: var(--r-sm); width: fit-content; }
        .tab { padding: 7px 18px; border-radius: 8px; font-weight: 600; color: var(--ink-soft); font-size: .9rem; transition: all .15s ease; }
        .tab.active { background: var(--surface); color: var(--brand-deep); box-shadow: var(--shadow-sm); }
        .row-actions { display: flex; gap: 6px; }
        .cat-note { margin-top: 14px; font-size: .84rem; color: var(--ink-faint); text-align: center; }
      `}</style>
    </div>
  );
}

function CategoryForm({ editing, defaultScope, defaultKind, onClose, onDone }) {
  const isEdit = !!editing;
  const [form, setForm] = useState({
    name: editing?.categoryName || '',
    icon: editing?.icon || '',
    kind: editing?.categoryKind || defaultKind,
    scope: editing?.categoryScope || defaultScope,
  });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      const dto = {
        name: form.name,
        icon: form.icon || null,
        kind: Number(form.kind),
        scope: Number(form.scope),
      };
      if (isEdit) await categoriesApi.update(editing.categoryId, dto);
      else await categoriesApi.create(dto);
      onDone(isEdit ? 'تم التعديل' : 'تمت إضافة التصنيف');
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={isEdit ? 'تعديل التصنيف' : 'تصنيف جديد'} onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="اسم التصنيف">
          <input value={form.name} onChange={set('name')} required placeholder="مثلاً: أكل ومشروبات" />
        </Field>

        <Field label="النوع">
          <select value={form.kind} onChange={set('kind')}>
            {KINDS.map((k) => <option key={k.value} value={k.value}>{k.label}</option>)}
          </select>
        </Field>

        <Field label="النطاق">
          <select value={form.scope} onChange={set('scope')}>
            {SCOPES.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
          </select>
        </Field>

        <Field label="أيقونة (اختياري)">
          <input value={form.icon} onChange={set('icon')} placeholder="مثلاً: food" />
        </Field>

        {err && <div className="form-err">{err}</div>}
        <button className="btn btn-primary" style={{ width: '100%' }} disabled={busy}>
          {busy ? 'جارٍ...' : isEdit ? 'حفظ التعديل' : 'حفظ'}
        </button>
      </form>
      <style>{`.form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); font-size: .88rem; margin-bottom: 14px; }`}</style>
    </Modal>
  );
}
