// ─────────── هياكل التحميل ───────────

export const SkLine = ({ w = '100%', h = 14, style }) => (
  <div className="sk" style={{ width: w, height: h, ...style }} />
);

/** هيكل بطاقة إحصائية */
export const SkStat = () => (
  <div className="card" style={{ padding: '18px 20px' }}>
    <SkLine w="55%" h={11} />
    <SkLine w="75%" h={26} style={{ margin: '10px 0 6px' }} />
    <SkLine w="40%" h={9} />
  </div>
);

/** شبكة بطاقات */
export const SkStatGrid = ({ count = 4 }) => (
  <div className="sk-grid">
    {Array.from({ length: count }).map((_, i) => <SkStat key={i} />)}
    <style>{`
      .sk-grid { display: grid; grid-template-columns: repeat(${count}, 1fr); gap: 16px; }
      @media (max-width: 900px) { .sk-grid { grid-template-columns: repeat(2, 1fr); } }
    `}</style>
  </div>
);

/** هيكل جدول */
export const SkTable = ({ rows = 5, cols = 5 }) => (
  <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
    <div className="sk-head">
      {Array.from({ length: cols }).map((_, i) => (
        <SkLine key={i} w={i === 0 ? '70%' : '50%'} h={10} />
      ))}
    </div>
    {Array.from({ length: rows }).map((_, r) => (
      <div className="sk-row" key={r} style={{ animationDelay: `${r * 60}ms` }}>
        {Array.from({ length: cols }).map((_, c) => (
          <SkLine key={c} w={c === 0 ? '80%' : '55%'} h={13} />
        ))}
      </div>
    ))}
    <style>{`
      .sk-head, .sk-row {
        display: grid; grid-template-columns: repeat(${cols}, 1fr);
        gap: 16px; padding: 14px 16px; align-items: center;
      }
      .sk-head { border-bottom: 1px solid var(--line); }
      .sk-row { border-bottom: 1px solid var(--line-soft); animation: riseIn .4s var(--ease) both; }
      .sk-row:last-child { border-bottom: none; }
    `}</style>
  </div>
);

/** هيكل بطاقة الميزان */
export const SkBeam = () => (
  <div className="card sk-beam">
    <div>
      <SkLine w="90px" h={11} />
      <SkLine w="180px" h={38} style={{ margin: '10px 0 8px' }} />
      <SkLine w="140px" h={10} />
    </div>
    <div className="sk-beam-art">
      <SkLine w="100%" h={92} style={{ borderRadius: 12 }} />
    </div>
    <style>{`
      .sk-beam {
        padding: 26px 28px; display: grid;
        grid-template-columns: 1fr auto; gap: 28px; align-items: center;
      }
      .sk-beam-art { width: 300px; }
      @media (max-width: 780px) {
        .sk-beam { grid-template-columns: 1fr; }
        .sk-beam-art { width: 100%; }
      }
    `}</style>
  </div>
);

// ─────────── الرسم المصغّر ───────────

/**
 * خط صغير بيوضّح الاتجاه — بيرسم من مصفوفة أرقام
 */
export function Sparkline({ data = [], width = 68, height = 22, tone = 'up' }) {
  if (!data.length || data.every((d) => d === 0)) {
    return <div style={{ width, height }} />;
  }

  const max = Math.max(...data);
  const min = Math.min(...data);
  const range = max - min || 1;

  const pts = data.map((v, i) => {
    const x = (i / (data.length - 1 || 1)) * width;
    const y = height - ((v - min) / range) * (height - 4) - 2;
    return [x, y];
  });

  const line = pts.map(([x, y], i) => `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`).join(' ');
  const area = `${line} L${width},${height} L0,${height} Z`;

  const color = tone === 'down' ? 'var(--down)' : tone === 'neutral' ? 'var(--ink-faint)' : 'var(--up)';
  const id = `sg-${tone}-${data.length}-${Math.round(max)}`;

  return (
    <svg width={width} height={height} className="spark" aria-hidden="true">
      <defs>
        <linearGradient id={id} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={color} stopOpacity=".22" />
          <stop offset="100%" stopColor={color} stopOpacity="0" />
        </linearGradient>
      </defs>
      <path d={area} fill={`url(#${id})`} />
      <path d={line} fill="none" stroke={color} strokeWidth="1.6"
        strokeLinecap="round" strokeLinejoin="round" className="spark-line" />
      <circle cx={pts[pts.length - 1][0]} cy={pts[pts.length - 1][1]} r="2.2" fill={color} />
      <style>{`
        .spark-line {
          stroke-dasharray: 200; stroke-dashoffset: 200;
          animation: draw 1.1s var(--ease) forwards;
        }
        @keyframes draw { to { stroke-dashoffset: 0; } }
      `}</style>
    </svg>
  );
}

/** مؤشّر التغيّر عن الفترة السابقة */
export function Delta({ current = 0, previous = 0, invert = false }) {
  if (!previous) return null;

  const pct = ((current - previous) / Math.abs(previous)) * 100;
  if (!isFinite(pct)) return null;

  const rising = pct > 0;
  // في المصاريف، الارتفاع سيئ — نعكس اللون
  const good = invert ? !rising : rising;
  const cls = Math.abs(pct) < 0.5 ? 'flat' : good ? 'good' : 'bad';

  return (
    <span className={`delta ${cls}`}>
      <svg width="10" height="10" viewBox="0 0 10 10" aria-hidden="true">
        {rising
          ? <path d="M5 1.5 L9 8 L1 8 Z" fill="currentColor" />
          : <path d="M5 8.5 L1 2 L9 2 Z" fill="currentColor" />}
      </svg>
      <span className="num">{Math.abs(pct).toFixed(0)}%</span>
      <style>{`
        .delta {
          display: inline-flex; align-items: center; gap: 3px;
          font-size: .72rem; font-weight: 700;
          padding: 2px 7px; border-radius: 999px;
        }
        .delta.good { background: var(--up-soft); color: var(--up); }
        .delta.bad  { background: var(--down-soft); color: var(--down); }
        .delta.flat { background: var(--surface-2); color: var(--ink-faint); }
      `}</style>
    </span>
  );
}
