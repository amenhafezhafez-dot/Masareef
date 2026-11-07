import { useEffect, useState, useCallback } from 'react';
import { productsApi } from '../api/client';
import { Spinner, Empty, Money, Modal, Field, Toast } from '../components/UI';
import SearchBar from '../components/SearchBar';

const PRODUCT_FIELDS = [
  { key: 'name', label: 'اسم المنتج', type: 'text', param: 'name', placeholder: 'ابحث بالاسم...' },
  { key: 'code', label: 'الكود', type: 'text', param: 'productCode', placeholder: 'DR-001' },
  { key: 'price', label: 'سعر البيع', type: 'range', minParam: 'minPrice', maxParam: 'maxPrice' },
  { key: 'cost', label: 'سعر الشراء', type: 'range', minParam: 'minCost', maxParam: 'maxCost' },
];

const SALE_FIELDS = [
  { key: 'customer', label: 'اسم العميل', type: 'text', param: 'customerName', placeholder: 'ابحث...' },
  { key: 'date', label: 'التاريخ', type: 'dateRange', fromParam: 'dateFrom', toParam: 'dateTo' },
  { key: 'price', label: 'سعر البيع', type: 'range', minParam: 'minPrice', maxParam: 'maxPrice' },
  { key: 'qty', label: 'الكمية', type: 'range', minParam: 'minQuantity', maxParam: 'maxQuantity' },
];

const QUICK_FILTERS = [
  { key: 'all', label: 'الكل' },
  { key: 'neverSold', label: 'راكدة' },
  { key: 'bestSellers', label: 'الأكثر مبيعاً' },
  { key: 'inactive', label: 'معطّلة' },
];

export default function Products({ business, onBack }) {
  const [tab, setTab] = useState('products');
  const [quick, setQuick] = useState('all');
  const [filter, setFilter] = useState(null);
  const [products, setProducts] = useState([]);
  const [sales, setSales] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showProductForm, setShowProductForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [sellFor, setSellFor] = useState(null);
  const [editingSale, setEditingSale] = useState(null);
  const [detailId, setDetailId] = useState(null);
  const [toast, setToast] = useState(null);

  const bizId = business.businessId;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      if (tab === 'products') {
        let data;
        if (filter) data = await productsApi.search(bizId, filter);
        else if (quick === 'neverSold') data = await productsApi.getNeverSold(bizId);
        else if (quick === 'bestSellers') data = await productsApi.getBestSellers(bizId);
        else if (quick === 'inactive') data = await productsApi.getInactive(bizId);
        else data = await productsApi.getByBusiness(bizId);
        setProducts(data || []);
      } else {
        const data = await productsApi.getSales(bizId, filter || {});
        setSales(data || []);
      }
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
    finally { setLoading(false); }
  }, [bizId, tab, quick, filter]);

  useEffect(() => { load(); }, [load]);

  const switchTab = (t) => { setTab(t); setFilter(null); setQuick('all'); };

  const totalRevenue = products.reduce((s, p) => s + (p.totalRevenue || 0), 0);
  const totalUnits = products.reduce((s, p) => s + (p.totalUnitsSold || 0), 0);
  const salesTotal = sales.reduce((s, x) => s + x.salePrice * x.quantitySold, 0);
  const salesUnits = sales.reduce((s, x) => s + x.quantitySold, 0);

  const toggleActive = async (p) => {
    try {
      if (p.isActive) {
        if (!confirm(`تعطيل "${p.productName}"؟ سجل المبيعات هيفضل محفوظ.`)) return;
        await productsApi.deactivate(p.productId);
        setToast({ type: 'ok', msg: 'تم التعطيل' });
      } else {
        await productsApi.reactivate(p.productId);
        setToast({ type: 'ok', msg: 'تم التفعيل' });
      }
      load();
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
  };

  const delSale = async (id) => {
    if (!confirm('حذف البيعة؟')) return;
    try {
      await productsApi.deleteSale(id);
      setToast({ type: 'ok', msg: 'تم الحذف' });
      load();
    } catch (e) { setToast({ type: 'err', msg: e.message }); }
  };

  return (
    <div>
      <button className="back-btn" onClick={onBack}>→ رجوع للمحل</button>

      <div className="page-head-row">
        <div>
          <h1>المنتجات — {business.businessName}</h1>
          <p className="page-sub">إدارة البضاعة والمبيعات</p>
        </div>
        {tab === 'products' && (
          <button className="btn btn-primary"
            onClick={() => { setEditingProduct(null); setShowProductForm(true); }}>
            + منتج جديد
          </button>
        )}
      </div>

      <div className="sum-row">
        {tab === 'products' ? (
          <>
            <SumCard label="عدد المنتجات" value={products.length} isCount />
            <SumCard label="القطع المباعة" value={totalUnits} isCount />
            <SumCard label="إجمالي المبيعات" value={totalRevenue} tone="up" />
          </>
        ) : (
          <>
            <SumCard label="عدد البيعات" value={sales.length} isCount />
            <SumCard label="عدد القطع" value={salesUnits} isCount />
            <SumCard label="إجمالي المبيعات" value={salesTotal} tone="up" />
          </>
        )}
      </div>

      <div className="tabs">
        <button className={`tab ${tab === 'products' ? 'active' : ''}`}
          onClick={() => switchTab('products')}>المنتجات</button>
        <button className={`tab ${tab === 'sales' ? 'active' : ''}`}
          onClick={() => switchTab('sales')}>المبيعات</button>
      </div>

      {tab === 'products' && (
        <div className="quick-row">
          {QUICK_FILTERS.map((q) => (
            <button key={q.key} className={`chip ${quick === q.key && !filter ? 'on' : ''}`}
              onClick={() => { setQuick(q.key); setFilter(null); }}>
              {q.label}
            </button>
          ))}
        </div>
      )}

      <SearchBar key={tab}
        fields={tab === 'products' ? PRODUCT_FIELDS : SALE_FIELDS}
        onSearch={(f) => setFilter(Object.keys(f).length ? f : null)}
        onReset={() => setFilter(null)} />

      <div className="card">
        {loading ? <Spinner /> : tab === 'products' ? (
          products.length === 0 ? (
            <Empty kind={filter || quick !== 'all' ? "search" : "box"} title={filter || quick !== 'all' ? 'لا توجد نتائج' : 'لا يوجد منتجات'}
              hint={filter || quick !== 'all' ? 'جرّب فلتر مختلف' : 'أضف أول منتج'} />
          ) : (
            <table className="table table-fill">
              <thead>
                <tr>
                  <th>المنتج</th>
                  <th>الشراء</th>
                  <th>البيع</th>
                  <th>اتباع</th>
                  <th>إجمالي المبيعات</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {products.map((p) => (
                  <tr key={p.productId} className="clickable" onClick={() => setDetailId(p.productId)}>
                    <td>
                      <div style={{ fontWeight: 600 }}>{p.productName}</div>
                      {p.productCode && <div className="p-code">{p.productCode}</div>}
                    </td>
                    <td><Money value={p.originalPrice} /></td>
                    <td><Money value={p.sellingPrice} /></td>
                    <td>
                      <span className="num">{p.timesSold}</span>
                      <span className="p-units"> ({p.totalUnitsSold} قطعة)</span>
                    </td>
                    <td><Money value={p.totalRevenue} className="up-c" /></td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="row-actions">
                        {p.isActive && (
                          <button className="btn btn-primary btn-sm" onClick={() => setSellFor(p)}>بيع</button>
                        )}
                        <button className="btn btn-ghost btn-sm"
                          onClick={() => { setEditingProduct(p); setShowProductForm(true); }}>تعديل</button>
                        <button className={`btn btn-sm ${p.isActive ? 'btn-danger' : 'btn-ghost'}`}
                          onClick={() => toggleActive(p)}>
                          {p.isActive ? 'تعطيل' : 'تفعيل'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )
        ) : (
          sales.length === 0 ? (
            <Empty kind={filter ? "search" : "money"} title={filter ? 'لا توجد نتائج' : 'لا يوجد مبيعات'}
              hint={filter ? 'جرّب بحث مختلف' : 'سجّل أول بيعة من تاب المنتجات'} />
          ) : (
            <table className="table table-fill">
              <thead>
                <tr>
                  <th>التاريخ</th>
                  <th>المنتج</th>
                  <th>الكمية</th>
                  <th>سعر الوحدة</th>
                  <th>الإجمالي</th>
                  <th>العميل</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {sales.map((s) => (
                  <tr key={s.saleId}>
                    <td className="num">{s.saleDate?.slice(0, 10)}</td>
                    <td style={{ fontWeight: 600 }}>{s.productName}</td>
                    <td className="num">{s.quantitySold}</td>
                    <td><Money value={s.salePrice} /></td>
                    <td><Money value={s.salePrice * s.quantitySold} className="bold" /></td>
                    <td style={{ color: 'var(--ink-faint)' }}>{s.customerName || '—'}</td>
                    <td>
                      <div className="row-actions">
                        <button className="btn btn-ghost btn-sm"
                          onClick={() => setEditingSale(s)}>تعديل</button>
                        <button className="btn btn-danger btn-sm"
                          onClick={() => delSale(s.saleId)}>حذف</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )
        )}
      </div>

      {showProductForm && (
        <ProductForm businessId={bizId} editing={editingProduct}
          onClose={() => { setShowProductForm(false); setEditingProduct(null); }}
          onDone={(msg) => { setShowProductForm(false); setEditingProduct(null); setToast({ type: 'ok', msg }); load(); }} />
      )}

      {sellFor && (
        <SaleForm product={sellFor} onClose={() => setSellFor(null)}
          onDone={(msg) => { setSellFor(null); setToast({ type: 'ok', msg }); load(); }} />
      )}

      {editingSale && (
        <SaleForm sale={editingSale} onClose={() => setEditingSale(null)}
          onDone={(msg) => { setEditingSale(null); setToast({ type: 'ok', msg }); load(); }} />
      )}

      {detailId && <ProductDetail id={detailId} onClose={() => setDetailId(null)} />}

      <Toast toast={toast} onDone={() => setToast(null)} />

      <style>{`
        .back-btn { color: var(--brand); font-weight: 600; font-size: .92rem; margin-bottom: 14px; }
        .back-btn:hover { text-decoration: underline; }
        .page-head-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 18px; gap: 12px; flex-wrap: wrap; }
        .page-head-row h1 { font-size: 1.7rem; }
        .page-sub { color: var(--ink-soft); margin-top: 4px; }
        .sum-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 14px; margin-bottom: 18px; }
        .tabs { display: flex; gap: 4px; margin-bottom: 12px; background: var(--surface-2); padding: 4px; border-radius: var(--r-sm); width: fit-content; }
        .tab { padding: 8px 24px; border-radius: 8px; font-weight: 600; color: var(--ink-soft); transition: all .15s ease; }
        .tab.active { background: var(--surface); color: var(--brand-deep); box-shadow: var(--shadow-sm); }
        .quick-row { display: flex; gap: 7px; margin-bottom: 14px; flex-wrap: wrap; }
        .chip {
          padding: 6px 15px; border-radius: 999px; font-size: .85rem; font-weight: 600;
          background: var(--surface); border: 1px solid var(--line); color: var(--ink-soft);
          transition: all .15s ease;
        }
        .chip:hover { border-color: var(--brand); color: var(--brand); }
        .chip.on { background: var(--brand); border-color: var(--brand); color: #fff; }
        .clickable { cursor: pointer; }
        .p-code { font-size: .76rem; color: var(--ink-faint); font-family: monospace; }
        .p-units { font-size: .78rem; color: var(--ink-faint); }
        .row-actions { display: flex; gap: 5px; }
        .bold { font-weight: 700; }
        .up-c { color: var(--up); font-weight: 600; }
        @media (max-width: 900px) { .sum-row { grid-template-columns: repeat(2, 1fr); } }
        @media (max-width: 800px) { .table th:nth-child(2), .table td:nth-child(2) { display: none; } }
      `}</style>
    </div>
  );
}

function SumCard({ label, value, tone, isCount }) {
  return (
    <div className="card sum-card rise">
      <div className="sum-label">{label}</div>
      <div className={`sum-val ${tone || 'neutral'}`}>
        {isCount ? <span className="num">{value}</span> : <Money value={value} />}
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

// ─────────── نموذج المنتج ───────────
function ProductForm({ businessId, editing, onClose, onDone }) {
  const isEdit = !!editing;
  const [form, setForm] = useState({
    productName: editing?.productName || '',
    productCode: editing?.productCode || '',
    originalPrice: editing?.originalPrice || '',
    sellingPrice: editing?.sellingPrice || '',
    description: editing?.description || '',
  });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      const dto = {
        productName: form.productName,
        productCode: form.productCode || null,
        originalPrice: Number(form.originalPrice),
        sellingPrice: Number(form.sellingPrice),
        // الباكند لسه بيطلبه — بنبعت قيمة كبيرة عشان ميمنعش البيع
        stockQuantity: editing?.stockQuantity ?? 999999,
        description: form.description || null,
      };
      if (isEdit) await productsApi.update(editing.productId, dto);
      else await productsApi.create(businessId, dto);
      onDone(isEdit ? 'تم التعديل' : 'تمت إضافة المنتج');
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  return (
    <Modal title={isEdit ? 'تعديل المنتج' : 'منتج جديد'} onClose={onClose}>
      <form onSubmit={submit}>
        <Field label="اسم المنتج">
          <input value={form.productName} onChange={set('productName')} required
            placeholder="مثلاً: فستان أحمر مقاس M" />
        </Field>

        <Field label="الكود (اختياري)">
          <input value={form.productCode} onChange={set('productCode')} placeholder="DR-001" />
        </Field>

        <div className="two-col">
          <Field label="سعر الشراء">
            <input type="number" step="0.01" min="0" value={form.originalPrice}
              onChange={set('originalPrice')} required placeholder="0.00" />
          </Field>
          <Field label="سعر البيع">
            <input type="number" step="0.01" min="0" value={form.sellingPrice}
              onChange={set('sellingPrice')} required placeholder="0.00" />
          </Field>
        </div>

        <Field label="الوصف (اختياري)">
          <input value={form.description} onChange={set('description')} placeholder="تفاصيل إضافية" />
        </Field>

        {err && <div className="form-err">{err}</div>}
        <button className="btn btn-primary" style={{ width: '100%' }} disabled={busy}>
          {busy ? 'جارٍ...' : isEdit ? 'حفظ التعديل' : 'حفظ'}
        </button>
      </form>
      <style>{`
        .two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); font-size: .88rem; margin-bottom: 14px; }
      `}</style>
    </Modal>
  );
}

// ─────────── نموذج البيعة ───────────
function SaleForm({ product, sale, onClose, onDone }) {
  const isEdit = !!sale;
  const [form, setForm] = useState({
    quantitySold: sale?.quantitySold || 1,
    salePrice: sale?.salePrice || product?.sellingPrice || '',
    saleDate: sale?.saleDate?.slice(0, 10) || new Date().toISOString().slice(0, 10),
    customerName: sale?.customerName || '',
    note: sale?.note || '',
  });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState('');
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const total = (Number(form.quantitySold) || 0) * (Number(form.salePrice) || 0);

  const submit = async (e) => {
    e.preventDefault();
    setErr(''); setBusy(true);
    try {
      const dto = {
        quantitySold: Number(form.quantitySold),
        salePrice: Number(form.salePrice),
        saleDate: form.saleDate,
        customerName: form.customerName || null,
        note: form.note || null,
      };
      if (isEdit) await productsApi.updateSale(sale.saleId, dto);
      else await productsApi.addSale(product.productId, dto);
      onDone(isEdit ? 'تم تعديل البيعة' : 'تم تسجيل البيعة');
    } catch (e) { setErr(e.message); }
    finally { setBusy(false); }
  };

  const name = product?.productName || sale?.productName;

  return (
    <Modal title={`${isEdit ? 'تعديل بيعة' : 'بيع'} — ${name}`} onClose={onClose}>
      <form onSubmit={submit}>
        <div className="two-col">
          <Field label="الكمية">
            <input type="number" min="1" value={form.quantitySold}
              onChange={set('quantitySold')} required />
          </Field>
          <Field label="سعر البيع للوحدة">
            <input type="number" step="0.01" min="0" value={form.salePrice}
              onChange={set('salePrice')} required placeholder="0.00" />
          </Field>
        </div>

        {total > 0 && (
          <div className="sale-calc">
            <span>الإجمالي</span>
            <Money value={total} className="calc-total" />
          </div>
        )}

        <Field label="التاريخ">
          <input type="date" value={form.saleDate} onChange={set('saleDate')} required />
        </Field>

        <Field label="اسم العميل (اختياري)">
          <input value={form.customerName} onChange={set('customerName')} placeholder="الاسم" />
        </Field>

        <Field label="ملاحظة (اختياري)">
          <input value={form.note} onChange={set('note')} placeholder="تفاصيل" />
        </Field>

        {err && <div className="form-err">{err}</div>}

        <button className="btn btn-primary" style={{ width: '100%' }} disabled={busy}>
          {busy ? 'جارٍ...' : isEdit ? 'حفظ التعديل' : 'تسجيل البيعة'}
        </button>
      </form>
      <style>{`
        .two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
        .sale-calc {
          display: flex; justify-content: space-between; align-items: center;
          background: var(--brand-soft); padding: 13px 16px;
          border-radius: var(--r-sm); margin-bottom: 16px;
        }
        .sale-calc span { color: var(--ink-soft); font-weight: 600; }
        .calc-total { font-size: 1.3rem; font-weight: 800; color: var(--brand-deep); }
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); font-size: .88rem; margin-bottom: 14px; }
      `}</style>
    </Modal>
  );
}

// ─────────── تفاصيل المنتج ───────────
function ProductDetail({ id, onClose }) {
  const [p, setP] = useState(null);
  const [err, setErr] = useState('');

  useEffect(() => {
    productsApi.getById(id).then(setP).catch((e) => setErr(e.message));
  }, [id]);

  return (
    <Modal title="تفاصيل المنتج" onClose={onClose}>
      {err ? <div className="form-err">{err}</div> : !p ? <Spinner /> : (
        <div>
          <div className="d-name">{p.productName}</div>
          {p.productCode && <div className="d-code">{p.productCode}</div>}
          {p.description && <div className="d-desc">{p.description}</div>}

          <div className="d-grid">
            <div className="d-cell"><span>سعر الشراء</span><Money value={p.originalPrice} /></div>
            <div className="d-cell"><span>سعر البيع</span><Money value={p.sellingPrice} /></div>
            <div className="d-cell"><span>اتباع</span><span className="num">{p.timesSold} مرة</span></div>
            <div className="d-cell"><span>القطع المباعة</span><span className="num">{p.totalUnitsSold}</span></div>
            <div className="d-cell"><span>متوسط سعر البيع</span><Money value={p.averageSalePrice} /></div>
            <div className="d-cell"><span>إجمالي المبيعات</span><Money value={p.totalRevenue} className="up-c" /></div>
          </div>

          <h4 className="d-h">سجل المبيعات ({p.sales?.length || 0})</h4>
          {p.sales?.length ? (
            <div className="d-sales">
              {p.sales.map((s) => (
                <div key={s.saleId} className="d-sale">
                  <span className="num">{s.saleDate?.slice(0, 10)}</span>
                  <span className="num">{s.quantitySold} ×</span>
                  <Money value={s.salePrice} />
                  <Money value={s.total} className="bold" />
                  <span className="d-cust">{s.customerName || '—'}</span>
                </div>
              ))}
            </div>
          ) : <div className="d-empty">لم يُبع بعد</div>}
        </div>
      )}
      <style>{`
        .d-name { font-family: var(--font-display); font-size: 1.35rem; font-weight: 700; }
        .d-code { font-size: .8rem; color: var(--ink-faint); font-family: monospace; }
        .d-desc { font-size: .88rem; color: var(--ink-soft); margin-top: 6px; }
        .d-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin: 16px 0 20px; }
        .d-cell { background: var(--surface-2); padding: 10px 13px; border-radius: var(--r-sm); }
        .d-cell span:first-child { display: block; font-size: .76rem; color: var(--ink-faint); margin-bottom: 3px; }
        .d-cell .num { font-size: 1.05rem; font-weight: 700; }
        .d-h { font-size: 1rem; margin-bottom: 10px; }
        .d-sales { display: flex; flex-direction: column; gap: 7px; }
        .d-sale {
          display: grid; grid-template-columns: auto auto auto auto 1fr; gap: 10px;
          padding: 9px 12px; background: var(--surface-2); border-radius: var(--r-sm);
          font-size: .86rem; align-items: center;
        }
        .d-cust { color: var(--ink-faint); text-align: left; }
        .d-empty { color: var(--ink-faint); text-align: center; padding: 16px; font-size: .9rem; }
        .up-c { color: var(--up); } .bold { font-weight: 700; }
        .form-err { background: var(--down-soft); color: var(--down); padding: 10px 14px; border-radius: var(--r-sm); }
      `}</style>
    </Modal>
  );
}
