import { useEffect, useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts';
import { reportsApi } from '../api/client';
import { Spinner, Empty, Money } from '../components/UI';
import PeriodPicker from '../components/PeriodPicker';

const PIE_COLORS = ['#1a6b52', '#c08a2d', '#3a8f6f', '#d4a852', '#5aa989', '#8a6d1f', '#7fc0a5'];

export default function Reports() {
  const now = new Date();
  const [period, setPeriod] = useState({
    mode: 'month', year: now.getFullYear(), month: now.getMonth() + 1,
  });
  const [spending, setSpending] = useState(null);
  const [breakdown, setBreakdown] = useState([]);
  const [trend, setTrend] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState('');

  useEffect(() => {
    (async () => {
      setLoading(true);
      setErr('');
      try {
        const y = period.year || now.getFullYear();
        const m = period.month || now.getMonth() + 1;

        const [s, b, t] = await Promise.all([
          reportsApi.homeSpending(y, m),
          reportsApi.homeBreakdown(y, m),
          reportsApi.homeTrend(6),
        ]);
        setSpending(s);
        setBreakdown(b || []);
        setTrend((t || []).map((mo) => ({ ...mo, name: mo.label })));
      } catch (e) { setErr(e.message); }
      finally { setLoading(false); }
    })();
  }, [period]);

  if (err) return <div className="page-error">{err}</div>;

  return (
    <div>
      <div className="page-head">
        <h1>التقارير</h1>
        <p className="page-sub">تحليل إنفاق البيت</p>
      </div>

      <PeriodPicker value={period} onChange={setPeriod} />

      {loading ? <Spinner /> : (
      <>
      {/* ملخص الفترة */}
      {spending && (
        <div className="rep-summary">
          <SumCell label="الدخل" value={spending.totalIncome} tone="up" />
          <SumCell label="المصروف" value={spending.totalSpent} tone="down" />
          <SumCell label="الرصيد" value={spending.balance} tone={spending.balance >= 0 ? 'up' : 'down'} />
          <SumCell label="نسبة الادّخار" value={spending.savingsRate} tone="neutral" isPercent />
        </div>
      )}

      <div className="rep-charts">
        {/* توزيع الإنفاق */}
        <div className="card chart-card">
          <h3 className="chart-title">أين ذهبت المصاريف</h3>
          {breakdown.length === 0 ? (
            <Empty kind="chart" title="لا يوجد بيانات" />
          ) : (
            <>
              <ResponsiveContainer width="100%" height={240}>
                <PieChart>
                  <Pie data={breakdown} dataKey="total" nameKey="categoryName"
                    cx="50%" cy="50%" outerRadius={90} innerRadius={52}>
                    {breakdown.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
                  </Pie>
                  <Tooltip formatter={(v) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2 }).format(v)} />
                </PieChart>
              </ResponsiveContainer>
              <div className="cat-list">
                {breakdown.map((c, i) => (
                  <div key={c.categoryId} className="cat-item">
                    <span className="cat-dot" style={{ background: PIE_COLORS[i % PIE_COLORS.length] }} />
                    <span className="cat-name">{c.categoryName}</span>
                    <span className="cat-pct num">{c.percentage}%</span>
                    <Money value={c.total} className="cat-val" />
                  </div>
                ))}
              </div>
            </>
          )}
        </div>

        {/* الاتجاه الشهري */}
        <div className="card chart-card">
          <h3 className="chart-title">آخر ٦ شهور</h3>
          {trend.length === 0 ? (
            <Empty kind="chart" title="لا يوجد بيانات" />
          ) : (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={trend} margin={{ top: 10, right: 0, left: -20, bottom: 0 }}>
                <XAxis dataKey="name" tick={{ fontSize: 12, fontFamily: 'IBM Plex Sans Arabic' }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip formatter={(v) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2 }).format(v)} />
                <Legend wrapperStyle={{ fontSize: 13, fontFamily: 'IBM Plex Sans Arabic' }} />
                <Bar dataKey="income" name="الدخل" fill="#1a6b52" radius={[4, 4, 0, 0]} />
                <Bar dataKey="expense" name="المصروف" fill="#c0392b" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>
      </>
      )}

      <style>{`
        .page-head { margin-bottom: 18px; }
        .page-head h1 { font-size: 1.9rem; }
        .page-sub { color: var(--ink-soft); margin-top: 4px; }
        .page-error { background: var(--down-soft); color: var(--down); padding: 16px; border-radius: var(--r-md); }
        .rep-summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 22px; }
        .rep-charts { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
        .chart-card { padding: 22px; }
        .chart-title { font-size: 1.1rem; margin-bottom: 16px; }
        .cat-list { display: flex; flex-direction: column; gap: 10px; margin-top: 16px; }
        .cat-item { display: grid; grid-template-columns: auto 1fr auto auto; gap: 10px; align-items: center; font-size: .9rem; }
        .cat-dot { width: 11px; height: 11px; border-radius: 3px; }
        .cat-name { color: var(--ink); }
        .cat-pct { color: var(--ink-faint); font-size: .82rem; }
        .cat-val { font-weight: 600; }
        @media (max-width: 900px) { .rep-charts { grid-template-columns: 1fr; } .rep-summary { grid-template-columns: repeat(2, 1fr); } }
      `}</style>
    </div>
  );
}

function SumCell({ label, value, tone, isPercent }) {
  return (
    <div className="card sum-cell">
      <div className="sum-label">{label}</div>
      <div className={`sum-val ${tone}`}>
        {isPercent ? <span className="num">{Number(value).toFixed(1)}%</span> : <Money value={value} />}
      </div>
      <style>{`
        .sum-cell { padding: 16px 18px; }
        .sum-label { font-size: .82rem; color: var(--ink-soft); font-weight: 600; }
        .sum-val { font-family: var(--font-display); font-size: 1.5rem; font-weight: 700; margin-top: 6px; }
        .sum-val.up { color: var(--up); } .sum-val.down { color: var(--down); } .sum-val.neutral { color: var(--ink); }
      `}</style>
    </div>
  );
}
