import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import {
  Logo, IconDashboard, IconHome, IconBusiness, IconDebts,
  IconReports, IconCategories, IconLogout, IconSun, IconMoon,
} from './Icons';

const NAV = [
  { to: '/', label: 'اللوحة', Icon: IconDashboard, end: true },
  { to: '/home', label: 'البيت', Icon: IconHome },
  { to: '/business', label: 'البيزنس', Icon: IconBusiness },
  { to: '/debts', label: 'الديون', Icon: IconDebts },
  { to: '/reports', label: 'التقارير', Icon: IconReports },
  { to: '/categories', label: 'التصنيفات', Icon: IconCategories },
];

export default function Layout() {
  const { user, logout } = useAuth();
  const { isDark, toggle } = useTheme();
  const navigate = useNavigate();

  const doLogout = () => { logout(); navigate('/login'); };

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="side-brand">
          <Logo size={32} />
          <span className="side-name">مصاريف</span>
        </div>

        <nav className="side-nav">
          {NAV.map(({ to, label, Icon, end }) => (
            <NavLink key={to} to={to} end={end}
              className={({ isActive }) => `side-link ${isActive ? 'active' : ''}`}>
              <span className="side-mark" />
              <Icon size={19} />
              <span>{label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="side-foot">
          <button className="theme-btn" onClick={toggle}
            aria-label={isDark ? 'الوضع النهاري' : 'الوضع الليلي'}>
            <span className="theme-icon">{isDark ? <IconSun size={17} /> : <IconMoon size={17} />}</span>
            <span>{isDark ? 'نهاري' : 'ليلي'}</span>
          </button>

          <div className="side-user">
            <div className="side-avatar">{(user?.name || '؟').charAt(0)}</div>
            <div className="side-user-info">
              <div className="side-user-name">{user?.name}</div>
              <div className="side-user-mail">{user?.email}</div>
            </div>
            <button className="side-logout" onClick={doLogout} aria-label="تسجيل الخروج">
              <IconLogout size={17} />
            </button>
          </div>
        </div>
      </aside>

      <main className="app-main">
        <Outlet />
      </main>

      <style>{`
        .app-shell { display: grid; grid-template-columns: 246px 1fr; min-height: 100vh; }

        .sidebar {
          background: var(--surface); border-left: 1px solid var(--line);
          display: flex; flex-direction: column; padding: 22px 14px 16px;
          position: sticky; top: 0; height: 100vh;
          transition: background .35s var(--ease), border-color .35s var(--ease);
        }

        .side-brand {
          display: flex; align-items: center; gap: 10px;
          padding: 0 10px 22px;
        }
        .side-name {
          font-family: var(--font-display); font-size: 1.42rem; font-weight: 700;
          color: var(--ink); letter-spacing: -.02em;
        }

        .side-nav { display: flex; flex-direction: column; gap: 3px; flex: 1; }
        .side-link {
          position: relative;
          display: flex; align-items: center; gap: 11px;
          padding: 11px 13px; border-radius: var(--r-sm);
          color: var(--ink-soft); text-decoration: none; font-weight: 600; font-size: .93rem;
          transition: background .18s var(--ease), color .18s var(--ease), transform .18s var(--ease);
        }
        .side-link:hover { background: var(--surface-2); color: var(--ink); }
        .side-link:active { transform: scale(.98); }
        .side-link.active { background: var(--brand-soft); color: var(--brand-deep); }
        [data-theme='dark'] .side-link.active { color: var(--brand-lift); }

        /* علامة الصفحة النشطة */
        .side-mark {
          position: absolute; right: 0; top: 50%;
          width: 3px; height: 0; border-radius: 3px;
          background: var(--brand); transform: translateY(-50%);
          transition: height .3s var(--ease-back);
        }
        .side-link.active .side-mark { height: 20px; }

        .side-foot { display: flex; flex-direction: column; gap: 6px; }

        .theme-btn {
          display: flex; align-items: center; gap: 10px;
          padding: 10px 13px; border-radius: var(--r-sm);
          color: var(--ink-soft); font-weight: 600; font-size: .88rem;
          transition: background .18s var(--ease), color .18s var(--ease);
        }
        .theme-btn:hover { background: var(--surface-2); color: var(--ink); }
        .theme-icon { display: grid; place-items: center; transition: transform .5s var(--ease-back); }
        .theme-btn:hover .theme-icon { transform: rotate(-25deg) scale(1.1); }

        .side-user {
          display: flex; align-items: center; gap: 9px;
          padding: 11px 10px; border-top: 1px solid var(--line-soft);
        }
        .side-avatar {
          width: 36px; height: 36px; border-radius: 11px;
          background: linear-gradient(140deg, var(--brand), var(--brand-deep));
          color: #fff; font-weight: 700; font-family: var(--font-display);
          display: grid; place-items: center; flex-shrink: 0;
        }
        .side-user-info { flex: 1; min-width: 0; }
        .side-user-name { font-weight: 700; font-size: .88rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .side-user-mail { font-size: .74rem; color: var(--ink-faint); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .side-logout {
          width: 32px; height: 32px; border-radius: 9px; color: var(--ink-faint);
          display: grid; place-items: center; flex-shrink: 0;
          transition: all .18s var(--ease);
        }
        .side-logout:hover { background: var(--down-soft); color: var(--down); }

        .app-main { padding: 30px 38px 46px; max-width: 1200px; width: 100%; }

        @media (max-width: 820px) {
          .app-shell { grid-template-columns: 1fr; }
          .sidebar {
            position: fixed; bottom: 0; top: auto; left: 0; right: 0; height: auto;
            flex-direction: row; padding: 6px 4px;
            border-left: none; border-top: 1px solid var(--line);
            z-index: 100; box-shadow: 0 -3px 16px rgba(0,0,0,.07);
          }
          .side-brand, .side-user { display: none; }
          .side-nav { flex-direction: row; justify-content: space-around; width: 100%; gap: 0; }
          .side-link { flex-direction: column; gap: 3px; font-size: .68rem; padding: 7px 4px; }
          .side-mark { right: 50%; top: 2px; transform: translateX(50%); width: 0; height: 3px; }
          .side-link.active .side-mark { width: 18px; height: 3px; }
          .side-foot { flex-direction: row; align-items: center; }
          .theme-btn { padding: 7px; }
          .theme-btn span:last-child { display: none; }
          .app-main { padding: 18px 14px 84px; }
        }
      `}</style>
    </div>
  );
}
