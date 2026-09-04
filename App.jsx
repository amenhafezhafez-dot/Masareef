import { useEffect, useState } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import Lenis from 'lenis';
import { useAuth } from './context/AuthContext';
import { useAdaptiveGrid, setLenis } from './lib/motion';
import AppLoader from './components/AppLoader';
import Layout from './components/Layout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Home from './pages/Home';
import Business from './pages/Business';
import Debts from './pages/Debts';
import Reports from './pages/Reports';
import Categories from './pages/Categories';

function Protected({ children }) {
  const { isAuthed } = useAuth();
  return isAuthed ? children : <Navigate to="/login" replace />;
}

export default function App() {
  const { isAuthed } = useAuth();
  const [ready, setReady] = useState(false);

  // الشبكة المتكيّفة
  useAdaptiveGrid();

  // السكرول الناعم
  useEffect(() => {
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduce) return;

    const lenis = new Lenis({ smoothWheel: true, duration: 1.05 });
    setLenis(lenis);

    let raf;
    const loop = (t) => { lenis.raf(t); raf = requestAnimationFrame(loop); };
    raf = requestAnimationFrame(loop);

    return () => { cancelAnimationFrame(raf); lenis.destroy(); setLenis(null); };
  }, []);

  return (
    <>
      {!ready && <AppLoader onDone={() => setReady(true)} />}

      <div className={`app-fade ${ready ? 'in' : ''}`}>
        <Routes>
          <Route path="/login" element={isAuthed ? <Navigate to="/" replace /> : <Login />} />

          <Route path="/" element={<Protected><Layout /></Protected>}>
            <Route index element={<Dashboard />} />
            <Route path="home" element={<Home />} />
            <Route path="business" element={<Business />} />
            <Route path="debts" element={<Debts />} />
            <Route path="reports" element={<Reports />} />
            <Route path="categories" element={<Categories />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>

      <style>{`
        .app-fade { opacity: 0; transition: opacity .5s var(--ease); }
        .app-fade.in { opacity: 1; }
      `}</style>
    </>
  );
}
