import { useEffect, useState, useCallback } from 'react';
import { reportsApi, debtsApi, businessApi, productsApi } from '../api/client';
import { Money } from '../components/UI';
import { CountMoney, CountUp } from '../components/Reveal';
import { SkBeam, SkStatGrid, Sparkline, Delta } from '../components/Skeleton';
import BalanceBeam from '../components/BalanceBeam';
import PeriodPicker from '../components/PeriodPicker';
import { useAuth } from '../context/AuthContext';

export default function Dashboard() {
  const { user } = useAuth();
  const now = new Date();

  const [period, setPeriod] = useState({
    mode: 'month', year: now.getFullYear(), month: now.getMonth() + 1,
  });
  const [home, setHome] = useState({ income: 0, expense: 0 });
  const [prevHome, setPrevHome] = useState({ income: 0, expense: 0 });
  const [biz, setBiz] = useState({ sales: 0, expense: 0, count: 0, units: 0 });
  const [totals, setTotals] = useState(null);
  const [trend, setTrend] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState('');

  const toRange = (p) => {
    if (!p.year) return {};
    if (p.month) {
      const m = String(p.month).padStart(2, '0');
      const last = new Date(p.year, p.month, 0).getDate();
      return { dateFrom: `${p.year}-${m}-01`, dateTo: `${p.year}-${m}-${last}` };
    }
    return { dateFrom: `${p.year}-01-01`, dateTo: `${p.year}-12-31` };
  };

  const load = useCallback(async () => {
    setLoading(true);
    setErr('');
    try {
      const { year: y, month: m } = period;

      // ── البيت + الفترة السابقة للمقارنة ──
      const homeTask = (async () => {
        if (y && m) {
          const pm = m === 1 ? 12 : m - 1;
          const py = m === 1 ? y - 1 : y;
          const [cur, prev] = await Promise.all([
            reportsApi.homeSpending(y, m),
            reportsApi.homeSpending(py, pm).catch(() => null),
          ]);
          return {
            cur: { income: cur.totalIncome || 0, expense: cur.totalSpent || 0 },
            prev: prev ? { income: prev.totalIncome || 0, expense: prev.totalSpent || 0 }
                       : { income: 0, expense: 0 },
          };
        }
        // سنوي / الكل — نجمع الشهور
        let inc = 0, exp = 0;
        const year = y || now.getFullYear();
        const months = await Promise.all(
          Array.from({ length: 12 }, (_, i) =>
            reportsApi.homeSpending(year, i + 1).catch(() => null))
        );
        months.forEach((s) => {
          if (s) { inc += s.totalIncome || 0; exp += s.totalSpent || 0; }
        });
        return { cur: { income: inc, expense: exp }, prev: { income: 0, expense: 0 } };
      })();

      // ── البيزنس: الدخل من مبيعات المنتجات ──
      const bizTask = (async () => {
        const list = await businessApi.getAll();
        if (!list?.length) return { sales: 0, expense: 0, count: 0, units: 0 };

        const saleFilter = toRange(period);
        const expFilter = {};
        if (y) expFilter.year = y;
        if (m) expFilter.month = m;

        const rows = await Promise.all(list.map(async (b) => {
          try {
            const [sales, exp] = await Promise.all([
              productsApi.getSales(b.businessId, saleFilter),
              businessApi.searchExpenses(b.businessId, expFilter),
            ]);
            return {
              sales: (sales || []).reduce((s, x) => s + Number(x.salePrice) * Number(x.quantitySold), 0),
              units: (sales || []).reduce((s, x) => s + Number(x.quantitySold), 0),
              expense: (exp || []).reduce((s, x) => s + Number(x.amount), 0),
            };
          } catch { return { sales: 0, units: 0, expense: 0 }; }
        }));

        return {
          sales: rows.reduce((s, r) => s + r.sales, 0),
          expense: rows.reduce((s, r) => s + r.expense, 0),
          units: rows.reduce((s, r) => s + r.units, 0),
          count: list.length,
        };
      })();

      const [h, b, t, tr] = await Promise.all([
        homeTask, bizTask,
        debtsApi.getTotals(),
        reportsApi.homeTrend(6).catch(() => []),
      ]);

      setHome(h.cur);
      setPrevHome(h.prev);
      setBiz(b);
      setTotals(t);
      setTrend(tr || []);
    } catch (e) { setErr(e.message); }
    finally { setLoading(false); }
  }, [period]);

  useEffect(() => { load(); }, [load]);

  const businessProfit = biz.sales - biz.expense;
  const homeBalance = home.income - home.expense;

  const totalIn = home.income + biz.sales;
  const totalOut = home.expense + biz.expense;

  // بيانات الرسوم المصغّرة من الاتجاه
  const incSpark = trend.map((t) => t.income || 0);
  const expSpark = trend.map((t) => t.expense || 0);

  const greeting = (() => {
    const h = now.getHours();
    if (h < 12) return 'صباح الخير';
    if (h < 17) return 'مساء الخير';
    return 'مساء الخير';
  })();

  return (
    <div>
      <div className="page-head rise">
        <h1>{greeting}، {user?.name?.split(' ')[0]}</h1>
        <p className="page-sub">ميزانك المالي في لمحة</p>
      </div>

      <PeriodPicker value={period} onChange={setPeriod} />

      {err && <div className="page-error">{err}</div>}

      {loading ? (
        <div style={{ display: 'grid', gap: 18 }}>
          <SkBeam />
          <SkStatGrid count={4} />
          <SkStatGrid count={4} />
        </div>
      ) : (
        <>
          {/* ⭐ العنصر المميّز */}
          <div className="rise">
            <BalanceBeam income={totalIn} expense={totalOut} label="الصافي الكلّي" />
          </div>

          {/* البيزنس */}
          <SectionTitle>البيزنس</SectionTitle>
          <div className="stat-grid">
            <StatCard cls="rise rise-1" title="مبيعات المنتجات" value={biz.sales} tone="up"
              sub={`${biz.units} قطعة مباعة`} />
            <StatCard cls="rise rise-2" title="مصاريف البيزنس" value={biz.expense} tone="down"
              sub={`${biz.count} محل`} />
            <StatCard cls="rise rise-3" title="صافي الربح" value={businessProfit}
              tone={businessProfit >= 0 ? 'up' : 'down'} sub="مبيعات − مصاريف" />
            <StatCard cls="rise rise-4" title="هامش الربح"
              value={biz.sales === 0 ? 0 : (businessProfit / biz.sales) * 100}
              tone="neutral" sub="من المبيعات" isPercent />
          </div>

          {/* البيت */}
          <SectionTitle>البيت</SectionTitle>
          <div className="stat-grid">
            <StatCard cls="rise rise-1" title="دخل البيت" value={home.income} tone="up"
              sub="الفترة المختارة" spark={incSpark} sparkTone="up"
              delta={{ current: home.income, previous: prevHome.income }} />
            <StatCard cls="rise rise-2" title="مصاريف البيت" value={home.expense} tone="down"
              sub="الفترة المختارة" spark={expSpark} sparkTone="down"
              delta={{ current: home.expense, previous: prevHome.expense, invert: true }} />
            <StatCard cls="rise rise-3" title="الرصيد" value={homeBalance}
              tone={homeBalance >= 0 ? 'up' : 'down'} sub="دخل − مصاريف" />
            <StatCard cls="rise rise-4" title="نسبة الادّخار"
              value={home.income === 0 ? 0 : (homeBalance / home.income) * 100}
              tone="neutral" sub="من الدخل" isPercent />
          </div>

          {/* الديون */}
          <SectionTitle>الديون</SectionTitle>
          <div className="stat-grid">
            <StatCard cls="rise rise-1" title="عليّ" value={totals?.iOwe} tone="down" sub="إجمالي مستحق" />
            <StatCard cls="rise rise-2" title="ليّ" value={totals?.owedToMe} tone="up" sub="إجمالي مستحق" />
            <StatCard cls="rise rise-3" title="الصافي" value={totals?.netPosition}
              tone={totals?.netPosition >= 0 ? 'up' : 'down'} sub="ليّ − عليّ" />
            <StatCard cls="rise rise-4" title="ديون مفتوحة" value={totals?.openCount} tone="neutral"
              sub={totals?.overdueCount ? `${totals.overdueCount} متأخرة` : 'مفيش متأخرات'}
              isCount alert={totals?.overdueCount > 0} />
          </div>
        </>
      )}

      <style>{`
        .page-head { margin-bottom: 16px; }
        .page-head h1 { font-size: 1.85rem; }
        .page-sub { color: var(--ink-soft); margin-top: 3px; font-size: .94rem; }
        .page-error {
          background: var(--down-soft); color: var(--down);
          padding: 14px 18px; border-radius: var(--r-md); margin-bottom: 16px;
          font-size: .92rem; font-weight: 500;
        }
        .stat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
        @media (max-width: 900px) { .stat-grid { grid-template-columns: repeat(2, 1fr); } }
      `}</style>
    </div>
  );
}

function SectionTitle({ children }) {
  return (
    <h2 className="sec">
      <span className="eyebrow">{children}</span>
      <span className="sec-line" />
      <style>{`
        .sec {
          display: flex; align-items: center; gap: 14px;
          margin: 30px 0 14px;
        }
        .sec .eyebrow { font-size: .92rem; color: var(--ink-soft); }
        .sec-line { flex: 1; height: 1px; background: var(--line); }
      `}</style>
    </h2>
  );
}

function StatCard({ title, value, tone, sub, isCount, isPercent, spark, sparkTone, delta, alert, cls = '' }) {
  return (
    <div className={`card stat ${alert ? 'alert' : ''} ${cls}`}>
      <div className="stat-top">
        <span className="stat-title">{title}</span>
        {delta && <Delta {...delta} />}
      </div>

      <div className="stat-mid">
        <div className={`stat-value ${tone}`}>
          {isCount ? <CountUp value={value ?? 0} />
            : isPercent ? <span className="num">{Number(value || 0).toFixed(1)}%</span>
              : <CountMoney value={value} />}
        </div>
        {spark?.length > 1 && <Sparkline data={spark} tone={sparkTone} />}
      </div>

      <div className="stat-sub">{sub}</div>

      <style>{`
        .stat {
          padding: 16px 18px;
          position: relative; overflow: hidden;
        }
        .stat:hover { transform: translateY(-2px); box-shadow: var(--shadow-md); }
        .stat.alert::before {
          content: ''; position: absolute; top: 0; right: 0; left: 0; height: 2px;
          background: var(--copper);
        }
        .stat-top { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
        .stat-title { font-size: .83rem; color: var(--ink-soft); font-weight: 600; }
        .stat-mid { display: flex; align-items: flex-end; justify-content: space-between; gap: 10px; margin: 7px 0 3px; }
        .stat-value {
          font-family: var(--font-display); font-size: 1.5rem; font-weight: 700;
          letter-spacing: -.02em; line-height: 1.15;
        }
        .stat-value.up { color: var(--up); }
        .stat-value.down { color: var(--down); }
        .stat-value.neutral { color: var(--ink); }
        .stat-sub { font-size: .76rem; color: var(--ink-faint); }
      `}</style>
    </div>
  );
}
