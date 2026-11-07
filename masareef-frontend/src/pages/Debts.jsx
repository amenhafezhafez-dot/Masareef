import { useEffect, useState, useCallback } from 'react';
import { debtsApi } from '../api/client';
import { Spinner, Empty, Money, Modal, Field, Toast, StatusBadge, methodName } from '../components/UI';
import SearchBar from '../components/SearchBar';

const QUICK = [
  { key: 'all', label: 'الكل' },
  { key: 'pending', label: 'المفتوحة' },
  { key: 'overdue', label: 'المتأخرة' },
  { key: 'iOwe', label: 'عليّ' },
  { key: 'owedToMe', label: 'ليّ' },
];

const SEARCH_FIELDS = [
  { key: 'person', label: 'اسم الشخص', type: 'text', param: 'personName', placeholder: 'ابحث بالاسم...' },
  { key: 'amount', label: 'المبلغ', type: 'range', minParam: 'minAmount', maxParam: 'maxAmount' },
  { key: 'due', label: 'الاستحقاق', type: 'dateRange', fromParam: 'dueFrom', toParam: 'dueTo' },
  { key: 'debtDate', label: 'تاريخ الدين', type: 'dateRange', fromParam: 'debtFrom', toParam: 'debtTo' },
  {
    key: 'status', label: 'الحالة', type: 'select', param: 'status',
    options: [
      { value: 1, label: 'مفتوح' }, { value: 2, label: 'جزئي' },
      { value: 3, label: 'مدفوع' }, { value: 4, label: 'مشطوب' },
    ],
  },
];

export default function Debts() {
  const [quick, setQuick] = useState('all');
  const [filter, setFilter] = useState(null);
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [payFor, setPayFor] = useState(null);
  const [writeOff, setWriteOff] = useState(null);
  const [detailId, setDetailId] = useState(null);
  const [toast, setToast] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      let data;
      if (filter) {
        data = await debtsApi.advancedSearch(filter);
      } else {
        if (quick === 'overdue') data = await debtsApi.getOverdue();
        else if (quick === 'pending') data = await debtsApi.getPending();
        else if (quick === 'iOwe') data = await debtsApi.getIOwe();
        else if (quick === 'owedToMe') data = await debtsApi.getOwedToMe();
        else data = await debtsApi.getAll();
      }
      setRows(data || []);
    } catch (e) { setToast({ type: 'err', msg: e.message }); setRows([]); }
    finally { setLoading(false); }
  }, [quick, filter]);

  useEffect(() => { load(); }, [load]);

  const pickQuick = (k) => { setQuick(k); setFilter(null); };

  const totalOutstanding = rows.reduce((s, d) => s + Number(d.outstanding || 0), 0);

  return (
    <div>
      <div className="page-head-row">
        <div>
          <h1>الديون</h1>
          <p className="page-sub">تتبّع اللي عليك واللي ليك</p>
        </div>
        <button className="btn btn-primary pill-lite" onClick={() => { setEditing(null); setShowForm(true); }}>
          + دين جديد
        </button>
      </div>

      <div className="tabs">
        {QUICK.map((f) => (
          <button key={f.key} className={`tab ${quick === f.key && !filter ? 'active' : ''}`}
            onClick={() => pickQuick(f.key)}>
            {f.label}
          </button>
        ))}
      </div>

      <SearchBar
        fields={SEARCH_FIELDS}
        onSearch={(f) => setFilter(Object.keys(f).length ? f : null)}
        onReset={() => setFilter(null)}
      />

      <div className="card">
        {loading ? <Spinner /> : rows.length === 0 ? (
          <Empty kind={filter ? "search" : "exchange"}
            title={filter ? 'لا توجد نتائج' : 'لا يوجد ديون'}
            hint={filter ? 'جرّب بحث مختلف' : 'أضف أول دين لتتبّعه'} />
        ) : (
          <>
            <table className="table table-fill">
              <thead>
                <tr>
                  <th>الشخص</th>
                  <th>النوع</th>
                  <th>المبلغ</th>
                  <th>المتبقّي</th>
                  <th>الاستحقاق</th>
                  <th>الحالة</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {rows.map((d) => (
                  <tr key={d.debtID} className="clickable" onClick={() => setDetailId(d.debtID)}>
                    <td style={{ fontWeight: 600 }}>{d.personName}</td>
                    <td>
                      <span className={`dir ${d.direction === 2 ? 'dir-in' : 'dir-out'}`}>
                        {d.direction === 2 ? 'ليّ' : 'عليّ'}
                      </span>
                    </td>
                    <td><Money value={d.amount} /></td>
                    <td><Money value={d.outstanding} className="bold" /></td>
                    <td className="num">{d.dueDate ? d.dueDate.slice(0, 10) : '—'}</td>
                    <td><StatusBadge status={d.status} overdue={d.isOverdue} /></td>
                    <td onClick={(e) => e.stopPropagation()}>
                      {(d.status === 1 || d.status === 2) && (
                        <div className="row-actions">
                          <button className="btn btn-ghost btn-sm" onClick={() => setPayFor(d)}>دفعة</button>
                          <button className="btn btn-ghost btn-sm"
                            onClick={() => { setEditing(d); setShowForm(true); }}>تعديل</button>
                          <button className="btn btn-danger btn-sm" onClick={() => setWriteOff(d)}>شطب</button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="table-foot">
              <span>{rows.length} دين</span>
              <span>إجمالي المتبقّي: <Money value={totalOutstanding} className="bold" /></span>
            </div>
          </>
        )}
      </div>

      {showForm && <DebtForm editing={editing}
        onClose={() => { setShowForm(false); setEditing(null); }}
        onDone={(msg) => { setShowForm(false); setEditing(null); setToast({ type: 'ok', msg }); load(); }} />}

      {payFor && <PayModal debt={payFor} onClose={() => setPayFor(null)}
        onDone={() => { setPayFor(null); setToast({ type: 'ok', msg: 'تم تسجيل الدفعة' }); load(); }} />}

      {writeOff && <WriteOffModal debt={writeOff} onClose={() => setWriteOff(null)}
        onDone={() => { setWriteOff(null); setToast({ type: 'ok', msg: 'تم شطب الدين' }); load(); }} />}

      {detailId && <DetailModal id={detailId} onClose={() => setDetailId(null)} />}

      <Toast toast={toast} onDone={() => setToast(null)} />

      <style>{`
        .page-head-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
        .page-head-row h1 { font-size: 1.9rem; }
        .page-sub { color: var(--ink-soft); margin-top: 4px; }
        .tabs { display: flex; gap: 4px; margin-bottom: 16px; background: var(--surface-2); padding: 4px; border-radius: var(--r-sm); width: fit-content; flex-wrap: wrap; }
        .tab { padding: 8px 18px; border-radius: 8px; font-weight: 600; color: var(--ink-soft); transition: all .15s ease; }
        .tab.active { background: var(--surface); color: var(--brand-deep); box-shadow: var(--shadow-sm); }
        .clickable { cursor: pointer; }
        .dir { padding: 3px 12px; border-radius: 999px; font-size: .8rem; font-weight: 700; }
        .dir-in { background: var(--up-soft); color: var(--up); }
        .dir-out { background: var(--down-soft); color: var(--down); }
        .bold { font-weight: 700; }
        .row-actions { display: flex; gap: 5px; }
        .table-foot { display: flex; justify-content: space-between; padding: 16px 18px; border-top: 1px solid var(--line); font-weight: 600; font-size: .92rem; }
        @media (max-width: 760px) { .table th:nth-child(5), .table td:nth-child(5) { display: none; } }
      `}</style>
    </div>
  );
}

// ─────────── إضافة/تعديل دين ───────────
function DebtForm({ editing, onClose, onDone }) {
  const isEdit = !!editing;
  const [form, setForm] = useState({
    personName: editing?.personName || '',
    personPhone: editing?.personPhone || '',
    direction: editing?.direction || 2,
    amount: editing?.amount || '',
    dueDate: editing?.dueDate?.slice(0, 10) || '',
    description: editing?.description || '',
  });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      if (isEdit) {
        await debtsApi.update(editing.debtID, {
          personName: form.personName,
          personPhone: form.personPhone || null,
          amount: Number(form.amount),
          dueDate: form.dueDate || null,
          description: form.description || null,
        });
      } else {
        await debtsApi.create({
          personName: form.personName,
          personPhone: form.personPhone || null,
          direction: Number(form.direction),
          amount: Number(form.amount),
          debtDate: new Date().toISOString().slice(0, 10),
          dueDate: form.dueDate || null,
          description: form.description || null,
        });
      }
      onDone(isEdit ? 'تم التعديل' : 'تمت إضافة الدين');
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={isEdit ? 'تعديل الدين' : 'دين جديد'} onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="اسم الشخص">
          <input value={form.personName} onChange={set('personName')} required placeholder="الاسم" />
        </Field>
        <Field label="الهاتف (اختياري)">
          <input value={form.personPhone} onChange={set('personPhone')} placeholder="01xxxxxxxxx" />
        </Field>

        {!isEdit && (
          <Field label="النوع">
            <select value={form.direction} onChange={set('direction')}>
              <option value={2}>ليّ (شخص مدين لي)</option>
              <option value={1}>عليّ (أنا مدين)</option>
            </select>
          </Field>
        )}

        <Field label="المبلغ">
          <input type="number" step="0.01" min="0" value={form.amount}
            onChange={set('amount')} required placeholder="0.00" />
        </Field>
        <Field label="تاريخ الاستحقاق (اختياري)">
          <input type="date" value={form.dueDate} onChange={set('dueDate')} />
        </Field>
        <Field label="ملاحظة (اختياري)">
          <input value={form.description} onChange={set('description')} placeholder="تفاصيل" />
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

// ─────────── دفعة ───────────
function PayModal({ debt, onClose, onDone }) {
  const [form, setForm] = useState({ amount: '', method: 1, note: '' });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      await debtsApi.addPayment(debt.debtID, {
        paidAmount: Number(form.amount),
        paymentDate: new Date().toISOString().slice(0, 10),
        paymentMethod: Number(form.method),
        note: form.note || null,
      });
      onDone();
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={`دفعة — ${debt.personName}`} onClose={onClose}>
      <div className="pay-info">
        <span>المتبقّي</span>
        <Money value={debt.outstanding} className="pay-out" />
      </div>
      <form onSubmit={submit}>
        <Field label="مبلغ الدفعة">
          <input type="number" step="0.01" min="0" max={debt.outstanding}
            value={form.amount} onChange={set('amount')} required placeholder="0.00" />
        </Field>
        <Field label="طريقة الدفع">
          <select value={form.method} onChange={set('method')}>
            <option value={1}>كاش</option>
            <option value={2}>كارت</option>
            <option value={3}>تحويل</option>
          </select>
        </Field>
        <Field label="ملاحظة (اختياري)">
          <input value={form.note} onChange={set('note')} placeholder="تفاصيل" />
        </Field>
        {err && <div className="form-err">{err}</div>}
        <button className="btn btn-primary" style={{ width: '100%' }} disabled={busy}>
          {busy ? 'جارٍ...' : 'تسجيل الدفعة'}
        </button>
      </form>
      <style>{`
        .pay-info { display: flex; justify-content: space-between; align-items: center; background: var(--brand-soft); padding: 14px 16px; border-radius: var(--r-sm); margin-bottom: 18px; }
        .pay-info span { color: var(--ink-soft); font-weight: 600; }
        .pay-out { font-size: 1.3rem; font-weight: 800; color: var(--brand-deep); }
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); font-size: .88rem; margin-bottom: 14px; }
      `}</style>
    </Modal>
  );
}

// ─────────── شطب ───────────
function WriteOffModal({ debt, onClose, onDone }) {
  const [reason, setReason] = useState('');
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      await debtsApi.writeOff(debt.debtID, reason);
      onDone();
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={`شطب دين — ${debt.personName}`} onClose={onClose}>
      <div className="wo-warn">
        الشطب معناه إن الدين مش هيتحصّل. المتبقّي <Money value={debt.outstanding} /> هيتشال من حساباتك.
      </div>
      <form onSubmit={submit}>
        <Field label="سبب الشطب">
          <input value={reason} onChange={(e) => setReason(e.target.value)} required
            placeholder="مثلاً: تنازل، تعذّر التحصيل" />
        </Field>
        {err && <div className="form-err">{err}</div>}
        <button className="btn btn-danger" style={{ width: '100%' }} disabled={busy}>
          {busy ? 'جارٍ...' : 'تأكيد الشطب'}
        </button>
      </form>
      <style>{`
        .wo-warn { background: var(--gold-soft); color: #7a5a1c; padding: 13px 15px; border-radius: var(--r-sm); font-size: .89rem; margin-bottom: 18px; line-height: 1.6; }
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); font-size: .88rem; margin-bottom: 14px; }
      `}</style>
    </Modal>
  );
}

// ─────────── التفاصيل ───────────
function DetailModal({ id, onClose }) {
  const [d, setD] = useState(null);
  const [err, setErr] = useState('');

  useEffect(() => {
    debtsApi.getById(id).then(setD).catch((e) => setErr(e.message));
  }, [id]);

  return (
    <Modal title="تفاصيل الدين" onClose={onClose}>
      {err ? <div className="form-err">{err}</div> : !d ? <Spinner /> : (
        <div>
          <div className="det-name">{d.personName}</div>
          {d.personPhone && <div className="det-phone">{d.personPhone}</div>}

          <div className="det-grid">
            <div className="det-cell"><span>الأصل</span><Money value={d.amount} /></div>
            <div className="det-cell"><span>المدفوع</span><Money value={d.totalPaid} className="up-c" /></div>
            <div className="det-cell"><span>المتبقّي</span><Money value={d.outstanding} className="down-c" /></div>
            <div className="det-cell"><span>نسبة السداد</span><span className="num">{d.paidPercentage}%</span></div>
          </div>

          <div className="det-bar">
            <div className="det-bar-fill" style={{ width: `${Math.min(d.paidPercentage, 100)}%` }} />
          </div>

          <h4 className="det-h">الدفعات ({d.payments?.length || 0})</h4>
          {d.payments?.length ? (
            <div className="det-pays">
              {d.payments.map((p) => (
                <div key={p.paymentID} className="det-pay">
                  <span className="num">{p.paymentDate?.slice(0, 10)}</span>
                  <span>{methodName(p.paymentMethod)}</span>
                  <Money value={p.paidAmount} className="up-c" />
                </div>
              ))}
            </div>
          ) : <div className="det-empty">لا يوجد دفعات بعد</div>}
        </div>
      )}
      <style>{`
        .det-name { font-family: var(--font-display); font-size: 1.4rem; font-weight: 700; }
        .det-phone { color: var(--ink-faint); font-size: .9rem; margin-bottom: 18px; }
        .det-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin: 16px 0; }
        .det-cell { background: var(--surface-2); padding: 12px 14px; border-radius: var(--r-sm); }
        .det-cell span:first-child { display: block; font-size: .8rem; color: var(--ink-faint); margin-bottom: 4px; }
        .det-cell .num { font-size: 1.15rem; font-weight: 700; }
        .up-c { color: var(--up); } .down-c { color: var(--down); }
        .det-bar { height: 8px; background: var(--surface-2); border-radius: 999px; overflow: hidden; margin-bottom: 20px; }
        .det-bar-fill { height: 100%; background: var(--brand); border-radius: 999px; transition: width .4s ease; }
        .det-h { font-size: 1rem; margin-bottom: 10px; }
        .det-pays { display: flex; flex-direction: column; gap: 8px; }
        .det-pay { display: grid; grid-template-columns: 1fr auto auto; gap: 12px; padding: 10px 12px; background: var(--surface-2); border-radius: var(--r-sm); font-size: .9rem; align-items: center; }
        .det-empty { color: var(--ink-faint); text-align: center; padding: 16px; font-size: .9rem; }
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); }
      `}</style>
    </Modal>
  );
}
