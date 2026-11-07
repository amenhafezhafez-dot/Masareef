import { useEffect, useState, useCallback } from 'react';
import { homeApi, categoriesApi } from '../api/client';
import { Spinner, Empty, Money, Modal, Field, Toast, methodName } from '../components/UI';
import SearchBar from '../components/SearchBar';
import PeriodPicker from '../components/PeriodPicker';

const EXPENSE_FIELDS = [
  { key: 'text', label: 'الوصف', type: 'text', param: 'searchText', placeholder: 'ابحث في الوصف...' },
  { key: 'amount', label: 'المبلغ', type: 'range', minParam: 'minAmount', maxParam: 'maxAmount' },
  { key: 'date', label: 'التاريخ', type: 'dateRange', fromParam: 'dateFrom', toParam: 'dateTo' },
  {
    key: 'method', label: 'طريقة الدفع', type: 'select', param: 'paymentMethod',
    options: [{ value: 1, label: 'كاش' }, { value: 2, label: 'كارت' }, { value: 3, label: 'تحويل' }],
  },
];

const INCOME_FIELDS = [
  { key: 'text', label: 'المصدر/الوصف', type: 'text', param: 'searchText', placeholder: 'ابحث...' },
  { key: 'amount', label: 'المبلغ', type: 'range', minParam: 'minAmount', maxParam: 'maxAmount' },
  { key: 'date', label: 'التاريخ', type: 'dateRange', fromParam: 'dateFrom', toParam: 'dateTo' },
];

export default function Home() {
  const now = new Date();
  const [tab, setTab] = useState('expenses');
  const [period, setPeriod] = useState({
    mode: 'month', year: now.getFullYear(), month: now.getMonth() + 1,
  });
  const [expenses, setExpenses] = useState([]);
  const [incomes, setIncomes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [toast, setToast] = useState(null);

  const isExpense = tab === 'expenses';

  // بنجيب الاتنين عشان نحسب الرصيد
  const load = useCallback(async () => {
    setLoading(true);
    try {
      const base = {};
      if (period.year) base.year = period.year;
      if (period.month) base.month = period.month;

      const expFilter = { ...base, ...(filter || {}) };
      const incFilter = { ...base, ...(filter || {}) };

      const [e, i] = await Promise.all([
        homeApi.advancedSearchExpenses(expFilter),
        homeApi.advancedSearchIncomes(incFilter),
      ]);

      setExpenses(e || []);
      setIncomes(i || []);
    } catch (e) {
      setToast({ type: 'err', msg: e.message });
      setExpenses([]); setIncomes([]);
    }
    finally { setLoading(false); }
  }, [period, filter]);

  useEffect(() => { load(); }, [load]);

  const rows = isExpense ? expenses : incomes;
  const totalExpense = expenses.reduce((s, r) => s + Number(r.amount), 0);
  const totalIncome = incomes.reduce((s, r) => s + Number(r.amount), 0);
  const balance = totalIncome - totalExpense;
  const savingsRate = totalIncome === 0 ? 0 : (balance / totalIncome) * 100;

  const del = async (id) => {
    if (!confirm('تأكيد الحذف؟')) return;
    try {
      if (isExpense) await homeApi.deleteExpense(id);
      else await homeApi.deleteIncome(id);
      setToast({ type: 'ok', msg: 'تم الحذف' });
      load();
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
  };

  return (
    <div>
      <div className="page-head-row">
        <div>
          <h1>البيت</h1>
          <p className="page-sub">مصاريفك ودخلك الشخصي</p>
        </div>
        <button className="btn btn-primary pill-lite" onClick={() => { setEditing(null); setShowForm(true); }}>
          + إضافة {isExpense ? 'مصروف' : 'دخل'}
        </button>
      </div>

      <PeriodPicker value={period} onChange={setPeriod} />

      {/* ملخّص الفترة */}
      <div className="sum-row">
        <SumCard label="الدخل" value={totalIncome} tone="up" count={incomes.length} />
        <SumCard label="المصاريف" value={totalExpense} tone="down" count={expenses.length} />
        <SumCard label="الرصيد" value={balance} tone={balance >= 0 ? 'up' : 'down'} />
        <SumCard label="نسبة الادّخار" value={savingsRate} tone="neutral" isPercent />
      </div>

      <div className="tabs">
        <button className={`tab ${isExpense ? 'active' : ''}`} onClick={() => setTab('expenses')}>
          المصاريف ({expenses.length})
        </button>
        <button className={`tab ${!isExpense ? 'active' : ''}`} onClick={() => setTab('incomes')}>
          الدخل ({incomes.length})
        </button>
      </div>

      <SearchBar key={tab}
        fields={isExpense ? EXPENSE_FIELDS : INCOME_FIELDS}
        onSearch={(f) => setFilter(Object.keys(f).length ? f : null)}
        onReset={() => setFilter(null)} />

      <div className="card">
        {loading ? <Spinner /> : rows.length === 0 ? (
          <Empty kind={filter ? "search" : isExpense ? "ledger" : "money"}
            title={filter ? 'لا توجد نتائج' : `لا يوجد ${isExpense ? 'مصاريف' : 'دخل'} في هذه الفترة`}
            hint={filter ? 'جرّب بحث مختلف' : 'ابدأ بإضافة أول عملية'} />
        ) : (
          <>
            <table className="table table-fill">
              <thead>
                <tr>
                  <th>التاريخ</th>
                  <th>التصنيف</th>
                  <th>{isExpense ? 'الطريقة' : 'المصدر'}</th>
                  <th>ملاحظة</th>
                  <th>المبلغ</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => {
                  const id = isExpense ? r.homeExpenseId : r.homeIncomeId;
                  const date = (r.expenseDate || r.incomeDate)?.slice(0, 10);
                  return (
                    <tr key={id}>
                      <td className="num">{date}</td>
                      <td>{r.categoryName}</td>
                      <td>{isExpense ? methodName(r.paymentMethod) : (r.source || '—')}</td>
                      <td style={{ color: 'var(--ink-faint)' }}>{r.description || '—'}</td>
                      <td><Money value={r.amount} className={isExpense ? 'down-c' : 'up-c'} /></td>
                      <td>
                        <div className="row-actions">
                          <button className="btn btn-ghost btn-sm"
                            onClick={() => { setEditing({ ...r, id, date }); setShowForm(true); }}>تعديل</button>
                          <button className="btn btn-danger btn-sm" onClick={() => del(id)}>حذف</button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            <div className="table-foot">
              <span>{rows.length} عملية</span>
              <span>الإجمالي: <Money value={isExpense ? totalExpense : totalIncome}
                className={isExpense ? 'down-c' : 'up-c'} /></span>
            </div>
          </>
        )}
      </div>

      {showForm && (
        <TransactionForm kind={tab} editing={editing}
          onClose={() => { setShowForm(false); setEditing(null); }}
          onDone={(msg) => { setShowForm(false); setEditing(null); setToast({ type: 'ok', msg }); load(); }} />
      )}

      <Toast toast={toast} onDone={() => setToast(null)} />

      <style>{`
        .page-head-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 18px; }
        .page-head-row h1 { font-size: 1.9rem; }
        .page-sub { color: var(--ink-soft); margin-top: 4px; }
        .sum-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 18px; }
        .tabs { display: flex; gap: 4px; margin-bottom: 16px; background: var(--surface-2); padding: 4px; border-radius: var(--r-sm); width: fit-content; }
        .tab { padding: 8px 22px; border-radius: 8px; font-weight: 600; color: var(--ink-soft); transition: all .15s ease; }
        .tab.active { background: var(--surface); color: var(--brand-deep); box-shadow: var(--shadow-sm); }
        .table-foot { display: flex; justify-content: space-between; padding: 16px 18px; border-top: 1px solid var(--line); font-weight: 600; font-size: .92rem; }
        .row-actions { display: flex; gap: 6px; }
        .down-c { color: var(--down); } .up-c { color: var(--up); }
        @media (max-width: 900px) { .sum-row { grid-template-columns: repeat(2, 1fr); } }
        @media (max-width: 700px) { .table th:nth-child(4), .table td:nth-child(4) { display: none; } }
      `}</style>
    </div>
  );
}

function SumCard({ label, value, tone, count, isPercent }) {
  return (
    <div className="card sum-card rise">
      <div className="sum-label">{label}</div>
      <div className={`sum-val ${tone}`}>
        {isPercent ? <span className="num">{Number(value).toFixed(1)}%</span> : <Money value={value} />}
      </div>
      {count !== undefined && <div className="sum-count">{count} عملية</div>}
      <style>{`
        .sum-card { padding: 14px 16px; }
        .sum-label { font-size: .82rem; color: var(--ink-soft); font-weight: 600; }
        .sum-val { font-family: var(--font-display); font-size: 1.4rem; font-weight: 700; margin-top: 4px; }
        .sum-val.up { color: var(--up); } .sum-val.down { color: var(--down); } .sum-val.neutral { color: var(--ink); }
        .sum-count { font-size: .76rem; color: var(--ink-faint); margin-top: 2px; }
      `}</style>
    </div>
  );
}

// ─────────── نموذج الإضافة/التعديل ───────────
function TransactionForm({ kind, editing, onClose, onDone }) {
  const isExpense = kind === 'expenses';
  const isEdit = !!editing;
  const [cats, setCats] = useState([]);
  const [form, setForm] = useState({
    categoryId: editing?.categoryId || '',
    amount: editing?.amount || '',
    date: editing?.date || new Date().toISOString().slice(0, 10),
    paymentMethod: editing?.paymentMethod || 1,
    source: editing?.source || '',
    description: editing?.description || '',
  });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');

  useEffect(() => {
    const fn = isExpense ? categoriesApi.getHomeExpenses : categoriesApi.getHomeIncomes;
    fn().then(setCats).catch(() => setCats([]));
  }, [isExpense]);

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      if (isExpense) {
        const dto = {
          categoryId: Number(form.categoryId),
          amount: Number(form.amount),
          expenseDate: form.date,
          paymentMethod: Number(form.paymentMethod),
          description: form.description || null,
        };
        if (isEdit) await homeApi.updateExpense(editing.id, dto);
        else await homeApi.addExpense(dto);
      } else {
        const dto = {
          categoryId: Number(form.categoryId),
          amount: Number(form.amount),
          incomeDate: form.date,
          source: form.source || null,
          description: form.description || null,
        };
        if (isEdit) await homeApi.updateIncome(editing.id, dto);
        else await homeApi.addIncome(dto);
      }
      onDone(isEdit ? 'تم التعديل' : (isExpense ? 'تمت إضافة المصروف' : 'تمت إضافة الدخل'));
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={`${isEdit ? 'تعديل' : 'إضافة'} ${isExpense ? 'مصروف' : 'دخل'}`} onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="التصنيف">
          <select value={form.categoryId} onChange={set('categoryId')} required>
            <option value="">اختر التصنيف</option>
            {cats.map((c) => <option key={c.categoryId} value={c.categoryId}>{c.categoryName}</option>)}
          </select>
        </Field>

        <Field label="المبلغ">
          <input type="number" step="0.01" min="0" value={form.amount}
            onChange={set('amount')} required placeholder="0.00" />
        </Field>

        <Field label="التاريخ">
          <input type="date" value={form.date} onChange={set('date')} required />
        </Field>

        {isExpense ? (
          <Field label="طريقة الدفع">
            <select value={form.paymentMethod} onChange={set('paymentMethod')}>
              <option value={1}>كاش</option>
              <option value={2}>كارت</option>
              <option value={3}>تحويل</option>
            </select>
          </Field>
        ) : (
          <Field label="المصدر (اختياري)">
            <input value={form.source} onChange={set('source')} placeholder="مثلاً: العمل" />
          </Field>
        )}

        <Field label="ملاحظة (اختياري)">
          <input value={form.description} onChange={set('description')} placeholder="تفاصيل إضافية" />
        </Field>

        {cats.length === 0 && <div className="form-warn">لا يوجد تصنيفات — أضف تصنيفات أولاً.</div>}
        {err && <div className="form-err">{err}</div>}

        <button className="btn btn-primary" style={{ width: '100%' }} disabled={busy}>
          {busy ? 'جارٍ...' : isEdit ? 'حفظ التعديل' : 'حفظ'}
        </button>
      </form>
      <style>{`
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); font-size: .88rem; margin-bottom: 14px; }
        .form-warn { background: var(--gold-soft); color: var(--gold); padding: 10px 14px; border-radius: var(--r-sm); font-size: .85rem; margin-bottom: 14px; }
      `}</style>
    </Modal>
  );
}
