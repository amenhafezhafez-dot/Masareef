import { useEffect, useRef, useState } from 'react';
import { Logo } from './Icons';
import { easeInOutCubic, spring, stopScroll, startScroll } from '../lib/motion';

const FILL_MS = 1300;

/**
 * لودر الافتتاح — بيعدّ من ٠٠٠ لـ ١٠٠ وبيطلع لفوق
 * onDone بتتنده بعد ما الخروج يخلص
 */
export default function AppLoader({ onDone }) {
  const [progress, setProgress] = useState(0);
  const [gone, setGone] = useState(false);
  const panelRef = useRef(null);
  const innerRef = useRef(null);

  useEffect(() => {
    stopScroll();

    // احترام تقليل الحركة — نخرج فوراً
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      setProgress(100);
      const t = setTimeout(() => { startScroll(); onDone?.(); setGone(true); }, 220);
      return () => clearTimeout(t);
    }

    const t0 = performance.now();
    let raf;

    const tick = (now) => {
      const t = Math.min(1, (now - t0) / FILL_MS);
      setProgress(Math.round(easeInOutCubic(t) * 100));

      if (t < 1) {
        raf = requestAnimationFrame(tick);
      } else {
        exit();
      }
    };

    const exit = () => {
      const panel = panelRef.current;
      const inner = innerRef.current;

      // المحتوى يبهت ويطلع شوية
      spring({
        from: 0, to: 1, tension: 260, friction: 26,
        onUpdate: (v) => {
          if (!inner) return;
          inner.style.opacity = String(1 - v);
          inner.style.transform = `translateY(${-12 * v}px)`;
        },
      });

      // اللوحة تنزلق لفوق
      spring({
        from: 0, to: 1, tension: 220, friction: 30,
        onUpdate: (v) => {
          if (panel) panel.style.transform = `translateY(${-100 * v}%)`;
        },
        onRest: () => {
          startScroll();
          onDone?.();
          setGone(true);
        },
      });
    };

    raf = requestAnimationFrame(tick);
    return () => { cancelAnimationFrame(raf); startScroll(); };
  }, [onDone]);

  if (gone) return null;

  return (
    <div className="loader" ref={panelRef} aria-hidden="true">
      <div className="loader-inner" ref={innerRef}>
        <div className="loader-brand">
          <Logo size={30} />
          <span>مصاريف</span>
        </div>
        <p className="loader-tag">كل قرش في مكانه.</p>
      </div>

      <div className="loader-bar" ref={null}>
        <div className="loader-track">
          <div className="loader-fill" style={{ width: `${progress}%` }} />
        </div>
        <div className="loader-meta">
          <span>جارٍ الفتح</span>
          <span className="num loader-count">{String(progress).padStart(3, '0')}</span>
        </div>
      </div>

      <style>{`
        .loader {
          position: fixed; inset: 0; z-index: 200;
          display: flex; flex-direction: column; align-items: center; justify-content: center;
          gap: 2rem;
          background: var(--brand-deep);
          color: #fff;
          border-radius: 0 0 2rem 2rem;
          will-change: transform;
        }
        /* شبكة دفتر خفيفة */
        .loader::before {
          content: ''; position: absolute; inset: 0;
          background-image:
            linear-gradient(rgba(255,255,255,.05) 1px, transparent 1px),
            linear-gradient(90deg, rgba(255,255,255,.05) 1px, transparent 1px);
          background-size: 36px 36px;
          mask-image: radial-gradient(ellipse at 50% 45%, #000 25%, transparent 72%);
        }
        .loader > * { position: relative; z-index: 1; }

        .loader-inner {
          display: flex; flex-direction: column; align-items: center;
          gap: .85rem; text-align: center;
        }
        .loader-brand {
          display: flex; align-items: center; gap: 11px;
          font-family: var(--font-display); font-size: 1.65rem; font-weight: 700;
          letter-spacing: -.02em;
        }
        .loader-tag {
          max-width: 24ch; font-size: .88rem;
          color: rgba(255,255,255,.55);
        }

        .loader-bar { width: min(22rem, 72vw); display: flex; flex-direction: column; gap: .7rem; }
        .loader-track { height: 1px; background: rgba(255,255,255,.15); overflow: hidden; }
        .loader-fill {
          height: 100%; background: var(--copper);
          transition: width .1s ease-out;
        }
        .loader-meta {
          display: flex; justify-content: space-between; align-items: center;
          font-size: .72rem; font-weight: 500; letter-spacing: .04em;
          color: rgba(255,255,255,.45);
        }
        .loader-count { color: rgba(255,255,255,.85); font-size: .8rem; }
      `}</style>
    </div>
  );
}
