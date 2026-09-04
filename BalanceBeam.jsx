import { useEffect, useState } from 'react';
import { money } from './UI';
import { useScrollCount } from '../lib/motion';

/**
 * ⭐ العنصر المميّز — شريط الميزان
 * ذراع بتميل حسب العلاقة بين الدخل والمصروف.
 * الميل محدود بـ ±14° عشان يفضل مقروء.
 */
export default function BalanceBeam({ income = 0, expense = 0, label = 'الصافي' }) {
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const t = setTimeout(() => setReady(true), 60);
    return () => clearTimeout(t);
  }, []);

  const net = income - expense;
  const total = income + expense;

  // نسبة الميل: +1 = الدخل غالب، -1 = المصروف غالب
  const ratio = total === 0 ? 0 : net / total;
  const angle = ready ? Math.max(-14, Math.min(14, ratio * 14)) : 0;

  // ارتفاع الكفّتين — عكس الميل
  const leftDrop = ready ? -angle * 1.6 : 0;    // كفّة الدخل (يمين بصرياً في RTL)
  const rightDrop = ready ? angle * 1.6 : 0;

  const state = net > 0 ? 'pos' : net < 0 ? 'neg' : 'even';

  // الرقم بيعدّ لما يظهر
  const [countRef, counted] = useScrollCount(Math.round(Math.abs(net)));
  const sign = net < 0 ? '-' : '';

  return (
    <div className={`beam-card ${state}`}>
      {/* الرقم */}
      <div className="beam-head">
        <span className="beam-label">{label}</span>
        <div className="beam-net">
          <span className="num" ref={countRef}>
            {sign}{new Intl.NumberFormat('en-US').format(counted)}
          </span>
        </div>
        <span className="beam-state">
          {net > 0 ? 'الميزان مايل لصالحك' : net < 0 ? 'المصروف أثقل' : 'متعادل'}
        </span>
      </div>

      {/* الميزان */}
      <div className="beam-stage" aria-hidden="true">
        <svg viewBox="0 0 300 132" className="beam-svg">
          {/* العمود */}
          <path d="M150 26 L150 104" className="beam-post" />
          <path d="M126 106 L174 106" className="beam-base" />
          <path d="M132 116 L168 116" className="beam-foot" />

          {/* الذراع — بتميل */}
          <g style={{ transform: `rotate(${angle}deg)`, transformOrigin: '150px 26px' }}
             className="beam-arm-g">
            <path d="M44 26 L256 26" className="beam-arm" />
            <circle cx="150" cy="26" r="4.5" className="beam-pivot" />
            {/* الخيوط */}
            <path d="M62 26 L62 44" className="beam-string" />
            <path d="M238 26 L238 44" className="beam-string" />
          </g>

          {/* الكفّتان — بتتحركوا لتحت وفوق */}
          <g style={{ transform: `translateY(${leftDrop}px)` }} className="beam-pan-g">
            <path d="M40 44 L84 44 L76 62 L48 62 Z" className="beam-pan pan-in" />
          </g>
          <g style={{ transform: `translateY(${rightDrop}px)` }} className="beam-pan-g">
            <path d="M216 44 L260 44 L252 62 L224 62 Z" className="beam-pan pan-out" />
          </g>
        </svg>

        {/* التسميات تحت الكفّتين */}
        <div className="beam-legend">
          <div className="leg leg-out">
            <span className="leg-dot out" />
            <span className="leg-txt">المصروف</span>
            <span className="num leg-val">{money(expense)}</span>
          </div>
          <div className="leg leg-in">
            <span className="leg-dot in" />
            <span className="leg-txt">الدخل</span>
            <span className="num leg-val">{money(income)}</span>
          </div>
        </div>
      </div>

      <style>{`
        .beam-card {
          background: var(--surface);
          border: 1px solid var(--line);
          border-radius: var(--r-lg);
          padding: 26px 28px 20px;
          display: grid; grid-template-columns: 1fr auto; gap: 28px;
          align-items: center;
          box-shadow: var(--shadow-sm);
          position: relative; overflow: hidden;
          transition: background .35s var(--ease), border-color .35s var(--ease);
        }
        /* خط علوي بلون الحالة */
        .beam-card::before {
          content: ''; position: absolute; top: 0; right: 0; left: 0; height: 3px;
          background: var(--ink-faint);
          transition: background .5s var(--ease);
        }
        .beam-card.pos::before { background: linear-gradient(90deg, var(--up), transparent); }
        .beam-card.neg::before { background: linear-gradient(90deg, var(--down), transparent); }

        .beam-label { font-size: .88rem; color: var(--ink-soft); font-weight: 600; }
        .beam-net .num {
          font-family: var(--font-display); font-size: 2.7rem; font-weight: 700;
          display: block; margin: 4px 0 2px; letter-spacing: -.03em;
          color: var(--ink);
          transition: color .5s var(--ease);
        }
        .beam-card.pos .beam-net .num { color: var(--up); }
        .beam-card.neg .beam-net .num { color: var(--down); }
        .beam-state { font-size: .84rem; color: var(--ink-faint); }

        .beam-stage { width: 300px; }
        .beam-svg { width: 100%; height: auto; overflow: visible; }

        .beam-post, .beam-base, .beam-foot, .beam-arm, .beam-string {
          stroke: var(--ink-faint); fill: none;
          stroke-linecap: round; stroke-width: 2;
        }
        .beam-base { stroke-width: 2.5; }
        .beam-foot { stroke-width: 3; stroke: var(--ink-soft); }
        .beam-string { stroke-width: 1.2; }
        .beam-pivot { fill: var(--copper); }

        .beam-arm { stroke: var(--ink-soft); stroke-width: 2.5; }

        .beam-arm-g, .beam-pan-g {
          transition: transform 1.1s var(--ease-back);
        }

        .beam-pan { stroke-width: 1.6; stroke-linejoin: round; }
        .pan-in  { fill: var(--up-soft);   stroke: var(--up); }
        .pan-out { fill: var(--down-soft); stroke: var(--down); }

        .beam-legend {
          display: flex; justify-content: space-between;
          margin-top: 6px; padding: 0 6px;
        }
        .leg { display: flex; flex-direction: column; align-items: center; gap: 1px; }
        .leg-dot { width: 8px; height: 8px; border-radius: 2px; }
        .leg-dot.in { background: var(--up); }
        .leg-dot.out { background: var(--down); }
        .leg-txt { font-size: .74rem; color: var(--ink-faint); }
        .leg-val { font-size: .88rem; font-weight: 700; }
        .leg-in .leg-val { color: var(--up); }
        .leg-out .leg-val { color: var(--down); }

        @media (max-width: 780px) {
          .beam-card { grid-template-columns: 1fr; gap: 18px; text-align: center; }
          .beam-stage { width: 100%; max-width: 300px; margin: 0 auto; }
          .beam-net .num { font-size: 2.2rem; }
        }
      `}</style>
    </div>
  );
}
