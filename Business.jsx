import { useEffect, useState, useCallback } from 'react';
import { businessApi, categoriesApi, productsApi } from '../api/client';
import { Spinner, Empty, Money, Modal, Field, Toast, methodName } from '../components/UI';
import SearchBar from '../components/SearchBar';
import PeriodPicker from '../components/PeriodPicker';
import Products from './Products';

const BIZ_FIELDS = [
  { key: 'name', label: 'اسم المحل', type: 'text', param: 'name', placeholder: 'ابحث بالاسم...' },
  { key: 'currency', label: 'العملة', type: 'text', param: 'currency', placeholder: 'EGP' },
  { key: 'created', label: 'تاريخ الإنشاء', type: 'dateRange', fromParam: 'createdFrom', toParam: 'createdTo' },
];

const TX_FIELDS = [
  { key: 'text', label: 'الوصف', type: 'text', param: 'searchText', placeholder: 'ابحث...' },
  { key: 'amount', label: 'المبلغ', type: 'range', minParam: 'minAmount', maxParam: 'maxAmount' },
  { key: 'date', label: 'التاريخ', type: 'dateRange', fromParam: 'dateFrom', toParam: 'dateTo' },
];

export default function Business() {
  const now = new Date();
  const [list, setList] = useState([]);
  const [stats, setStats] = useState({});      // businessId → { income, expense }
  const [loading, setLoading] = useState(true);
  const [showInactive, setShowInactive] = useState(false);
  const [filter, setFilter] = useState(null);
  const [period, setPeriod] = useState({
    mode: 'month', year: now.getFullYear(), month: now.getMonth() + 1,
  });
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [openBiz, setOpenBiz] = useState(null);
  const [toast, setToast] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      // ١. المحلات
      let bizList;
      if (filter) bizList = await businessApi.advancedSearch({ ...filter, isActive: !showInactive });
      else bizList = showInactive ? await businessApi.getInactive() : await businessApi.getAll();

      bizList = bizList || [];
      setList(bizList);

      // ٢. إحصائيات كل محل حسب الفترة المختارة
      //    (بنجيب العمليات ونجمعها — عشان الشهري والسنوي والكل يشتغلوا)
      const txFilter = {};
      if (period.year) txFilter.year = period.year;
      if (period.month) txFilter.month = period.month;

      const results = await Promise.all(
        bizList.map(async (b) => {
          try {
            // ⭐ الدخل من مبيعات المنتجات، والمصاريف من BusinessExpenses
            const saleFilter = {};
            if (period.year) {
              const y = period.year;
              if (period.month) {
                const m = String(period.month).padStart(2, '0');
                const lastDay = new Date(y, period.month, 0).getDate();
                saleFilter.dateFrom = `${y}-${m}-01`;
                saleFilter.dateTo = `${y}-${m}-${lastDay}`;
              } else {
                saleFilter.dateFrom = `${y}-01-01`;
                saleFilter.dateTo = `${y}-12-31`;
              }
            }

            const [exp, sales] = await Promise.all([
              businessApi.searchExpenses(b.businessId, txFilter),
              productsApi.getSales(b.businessId, saleFilter),
            ]);

            return {
              id: b.businessId,
              expense: (exp || []).reduce((s, x) => s + Number(x.amount), 0),
              income: (sales || []).reduce(
                (s, x) => s + Number(x.salePrice) * Number(x.quantitySold), 0),
              expenseCount: (exp || []).length,
              incomeCount: (sales || []).length,
            };
          } catch {
            return { id: b.businessId, expense: 0, income: 0, expenseCount: 0, incomeCount: 0 };
          }
        })
      );

      const map = {};
      results.forEach((r) => { map[r.id] = r; });
      setStats(map);
    } catch (e) { setToast({ type: 'err', msg: e.message }); setList([]); }
    finally { setLoading(false); }
  }, [filter, showInactive, period]);

  useEffect(() => { load(); }, [load]);

  // إجماليات كل المحلات
  const grandIncome = Object.values(stats).reduce((s, x) => s + x.income, 0);
  const grandExpense = Object.values(stats).reduce((s, x) => s + x.expense, 0);
  const grandNet = grandIncome - grandExpense;

  const toggleActive = async (b, e) => {
    e.stopPropagation();
    try {
      if (b.isActive) {
        if (!confirm(`تعطيل "${b.businessName}"؟ التاريخ المالي هيفضل محفوظ.`)) return;
        await businessApi.deactivate(b.businessId);
        setToast({ type: 'ok', msg: 'تم التعطيل' });
      } else {
        await businessApi.reactivate(b.businessId);
        setToast({ type: 'ok', msg: 'تم التفعيل' });
      }
      load();
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
  };

  if (openBiz) {
    return <BusinessDetail biz={openBiz} onBack={() => { setOpenBiz(null); load(); }} />;
  }

  return (
    <div>
      <div className="page-head-row">
        <div>
          <h1>البيزنس</h1>
          <p className="page-sub">{showInactive ? 'المحلات المعطّلة' : 'المحلات النشطة'}</p>
        </div>
        <button className="btn btn-primary pill-lite" onClick={() => { setEditing(null); setShowForm(true); }}>
          + محل جديد
        </button>
      </div>

      <PeriodPicker value={period} onChange={setPeriod} />

      {/* إجمالي كل المحلات في الفترة */}
      {list.length > 0 && (
        <div className="sum-row">
          <SumCard label="إجمالي المبيعات" value={grandIncome} tone="up" />
          <SumCard label="إجمالي المصاريف" value={grandExpense} tone="down" />
          <SumCard label="صافي الربح" value={grandNet} tone={grandNet >= 0 ? 'up' : 'down'} />
          <SumCard label="عدد المحلات" value={list.length} tone="neutral" isCount />
        </div>
      )}

      <div className="tabs">
        <button className={`tab ${!showInactive ? 'active' : ''}`}
          onClick={() => { setShowInactive(false); setFilter(null); }}>النشطة</button>
        <button className={`tab ${showInactive ? 'active' : ''}`}
          onClick={() => { setShowInactive(true); setFilter(null); }}>المعطّلة</button>
      </div>

      <SearchBar fields={BIZ_FIELDS}
        onSearch={(f) => setFilter(Object.keys(f).length ? f : null)}
        onReset={() => setFilter(null)} />

      {loading ? <Spinner /> : list.length === 0 ? (
        <div className="card">
          <Empty kind={filter ? "search" : "ledger"} title={filter ? 'لا توجد نتائج' : 'لا يوجد محلات'}
            hint={filter ? 'جرّب بحث مختلف' : 'أضف أول محل لتتبّع أرباحه'} />
        </div>
      ) : (
        <div className="biz-grid">
          {list.map((b) => {
            const s = stats[b.businessId] || { income: 0, expense: 0, incomeCount: 0, expenseCount: 0 };
            const net = s.income - s.expense;
            const margin = s.income === 0 ? 0 : (net / s.income) * 100;
            return (
              <div key={b.businessId} className="card biz-card" onClick={() => setOpenBiz(b)}>
                <div className="biz-head">
                  <div>
                    <div className="biz-name">{b.businessName}</div>
                    <div className="biz-desc">{b.description || 'بدون وصف'}</div>
                  </div>
                  <span className="biz-cur">{b.currency}</span>
                </div>

                <div className={`biz-net ${net >= 0 ? 'pos' : 'neg'}`}>
                  <Money value={net} />
                  <span className="biz-net-label">
                    {net >= 0 ? 'ربح' : 'خسارة'} · هامش {margin.toFixed(1)}%
                  </span>
                </div>

                <div className="biz-rows">
                  <div className="biz-row">
                    <span>مبيعات المنتجات ({s.incomeCount})</span>
                    <Money value={s.income} className="up-c" />
                  </div>
                  <div className="biz-row">
                    <span>المصاريف ({s.expenseCount})</span>
                    <Money value={s.expense} className="down-c" />
                  </div>
                </div>

                <div className="biz-foot" onClick={(e) => e.stopPropagation()}>
                  <button className="btn btn-ghost btn-sm"
                    onClick={(e) => { e.stopPropagation(); setEditing(b); setShowForm(true); }}>تعديل</button>
                  <button className={`btn btn-sm ${b.isActive ? 'btn-danger' : 'btn-ghost'}`}
                    onClick={(e) => toggleActive(b, e)}>
                    {b.isActive ? 'تعطيل' : 'تفعيل'}
                  </button>
                  <span className="biz-open">العمليات والمنتجات ←</span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {showForm && <BusinessForm editing={editing}
        onClose={() => { setShowForm(false); setEditing(null); }}
        onDone={(msg) => { setShowForm(false); setEditing(null); setToast({ type: 'ok', msg }); load(); }} />}

      <Toast toast={toast} onDone={() => setToast(null)} />

      <style>{`
        .page-head-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 18px; }
        .page-head-row h1 { font-size: 1.9rem; }
        .page-sub { color: var(--ink-soft); margin-top: 4px; }
        .sum-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 18px; }
        .tabs { display: flex; gap: 4px; margin-bottom: 16px; background: var(--surface-2); padding: 4px; border-radius: var(--r-sm); width: fit-content; }
        .tab { padding: 8px 22px; border-radius: 8px; font-weight: 600; color: var(--ink-soft); transition: all .15s ease; }
        .tab.active { background: var(--surface); color: var(--brand-deep); box-shadow: var(--shadow-sm); }
        .biz-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(290px, 1fr)); gap: 18px; }
        .biz-card { padding: 22px; cursor: pointer; transition: transform .15s ease, box-shadow .2s ease; }
        .biz-card:hover { transform: translateY(-2px); box-shadow: var(--shadow-md); }
        .biz-head { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 18px; }
        .biz-name { font-family: var(--font-display); font-size: 1.2rem; font-weight: 700; }
        .biz-desc { font-size: .84rem; color: var(--ink-faint); margin-top: 2px; }
        .biz-cur { background: var(--surface-2); padding: 3px 10px; border-radius: 6px; font-size: .78rem; font-weight: 700; color: var(--ink-soft); }
        .biz-net { padding: 16px 0; border-top: 1px solid var(--line-soft); border-bottom: 1px solid var(--line-soft); text-align: center; margin-bottom: 14px; }
        .biz-net .num { font-family: var(--font-display); font-size: 1.9rem; font-weight: 800; display: block; }
        .biz-net.pos .num { color: var(--up); }
        .biz-net.neg .num { color: var(--down); }
        .biz-net-label { font-size: .8rem; color: var(--ink-faint); }
        .biz-rows { display: flex; flex-direction: column; gap: 8px; margin-bottom: 14px; }
        .biz-row { display: flex; justify-content: space-between; font-size: .92rem; }
        .biz-row span { color: var(--ink-soft); }
        .biz-foot { display: flex; align-items: center; gap: 6px; padding-top: 12px; border-top: 1px solid var(--line-soft); }
        .biz-open { margin-right: auto; font-size: .85rem; color: var(--brand); font-weight: 600; }
        .up-c { color: var(--up); font-weight: 600; }
        .down-c { color: var(--down); font-weight: 600; }
        @media (max-width: 900px) { .sum-row { grid-template-columns: repeat(2, 1fr); } }
      `}</style>
    </div>
  );
}

function SumCard({ label, value, tone, isCount, isPercent }) {
  return (
    <div className="card sum-card rise">
      <div className="sum-label">{label}</div>
      <div className={`sum-val ${tone}`}>
        {isCount ? <span className="num">{value}</span>
          : isPercent ? <span className="num">{Number(value).toFixed(1)}%</span>
            : <Money value={value} />}
      </div>
      <style>{`
        .sum-card { padding: 14px 16px; }
        .sum-label { font-size: .82rem; color: var(--ink-soft); font-weight: 600; }
        .sum-val { font-family: var(--font-display); font-size: 1.4rem; font-weight: 700; margin-top: 4px; }
        .sum-val.up { color: var(--up); } .sum-val.down { color: var(--down); } .sum-val.neutral { color: var(--ink); }
      `}</style>
    </div>
  );
}

// ═══════════════ تفاصيل المحل ═══════════════
function BusinessDetail({ biz, onBack }) {
  const now = new Date();
  const [showProducts, setShowProducts] = useState(false);
  const [tab, setTab] = useState('expenses');
  const [period, setPeriod] = useState({
    mode: 'month', year: now.getFullYear(), month: now.getMonth() + 1,
  });
  const [expenses, setExpenses] = useState([]);
  const [sales, setSales] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [toast, setToast] = useState(null);

  const isExpense = tab === 'expenses';

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const base = {};
      if (period.year) base.year = period.year;
      if (period.month) base.month = period.month;
      const f = { ...base, ...(filter || {}) };

      // ⭐ الدخل بقى من مبيعات المنتجات مش من BusinessIncomes
      // بنحوّل فلتر الفترة لنطاق تواريخ عشان الـ sales API
      const saleFilter = { ...(filter || {}) };
      if (period.year) {
        const y = period.year;
        if (period.month) {
          const m = String(period.month).padStart(2, '0');
          const lastDay = new Date(y, period.month, 0).getDate();
          saleFilter.dateFrom = `${y}-${m}-01`;
          saleFilter.dateTo = `${y}-${m}-${lastDay}`;
        } else {
          saleFilter.dateFrom = `${y}-01-01`;
          saleFilter.dateTo = `${y}-12-31`;
        }
      }

      const [e, s] = await Promise.all([
        businessApi.searchExpenses(biz.businessId, f),
        productsApi.getSales(biz.businessId, saleFilter),
      ]);
      setExpenses(e || []);
      setSales(s || []);
    } catch (e) {
      setToast({ type: 'err', msg: e.message });
      setExpenses([]); setSales([]);
    }
    finally { setLoading(false); }
  }, [biz.businessId, period, filter]);

  useEffect(() => { load(); }, [load]);

  const rows = isExpense ? expenses : sales;
  const totalExpense = expenses.reduce((s, r) => s + Number(r.amount), 0);
  // ⭐ الدخل = مجموع مبيعات المنتجات
  const totalIncome = sales.reduce((s, r) => s + Number(r.salePrice) * Number(r.quantitySold), 0);
  const net = totalIncome - totalExpense;
  const margin = totalIncome === 0 ? 0 : (net / totalIncome) * 100;

  const del = async (id) => {
    if (!confirm('تأكيد الحذف؟')) return;
    try {
      if (isExpense) await businessApi.deleteExpense(id);
      else await businessApi.deleteIncome(id);
      setToast({ type: 'ok', msg: 'تم الحذف' });
      load();
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
  };

  // شاشة المنتجات — بتفتح جوّه المحل
  if (showProducts) {
    return <Products business={biz} onBack={() => setShowProducts(false)} />;
  }

  return (
    <div>
      <button className="back-btn" onClick={onBack}>→ رجوع للمحلات</button>

      <div className="page-head-row">
        <div>
          <h1>{biz.businessName}</h1>
          <p className="page-sub">{biz.description || 'بدون وصف'} · {biz.currency}</p>
        </div>
        <div className="head-actions">
          <button className="btn btn-ghost" onClick={() => setShowProducts(true)}>
            ▤ المنتجات والمبيعات
          </button>
          {isExpense && (
            <button className="btn btn-primary pill-lite" onClick={() => { setEditing(null); setShowForm(true); }}>
              + إضافة مصروف
            </button>
          )}
        </div>
      </div>

      <PeriodPicker value={period} onChange={setPeriod} />

      <div className="sum-row">
        <SumCard label="المبيعات" value={totalIncome} tone="up" />
        <SumCard label="المصاريف" value={totalExpense} tone="down" />
        <SumCard label="صافي الربح" value={net} tone={net >= 0 ? 'up' : 'down'} />
        <SumCard label="هامش الربح" value={margin} tone="neutral" isPercent />
      </div>

      <div className="tabs">
        <button className={`tab ${isExpense ? 'active' : ''}`} onClick={() => setTab('expenses')}>
          المصاريف ({expenses.length})
        </button>
        <button className={`tab ${!isExpense ? 'active' : ''}`} onClick={() => setTab('incomes')}>
          مبيعات المنتجات ({sales.length})
        </button>
      </div>

      <SearchBar key={tab} fields={TX_FIELDS}
        onSearch={(f) => setFilter(Object.keys(f).length ? f : null)}
        onReset={() => setFilter(null)} />

      <div className="card">
        {loading ? <Spinner /> : rows.length === 0 ? (
          <Empty kind={filter ? "search" : "ledger"}
            title={filter ? 'لا توجد نتائج' : `لا يوجد ${isExpense ? 'مصاريف' : 'مبيعات'} في هذه الفترة`}
            hint={isExpense ? 'ابدأ بإضافة أول مصروف' : 'سجّل مبيعات من شاشة المنتجات'} />
        ) : (
          <>
            <table className="table table-fill">
              <thead>
                {isExpense ? (
                  <tr>
                    <th>التاريخ</th>
                    <th>التصنيف</th>
                    <th>الطريقة</th>
                    <th>ملاحظة</th>
                    <th>المبلغ</th>
                    <th></th>
                  </tr>
                ) : (
                  <tr>
                    <th>التاريخ</th>
                    <th>المنتج</th>
                    <th>الكمية</th>
                    <th>سعر الوحدة</th>
                    <th>الإجمالي</th>
                    <th>العميل</th>
                  </tr>
                )}
              </thead>
              <tbody>
                {isExpense ? rows.map((r) => (
                  <tr key={r.id}>
                    <td className="num">{r.date?.slice(0, 10)}</td>
                    <td>{r.categoryName}</td>
                    <td>{methodName(r.paymentMethod)}</td>
                    <td style={{ color: 'var(--ink-faint)' }}>{r.description || '—'}</td>
                    <td><Money value={r.amount} className="down-c" /></td>
                    <td>
                      <div className="row-actions">
                        <button className="btn btn-ghost btn-sm"
                          onClick={() => { setEditing(r); setShowForm(true); }}>تعديل</button>
                        <button className="btn btn-danger btn-sm" onClick={() => del(r.id)}>حذف</button>
                      </div>
                    </td>
                  </tr>
                )) : rows.map((s) => (
                  <tr key={s.saleId}>
                    <td className="num">{s.saleDate?.slice(0, 10)}</td>
                    <td style={{ fontWeight: 600 }}>{s.productName}</td>
                    <td className="num">{s.quantitySold}</td>
                    <td><Money value={s.salePrice} /></td>
                    <td><Money value={s.salePrice * s.quantitySold} className="up-c" /></td>
                    <td style={{ color: 'var(--ink-faint)' }}>{s.customerName || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="table-foot">
              <span>{rows.length} {isExpense ? 'عملية' : 'بيعة'}</span>
              <span>الإجمالي: <Money value={isExpense ? totalExpense : totalIncome}
                className={isExpense ? 'down-c' : 'up-c'} /></span>
            </div>
          </>
        )}
      </div>

      {showForm && (
        <TransactionForm businessId={biz.businessId} kind="expenses" editing={editing}
          onClose={() => { setShowForm(false); setEditing(null); }}
          onDone={(msg) => { setShowForm(false); setEditing(null); setToast({ type: 'ok', msg }); load(); }} />
      )}
      <Toast toast={toast} onDone={() => setToast(null)} />

      <style>{`
        .back-btn { color: var(--brand); font-weight: 600; font-size: .92rem; margin-bottom: 14px; }
        .back-btn:hover { text-decoration: underline; }
        .head-actions { display: flex; gap: 8px; flex-wrap: wrap; }
        .page-head-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 18px; gap: 12px; flex-wrap: wrap; }
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

// ─────────── نموذج عملية المحل ───────────
function TransactionForm({ businessId, kind, editing, onClose, onDone }) {
  const isExpense = kind === 'expenses';
  const isEdit = !!editing;
  const [cats, setCats] = useState([]);
  const [form, setForm] = useState({
    categoryId: editing?.categoryId || '',
    amount: editing?.amount || '',
    date: editing?.date?.slice(0, 10) || new Date().toISOString().slice(0, 10),
    paymentMethod: editing?.paymentMethod || 1,
    source: editing?.source || '',
    description: editing?.description || '',
  });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');

  useEffect(() => {
    const fn = isExpense ? categoriesApi.getBusinessExpenses : categoriesApi.getBusinessIncomes;
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
        if (isEdit) await businessApi.updateExpense(editing.id, dto);
        else await businessApi.addExpense(businessId, dto);
      } else {
        const dto = {
          categoryId: Number(form.categoryId),
          amount: Number(form.amount),
          incomeDate: form.date,
          source: form.source || null,
          description: form.description || null,
        };
        if (isEdit) await businessApi.updateIncome(editing.id, dto);
        else await businessApi.addIncome(businessId, dto);
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
            <input value={form.source} onChange={set('source')} placeholder="مثلاً: مبيعات كاش" />
          </Field>
        )}

        <Field label="ملاحظة (اختياري)">
          <input value={form.description} onChange={set('description')} placeholder="تفاصيل" />
        </Field>

        {cats.length === 0 && <div className="form-warn">لا يوجد تصنيفات للبيزنس — أضف تصنيفات أولاً.</div>}
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

// ─────────── نموذج المحل ───────────
function BusinessForm({ editing, onClose, onDone }) {
  const isEdit = !!editing;
  const [form, setForm] = useState({
    name: editing?.businessName || '',
    description: editing?.description || '',
    currency: editing?.currency || 'EGP',
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
        description: form.description || null,
        currency: form.currency,
      };
      if (isEdit) await businessApi.update(editing.businessId, dto);
      else await businessApi.create(dto);
      onDone(isEdit ? 'تم التعديل' : 'تمت إضافة المحل');
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={isEdit ? 'تعديل المحل' : 'محل جديد'} onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="اسم المحل">
          <input value={form.name} onChange={set('name')} required placeholder="مثلاً: محل الأدوات" />
        </Field>
        <Field label="الوصف (اختياري)">
          <input value={form.description} onChange={set('description')} placeholder="نوع النشاط" />
        </Field>
        <Field label="العملة">
          <input value={form.currency} onChange={set('currency')} maxLength={3} required placeholder="EGP" />
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
