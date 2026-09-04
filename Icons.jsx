// أيقونات مرسومة خصيصاً — خط موحّد 1.6، نهايات مستديرة

const base = {
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.6,
  strokeLinecap: 'round',
  strokeLinejoin: 'round',
};

const Svg = ({ size = 20, children, ...rest }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" {...base} {...rest}>{children}</svg>
);

// ── التنقّل ──

/** اللوحة — ميزان */
export const IconDashboard = (p) => (
  <Svg {...p}>
    <path d="M12 4v16" />
    <path d="M5 8h14" />
    <path d="M5 8l-2.5 5a2.8 2.8 0 0 0 5 0z" />
    <path d="M19 8l-2.5 5a2.8 2.8 0 0 0 5 0z" />
    <path d="M8.5 20h7" />
  </Svg>
);

/** البيت */
export const IconHome = (p) => (
  <Svg {...p}>
    <path d="M4 10.5 12 4l8 6.5" />
    <path d="M6 10v9.5h12V10" />
    <path d="M10 19.5v-5h4v5" />
  </Svg>
);

/** البيزنس — واجهة محل بمظلّة */
export const IconBusiness = (p) => (
  <Svg {...p}>
    <path d="M4 9h16l-1.2-4.2A1 1 0 0 0 17.8 4H6.2a1 1 0 0 0-1 .8z" />
    <path d="M5.5 9v10.5h13V9" />
    <path d="M9.5 19.5v-6h5v6" />
    <path d="M8.7 4v5M12 4v5M15.3 4v5" />
  </Svg>
);

/** الديون — سهمان متبادلان */
export const IconDebts = (p) => (
  <Svg {...p}>
    <path d="M4 8.5h13" /><path d="M13.5 5 17 8.5 13.5 12" />
    <path d="M20 15.5H7" /><path d="M10.5 12 7 15.5 10.5 19" />
  </Svg>
);

/** التقارير — أعمدة */
export const IconReports = (p) => (
  <Svg {...p}>
    <path d="M4 20h16" />
    <rect x="5.5" y="12" width="3.4" height="6" rx="1" />
    <rect x="10.3" y="7" width="3.4" height="11" rx="1" />
    <rect x="15.1" y="14" width="3.4" height="4" rx="1" />
  </Svg>
);

/** التصنيفات — مربعات */
export const IconCategories = (p) => (
  <Svg {...p}>
    <rect x="4" y="4" width="7" height="7" rx="1.6" />
    <rect x="13" y="4" width="7" height="7" rx="1.6" />
    <rect x="4" y="13" width="7" height="7" rx="1.6" />
    <circle cx="16.5" cy="16.5" r="3.5" />
  </Svg>
);

/** المنتجات — صندوق */
export const IconProducts = (p) => (
  <Svg {...p}>
    <path d="M12 3 4 7v10l8 4 8-4V7z" />
    <path d="M4 7l8 4 8-4" />
    <path d="M12 11v10" />
  </Svg>
);

// ── الأفعال ──

export const IconPlus = (p) => (
  <Svg {...p}><path d="M12 5v14M5 12h14" /></Svg>
);

export const IconSearch = (p) => (
  <Svg {...p}><circle cx="10.5" cy="10.5" r="6.5" /><path d="M15.5 15.5 20 20" /></Svg>
);

export const IconEdit = (p) => (
  <Svg {...p}>
    <path d="M4 20h4L19 9a2.1 2.1 0 0 0-3-3L5 17z" />
    <path d="M14.5 6.5 17.5 9.5" />
  </Svg>
);

export const IconTrash = (p) => (
  <Svg {...p}>
    <path d="M4.5 6.5h15" />
    <path d="M9.5 6.5V4.8a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v1.7" />
    <path d="M6.5 6.5 7.4 20a1 1 0 0 0 1 1h7.2a1 1 0 0 0 1-1l.9-13.5" />
    <path d="M10.5 10.5v6M13.5 10.5v6" />
  </Svg>
);

export const IconClose = (p) => (
  <Svg {...p}><path d="M6 6l12 12M18 6L6 18" /></Svg>
);

export const IconBack = (p) => (
  <Svg {...p}><path d="M20 12H4" /><path d="M10 6 4 12l6 6" /></Svg>
);

/** سهم لليسار — اتجاه التقدّم في RTL */
export const IconArrowLeft = (p) => (
  <Svg {...p}><path d="M19 12H5" /><path d="M11 6 5 12l6 6" /></Svg>
);

/** سهم قُطري — لروابط الخروج */
export const IconArrowUpLeft = (p) => (
  <Svg {...p}><path d="M17 7 7 17" /><path d="M16 17H7V8" /></Svg>
);

export const IconCheck = (p) => (
  <Svg {...p}><path d="M4.5 12.5 9.5 17.5 19.5 7" /></Svg>
);

export const IconAlert = (p) => (
  <Svg {...p}>
    <circle cx="12" cy="12" r="8.5" />
    <path d="M12 7.5v5.5" /><circle cx="12" cy="16.3" r=".6" fill="currentColor" />
  </Svg>
);

export const IconLogout = (p) => (
  <Svg {...p}>
    <path d="M14 5.5H7a1.5 1.5 0 0 0-1.5 1.5v10A1.5 1.5 0 0 0 7 18.5h7" />
    <path d="M17 8.5 20.5 12 17 15.5" /><path d="M20.5 12H10" />
  </Svg>
);

export const IconSun = (p) => (
  <Svg {...p}>
    <circle cx="12" cy="12" r="4" />
    <path d="M12 2.5v2M12 19.5v2M2.5 12h2M19.5 12h2M5.2 5.2l1.4 1.4M17.4 17.4l1.4 1.4M18.8 5.2l-1.4 1.4M6.6 17.4l-1.4 1.4" />
  </Svg>
);

export const IconMoon = (p) => (
  <Svg {...p}>
    <path d="M20 13.5A8 8 0 0 1 10.5 4a8 8 0 1 0 9.5 9.5z" />
  </Svg>
);

export const IconChevronRight = (p) => (
  <Svg {...p}><path d="M9.5 5.5 16 12l-6.5 6.5" /></Svg>
);

export const IconChevronLeft = (p) => (
  <Svg {...p}><path d="M14.5 5.5 8 12l6.5 6.5" /></Svg>
);

// ── اللوجو ──

/** شعار مصاريف — ميزان داخل دائرة الحبر */
export const Logo = ({ size = 34 }) => (
  <svg width={size} height={size} viewBox="0 0 40 40" fill="none">
    <circle cx="20" cy="20" r="18.5" fill="var(--brand)" />
    <g stroke="#fff" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round">
      <path d="M20 11v18" />
      <path d="M11 15h18" />
      <path d="M11 15l-3 6a3.3 3.3 0 0 0 6 0z" />
      <path d="M29 15l-3 6a3.3 3.3 0 0 0 6 0z" />
      <path d="M16 29h8" />
    </g>
    <circle cx="20" cy="15" r="1.8" fill="var(--copper)" />
  </svg>
);
