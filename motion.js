import { useEffect, useRef, useState } from 'react';

// ═══════════════════════════════════════════════════════
//  نظام الحركة — نوابض، شبكة متكيّفة، عدّادات
// ═══════════════════════════════════════════════════════

/**
 * نابض بسيط — يحاكي react-spring
 * accel = tension*(target - x) - friction*v
 */
export function spring({ from = 0, to = 1, tension = 210, friction = 26, onUpdate, onRest }) {
  let x = from;
  let v = 0;
  let raf;
  const dt = 1 / 60;

  const step = () => {
    const accel = tension * (to - x) - friction * v;
    v += accel * dt;
    x += v * dt;

    onUpdate?.(x);

    if (Math.abs(to - x) < 0.001 && Math.abs(v) < 0.001) {
      onUpdate?.(to);
      onRest?.();
      return;
    }
    raf = requestAnimationFrame(step);
  };

  raf = requestAnimationFrame(step);
  return () => cancelAnimationFrame(raf);
}

// ─────────── منحنيات مكافئة للنوابض ───────────
export const EASE = {
  entrance: 'cubic-bezier(.22,1,.36,1)',      // 210/26
  smooth:   'cubic-bezier(.16,1,.3,1)',        // 200/24
  snappy:   'cubic-bezier(.2,.8,.2,1)',        // 320/18
  outCubic: 'cubic-bezier(.215,.61,.355,1)',
  outQuart: 'cubic-bezier(.165,.84,.44,1)',
};

// ─────────── منحنيات التوقيت ───────────
export const easeInOutCubic = (t) =>
  t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;

export const easeOutCubic = (t) => 1 - Math.pow(1 - t, 3);

// ═══════════════════════════════════════════════════════
//  الشبكة المتكيّفة
// ═══════════════════════════════════════════════════════

/**
 * حجم الخط بيتغيّر مع عرض الشاشة عشان التصميم يفضل متناسق.
 *
 * ملاحظة: النسخة الأصلية (لصفحة تسويقية) بتخلّي كل حاجة تكبر
 * وتصغر بحرية. هنا التطبيق كثيف بالبيانات، فالمدى محدود بين
 * 14px و 19px — الجداول تفضل مقروءة على أي شاشة.
 */
export function applyAdaptiveGrid() {
  const BASE = 16;
  const REF = 1600;
  const COEF = 0.55;

  const w = window.innerWidth;
  if (w < 640) {
    document.documentElement.style.removeProperty('font-size');
    return;
  }

  const reduction = ((REF - w) / REF) * 100;
  const raw = BASE - (BASE * (reduction * COEF)) / 100;
  const size = Math.max(14, Math.min(19, raw));

  document.documentElement.style.fontSize = `${size.toFixed(2)}px`;
}

export function useAdaptiveGrid() {
  useEffect(() => {
    applyAdaptiveGrid();
    window.addEventListener('resize', applyAdaptiveGrid);
    return () => window.removeEventListener('resize', applyAdaptiveGrid);
  }, []);
}

// ═══════════════════════════════════════════════════════
//  قفل السكرول
// ═══════════════════════════════════════════════════════

let lenisRef = null;
export const setLenis = (l) => { lenisRef = l; };

export function stopScroll() {
  lenisRef?.stop();
  const h = document.documentElement;
  h.style.position = 'relative';
  h.style.overflow = 'hidden';
  h.style.height = '100%';
}

export function startScroll() {
  lenisRef?.start();
  const h = document.documentElement;
  h.style.removeProperty('position');
  h.style.removeProperty('overflow');
  h.style.removeProperty('height');
}

// ═══════════════════════════════════════════════════════
//  الظهور عند التمرير
// ═══════════════════════════════════════════════════════

/**
 * بيرجّع ref + هل ظهر — مرة واحدة بس
 */
export function useInView({ threshold = 0.15, once = true } = {}) {
  const ref = useRef(null);
  const [seen, setSeen] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el || (once && seen)) return;

    const io = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setSeen(true);
          if (once) io.disconnect();
        } else if (!once) {
          setSeen(false);
        }
      },
      { threshold }
    );

    io.observe(el);
    return () => io.disconnect();
  }, [threshold, once, seen]);

  return [ref, seen];
}

/**
 * عدّاد بيتحرّك حسب موضع العنصر من الشاشة
 * يبدأ لما أعلى العنصر يوصل أسفل الشاشة، ويكتمل لما منتصفه يوصل منتصفها
 */
export function useScrollCount(target = 0, { throttle = 30 } = {}) {
  const ref = useRef(null);
  const [value, setValue] = useState(0);
  const lastRun = useRef(0);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    // احترام تقليل الحركة
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      setValue(target);
      return;
    }

    const compute = () => {
      const now = Date.now();
      if (now - lastRun.current < throttle) return;
      lastRun.current = now;

      const r = el.getBoundingClientRect();
      const vh = window.innerHeight;

      const start = vh;                    // أعلى العنصر عند أسفل الشاشة
      const end = vh / 2 - r.height / 2;   // منتصف العنصر عند منتصف الشاشة
      const span = start - end || 1;

      const p = Math.max(0, Math.min(1, (start - r.top) / span));
      setValue(Math.round(p * target));
    };

    compute();
    window.addEventListener('scroll', compute, { passive: true });
    window.addEventListener('resize', compute);
    return () => {
      window.removeEventListener('scroll', compute);
      window.removeEventListener('resize', compute);
    };
  }, [target, throttle]);

  return [ref, value];
}
